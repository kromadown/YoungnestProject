using UnityEngine;

namespace Room.Movement
{
    public class PlayerMover : MonoBehaviour
    {
        [SerializeField] float speed = 3f;

        Vector3 target;
        bool isMoving;

        void Awake()
        {
            target = transform.position;
        }

        public void MoveTo(Vector3 point)
        {
            target = new Vector3(point.x, point.y, transform.position.z);
            isMoving = true;
        }

        void Update()
        {
            if (!isMoving) return;

            transform.position = Vector3.MoveTowards(
                transform.position, target, speed * Time.deltaTime);

            if (transform.position == target) isMoving = false;
        }
    }
}