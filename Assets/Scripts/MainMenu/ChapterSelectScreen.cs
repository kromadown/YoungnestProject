using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainMenu
{
    // The chapter select panel: chapters on the left, then that chapter's subchapters,
    // and a preview of the selected subchapter on the right.
    public class ChapterSelectScreen : MonoBehaviour
    {
        [Header("Chapters")]
        [SerializeField] private List<ChapterData> chapters = new();

        [Header("Row Text")]
        [Tooltip("{0} = chapter number, {1} = chapter name")]
        [SerializeField] private string chapterLabelFormat = "Chapter {0}: {1}";
        [SerializeField] private string lockedChapterName = "???";
        [SerializeField] private string lockedText = "Locked";
        [SerializeField] private string doneText = "Done";
        [SerializeField] private Color rowColor = new(0.91f, 0.91f, 0.91f);
        [SerializeField] private Color selectedRowColor = new(0.81f, 0.88f, 0.96f);

        [Header("Chapter Panel")]
        [SerializeField] private GameObject chapterPanel;
        [SerializeField] private TMP_Text breadcrumbText;
        [SerializeField] private TMP_Text listHeaderText;
        [SerializeField] private RectTransform listContent;
        [SerializeField] private UnityEngine.UI.Button listRowTemplate;
        [SerializeField] private UnityEngine.UI.Button backToMenuButton;
        [SerializeField] private UnityEngine.UI.Button backToChaptersButton;

        [Header("Preview")]
        [SerializeField] private GameObject emptyChaptersMessage;
        [SerializeField] private GameObject emptySubChaptersMessage;
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
        [SerializeField] private bool unlockAllChapters;

        // Texts set in the editor, restored when going back to the chapter list.
        private string _defaultBreadcrumbText;
        private string _defaultListHeaderText;

        private readonly List<GameObject> _spawnedRows = new();
        private readonly List<GameObject> _spawnedLearnItems = new();

        private ChapterData _currentChapter;
        private SubChapterData _currentSubChapter;

        // Raised when the player presses "Back to menu" on the chapter list.
        public event Action BackToMenuRequested;

        private void Awake()
        {
            _defaultBreadcrumbText = breadcrumbText.text;
            _defaultListHeaderText = listHeaderText.text;

            listRowTemplate.gameObject.SetActive(false);
            learnItemTemplate.gameObject.SetActive(false);

            backToMenuButton.onClick.AddListener(() => BackToMenuRequested?.Invoke());
            backToChaptersButton.onClick.AddListener(ShowChapterList);
            playButton.onClick.AddListener(OnPlay);
            replayButton.onClick.AddListener(OnPlay);
        }

        public void Show()
        {
            chapterPanel.SetActive(true);
            ShowChapterList();
        }

        public void Hide()
        {
            chapterPanel.SetActive(false);
        }

        private void ShowChapterList()
        {
            _currentChapter = null;
            _currentSubChapter = null;

            breadcrumbText.text = _defaultBreadcrumbText;
            listHeaderText.text = _defaultListHeaderText;
            backToMenuButton.gameObject.SetActive(true);
            backToChaptersButton.gameObject.SetActive(false);

            ClearRows();
            for (var i = 0; i < chapters.Count; i++)
            {
                var chapter = chapters[i];
                if (chapter == null) continue;

                var unlocked = IsUnlocked(chapter);
                var label = string.Format(chapterLabelFormat, i + 1, unlocked ? chapter.DisplayName : lockedChapterName);
                var status = unlocked ? $"{ChapterProgress.CountCompleted(chapter)}/{chapter.SubChapters.Count}" : lockedText;

                AddRow(label, status, unlocked, false, () => ShowSubChapterList(chapter));
            }

            ShowEmptyPreview(emptyChaptersMessage);
        }

        private void ShowSubChapterList(ChapterData chapter)
        {
            _currentChapter = chapter;

            var chapterNumber = chapters.IndexOf(chapter) + 1;
            breadcrumbText.text = string.Format(chapterLabelFormat, chapterNumber, chapter.DisplayName);
            listHeaderText.text = chapter.DisplayName;
            backToMenuButton.gameObject.SetActive(false);
            backToChaptersButton.gameObject.SetActive(true);

            ClearRows();
            foreach (var subChapter in chapter.SubChapters)
            {
                if (subChapter == null) continue;

                var unlocked = IsUnlocked(subChapter);
                var status = !unlocked ? lockedText : ChapterProgress.IsCompleted(subChapter) ? doneText : "";
                var selected = subChapter == _currentSubChapter;

                AddRow(subChapter.DisplayName, status, unlocked, selected, () => SelectSubChapter(subChapter));
            }

            if (_currentSubChapter == null)
            {
                ShowEmptyPreview(emptySubChaptersMessage);
            }
        }

        private void SelectSubChapter(SubChapterData subChapter)
        {
            _currentSubChapter = subChapter;

            // Rebuild the list so the selected row is highlighted.
            ShowSubChapterList(_currentChapter);
            ShowPreview(subChapter);
        }

        private void ShowEmptyPreview(GameObject message)
        {
            emptyChaptersMessage.SetActive(message == emptyChaptersMessage);
            emptySubChaptersMessage.SetActive(message == emptySubChaptersMessage);
            previewContent.SetActive(false);
        }

        private void ShowPreview(SubChapterData subChapter)
        {
            emptyChaptersMessage.SetActive(false);
            emptySubChaptersMessage.SetActive(false);
            previewContent.SetActive(true);

            var hasScreenshot = subChapter.Screenshot != null;
            previewScreenshot.sprite = subChapter.Screenshot;
            previewScreenshot.color = hasScreenshot ? Color.white : new Color(0.89f, 0.89f, 0.89f);
            screenshotPlaceholder.SetActive(!hasScreenshot);

            previewNameText.text = subChapter.DisplayName;
            previewDescriptionText.text = subChapter.Description;

            foreach (var item in _spawnedLearnItems) Destroy(item);
            _spawnedLearnItems.Clear();

            foreach (var goal in subChapter.LearningGoals)
            {
                var item = Instantiate(learnItemTemplate, learnListContent);
                item.text = $"• {goal}";
                item.gameObject.SetActive(true);
                _spawnedLearnItems.Add(item.gameObject);
            }

            var completed = ChapterProgress.IsCompleted(subChapter);
            playButton.gameObject.SetActive(!completed);
            replayButton.gameObject.SetActive(completed);
        }

        private void AddRow(string label, string status, bool interactable, bool selected, UnityEngine.Events.UnityAction onClick)
        {
            var row = Instantiate(listRowTemplate, listContent);
            row.gameObject.SetActive(true);
            row.interactable = interactable;
            row.onClick.AddListener(onClick);

            if (row.targetGraphic != null)
            {
                row.targetGraphic.color = selected ? selectedRowColor : rowColor;
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

        private bool IsUnlocked(ChapterData chapter)
        {
            return unlockAllChapters || ChapterProgress.IsChapterUnlocked(chapter, chapters);
        }

        private bool IsUnlocked(SubChapterData subChapter)
        {
            return unlockAllChapters || ChapterProgress.IsSubChapterUnlocked(subChapter, chapters);
        }

        private void OnPlay()
        {
            if (_currentSubChapter == null) return;

            if (string.IsNullOrEmpty(_currentSubChapter.SceneName))
            {
                Debug.LogWarning($"Subchapter '{_currentSubChapter.DisplayName}' has no scene set.");
                return;
            }

            ChapterProgress.CurrentSubChapter = _currentSubChapter;
            SceneManager.LoadScene(_currentSubChapter.SceneName);
        }
    }
}
