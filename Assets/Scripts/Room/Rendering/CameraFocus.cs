using System.Collections;
using UnityEngine;

namespace Room.Rendering
{
    [RequireComponent(typeof(Camera))]
    public class CameraFocus : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] float duration = 0.6f;

        Camera cam;
        Coroutine moving;
        Vector3 returnPosition;
        float returnSize;

        public bool IsFocusing { get; private set; }

        void Awake()
        {
            cam = GetComponent<Camera>();
        }

        public void FocusOn(Vector3 point, float size)
        {
            if (!IsFocusing)
            {
                returnPosition = transform.position;
                returnSize = cam.orthographicSize;
            }

            IsFocusing = true;
            MoveTo(new Vector3(point.x, point.y, returnPosition.z), size, false);
        }

        public void Release()
        {
            if (!IsFocusing) return;

            MoveTo(returnPosition, returnSize, true);
        }

        void MoveTo(Vector3 position, float size, bool releaseAtEnd)
        {
            if (moving != null) StopCoroutine(moving);
            moving = StartCoroutine(Move(position, size, releaseAtEnd));
        }

        IEnumerator Move(Vector3 position, float size, bool releaseAtEnd)
        {
            Vector3 startPosition = transform.position;
            float startSize = cam.orthographicSize;

            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / duration);
                transform.position = Vector3.Lerp(startPosition, position, k);
                cam.orthographicSize = Mathf.Lerp(startSize, size, k);
                yield return null;
            }

            transform.position = position;
            cam.orthographicSize = size;
            moving = null;
            if (releaseAtEnd) IsFocusing = false;
        }
    }
}
