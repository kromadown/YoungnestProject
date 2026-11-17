using System;
using System.Collections.Generic;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu
{
    // Lets the player pick their character after New Game, before the name entry.
    // Each card shows one character from the Player Characters list; the picked card is blue,
    // hovering or keyboard-selecting a card turns it orange like the other buttons.
    public class CharacterSelectScreen : MonoBehaviour
    {
        [SerializeField] private PlayerCharacters characters;
        [SerializeField] private GameObject panel;
        [Tooltip("One card per character, in the same order as the Player Characters list.")]
        [SerializeField] private List<Button> cards = new();
        [Tooltip("The character picture on each card, same order as the cards.")]
        [SerializeField] private List<Image> cardPictures = new();
        [Tooltip("The slate border on each card (an Outline), shown only on the picked one. Same order as the cards.")]
        [SerializeField] private List<Outline> cardBorders = new();
        [SerializeField] private Button backButton;
        [SerializeField] private Button continueButton;

        [Header("Card Colours")]
        [SerializeField] private Color cardColor = new(0.9608f, 0.9412f, 0.9137f);
        [SerializeField] private Color pickedCardColor = new(0.7137f, 0.8157f, 0.8941f);
        [SerializeField] private Color hoverCardColor = new(1f, 0.5059f, 0.1843f);
        [SerializeField] private Color pressedCardColor = new(0.9098f, 0.4353f, 0.1255f);

        [Tooltip("Continue's colour while no character is picked yet (it can't be pressed then).")]
        [SerializeField] private Color disabledContinueColor = new(0.7137f, 0.8157f, 0.8941f, 0.45f);

        private const int NothingPicked = -1;
        private int _picked = NothingPicked;

        // Raised once a character has been saved.
        public event Action Confirmed;

        // Raised when the player presses Back.
        public event Action BackRequested;

        private void Awake()
        {
            for (var i = 0; i < cards.Count; i++)
            {
                var index = i;
                cards[i].onClick.AddListener(() => Pick(index));
            }

            backButton.onClick.AddListener(() => BackRequested?.Invoke());
            continueButton.onClick.AddListener(Confirm);
        }

        public void Show()
        {
            panel.SetActive(true);

            var all = characters != null ? characters.All : null;
            for (var i = 0; i < cards.Count; i++)
            {
                var character = all != null && i < all.Count ? all[i] : null;
                cards[i].gameObject.SetActive(character != null);
                if (character != null && i < cardPictures.Count && cardPictures[i] != null)
                {
                    cardPictures[i].sprite = character.front;
                }
            }

            // Nothing is picked until the player chooses (unless they already did, e.g. coming Back from the name entry).
            Pick(PlayerProfile.HasCharacter ? Mathf.Clamp(PlayerProfile.CharacterIndex, 0, Mathf.Max(0, cards.Count - 1)) : NothingPicked);
        }

        public void Hide()
        {
            panel.SetActive(false);
        }

        private void Pick(int index)
        {
            _picked = index;

            for (var i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                if (card.targetGraphic != null) card.targetGraphic.color = Color.white;

                var colors = card.colors;
                colors.normalColor = i == _picked ? pickedCardColor : cardColor;
                colors.highlightedColor = hoverCardColor;
                // A clicked card stays "selected" in Unity, so the picked one keeps its blue instead of orange.
                colors.selectedColor = i == _picked ? pickedCardColor : hoverCardColor;
                colors.pressedColor = pressedCardColor;
                colors.colorMultiplier = 1f;
                card.colors = colors;

                if (i < cardBorders.Count && cardBorders[i] != null) cardBorders[i].enabled = i == _picked;
            }

            // Continue only works once a character is picked.
            var continueColors = continueButton.colors;
            continueColors.disabledColor = disabledContinueColor;
            continueButton.colors = continueColors;
            continueButton.interactable = _picked != NothingPicked;

            // Fade the label too, so the whole button reads as "not yet".
            foreach (var label in continueButton.GetComponentsInChildren<TMP_Text>(true))
            {
                var color = label.color;
                color.a = continueButton.interactable ? 1f : disabledContinueColor.a;
                label.color = color;
            }
        }

        private void Confirm()
        {
            if (_picked == NothingPicked) return;

            PlayerProfile.SetCharacter(_picked);
            Confirmed?.Invoke();
        }
    }
}
