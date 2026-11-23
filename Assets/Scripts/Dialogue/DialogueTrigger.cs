using System.Collections;
using Phone.Messages;
using Story;
using UnityEngine;
using UnityEngine.Events;

namespace Dialogue
{
    // Put this on any GameObject to play a dialogue without writing code.
    // Either tick Play On Start, or call Play() from a button or another UnityEvent.
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] private DialogueData dialogue;

        [Header("When")]
        [SerializeField] private bool playOnStart;
        [SerializeField, Min(0)] private float delay;
        [SerializeField] private bool playOnlyOnce = true;

        [Header("Only If (optional)")]
        [Tooltip("Only plays if this remembered choice matches, e.g. key 'insurance', value 'skipped'. Leave empty to always play.")]
        [SerializeField] private string onlyIfChoiceKey;
        [SerializeField] private string onlyIfChoiceValue;

        [Header("After")]
        [Tooltip("Runs when the dialogue ends, e.g. send a phone message or play the next dialogue.")]
        [SerializeField] private UnityEvent onFinished;

        [Header("Phone Dialogue (Optional)")] 
        [SerializeField] private PhoneContact phoneContact;

        private bool _hasPlayed;

        private void Start()
        {
            if (playOnStart) Play();
        }

        public void Play()
        {
            if (playOnlyOnce && _hasPlayed) return;
            if (!string.IsNullOrEmpty(onlyIfChoiceKey) && !StoryChoices.Is(onlyIfChoiceKey, onlyIfChoiceValue)) return;
            _hasPlayed = true;

            StartCoroutine(PlayAfterDelay());
        }

        private IEnumerator PlayAfterDelay()
        {
            if (delay > 0) yield return new WaitForSeconds(delay);

            var router = DialogueRouter.Instance;
            if (router == null)
            {
                Debug.LogWarning("No DialogueRouter found in the scene.", this);
                yield break;
            }

            router.Play(dialogue, onFinished.Invoke, phoneContact);
        }
    }
}
