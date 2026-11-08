using System;
using System.Collections.Generic;
using UnityEngine;

namespace Room.Movement
{
    public class PlayerMover : MonoBehaviour
    {
        [SerializeField] float speed = 3f;
        [SerializeField] FloorGrid grid;
        [SerializeField] SpriteRenderer body;
        [SerializeField] Sprite frontSprite;
        [SerializeField] Sprite backSprite;

        readonly Queue<Vector3> waypoints = new();
        Action onArrived;

        void Awake()
        {
            if (grid == null) grid = FindFirstObjectByType<FloorGrid>();
            if (body == null) body = GetComponentInChildren<SpriteRenderer>();
        }

        public void MoveTo(Vector3 point, Action arrived = null)
        {
            waypoints.Clear();
            onArrived = arrived;

            if (grid != null)
                foreach (var p in grid.FindPath(transform.position, point))
                    waypoints.Enqueue(Flat(p));
            else
                waypoints.Enqueue(Flat(point));

            if (waypoints.Count == 0) Arrive();
        }

        void Update()
        {
            if (waypoints.Count == 0) return;

            Vector3 target = waypoints.Peek();
            Face(target - transform.position);
            transform.position = Vector3.MoveTowards(
                transform.position, target, speed * Time.deltaTime);

            if (transform.position != target) return;

            waypoints.Dequeue();
            if (waypoints.Count == 0) Arrive();
        }

        void Arrive()
        {
            var done = onArrived;
            onArrived = null;
            done?.Invoke();
        }

        void Face(Vector3 direction)
        {
            if (body == null || direction.sqrMagnitude < 0.0001f) return;

            bool up = direction.y > 0f;
            Sprite sprite = up ? backSprite : frontSprite;
            if (sprite != null) body.sprite = sprite;
            body.flipX = direction.x > 0f;
        }

        Vector3 Flat(Vector3 p) => new(p.x, p.y, transform.position.z);
    }
}
