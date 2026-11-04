using System;
using System.Collections.Generic;
using UnityEngine;

namespace Room.Movement
{
    public class PlayerMover : MonoBehaviour
    {
        [SerializeField] float speed = 3f;
        [SerializeField] FloorGrid grid;

        readonly Queue<Vector3> waypoints = new();
        Action onArrived;

        void Awake()
        {
            if (grid == null) grid = FindFirstObjectByType<FloorGrid>();
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

        Vector3 Flat(Vector3 p) => new(p.x, p.y, transform.position.z);
    }
}
