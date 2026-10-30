using UnityEngine;
using UnityEngine.Events;

namespace Room.Interaction
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Interactable : MonoBehaviour
    {
        [SerializeField] InteractableData data;
        [SerializeField] Color highlightColor = new Color(0.5f, 1f, 0.5f);
        [SerializeField] Transform interactPoint;

        [Tooltip("Runs when the player clicks this object, e.g. DialogueTrigger.Play.")]
        [SerializeField] UnityEvent onClicked;

        SpriteRenderer sr;
        Color originalColor;
        bool hasBeenClicked;

        public string PromptText => data != null ? data.promptText : "Click";
        public Vector3 InteractPosition =>
            interactPoint != null ? interactPoint.position : transform.position;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            originalColor = sr.color;
        }

        public void SetHighlight(bool on)
        {
            sr.color = on ? highlightColor : originalColor;
        }

        // Called by RoomClickController when the player clicks this object.
        public void Click()
        {
            bool canRepeat = data == null || data.canRepeat;
            if (hasBeenClicked && !canRepeat) return;

            hasBeenClicked = true;
            onClicked.Invoke();
        }

        void Reset()
        {
            if (GetComponent<Collider2D>() == null)
                gameObject.AddComponent<PolygonCollider2D>();

            int layer = LayerMask.NameToLayer("Interactable");
            if (layer >= 0) gameObject.layer = layer;
        }
    }
}