using System;
using System.Collections.Generic;
using Player;
using TMPro;
using UnityEngine;
using Transitions;

namespace MainMenu
{
    // The level select panel: one list on the left where each level opens like a dropdown to show
    // its chapters underneath, and a preview of the selected chapter on the right.
    // It opens with the player's current level open and their next chapter already selected.
    public class LevelSelectScreen : MonoBehaviour
    {
        [Header("Levels")]
        [SerializeField] private List<LevelData> levels = new();

        [Header("Row Text")]
        [Tooltip("{0} = level number, {1} = level name")]
        [SerializeField] private string levelLabelFormat = "Level {0}: {1}";
        [Tooltip("{0} = chapter number in its level, {1} = chapter name")]
        [SerializeField] private string chapterLabelFormat = "{0}. {1}";
        [SerializeField] private string lockedLevelName = "???";
        [SerializeField] private string lockedText = "Locked";
        [SerializeField] private string doneText = "Done";
        [Tooltip("Shown on the level and chapter the player is up to.")]
        [SerializeField] private string inProgressText = "In progress";
        [Tooltip("Shown after a level's status while its chapters are showing.")]
        [SerializeField] private string openArrow = "\u25BC";
        [Tooltip("Shown after a level's status while its chapters are hidden.")]
        [SerializeField] private string closedArrow = "\u25BA";
        [Tooltip("How far chapter rows sit in from the level rows.")]
        [SerializeField, Min(0)] private int chapterIndent = 48;
        [SerializeField] private Color rowColor = new(0.9608f, 0.9412f, 0.9137f);
        [SerializeField] private Color selectedRowColor = new(0.7137f, 0.8157f, 0.8941f);
        [SerializeField] private Color inProgressRowColor = new(0.9608f, 0.9412f, 0.9137f);
        [Tooltip("Row under the mouse, or picked with the keyboard.")]
        [SerializeField] private Color hoverRowColor = new(1f, 0.5059f, 0.1843f);
        [SerializeField] private Color pressedRowColor = new(0.9098f, 0.4353f, 0.1255f);
        [SerializeField] private Color lockedRowColor = new(0.91f, 0.89f, 0.87f);
        [Tooltip("Fills the preview box when a chapter has no screenshot yet.")]
        [SerializeField] private Color screenshotPlaceholderColor = new(0.8471f, 0.898f, 0.9373f);

        [Header("Level Panel")]
        [SerializeField] private GameObject levelPanel;
        [SerializeField] private TMP_Text breadcrumbText;
        [SerializeField] private RectTransform listContent;
        [SerializeField] private UnityEngine.UI.Button listRowTemplate;
        [SerializeField] private UnityEngine.UI.Button backToMenuButton;
        [Tooltip("Not used any more (levels open in place). Kept hidden so the scene doesn't break.")]
        [SerializeField] private UnityEngine.UI.Button backToLevelsButton;

        [Header("Preview")]
        [SerializeField] private GameObject emptyLevelsMessage;
        [SerializeField] private GameObject emptyChaptersMessage;
        [SerializeField] private GameObject previewContent;
        [SerializeField] private UnityEngine.UI.Image previewScreenshot;
        [SerializeField] private GameObject screenshotPlaceholder;
        [Tooltip("Optional: shows the chapter name above the description.")]
        [SerializeField] private TMP_Text previewNameText;
        [SerializeField] private TMP_Text previewDescriptionText;
        [SerializeField] private RectTransform learnListContent;
        [SerializeField] private TMP_Text learnItemTemplate;
        [SerializeField] private UnityEngine.UI.Button playButton;
        [SerializeField] private UnityEngine.UI.Button replayButton;

        [Header("Debug")]
        [SerializeField] private bool unlockAllLevels;

        private readonly List<GameObject> _spawnedRows = new();
        private readonly List<GameObject> _spawnedLearnItems = new();

        // Levels whose chapters are showing.
        private readonly HashSet<LevelData> _openLevels = new();
        private ChapterData _selectedChapter;

        // Where the player is up to: the first chapter that isn't finished yet (none before they've played).
        private LevelData _progressLevel;
        private ChapterData _progressChapter;

        // Raised when the player presses "Back to menu" on the level list.
        public event Action BackToMenuRequested;

        private void Awake()
        {
            listRowTemplate.gameObject.SetActive(false);
            learnItemTemplate.gameObject.SetActive(false);

            backToMenuButton.onClick.AddListener(() => BackToMenuRequested?.Invoke());
            if (backToLevelsButton != null) backToLevelsButton.gameObject.SetActive(false);
            playButton.onClick.AddListener(OnPlay);
            replayButton.onClick.AddListener(OnPlay);
        }

        // On a new game every level starts closed. After that it opens at where the player is: their
        // current level open, and the chapter they started but didn't finish (or else the next one to
        // play) selected. openAtProgress is kept for callers.
        public void Show(bool openAtProgress = false)
        {
            levelPanel.SetActive(true);
            backToMenuButton.gameObject.SetActive(true);
            if (backToLevelsButton != null) backToLevelsButton.gameObject.SetActive(false);

            ChapterProgress.TryFindInProgress(levels, out _progressLevel, out _progressChapter);

            var level = _progressLevel;
            var chapter = _progressChapter;
            if (chapter == null) ChapterProgress.TryFindNextToPlay(levels, out level, out chapter);

            _openLevels.Clear();
            _selectedChapter = null;

            if (!ChapterProgress.HasStarted)
            {
                // New game: everything starts closed, so the player opens Level 1 themselves.
            }
            else if (chapter != null && IsUnlocked(chapter))
            {
                _openLevels.Add(level);
                _selectedChapter = chapter;
            }
            else
            {
                // Nothing to continue (e.g. everything is finished): open the first level.
                var first = levels.Find(l => l != null && IsUnlocked(l));
                if (first != null) _openLevels.Add(first);
            }

            RebuildList();

            if (_selectedChapter != null) ShowPreview(_selectedChapter);
            else ShowEmptyPreview(levels.Count == 0 ? emptyLevelsMessage : emptyChaptersMessage);
        }

        public void Hide()
        {
            levelPanel.SetActive(false);
        }

        // Tapping a level opens or closes its chapters.
        private void ToggleLevel(LevelData level)
        {
            if (!_openLevels.Remove(level)) _openLevels.Add(level);
            RebuildList();
        }

        private void SelectChapter(ChapterData chapter)
        {
            _selectedChapter = chapter;

            // Rebuild the list so the selected row is highlighted.
            RebuildList();
            ShowPreview(chapter);
        }

        private void RebuildList()
        {
            ClearRows();

            for (var i = 0; i < levels.Count; i++)
            {
                var level = levels[i];
                if (level == null) continue;

                var unlocked = IsUnlocked(level);
                var open = unlocked && _openLevels.Contains(level);
                var label = string.Format(levelLabelFormat, i + 1, unlocked ? level.DisplayName : lockedLevelName);
                var count = $"{ChapterProgress.CountCompleted(level)}/{level.Chapters.Count}";
                var inProgress = unlocked && level == _progressLevel;
                var status = !unlocked
                    ? lockedText
                    : $"{(inProgress ? inProgressText : count)}  {(open ? openArrow : closedArrow)}";

                AddRow(listContent, label, status, unlocked, false, inProgress, () => ToggleLevel(level));

                if (open) AddChapterRows(level);
            }
        }

        // The level's chapters, in a group that sits a little to the right of the level rows.
        private void AddChapterRows(LevelData level)
        {
            var group = new GameObject("Chapters", typeof(RectTransform), typeof(UnityEngine.UI.VerticalLayoutGroup));
            group.transform.SetParent(listContent, false);
            _spawnedRows.Add(group);

            var layout = group.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            var parentLayout = listContent.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.padding = new RectOffset(chapterIndent, 0, 0, 0);
            layout.spacing = parentLayout != null ? parentLayout.spacing : 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

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

                AddRow(group.transform, ChapterLabel(chapter), status, unlocked, selected, inProgress,
                    () => SelectChapter(chapter));
            }
        }

        private void ShowEmptyPreview(GameObject message)
        {
            emptyLevelsMessage.SetActive(message == emptyLevelsMessage);
            emptyChaptersMessage.SetActive(message == emptyChaptersMessage);
            previewContent.SetActive(false);
        }

        // e.g. "1. First night": the number comes from the chapter's position in its level.
        private string ChapterLabel(ChapterData chapter)
        {
            return string.Format(chapterLabelFormat, ChapterProgress.GetChapterNumber(chapter, levels), chapter.DisplayName);
        }

        private void ShowPreview(ChapterData chapter)
        {
            emptyLevelsMessage.SetActive(false);
            emptyChaptersMessage.SetActive(false);
            previewContent.SetActive(true);

            var hasScreenshot = chapter.Screenshot != null;
            previewScreenshot.sprite = chapter.Screenshot;
            previewScreenshot.color = hasScreenshot ? Color.white : screenshotPlaceholderColor;
            screenshotPlaceholder.SetActive(!hasScreenshot);

            if (previewNameText != null) previewNameText.text = ChapterLabel(chapter);
            previewDescriptionText.text = PlayerProfile.FillName(chapter.Description);

            foreach (var item in _spawnedLearnItems) Destroy(item);
            _spawnedLearnItems.Clear();

            foreach (var goal in chapter.LearningGoals)
            {
                var item = Instantiate(learnItemTemplate, learnListContent);
                item.text = $"• {PlayerProfile.FillName(goal)}";
                item.gameObject.SetActive(true);
                _spawnedLearnItems.Add(item.gameObject);
            }

            var completed = ChapterProgress.IsCompleted(chapter);
            playButton.gameObject.SetActive(!completed);
            replayButton.gameObject.SetActive(completed);
        }

        private void AddRow(Transform parent, string label, string status, bool interactable, bool selected,
            bool inProgress, UnityEngine.Events.UnityAction onClick)
        {
            var row = Instantiate(listRowTemplate, parent);
            row.gameObject.SetActive(true);
            row.interactable = interactable;
            row.onClick.AddListener(onClick);

            // The row's own colour comes from the button states, so hovering, keyboard selection
            // and pressing can turn it orange like the other buttons.
            if (row.targetGraphic != null) row.targetGraphic.color = Color.white;
            var colors = row.colors;
            colors.normalColor = selected ? selectedRowColor : inProgress ? inProgressRowColor : rowColor;
            colors.highlightedColor = hoverRowColor;
            colors.selectedColor = hoverRowColor;
            colors.pressedColor = pressedRowColor;
            colors.disabledColor = lockedRowColor;
            colors.colorMultiplier = 1f;
            row.colors = colors;

            // The template has two texts: the first is the name, the second the status.
            var texts = row.GetComponentsInChildren<TMP_Text>(true);
            if (texts.Length > 0) texts[0].text = label;
            if (texts.Length > 1) texts[1].text = status;

            // Chapter rows live inside their level's group, which is cleared as a whole.
            if (parent == listContent) _spawnedRows.Add(row.gameObject);
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

            ChapterProgress.StartChapter(_selectedChapter, levels);
            SceneFader.LoadScene(_selectedChapter.SceneName);
        }
    }
}
