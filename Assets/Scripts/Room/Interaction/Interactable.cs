using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Dialogue;

namespace Room.Interaction
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Interactable : MonoBehaviour
    {
        [SerializeField] InteractableData data;
        [SerializeField] Color highlightColor = new Color(0.5f, 1f, 0.5f);
        [SerializeField] Transform interactPoint;
        [SerializeField, TextArea(2, 5)] string clickText;

        [Tooltip("Runs when the player clicks this object, e.g. DialogueTrigger.Play.")]
        [SerializeField] UnityEvent onClicked;

        SpriteRenderer sr;
        Color originalColor;
        DialogueData dialogue;
        bool hasBeenClicked;

        public string PromptText => data != null ? data.promptText : "Click";
        public bool HasInteractPoint => interactPoint != null;
        public Vector3 InteractPosition =>
            interactPoint != null ? interactPoint.position : transform.position;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            originalColor = sr.color;
        }

        void OnDestroy()
        {
            if (dialogue != null) Destroy(dialogue);
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
            ShowClickText();
            onClicked.Invoke();
        }

        void ShowClickText()
        {
            if (string.IsNullOrWhiteSpace(clickText)) return;

            var manager = DialogueManager.Instance;
            if (manager == null)
            {
                Debug.LogWarning("No DialogueManager in the scene. Drag the DialogueBox prefab into the scene.", this);
                return;
            }

            if (dialogue == null) dialogue = CreateDialogue(clickText);
            manager.Play(dialogue);
        }

        static DialogueData CreateDialogue(string text)
        {
            var created = ScriptableObject.CreateInstance<DialogueData>();
            var lines = new DialogueLines();
            lines.lines.Add(new DialogueLine { text = text });
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(lines), created);
            return created;
        }

        [Serializable]
        class DialogueLines
        {
            public List<DialogueLine> lines = new();
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
