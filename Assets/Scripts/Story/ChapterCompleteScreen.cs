using System.Collections.Generic;
using MainMenu;
using Player;
using TMPro;
using UnityEngine;
using Transitions;

namespace Story
{
    // The screen shown when a chapter ends: the chapter name, a card per choice the player made,
    // what they learned, and buttons for the next chapter or back to level select.
    // A chapter flow calls ChapterCompleteScreen.Finish() at its end.
    public class ChapterCompleteScreen : MonoBehaviour
    {
        [Header("Text")]
        [Tooltip("{0} = the chapter's number in its level")]
        [SerializeField] private string subtitleFormat = "Chapter {0} complete";
        [Tooltip("Used when the chapter number isn't known, e.g. when testing the scene directly.")]
        [SerializeField] private string subtitleWithoutNumber = "Chapter complete";

        [Header("References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private RectTransform choiceList;
        [SerializeField] private GameObject choiceCardTemplate;
        [SerializeField] private GameObject learnedSection;
        [SerializeField] private RectTransform learnList;
        [SerializeField] private TMP_Text learnItemTemplate;
        [SerializeField] private UnityEngine.UI.Button backButton;
        [SerializeField] private UnityEngine.UI.Button nextButton;

        [Header("Scenes")]
        [SerializeField] private string mainMenuScene = "MainMenu";

        [Header("Testing")]
        [Tooltip("Shown when you press Play in this scene directly instead of coming from level select.")]
        [SerializeField] private ChapterData previewChapter;

        private static ChapterCompleteScreen _instance;

        private readonly List<GameObject> _spawned = new();
        private ChapterData _nextChapter;

        // Marks the current chapter as finished and fades into this screen.
        public static void Finish()
        {
            ChapterProgress.MarkCurrentCompleted();

            var screen = _instance != null
                ? _instance
                : FindAnyObjectByType<ChapterCompleteScreen>(FindObjectsInactive.Include);

            if (screen == null)
            {
                Debug.LogWarning("No Chapter Complete screen in this scene. " +
                                 "Run Tools > Youngnest > Add Chapter Complete Screen To Scene.");
                return;
            }

            SceneFader.FadeOutIn(screen.Show);
        }

        private void Awake()
        {
            _instance = this;

            panel.SetActive(false);
            choiceCardTemplate.SetActive(false);
            learnItemTemplate.gameObject.SetActive(false);

            backButton.onClick.AddListener(OnBack);
            nextButton.onClick.AddListener(OnNext);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        public void Show()
        {
            var chapter = ChapterProgress.CurrentChapter != null ? ChapterProgress.CurrentChapter : previewChapter;
            var levels = ChapterProgress.CurrentLevels;

            ClearSpawned();

            titleText.text = chapter != null ? chapter.DisplayName : "";
            var number = ChapterProgress.GetChapterNumber(chapter, levels);
            subtitleText.text = number > 0 ? string.Format(subtitleFormat, number) : subtitleWithoutNumber;

            if (chapter != null)
            {
                foreach (var item in chapter.Summary)
                {
                    var (sentence, otherOption) = PickSentence(item);
                    if (string.IsNullOrWhiteSpace(sentence)) continue;

                    var card = Instantiate(choiceCardTemplate, choiceList);
                    SetCardText(card, sentence, otherOption);
                    card.SetActive(true);
                    _spawned.Add(card);
                }

                foreach (var goal in chapter.LearningGoals)
                {
                    var learnItem = Instantiate(learnItemTemplate, learnList);
                    learnItem.text = $"• {goal}";
                    learnItem.gameObject.SetActive(true);
                    _spawned.Add(learnItem.gameObject);
                }
            }

            learnedSection.SetActive(chapter != null && chapter.LearningGoals.Count > 0);

            // Next chapter only when there is one with a scene to load.
            _nextChapter = ChapterProgress.FindNextChapter(chapter, levels);
            nextButton.gameObject.SetActive(_nextChapter != null && !string.IsNullOrEmpty(_nextChapter.SceneName));

            panel.SetActive(true);
        }

        // The card's main line and its small "other option" line.
        // The template has a "Text" child for the sentence and a "SubText" child for the other option.
        private static void SetCardText(GameObject card, string sentence, string otherOption)
        {
            var main = card.transform.Find("Text")?.GetComponent<TMP_Text>();
            var sub = card.transform.Find("SubText")?.GetComponent<TMP_Text>();
            if (main == null) main = card.GetComponentInChildren<TMP_Text>(true);

            main.text = PlayerProfile.FillName(sentence);

            if (sub != null)
            {
                var hasOther = !string.IsNullOrWhiteSpace(otherOption);
                sub.gameObject.SetActive(hasOther);
                sub.text = hasOther ? PlayerProfile.FillName(otherOption) : "";
            }
        }

        // The sentence (and other-option line) for the answer the player gave, or the "not chosen" ones.
        private static (string sentence, string otherOption) PickSentence(ChapterSummaryItem item)
        {
            var answer = StoryChoices.Get(item.choiceKey);
            if (string.IsNullOrEmpty(answer)) return (item.sentenceIfNotChosen, item.otherOptionIfNotChosen);

            foreach (var option in item.sentences)
            {
                if (option.value == answer) return (option.sentence, option.otherOption);
            }

            return ("", "");
        }

        private void ClearSpawned()
        {
            foreach (var go in _spawned) Destroy(go);
            _spawned.Clear();
        }

        private void OnBack()
        {
            MainMenuController.OpenLevelSelectOnLoad = true;
            SceneFader.LoadScene(mainMenuScene);
        }

        private void OnNext()
        {
            if (_nextChapter == null) return;

            ChapterProgress.StartChapter(_nextChapter, ChapterProgress.CurrentLevels);
            SceneFader.LoadScene(_nextChapter.SceneName);
        }
    }
}
