using UnityEngine;

namespace Room.Movement
{
    public class ClickMarker : MonoBehaviour
    {
        [SerializeField] float lifetime = 0.6f;
        [SerializeField] float startScale = 0.2f;
        [SerializeField] float endScale = 0.5f;

        float timer;

        void Start()
        {
            transform.localScale = Vector3.one * startScale;
        }

        void Update()
        {
            timer += Time.deltaTime;
            float t = timer / lifetime;

            transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, t);

            var sr = GetComponent<SpriteRenderer>();
            Color c = sr.color;
            c.a = 1f - t;
            sr.color = c;

            if (t >= 1f) Destroy(gameObject);
        }
    }
}