using System.Collections;
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

        [Header("After")]
        [Tooltip("Runs when the dialogue ends, e.g. send a phone message or play the next dialogue.")]
        [SerializeField] private UnityEvent onFinished;

        private bool _hasPlayed;

        private void Start()
        {
            if (playOnStart) Play();
        }

        public void Play()
        {
            if (playOnlyOnce && _hasPlayed) return;
            _hasPlayed = true;

            StartCoroutine(PlayAfterDelay());
        }

        private IEnumerator PlayAfterDelay()
        {
            if (delay > 0) yield return new WaitForSeconds(delay);

            var manager = DialogueManager.Instance;
            if (manager == null)
            {
                Debug.LogWarning("No DialogueManager in the scene. Drag the DialogueBox prefab into the scene.", this);
                yield break;
            }

            manager.Play(dialogue, onFinished.Invoke);
        }
    }
}
