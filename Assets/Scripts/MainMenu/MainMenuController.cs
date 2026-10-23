using UnityEngine;

namespace MainMenu
{
    // The title screen: New Game opens the chapter select, Quit closes the game.
    public class MainMenuController : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private UnityEngine.UI.Button newGameButton;
        [SerializeField] private UnityEngine.UI.Button settingsButton;
        [SerializeField] private UnityEngine.UI.Button quitButton;

        [Header("Screens")]
        [SerializeField] private ChapterSelectScreen chapterSelect;

        private void Awake()
        {
            newGameButton.onClick.AddListener(OnNewGame);
            settingsButton.onClick.AddListener(OnSettings);
            quitButton.onClick.AddListener(OnQuit);
            chapterSelect.BackToMenuRequested += ShowMainPanel;
        }

        private void OnDestroy()
        {
            if (chapterSelect != null) chapterSelect.BackToMenuRequested -= ShowMainPanel;
        }

        private void Start()
        {
            ShowMainPanel();
        }

        private void ShowMainPanel()
        {
            mainPanel.SetActive(true);
            chapterSelect.Hide();
        }

        private void OnNewGame()
        {
            mainPanel.SetActive(false);
            chapterSelect.Show();
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
