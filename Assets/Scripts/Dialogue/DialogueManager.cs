using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using Story;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Dialogue
{
    // Lives on the DialogueBox prefab. Shows one line at a time with the speaker's name.
    // Click the box (or press Space/Enter) to finish the current line, then again for the next one.
    // If the dialogue has choices, they appear after the last line and the picked one decides what plays next.
    public class DialogueManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject dialogueRoot;
        [SerializeField] private GameObject nameTag;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private UnityEngine.UI.Button advanceButton;

        [Header("Choices")]
        [Tooltip("Run Tools > Youngnest > Create Dialogue Box Prefab > Update to create these.")]
        [SerializeField] private GameObject choicePanel;
        [SerializeField] private TMP_Text choicePromptText;
        [SerializeField] private RectTransform choiceList;
        [SerializeField] private UnityEngine.UI.Button choiceButtonTemplate;
        [SerializeField] private GameObject nextIndicator;
        [Tooltip("The box that grows taller when the options don't fit.")]
        [SerializeField] private RectTransform boxRect;

        [Header("Typing")]
        [Tooltip("Letters per second. 0 shows the whole line at once.")]
        [SerializeField, Min(0)] private float charactersPerSecond = 40f;

        private readonly Queue<(DialogueData dialogue, Action onFinished)> _queue = new();
        private readonly List<GameObject> _spawnedChoices = new();

        private DialogueData _current;
        private Action _onFinished;
        private int _lineIndex;
        private Coroutine _typing;
        private bool _choosing;

        private Vector2 _baseBoxSize;
        private Vector2 _baseNameTagPosition;

        public static DialogueManager Instance { get; private set; }

        public bool IsPlaying => _current != null;

        public event Action<DialogueData> DialogueStarted;
        public event Action<DialogueData> DialogueEnded;
        public event Action<DialogueChoice> ChoiceMade;

        // Raised with a line's Cue name when that line appears. DialogueCue components listen to this.
        public static event Action<string> CueFired;

        private bool HasChoiceUI => choicePanel != null && choiceList != null && choiceButtonTemplate != null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("There is more than one DialogueManager in the scene.", this);
            }
            Instance = this;

            advanceButton.onClick.AddListener(OnAdvanceClicked);

            if (boxRect != null) _baseBoxSize = boxRect.sizeDelta;
            _baseNameTagPosition = ((RectTransform)nameTag.transform).anchoredPosition;

            if (HasChoiceUI)
            {
                choiceButtonTemplate.gameObject.SetActive(false);
                choicePanel.SetActive(false);
            }

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
            if (keyboard == null) return;

            if (_choosing)
            {
                // Number keys 1-9 pick an option.
                for (var i = 0; i < _current.Choices.Count && i < 9; i++)
                {
                    if (keyboard[UnityEngine.InputSystem.Key.Digit1 + i].wasPressedThisFrame)
                    {
                        Choose(_current.Choices[i]);
                        return;
                    }
                }
            }
            else if (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)
            {
                Advance();
            }
#else
            if (_choosing)
            {
                for (var i = 0; i < _current.Choices.Count && i < 9; i++)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                    {
                        Choose(_current.Choices[i]);
                        return;
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                Advance();
            }
#endif
        }

        // Plays a dialogue. If one is already playing, this one waits until it ends.
        public void Play(DialogueData dialogue, Action onFinished = null)
        {
            if (dialogue == null || (dialogue.Lines.Count == 0 && !dialogue.HasChoices))
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

            _onFinished = onFinished;
            dialogueRoot.SetActive(true);
            DialogueStarted?.Invoke(dialogue);
            StartDialogue(dialogue);
        }

        // Starts (or continues into) a dialogue, without touching the finished callback.
        private void StartDialogue(DialogueData dialogue)
        {
            _current = dialogue;
            _lineIndex = 0;

            if (_current.Lines.Count > 0)
            {
                ShowLine(_current.Lines[_lineIndex]);
            }
            else
            {
                ShowChoices();
            }
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
            if (!IsPlaying || _choosing) return;

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
            else if (_current.HasChoices)
            {
                ShowChoices();
            }
            else
            {
                EndDialogue();
            }
        }

        private void ShowLine(DialogueLine line)
        {
            ShowSpeaker(line.speaker);
            FireCue(line.cue);

            bodyText.text = PlayerProfile.FillName(line.text);
            if (charactersPerSecond > 0)
            {
                _typing = StartCoroutine(TypeLine());
            }
            else
            {
                bodyText.maxVisibleCharacters = int.MaxValue;
            }
        }

        private static void FireCue(string cue)
        {
            if (!string.IsNullOrWhiteSpace(cue)) CueFired?.Invoke(cue.Trim());
        }

        private void ShowSpeaker(string speaker)
        {
            nameTag.SetActive(!string.IsNullOrWhiteSpace(speaker));
            nameText.text = PlayerProfile.FillName(speaker);
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

        private void ShowChoices()
        {
            if (!HasChoiceUI)
            {
                Debug.LogWarning("This dialogue has choices but the DialogueBox has no choice list. " +
                                 "Run Tools > Youngnest > Create Dialogue Box Prefab and choose Update.", this);
                EndDialogue();
                return;
            }

            _choosing = true;

            var prompt = _current.ChoicePrompt;
            ShowSpeaker(prompt.speaker);
            FireCue(prompt.cue);
            var hasPrompt = !string.IsNullOrWhiteSpace(prompt.text);
            choicePromptText.gameObject.SetActive(hasPrompt);
            choicePromptText.text = PlayerProfile.FillName(prompt.text);

            bodyText.gameObject.SetActive(false);
            if (nextIndicator != null) nextIndicator.SetActive(false);
            choicePanel.SetActive(true);

            foreach (var choice in _current.Choices)
            {
                var button = Instantiate(choiceButtonTemplate, choiceList);
                button.gameObject.SetActive(true);

                // The template's last text is the option label (the first may be the arrow).
                var texts = button.GetComponentsInChildren<TMP_Text>(true);
                if (texts.Length > 0) texts[^1].text = PlayerProfile.FillName(choice.text);

                var picked = choice;
                button.onClick.AddListener(() => Choose(picked));
                _spawnedChoices.Add(button.gameObject);
            }

            FitBoxToChoices();
        }

        private void Choose(DialogueChoice choice)
        {
            if (!_choosing) return;

            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            HideChoices();

            if (!string.IsNullOrEmpty(choice.rememberKey))
            {
                StoryChoices.Set(choice.rememberKey, choice.rememberValue);
            }
            ChoiceMade?.Invoke(choice);

            var next = choice.nextDialogue;
            if (next != null && (next.Lines.Count > 0 || next.HasChoices))
            {
                StartDialogue(next);
            }
            else
            {
                EndDialogue();
            }
        }

        private void HideChoices()
        {
            _choosing = false;

            foreach (var spawned in _spawnedChoices) Destroy(spawned);
            _spawnedChoices.Clear();

            if (HasChoiceUI) choicePanel.SetActive(false);
            bodyText.gameObject.SetActive(true);
            if (nextIndicator != null) nextIndicator.SetActive(true);

            if (boxRect != null) boxRect.sizeDelta = _baseBoxSize;
            ((RectTransform)nameTag.transform).anchoredPosition = _baseNameTagPosition;
        }

        // Makes the box taller (and lifts the name tag) when the options don't fit.
        private void FitBoxToChoices()
        {
            if (boxRect == null) return;

            var panelRect = (RectTransform)choicePanel.transform;
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);

            var padding = boxRect.rect.height - panelRect.rect.height;
            var needed = UnityEngine.UI.LayoutUtility.GetPreferredHeight(panelRect) + padding;
            var extra = Mathf.Max(0f, needed - _baseBoxSize.y);

            boxRect.sizeDelta = new Vector2(_baseBoxSize.x, _baseBoxSize.y + extra);
            ((RectTransform)nameTag.transform).anchoredPosition = _baseNameTagPosition + Vector2.up * extra;
        }

        private void EndDialogue()
        {
            if (_choosing) HideChoices();

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
