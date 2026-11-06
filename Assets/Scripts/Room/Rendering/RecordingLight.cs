using UnityEngine;

namespace Room.Rendering
{
    public class RecordingLight : MonoBehaviour
    {
        [SerializeField] float worldSize = 0.12f;
        [SerializeField] Vector2 corner = new(0.85f, 0.85f);
        [SerializeField] float blinkInterval = 0.6f;
        [SerializeField] Color color = new(0.9f, 0.1f, 0.1f);

        SpriteRenderer target;
        SpriteRenderer dot;

        void Start()
        {
            target = GetComponent<SpriteRenderer>();
            if (target == null)
            {
                enabled = false;
                return;
            }

            var go = new GameObject("RecordingLight");
            go.transform.SetParent(transform, false);
            dot = go.AddComponent<SpriteRenderer>();
            dot.sprite = CreateCircle();
            dot.sharedMaterial = target.sharedMaterial;
            dot.color = color;
            dot.sortingLayerID = target.sortingLayerID;

            Bounds b = target.bounds;
            go.transform.position = new Vector3(
                Mathf.Lerp(b.min.x, b.max.x, corner.x),
                Mathf.Lerp(b.min.y, b.max.y, corner.y),
                transform.position.z);

            Vector3 parentScale = transform.lossyScale;
            float spriteSize = dot.sprite.bounds.size.x;
            go.transform.localScale = new Vector3(
                worldSize / spriteSize / Mathf.Abs(parentScale.x),
                worldSize / spriteSize / Mathf.Abs(parentScale.y),
                1f);
        }

        void LateUpdate()
        {
            if (dot == null) return;

            dot.sortingOrder = target.sortingOrder + 1;
            dot.enabled = Mathf.Repeat(Time.time, blinkInterval * 2f) < blinkInterval;
        }

        void OnDestroy()
        {
            if (dot == null) return;

            Destroy(dot.sprite.texture);
            Destroy(dot.sprite);
            Destroy(dot.gameObject);
        }

        static Sprite CreateCircle()
        {
            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float radius = size / 2f;

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(radius, radius));
                float alpha = Mathf.Clamp01(radius - distance);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
