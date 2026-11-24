using System;
using System.Collections.Generic;
using Player;
using TMPro;
using UnityEngine;

namespace MainMenu
{
    // Asks for the player's name the first time they press New Game.
    public class NameEntryScreen : MonoBehaviour
    {
        [SerializeField] private GameObject namePanel;
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private GameObject errorMessage;
        [SerializeField] private UnityEngine.UI.Button backButton;
        [SerializeField] private UnityEngine.UI.Button continueButton;

        [Tooltip("On phones the keyboard covers almost the whole screen, so while typing the name box " +
                 "moves to the top, this far from the top edge.")]
        [SerializeField, Min(0)] private int typingTopPadding = 24;

        [Tooltip("Hidden while typing on a phone so the name box fits above the keyboard " +
                 "(e.g. the question, hint and buttons). They come back when the keyboard closes.")]
        [SerializeField] private List<GameObject> hideWhileTyping = new();

        private UnityEngine.UI.VerticalLayoutGroup _layout;
        private TextAnchor _normalAlignment;
        private RectOffset _normalPadding;
        private bool _typingLayout;
        private readonly List<GameObject> _hiddenForTyping = new();

        // Raised once a valid name has been saved.
        public event Action Confirmed;

        // Raised when the player presses Back.
        public event Action BackRequested;

        private void Awake()
        {
            nameInput.characterLimit = PlayerProfile.MaxNameLength;
            nameInput.onValueChanged.AddListener(_ => errorMessage.SetActive(false));
            // Enter on a computer keyboard confirms the name. On a phone, Enter only closes the
            // keyboard and the player confirms with the in-game button.
            nameInput.onSubmit.AddListener(_ =>
            {
                if (!Application.isMobilePlatform) Submit();
            });

            _layout = namePanel.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            if (_layout != null)
            {
                _normalAlignment = _layout.childAlignment;
                _normalPadding = new RectOffset(_layout.padding.left, _layout.padding.right, _layout.padding.top, _layout.padding.bottom);
            }

            nameInput.onSelect.AddListener(_ => SetTypingLayout(true));
            nameInput.onDeselect.AddListener(_ => SetTypingLayout(false));
            nameInput.onEndEdit.AddListener(_ => SetTypingLayout(false));

            backButton.onClick.AddListener(() => BackRequested?.Invoke());
            continueButton.onClick.AddListener(Submit);
        }

        // Phones and tablets (also phone browsers): the on-screen keyboard covers the bottom of the screen.
        private static bool HasOnScreenKeyboard =>
            Application.isMobilePlatform || UnityEngine.InputSystem.Touchscreen.current != null;

        // While typing on a phone, only the name box shows, at the top, so it stays visible above the keyboard.
        private void SetTypingLayout(bool typing)
        {
            if (typing && !HasOnScreenKeyboard) return;
            if (typing == _typingLayout) return;
            _typingLayout = typing;

            if (typing)
            {
                errorMessage.SetActive(false);
                foreach (var item in hideWhileTyping)
                {
                    if (item == null || !item.activeSelf) continue;
                    item.SetActive(false);
                    _hiddenForTyping.Add(item);
                }
            }
            else
            {
                foreach (var item in _hiddenForTyping)
                {
                    if (item != null) item.SetActive(true);
                }
                _hiddenForTyping.Clear();
            }

            if (_layout == null) return;

            _layout.childAlignment = typing ? TextAnchor.UpperCenter : _normalAlignment;
            _layout.padding = typing
                ? new RectOffset(_normalPadding.left, _normalPadding.right, typingTopPadding, _normalPadding.bottom)
                : new RectOffset(_normalPadding.left, _normalPadding.right, _normalPadding.top, _normalPadding.bottom);
        }

        public void Show()
        {
            namePanel.SetActive(true);
            nameInput.text = PlayerProfile.Name;
            errorMessage.SetActive(false);
            nameInput.ActivateInputField();
        }

        public void Hide()
        {
            SetTypingLayout(false);
            namePanel.SetActive(false);
        }

        private void Submit()
        {
            var name = nameInput.text.Trim();
            if (name.Length == 0)
            {
                errorMessage.SetActive(true);
                return;
            }

            PlayerProfile.SetName(name);
            Confirmed?.Invoke();
        }

        // Right-click the component in the Inspector to test the name screen again.
        [ContextMenu("Clear Saved Name")]
        private void ClearSavedName()
        {
            PlayerProfile.ClearName();
            Debug.Log("Saved player name cleared.");
        }
    }
}
