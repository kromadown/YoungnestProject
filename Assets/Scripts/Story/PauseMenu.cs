using MainMenu;
using Transitions;
using UnityEngine;
using UnityEngine.UI;

namespace Story
{
    // The pause button (top-left) and the pause menu it opens. Drop the PauseMenu prefab into each chapter scene.
    // Pausing stops time (timers, dialogue typing, the chapter flow) and the dark overlay blocks clicks underneath.
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private Button pauseButton;
        [Tooltip("The dark overlay with the menu on it. Hidden until the game is paused.")]
        [SerializeField] private GameObject menu;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private string mainMenuScene = "MainMenu";

        public static bool IsPaused { get; private set; }

        private float _timeScaleBeforePause = 1f;
        private bool _leaving;

        private void Awake()
        {
            pauseButton.onClick.AddListener(Pause);
            resumeButton.onClick.AddListener(Resume);
            levelSelectButton.onClick.AddListener(() => Leave(openLevelSelect: true));
            mainMenuButton.onClick.AddListener(() => Leave(openLevelSelect: false));

            menu.SetActive(false);
        }

        private void OnDestroy()
        {
            // Leaving the scene (or stopping play) while paused: put time and sound back to normal.
            if (IsPaused) Unfreeze();
        }

        private void Update()
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame) return;

            if (IsPaused) Resume();
            else Pause();
        }

        public void Pause()
        {
            if (IsPaused || _leaving || ChapterCompleteScreen.IsShowing) return;

            _timeScaleBeforePause = Time.timeScale;
            Time.timeScale = 0f;
            AudioListener.pause = true;
            IsPaused = true;
            menu.SetActive(true);
        }

        public void Resume()
        {
            if (!IsPaused || _leaving) return;

            Unfreeze();
            menu.SetActive(false);
        }

        private void Unfreeze()
        {
            Time.timeScale = _timeScaleBeforePause;
            AudioListener.pause = false;
            IsPaused = false;
        }

        // Stays paused during the fade out; OnDestroy puts time back once the scene is gone.
        private void Leave(bool openLevelSelect)
        {
            if (_leaving) return;
            _leaving = true;

            MainMenuController.OpenLevelSelectOnLoad = openLevelSelect;
            SceneFader.LoadScene(mainMenuScene);
        }
    }
}
