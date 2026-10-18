using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace MainMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuController : MonoBehaviour
    {
        [Header("Scenes")]
        [SerializeField] private string gameSceneName = "JulianScene";

        private Button _newGameButton;
        private Button _settingsButton;
        private Button _quitButton;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _newGameButton = root.Q<Button>("NewGameButton");
            _settingsButton = root.Q<Button>("SettingsButton");
            _quitButton = root.Q<Button>("QuitButton");

            _newGameButton.clicked += OnNewGame;
            _settingsButton.clicked += OnSettings;
            _quitButton.clicked += OnQuit;
        }

        private void OnDisable()
        {
            _newGameButton.clicked -= OnNewGame;
            _settingsButton.clicked -= OnSettings;
            _quitButton.clicked -= OnQuit;
        }

        private void OnNewGame()
        {
            SceneManager.LoadScene(gameSceneName);
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
