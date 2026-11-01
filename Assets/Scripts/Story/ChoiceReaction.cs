using UnityEngine;
using UnityEngine.Events;

namespace Story
{
    // Does something in the scene when the player has made a certain choice,
    // e.g. key "insurance", value "skipped" -> show the broken laptop.
    // Fires the moment the option is picked, and when the scene starts if it was picked earlier.
    public class ChoiceReaction : MonoBehaviour
    {
        [SerializeField] private string choiceKey;
        [SerializeField] private string choiceValue;

        [Tooltip("Also check when the scene starts, for choices made in an earlier scene.")]
        [SerializeField] private bool checkOnStart = true;

        [SerializeField] private UnityEvent onMatch;

        private void OnEnable()
        {
            StoryChoices.Changed += OnChoiceChanged;
        }

        private void OnDisable()
        {
            StoryChoices.Changed -= OnChoiceChanged;
        }

        private void Start()
        {
            if (checkOnStart && StoryChoices.Is(choiceKey, choiceValue))
            {
                onMatch.Invoke();
            }
        }

        private void OnChoiceChanged(string key, string value)
        {
            if (key == choiceKey && value == choiceValue)
            {
                onMatch.Invoke();
            }
        }
    }
}
