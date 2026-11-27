using Phone.CameraApp;
using Phone.Messages;
using UnityEngine;
using UnityEngine.UIElements;

namespace Phone
{
    [RequireComponent(typeof(UIDocument))]
    public class PhoneUIController : MonoBehaviour
    {
        [Header("Test")] 
        [SerializeField] private bool previewMobileInEditor;

        private PhoneMessagesUI _messagesUI;
        private PhoneCameraUI _cameraUI;
        
        private VisualElement _root;
        private VisualElement _phoneOverlay;
        private VisualElement _homeScreen;

        private Button _phoneToggleButton;
        private Button _messagesButton;
        private Button _cameraButton;
        private Button _homeButton;
        private Button _backButton;
        
        private bool? _currentMobileMode;
        private bool _isPhoneOpen;
        
        public bool IsMobilePhoneOpen => _currentMobileMode == true && _isPhoneOpen;
        public bool IsMobileLayout => _currentMobileMode == true;
        
        private enum PhoneApp
        {
            Home,
            Messages,
            Camera
        }
        
        private PhoneApp _currentApp = PhoneApp.Home;

        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _root.pickingMode = PickingMode.Ignore;
            
            _messagesUI = GetComponent<PhoneMessagesUI>();
            _cameraUI = GetComponent<PhoneCameraUI>();
            
            _homeScreen = _root.Q<VisualElement>("HomeScreen");
            _phoneOverlay = _root.Q<VisualElement>("PhoneOverlay");
            
            _phoneToggleButton = _root.Q<Button>("PhoneToggleButton");
            _messagesButton = _root.Q<Button>("MessagesButton");
            _cameraButton = _root.Q<Button>("CameraButton");
            _homeButton = _root.Q<Button>("HomeIndicator");
            _backButton = _root.Q<Button>("BackButton");

            if (_phoneOverlay == null ||
                _phoneToggleButton == null ||
                _homeScreen == null ||
                _messagesButton == null ||
                _cameraButton == null ||
                _homeButton == null ||
                _backButton == null)
            {
                Debug.LogError("Phone UI elements are missing.", this);
                enabled = false;
                return;
            }

            _phoneToggleButton.clicked += TogglePhone;
            _messagesButton.clicked += OpenMessages;
            _cameraButton.clicked += OpenCamera;
            _homeButton.clicked += GoHome;
            _backButton.clicked += GoBack;
            
            SetCurrentApp(PhoneApp.Home);
            
            _currentMobileMode = null;
            UpdateUI();
        }

        private void OnDisable()
        {
            if (_phoneToggleButton != null)
            {
                _phoneToggleButton.clicked -= TogglePhone;
            }

            if (_messagesButton != null)
            {
                _messagesButton.clicked -= OpenMessages;
            }

            if (_cameraButton != null)
            {
                _cameraButton.clicked -= OpenCamera;
            }

            if (_homeButton != null)
            {
                _homeButton.clicked -= GoHome;
            }

            if (_backButton != null)
            {
                _backButton.clicked -= GoBack;
            }
        }

        private void TogglePhone()
        {
            if (_currentMobileMode != true) return;

            SetPhoneOpen(!_isPhoneOpen);
        }

        // Checked every frame so turning the phone switches the layout.
        private void Update()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            var useMobileLayout = ShouldUseMobileLayout();

            if (_currentMobileMode == useMobileLayout) return;
            
            _root.EnableInClassList("mobile-layout", useMobileLayout);
            _currentMobileMode = useMobileLayout;
            SetPhoneOpen(!useMobileLayout);
        }
        
        private void SetPhoneOpen(bool isOpen)
        {
            _isPhoneOpen  = isOpen;
            
            _phoneOverlay.style.display = 
                isOpen
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            
            _phoneToggleButton.text = isOpen ? "Close" : "Open";
        }

        private bool ShouldUseMobileLayout()
        {
            // Only a phone held upright gets the Open/Close phone. Sideways (the way the game is
            // played for now) keeps the phone on the side of the screen, like on a computer.
            var upright = Screen.height > Screen.width;

            #if UNITY_EDITOR
                // Preview Mobile In Editor acts like a phone: set the Game view to a tall size to see it.
                return previewMobileInEditor && upright;
            #else
                return Application.isMobilePlatform && upright;
            #endif
        }

        public void CloseForRoomDialogue()
        {
            if (IsMobilePhoneOpen)
            {
                SetPhoneOpen(false);
            }
        }

        private void OpenMessages()
        {
            if (_currentApp != PhoneApp.Home) return;
            if (!_messagesUI.isActiveAndEnabled) return;

            _messagesUI.OpenMessages();
            SetCurrentApp(PhoneApp.Messages);
        }
        
        private void OpenCamera()
        {
            if (_currentApp != PhoneApp.Home) return;
            if (!_cameraUI.isActiveAndEnabled) return;

            _cameraUI.OpenCamera();
            SetCurrentApp(PhoneApp.Camera);
        }

        private void GoBack()
        {
            if (_currentApp == PhoneApp.Home) return;
            
            if (_currentApp == PhoneApp.Messages)
            {
                if (_messagesUI.TryGoBack()) return;
            }
            
            GoHome();
        }

        private void GoHome()
        {
            switch (_currentApp)
            {
                case PhoneApp.Messages:
                    _messagesUI.CloseMessages();
                    break;
                
                case PhoneApp.Camera:
                    _cameraUI.CloseCamera();
                    break;
            }
            
            SetCurrentApp(PhoneApp.Home);
        }
        
        private void SetCurrentApp(PhoneApp app)
        {
            _currentApp = app;

            var isHome = app == PhoneApp.Home;
            _homeScreen.style.display = isHome ? DisplayStyle.Flex : DisplayStyle.None;
            
            _backButton.SetEnabled(!isHome);
        }
    }
}
