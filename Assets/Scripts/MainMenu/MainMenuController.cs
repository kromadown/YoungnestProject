using Player;
using UnityEngine;

namespace MainMenu
{
    // The title screen.
    // Continue (only shown when there's a save) opens the level select at the player's progress.
    // New Game deletes the save after asking, then asks for a name and opens the level select.
    public class MainMenuController : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private UnityEngine.UI.Button continueButton;
        [SerializeField] private UnityEngine.UI.Button newGameButton;
        [SerializeField] private UnityEngine.UI.Button settingsButton;
        [SerializeField] private UnityEngine.UI.Button quitButton;

        [Header("New Game Confirmation")]
        [Tooltip("Asks before New Game deletes an existing save.")]
        [SerializeField] private GameObject newGameConfirmPanel;
        [SerializeField] private UnityEngine.UI.Button confirmDeleteButton;
        [SerializeField] private UnityEngine.UI.Button confirmCancelButton;

        [Header("Screens")]
        [SerializeField] private NameEntryScreen nameEntry;
        [SerializeField] private LevelSelectScreen levelSelect;

        private bool HasConfirmUI => newGameConfirmPanel != null && confirmDeleteButton != null && confirmCancelButton != null;

        private void Awake()
        {
            newGameButton.onClick.AddListener(OnNewGame);
            settingsButton.onClick.AddListener(OnSettings);
            quitButton.onClick.AddListener(OnQuit);
            if (continueButton != null) continueButton.onClick.AddListener(OnContinue);

            if (HasConfirmUI)
            {
                confirmDeleteButton.onClick.AddListener(StartNewGame);
                confirmCancelButton.onClick.AddListener(() => newGameConfirmPanel.SetActive(false));
            }

            levelSelect.BackToMenuRequested += ShowMainPanel;

            if (nameEntry != null)
            {
                nameEntry.Confirmed += ShowLevelSelect;
                nameEntry.BackRequested += ShowMainPanel;
            }
        }

        private void OnDestroy()
        {
            if (levelSelect != null) levelSelect.BackToMenuRequested -= ShowMainPanel;

            if (nameEntry != null)
            {
                nameEntry.Confirmed -= ShowLevelSelect;
                nameEntry.BackRequested -= ShowMainPanel;
            }
        }

        private void Start()
        {
            ShowMainPanel();
        }

        private void ShowMainPanel()
        {
            mainPanel.SetActive(true);
            if (continueButton != null) continueButton.gameObject.SetActive(SaveData.HasSave);
            if (HasConfirmUI) newGameConfirmPanel.SetActive(false);
            if (nameEntry != null) nameEntry.Hide();
            levelSelect.Hide();
        }

        private void ShowLevelSelect()
        {
            mainPanel.SetActive(false);
            if (nameEntry != null) nameEntry.Hide();
            levelSelect.Show();
        }

        private void OnContinue()
        {
            mainPanel.SetActive(false);
            levelSelect.Show(openAtProgress: true);
        }

        private void OnNewGame()
        {
            if (SaveData.HasSave && HasConfirmUI)
            {
                newGameConfirmPanel.SetActive(true);
                return;
            }

            StartNewGame();
        }

        private void StartNewGame()
        {
            if (HasConfirmUI) newGameConfirmPanel.SetActive(false);

            // Without the confirmation panel, never delete a save without asking.
            if (SaveData.HasSave && !HasConfirmUI)
            {
                Debug.LogWarning("New Game confirmation isn't set up, so the save was kept. " +
                                 "Run Tools > Youngnest > Update Main Menu.", this);
            }
            else
            {
                SaveData.DeleteAll();
            }

            if (nameEntry == null || PlayerProfile.HasName)
            {
                ShowLevelSelect();
                return;
            }

            mainPanel.SetActive(false);
            nameEntry.Show();
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
