using System;
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

        [Tooltip("On phones the keyboard covers the bottom half, so while typing the question and name box " +
                 "move to the top, this far from the top edge.")]
        [SerializeField, Min(0)] private int typingTopPadding = 40;

        private UnityEngine.UI.VerticalLayoutGroup _layout;
        private TextAnchor _normalAlignment;
        private RectOffset _normalPadding;

        // Raised once a valid name has been saved.
        public event Action Confirmed;

        // Raised when the player presses Back.
        public event Action BackRequested;

        private void Awake()
        {
            nameInput.characterLimit = PlayerProfile.MaxNameLength;
            nameInput.onValueChanged.AddListener(_ => errorMessage.SetActive(false));
            nameInput.onSubmit.AddListener(_ => Submit());

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

        // While typing on a phone, stack everything at the top so the name stays visible above the keyboard.
        private void SetTypingLayout(bool typing)
        {
            if (_layout == null || (typing && !HasOnScreenKeyboard)) return;

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
