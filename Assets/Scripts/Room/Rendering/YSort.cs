using UnityEngine;

namespace Room.Rendering
{
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class YSort : MonoBehaviour
    {
        [SerializeField] SpriteRenderer sortWith;
        [SerializeField] int offset;
        [SerializeField] float precision = 100f;

        SpriteRenderer sr;

        void OnEnable()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        void LateUpdate()
        {
            int order = sortWith != null
                ? sortWith.sortingOrder + offset
                : offset - Mathf.RoundToInt(sr.bounds.min.y * precision);

            if (sr.sortingOrder != order) sr.sortingOrder = order;
        }
    }
}
