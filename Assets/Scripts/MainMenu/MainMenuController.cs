using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace MainMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuController : MonoBehaviour
    {
        [Header("Chapters")]
        [SerializeField] private List<ChapterData> chapters = new();

        [Header("Text")]
        [Tooltip("{0} = chapter number, {1} = chapter name")]
        [SerializeField] private string chapterLabelFormat = "Chapter {0}: {1}";
        [SerializeField] private string lockedChapterName = "???";
        [SerializeField] private string lockedText = "Locked";
        [SerializeField] private string doneText = "Done";
        [SerializeField] private string backToMenuText = "Back to menu";
        [SerializeField] private string backToChaptersText = "Back to chapters";
        [SerializeField] private string pickSubChapterText = "Pick a part to see what happens";
        [SerializeField] private string playText = "Play";
        [SerializeField] private string replayText = "Replay";

        [Header("Debug")]
        [SerializeField] private bool unlockAllChapters;

        private VisualElement _mainPanel;
        private VisualElement _chapterPanel;

        private Button _newGameButton;
        private Button _settingsButton;
        private Button _quitButton;

        private Label _breadcrumbLabel;
        private Label _listHeaderLabel;
        private ScrollView _listScroll;
        private Button _listBackButton;

        private Label _previewEmptyLabel;
        private VisualElement _previewContent;
        private VisualElement _previewScreenshot;
        private Label _screenshotPlaceholderLabel;
        private Label _previewNameLabel;
        private Label _previewDescriptionLabel;
        private VisualElement _learnList;
        private Button _playButton;

        // Texts from the UXML, so edits made in UI Builder are kept.
        private string _defaultBreadcrumbText;
        private string _defaultListHeaderText;
        private string _defaultEmptyPreviewText;

        private ChapterData _currentChapter;
        private SubChapterData _currentSubChapter;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _mainPanel = root.Q<VisualElement>("MainPanel");
            _chapterPanel = root.Q<VisualElement>("ChapterPanel");

            _newGameButton = root.Q<Button>("NewGameButton");
            _settingsButton = root.Q<Button>("SettingsButton");
            _quitButton = root.Q<Button>("QuitButton");

            _breadcrumbLabel = root.Q<Label>("BreadcrumbLabel");
            _listHeaderLabel = root.Q<Label>("ListHeaderLabel");
            _listScroll = root.Q<ScrollView>("ListScroll");
            _listBackButton = root.Q<Button>("ListBackButton");

            _previewEmptyLabel = root.Q<Label>("PreviewEmptyLabel");
            _previewContent = root.Q<VisualElement>("PreviewContent");
            _previewScreenshot = root.Q<VisualElement>("PreviewScreenshot");
            _screenshotPlaceholderLabel = root.Q<Label>("ScreenshotPlaceholderLabel");
            _previewNameLabel = root.Q<Label>("PreviewNameLabel");
            _previewDescriptionLabel = root.Q<Label>("PreviewDescriptionLabel");
            _learnList = root.Q<VisualElement>("LearnList");
            _playButton = root.Q<Button>("PlayButton");

            _defaultBreadcrumbText = _breadcrumbLabel.text;
            _defaultListHeaderText = _listHeaderLabel.text;
            _defaultEmptyPreviewText = _previewEmptyLabel.text;

            _newGameButton.clicked += OnNewGame;
            _settingsButton.clicked += OnSettings;
            _quitButton.clicked += OnQuit;
            _listBackButton.clicked += OnListBack;
            _playButton.clicked += OnPlay;

            ShowMainPanel();
        }

        private void OnDisable()
        {
            _newGameButton.clicked -= OnNewGame;
            _settingsButton.clicked -= OnSettings;
            _quitButton.clicked -= OnQuit;
            _listBackButton.clicked -= OnListBack;
            _playButton.clicked -= OnPlay;
        }

        private void ShowMainPanel()
        {
            _mainPanel.style.display = DisplayStyle.Flex;
            _chapterPanel.style.display = DisplayStyle.None;
        }

        private void ShowChapterList()
        {
            _mainPanel.style.display = DisplayStyle.None;
            _chapterPanel.style.display = DisplayStyle.Flex;

            _currentChapter = null;
            _currentSubChapter = null;

            _breadcrumbLabel.text = _defaultBreadcrumbText;
            _listHeaderLabel.text = _defaultListHeaderText;
            _listBackButton.text = backToMenuText;

            _listScroll.Clear();
            for (var i = 0; i < chapters.Count; i++)
            {
                var chapter = chapters[i];
                if (chapter == null) continue;

                var unlocked = IsChapterUnlocked(chapter);
                var label = string.Format(chapterLabelFormat, i + 1, unlocked ? chapter.DisplayName : lockedChapterName);
                var status = unlocked ? $"{CountCompleted(chapter)}/{chapter.SubChapters.Count}" : lockedText;

                _listScroll.Add(CreateListButton(label, status, unlocked, false, () => ShowSubChapterList(chapter)));
            }

            ShowEmptyPreview(_defaultEmptyPreviewText);
        }

        private void ShowSubChapterList(ChapterData chapter)
        {
            _currentChapter = chapter;

            var chapterNumber = chapters.IndexOf(chapter) + 1;
            _breadcrumbLabel.text = string.Format(chapterLabelFormat, chapterNumber, chapter.DisplayName);
            _listHeaderLabel.text = chapter.DisplayName;
            _listBackButton.text = backToChaptersText;

            _listScroll.Clear();
            foreach (var subChapter in chapter.SubChapters)
            {
                if (subChapter == null) continue;

                var unlocked = IsSubChapterUnlocked(subChapter);
                var status = !unlocked ? lockedText : ChapterProgress.IsCompleted(subChapter) ? doneText : "";
                var selected = subChapter == _currentSubChapter;

                _listScroll.Add(CreateListButton(subChapter.DisplayName, status, unlocked, selected, () => SelectSubChapter(subChapter)));
            }

            if (_currentSubChapter == null)
            {
                ShowEmptyPreview(pickSubChapterText);
            }
        }

        private void SelectSubChapter(SubChapterData subChapter)
        {
            _currentSubChapter = subChapter;

            // Rebuild the list so the selected row is highlighted.
            ShowSubChapterList(_currentChapter);
            ShowPreview(subChapter);
        }

        private void ShowEmptyPreview(string message)
        {
            _previewEmptyLabel.text = message;
            _previewEmptyLabel.style.display = DisplayStyle.Flex;
            _previewContent.style.display = DisplayStyle.None;
        }

        private void ShowPreview(SubChapterData subChapter)
        {
            _previewEmptyLabel.style.display = DisplayStyle.None;
            _previewContent.style.display = DisplayStyle.Flex;

            var hasScreenshot = subChapter.Screenshot != null;
            _previewScreenshot.style.backgroundImage = hasScreenshot
                ? new StyleBackground(subChapter.Screenshot)
                : new StyleBackground(StyleKeyword.None);
            _screenshotPlaceholderLabel.style.display = hasScreenshot ? DisplayStyle.None : DisplayStyle.Flex;

            _previewNameLabel.text = subChapter.DisplayName;
            _previewDescriptionLabel.text = subChapter.Description;

            _learnList.Clear();
            foreach (var goal in subChapter.LearningGoals)
            {
                var goalLabel = new Label($"• {goal}");
                goalLabel.style.fontSize = 20;
                goalLabel.style.whiteSpace = WhiteSpace.Normal;
                _learnList.Add(goalLabel);
            }

            _playButton.text = ChapterProgress.IsCompleted(subChapter) ? replayText : playText;
        }

        private Button CreateListButton(string label, string status, bool enabled, bool selected, System.Action onClick)
        {
            var button = new Button(onClick);
            button.style.flexDirection = FlexDirection.Row;
            button.style.justifyContent = Justify.SpaceBetween;
            button.style.height = 56;
            button.style.marginBottom = 8;
            button.style.paddingLeft = 16;
            button.style.paddingRight = 16;
            button.style.fontSize = 22;

            if (selected)
            {
                button.style.backgroundColor = new Color(0.81f, 0.88f, 0.96f);
            }

            var nameLabel = new Label(label);
            nameLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            button.Add(nameLabel);

            var statusLabel = new Label(status);
            statusLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            statusLabel.style.color = new Color(0.4f, 0.4f, 0.4f);
            button.Add(statusLabel);

            button.SetEnabled(enabled);
            return button;
        }

        // A subchapter is unlocked if it's marked as unlocked by default
        // or the subchapter before it (across all chapters) is completed.
        private bool IsSubChapterUnlocked(SubChapterData subChapter)
        {
            if (unlockAllChapters || subChapter.UnlockedByDefault) return true;

            SubChapterData previous = null;
            foreach (var chapter in chapters)
            {
                if (chapter == null) continue;

                foreach (var other in chapter.SubChapters)
                {
                    if (other == null) continue;
                    if (other == subChapter)
                    {
                        // The very first subchapter of the game is always playable.
                        return previous == null || ChapterProgress.IsCompleted(previous);
                    }
                    previous = other;
                }
            }

            return false;
        }

        private bool IsChapterUnlocked(ChapterData chapter)
        {
            foreach (var subChapter in chapter.SubChapters)
            {
                if (subChapter != null && IsSubChapterUnlocked(subChapter)) return true;
            }

            return false;
        }

        private static int CountCompleted(ChapterData chapter)
        {
            var count = 0;
            foreach (var subChapter in chapter.SubChapters)
            {
                if (ChapterProgress.IsCompleted(subChapter)) count++;
            }

            return count;
        }

        private void OnNewGame()
        {
            ShowChapterList();
        }

        private void OnListBack()
        {
            if (_currentChapter != null)
            {
                ShowChapterList();
            }
            else
            {
                ShowMainPanel();
            }
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

        private void OnSettings()
        {
            // TODO: settings screen
            Debug.Log("Settings not implemented yet");
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
