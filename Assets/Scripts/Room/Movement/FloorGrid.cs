using System.Collections.Generic;
using UnityEngine;

namespace Room.Movement
{
    public class FloorGrid : MonoBehaviour
    {
        [SerializeField] LayerMask groundMask;
        [SerializeField] LayerMask obstacleMask;
        [SerializeField] float cellSize = 0.15f;
        [SerializeField] float clearance = 0.1f;

        static readonly Vector2Int[] Steps =
        {
            new(1, 0), new(-1, 0), new(0, 1), new(0, -1),
            new(1, 1), new(1, -1), new(-1, 1), new(-1, -1)
        };

        Vector2 origin;
        int width;
        int height;
        bool[] walkable = new bool[0];
        bool built;

        public void Rebuild()
        {
            built = true;
            Bounds bounds = default;
            bool any = false;

            foreach (var col in FindObjectsByType<Collider2D>(FindObjectsSortMode.None))
            {
                if ((groundMask.value & (1 << col.gameObject.layer)) == 0) continue;
                if (any) bounds.Encapsulate(col.bounds);
                else bounds = col.bounds;
                any = true;
            }

            if (!any)
            {
                width = height = 0;
                walkable = new bool[0];
                return;
            }

            origin = bounds.min;
            width = Mathf.CeilToInt(bounds.size.x / cellSize);
            height = Mathf.CeilToInt(bounds.size.y / cellSize);
            walkable = new bool[width * height];

            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                Vector2 p = CellCenter(new Vector2Int(x, y));
                walkable[y * width + x] =
                    Physics2D.OverlapPoint(p, groundMask) != null &&
                    Physics2D.OverlapCircle(p, clearance, obstacleMask) == null;
            }
        }

        public List<Vector3> FindPath(Vector3 from, Vector3 to)
        {
            if (!built) Rebuild();
            var path = new List<Vector3>();

            Vector2Int start = NearestWalkable(ToCell(from));
            Vector2Int goal = NearestWalkable(ToCell(to));
            if (start.x < 0 || goal.x < 0) return path;

            int count = width * height;
            int startIndex = Index(start);
            int goalIndex = Index(goal);
            var cost = new float[count];
            var cameFrom = new int[count];
            var closed = new bool[count];
            for (int i = 0; i < count; i++)
            {
                cost[i] = float.MaxValue;
                cameFrom[i] = -1;
            }

            var open = new List<int> { startIndex };
            cost[startIndex] = 0f;

            while (open.Count > 0)
            {
                int best = 0;
                float bestScore = float.MaxValue;
                for (int i = 0; i < open.Count; i++)
                {
                    float score = cost[open[i]] + Distance(open[i], goalIndex);
                    if (score >= bestScore) continue;
                    bestScore = score;
                    best = i;
                }

                int current = open[best];
                open[best] = open[open.Count - 1];
                open.RemoveAt(open.Count - 1);

                if (current == goalIndex) break;
                if (closed[current]) continue;
                closed[current] = true;

                var c = new Vector2Int(current % width, current / width);
                foreach (var step in Steps)
                {
                    var n = c + step;
                    if (!IsWalkable(n)) continue;

                    bool diagonal = step.x != 0 && step.y != 0;
                    if (diagonal && (!IsWalkable(new Vector2Int(n.x, c.y)) || !IsWalkable(new Vector2Int(c.x, n.y))))
                        continue;

                    int next = Index(n);
                    if (closed[next]) continue;

                    float nextCost = cost[current] + (diagonal ? 1.4142f : 1f);
                    if (nextCost >= cost[next]) continue;

                    cost[next] = nextCost;
                    cameFrom[next] = current;
                    open.Add(next);
                }
            }

            if (goalIndex != startIndex && cameFrom[goalIndex] < 0) return path;

            var cells = new List<Vector2>();
            for (int i = goalIndex; i >= 0; i = i == startIndex ? -1 : cameFrom[i])
                cells.Add(CellCenter(new Vector2Int(i % width, i / width)));
            cells.Reverse();
            if (ToCell(to) == goal) cells[cells.Count - 1] = to;

            Vector2 anchor = from;
            int k = 0;
            while (k < cells.Count)
            {
                int far = k;
                for (int j = cells.Count - 1; j > k; j--)
                {
                    if (!HasLineOfSight(anchor, cells[j])) continue;
                    far = j;
                    break;
                }

                path.Add(cells[far]);
                anchor = cells[far];
                k = far + 1;
            }

            return path;
        }

        bool HasLineOfSight(Vector2 a, Vector2 b)
        {
            int samples = Mathf.CeilToInt(Vector2.Distance(a, b) / (cellSize * 0.5f));
            for (int i = 1; i <= samples; i++)
                if (!IsWalkable(ToCell(Vector2.Lerp(a, b, (float)i / samples))))
                    return false;
            return true;
        }

        Vector2Int NearestWalkable(Vector2Int cell)
        {
            if (IsWalkable(cell)) return cell;

            var nearest = new Vector2Int(-1, -1);
            int nearestDistance = int.MaxValue;
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                if (!walkable[y * width + x]) continue;
                int d = (x - cell.x) * (x - cell.x) + (y - cell.y) * (y - cell.y);
                if (d >= nearestDistance) continue;
                nearestDistance = d;
                nearest = new Vector2Int(x, y);
            }

            return nearest;
        }

        bool IsWalkable(Vector2Int c) =>
            c.x >= 0 && c.y >= 0 && c.x < width && c.y < height && walkable[c.y * width + c.x];

        int Index(Vector2Int c) => c.y * width + c.x;

        float Distance(int a, int b) =>
            Vector2.Distance(new Vector2(a % width, a / width), new Vector2(b % width, b / width));

        Vector2Int ToCell(Vector2 p) =>
            new(Mathf.FloorToInt((p.x - origin.x) / cellSize), Mathf.FloorToInt((p.y - origin.y) / cellSize));

        Vector2 CellCenter(Vector2Int c) =>
            origin + new Vector2((c.x + 0.5f) * cellSize, (c.y + 0.5f) * cellSize);

        void OnDrawGizmosSelected()
        {
            if (!built) return;

            Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
            var size = new Vector3(cellSize, cellSize, 0f);
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var center = CellCenter(new Vector2Int(x, y));
                if (!walkable[y * width + x] && Physics2D.OverlapPoint(center, groundMask) != null)
                    Gizmos.DrawCube(center, size);
            }
        }
    }
}
