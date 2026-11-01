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

        // Raised once a valid name has been saved.
        public event Action Confirmed;

        // Raised when the player presses Back.
        public event Action BackRequested;

        private void Awake()
        {
            nameInput.characterLimit = PlayerProfile.MaxNameLength;
            nameInput.onValueChanged.AddListener(_ => errorMessage.SetActive(false));
            nameInput.onSubmit.AddListener(_ => Submit());

            backButton.onClick.AddListener(() => BackRequested?.Invoke());
            continueButton.onClick.AddListener(Submit);
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
