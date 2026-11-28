using UnityEngine;

namespace Room.Rendering
{
    public class PortraitCameraFollow : MonoBehaviour
    {
        [SerializeField] Camera cam;
        [SerializeField] Transform target;
        [SerializeField] Renderer area;
        [SerializeField, Min(0f)] float smoothTime = 0.25f;

        Vector3 landscapePosition;
        float velocity;
        bool wasPortrait;

        void Start()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            landscapePosition = cam.transform.position;
        }

        void LateUpdate()
        {
            if (cam == null || target == null || area == null) return;

            bool portrait = Screen.height > Screen.width;
            if (!portrait)
            {
                if (wasPortrait) cam.transform.position = landscapePosition;
                wasPortrait = false;
                return;
            }

            float goal = ClampedX(target.position.x);
            var pos = cam.transform.position;
            pos.x = wasPortrait ? Mathf.SmoothDamp(pos.x, goal, ref velocity, smoothTime) : goal;
            cam.transform.position = pos;
            wasPortrait = true;
        }

        float ClampedX(float x)
        {
            Bounds bounds = area.bounds;
            float halfWidth = cam.orthographicSize * cam.aspect;
            float min = bounds.min.x + halfWidth;
            float max = bounds.max.x - halfWidth;
            return min > max ? bounds.center.x : Mathf.Clamp(x, min, max);
        }
    }
}
