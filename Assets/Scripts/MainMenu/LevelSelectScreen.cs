using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainMenu
{
    // The level select panel: levels on the left, then that level's chapters,
    // and a preview of the selected chapter on the right.
    public class LevelSelectScreen : MonoBehaviour
    {
        [Header("Levels")]
        [SerializeField] private List<LevelData> levels = new();

        [Header("Row Text")]
        [Tooltip("{0} = level number, {1} = level name")]
        [SerializeField] private string levelLabelFormat = "Level {0}: {1}";
        [SerializeField] private string lockedLevelName = "???";
        [SerializeField] private string lockedText = "Locked";
        [SerializeField] private string doneText = "Done";
        [Tooltip("Shown on the level and chapter the player is up to.")]
        [SerializeField] private string inProgressText = "In progress";
        [SerializeField] private Color rowColor = new(0.91f, 0.91f, 0.91f);
        [SerializeField] private Color selectedRowColor = new(0.81f, 0.88f, 0.96f);
        [SerializeField] private Color inProgressRowColor = new(0.98f, 0.93f, 0.85f);

        [Header("Level Panel")]
        [SerializeField] private GameObject levelPanel;
        [SerializeField] private TMP_Text breadcrumbText;
        [SerializeField] private TMP_Text listHeaderText;
        [SerializeField] private RectTransform listContent;
        [SerializeField] private UnityEngine.UI.Button listRowTemplate;
        [SerializeField] private UnityEngine.UI.Button backToMenuButton;
        [SerializeField] private UnityEngine.UI.Button backToLevelsButton;

        [Header("Preview")]
        [SerializeField] private GameObject emptyLevelsMessage;
        [SerializeField] private GameObject emptyChaptersMessage;
        [SerializeField] private GameObject previewContent;
        [SerializeField] private UnityEngine.UI.Image previewScreenshot;
        [SerializeField] private GameObject screenshotPlaceholder;
        [SerializeField] private TMP_Text previewNameText;
        [SerializeField] private TMP_Text previewDescriptionText;
        [SerializeField] private RectTransform learnListContent;
        [SerializeField] private TMP_Text learnItemTemplate;
        [SerializeField] private UnityEngine.UI.Button playButton;
        [SerializeField] private UnityEngine.UI.Button replayButton;

        [Header("Debug")]
        [SerializeField] private bool unlockAllLevels;

        // Texts set in the editor, restored when going back to the level list.
        private string _defaultBreadcrumbText;
        private string _defaultListHeaderText;

        private readonly List<GameObject> _spawnedRows = new();
        private readonly List<GameObject> _spawnedLearnItems = new();

        private LevelData _selectedLevel;
        private ChapterData _selectedChapter;

        // Where the player is up to: the first chapter that isn't finished yet (none before they've played).
        private LevelData _progressLevel;
        private ChapterData _progressChapter;

        // Raised when the player presses "Back to menu" on the level list.
        public event Action BackToMenuRequested;

        private void Awake()
        {
            _defaultBreadcrumbText = breadcrumbText.text;
            _defaultListHeaderText = listHeaderText.text;

            listRowTemplate.gameObject.SetActive(false);
            learnItemTemplate.gameObject.SetActive(false);

            backToMenuButton.onClick.AddListener(() => BackToMenuRequested?.Invoke());
            backToLevelsButton.onClick.AddListener(ShowLevelList);
            playButton.onClick.AddListener(OnPlay);
            replayButton.onClick.AddListener(OnPlay);
        }

        // openAtProgress: jump straight to the chapter the player is up to (used by Continue).
        public void Show(bool openAtProgress = false)
        {
            levelPanel.SetActive(true);
            ShowLevelList();

            if (openAtProgress && _progressChapter != null && IsUnlocked(_progressChapter))
            {
                ShowChapterList(_progressLevel);
                SelectChapter(_progressChapter);
            }
        }

        public void Hide()
        {
            levelPanel.SetActive(false);
        }

        private void ShowLevelList()
        {
            _selectedLevel = null;
            _selectedChapter = null;
            ChapterProgress.TryFindInProgress(levels, out _progressLevel, out _progressChapter);

            breadcrumbText.text = _defaultBreadcrumbText;
            listHeaderText.text = _defaultListHeaderText;
            backToMenuButton.gameObject.SetActive(true);
            backToLevelsButton.gameObject.SetActive(false);

            ClearRows();
            for (var i = 0; i < levels.Count; i++)
            {
                var level = levels[i];
                if (level == null) continue;

                var unlocked = IsUnlocked(level);
                var label = string.Format(levelLabelFormat, i + 1, unlocked ? level.DisplayName : lockedLevelName);
                var count = $"{ChapterProgress.CountCompleted(level)}/{level.Chapters.Count}";
                var inProgress = unlocked && level == _progressLevel;
                var status = !unlocked ? lockedText : inProgress ? inProgressText : count;

                AddRow(label, status, unlocked, false, inProgress, () => ShowChapterList(level));
            }

            ShowEmptyPreview(emptyLevelsMessage);
        }

        private void ShowChapterList(LevelData level)
        {
            _selectedLevel = level;

            var levelNumber = levels.IndexOf(level) + 1;
            breadcrumbText.text = string.Format(levelLabelFormat, levelNumber, level.DisplayName);
            listHeaderText.text = level.DisplayName;
            backToMenuButton.gameObject.SetActive(false);
            backToLevelsButton.gameObject.SetActive(true);

            ClearRows();
            foreach (var chapter in level.Chapters)
            {
                if (chapter == null) continue;

                var unlocked = IsUnlocked(chapter);
                var inProgress = unlocked && chapter == _progressChapter;
                var status = !unlocked ? lockedText
                    : ChapterProgress.IsCompleted(chapter) ? doneText
                    : inProgress ? inProgressText
                    : "";
                var selected = chapter == _selectedChapter;

                AddRow(chapter.DisplayName, status, unlocked, selected, inProgress, () => SelectChapter(chapter));
            }

            if (_selectedChapter == null)
            {
                ShowEmptyPreview(emptyChaptersMessage);
            }
        }

        private void SelectChapter(ChapterData chapter)
        {
            _selectedChapter = chapter;

            // Rebuild the list so the selected row is highlighted.
            ShowChapterList(_selectedLevel);
            ShowPreview(chapter);
        }

        private void ShowEmptyPreview(GameObject message)
        {
            emptyLevelsMessage.SetActive(message == emptyLevelsMessage);
            emptyChaptersMessage.SetActive(message == emptyChaptersMessage);
            previewContent.SetActive(false);
        }

        private void ShowPreview(ChapterData chapter)
        {
            emptyLevelsMessage.SetActive(false);
            emptyChaptersMessage.SetActive(false);
            previewContent.SetActive(true);

            var hasScreenshot = chapter.Screenshot != null;
            previewScreenshot.sprite = chapter.Screenshot;
            previewScreenshot.color = hasScreenshot ? Color.white : new Color(0.89f, 0.89f, 0.89f);
            screenshotPlaceholder.SetActive(!hasScreenshot);

            previewNameText.text = chapter.DisplayName;
            previewDescriptionText.text = chapter.Description;

            foreach (var item in _spawnedLearnItems) Destroy(item);
            _spawnedLearnItems.Clear();

            foreach (var goal in chapter.LearningGoals)
            {
                var item = Instantiate(learnItemTemplate, learnListContent);
                item.text = $"• {goal}";
                item.gameObject.SetActive(true);
                _spawnedLearnItems.Add(item.gameObject);
            }

            var completed = ChapterProgress.IsCompleted(chapter);
            playButton.gameObject.SetActive(!completed);
            replayButton.gameObject.SetActive(completed);
        }

        private void AddRow(string label, string status, bool interactable, bool selected, bool inProgress,
            UnityEngine.Events.UnityAction onClick)
        {
            var row = Instantiate(listRowTemplate, listContent);
            row.gameObject.SetActive(true);
            row.interactable = interactable;
            row.onClick.AddListener(onClick);

            if (row.targetGraphic != null)
            {
                row.targetGraphic.color = selected ? selectedRowColor : inProgress ? inProgressRowColor : rowColor;
            }

            // The template has two texts: the first is the name, the second the status.
            var texts = row.GetComponentsInChildren<TMP_Text>(true);
            if (texts.Length > 0) texts[0].text = label;
            if (texts.Length > 1) texts[1].text = status;

            _spawnedRows.Add(row.gameObject);
        }

        private void ClearRows()
        {
            foreach (var row in _spawnedRows) Destroy(row);
            _spawnedRows.Clear();
        }

        private bool IsUnlocked(LevelData level)
        {
            return unlockAllLevels || ChapterProgress.IsLevelUnlocked(level, levels);
        }

        private bool IsUnlocked(ChapterData chapter)
        {
            return unlockAllLevels || ChapterProgress.IsChapterUnlocked(chapter, levels);
        }

        private void OnPlay()
        {
            if (_selectedChapter == null) return;

            if (string.IsNullOrEmpty(_selectedChapter.SceneName))
            {
                Debug.LogWarning($"Chapter '{_selectedChapter.DisplayName}' has no scene set.");
                return;
            }

            ChapterProgress.StartChapter(_selectedChapter);
            SceneManager.LoadScene(_selectedChapter.SceneName);
        }
    }
}
