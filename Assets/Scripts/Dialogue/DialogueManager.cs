using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Dialogue
{
    // Lives on the DialogueBox prefab. Shows one line at a time with the speaker's name.
    // Click the box (or press Space/Enter) to finish the current line, then again for the next one.
    public class DialogueManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject dialogueRoot;
        [SerializeField] private GameObject nameTag;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private UnityEngine.UI.Button advanceButton;

        [Header("Typing")]
        [Tooltip("Letters per second. 0 shows the whole line at once.")]
        [SerializeField, Min(0)] private float charactersPerSecond = 40f;

        private readonly Queue<(DialogueData dialogue, Action onFinished)> _queue = new();

        private DialogueData _current;
        private Action _onFinished;
        private int _lineIndex;
        private Coroutine _typing;

        public static DialogueManager Instance { get; private set; }

        public bool IsPlaying => _current != null;

        public event Action<DialogueData> DialogueStarted;
        public event Action<DialogueData> DialogueEnded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("There is more than one DialogueManager in the scene.", this);
            }
            Instance = this;

            advanceButton.onClick.AddListener(OnAdvanceClicked);
            dialogueRoot.SetActive(false);
            EnsureEventSystem();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (!IsPlaying) return;

#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame))
            {
                Advance();
            }
#else
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                Advance();
            }
#endif
        }

        // Plays a dialogue. If one is already playing, this one waits until it ends.
        public void Play(DialogueData dialogue, Action onFinished = null)
        {
            if (dialogue == null || dialogue.Lines.Count == 0)
            {
                Debug.LogWarning("Tried to play an empty dialogue.", this);
                onFinished?.Invoke();
                return;
            }

            if (IsPlaying)
            {
                _queue.Enqueue((dialogue, onFinished));
                return;
            }

            _current = dialogue;
            _onFinished = onFinished;
            _lineIndex = 0;

            dialogueRoot.SetActive(true);
            DialogueStarted?.Invoke(dialogue);
            ShowLine(_current.Lines[_lineIndex]);
        }

        private void OnAdvanceClicked()
        {
            // Deselect the box so Space doesn't also "submit" the button and skip a line.
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            Advance();
        }

        // Finishes the line being typed, or moves to the next line.
        public void Advance()
        {
            if (!IsPlaying) return;

            if (_typing != null)
            {
                StopCoroutine(_typing);
                _typing = null;
                bodyText.maxVisibleCharacters = int.MaxValue;
                return;
            }

            _lineIndex++;
            if (_lineIndex < _current.Lines.Count)
            {
                ShowLine(_current.Lines[_lineIndex]);
            }
            else
            {
                EndDialogue();
            }
        }

        private void ShowLine(DialogueLine line)
        {
            var hasSpeaker = !string.IsNullOrWhiteSpace(line.speaker);
            nameTag.SetActive(hasSpeaker);
            nameText.text = line.speaker;

            bodyText.text = line.text;
            if (charactersPerSecond > 0)
            {
                _typing = StartCoroutine(TypeLine());
            }
            else
            {
                bodyText.maxVisibleCharacters = int.MaxValue;
            }
        }

        private IEnumerator TypeLine()
        {
            bodyText.maxVisibleCharacters = 0;
            bodyText.ForceMeshUpdate();
            var total = bodyText.textInfo.characterCount;

            var visible = 0f;
            while (visible < total)
            {
                visible += charactersPerSecond * Time.deltaTime;
                bodyText.maxVisibleCharacters = Mathf.Min(total, (int)visible);
                yield return null;
            }

            bodyText.maxVisibleCharacters = int.MaxValue;
            _typing = null;
        }

        private void EndDialogue()
        {
            var finished = _current;
            var onFinished = _onFinished;

            _current = null;
            _onFinished = null;
            dialogueRoot.SetActive(false);

            DialogueEnded?.Invoke(finished);
            onFinished?.Invoke();

            if (!IsPlaying && _queue.Count > 0)
            {
                var (next, nextFinished) = _queue.Dequeue();
                Play(next, nextFinished);
            }
        }

        // Canvas buttons need an EventSystem; add one if the scene doesn't have it.
        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;

            var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
