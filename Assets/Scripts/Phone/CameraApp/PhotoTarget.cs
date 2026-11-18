using System;
using Room.Interaction;
using UnityEngine;

namespace Phone.CameraApp
{
    public class PhotoTarget : MonoBehaviour
    { 
        public event Action Photographed;

        [Header("Highlight settings")] 
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Color highlightColor;
        
        private Interactable _interactable;
        private Color _originalColor;


        private void Awake()
        {
            _interactable = GetComponentInParent<Interactable>();
            
            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInParent<SpriteRenderer>();

                if (targetRenderer == null)
                {
                    targetRenderer = GetComponentInChildren<SpriteRenderer>();
                }
            }

            if (targetRenderer != null)
            {
                _originalColor = targetRenderer.color;
            }

            if (_interactable == null && targetRenderer == null)
            {
                Debug.LogError("PhotoTarget has no available highlighting component.", this);
            }
        }

        public void SetHighlighted(bool highlighted)
        {
            if (_interactable != null)
            {
                _interactable.SetHighlight(highlighted);
                return;
            }

            if (targetRenderer != null)
            {
                targetRenderer.color = highlighted ? highlightColor : _originalColor;
            }
        }

        private void OnDisable()
        {
            SetHighlighted(false);
        }

        public void OnPhotographed()
        {
            Debug.Log($"Took a picture of {name}");
            Photographed?.Invoke();
        }
    }
}