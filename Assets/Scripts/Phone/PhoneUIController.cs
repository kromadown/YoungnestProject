using System;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UIElements;

namespace Phone
{
    [RequireComponent(typeof(UIDocument))]
    public class PhoneUIController : MonoBehaviour
    {
        [Header("Test")] 
        [SerializeField] private bool previewMobileInEditor;

        private VisualElement _root;
        private VisualElement _phoneOverlay;

        private Button _phoneToggleButton;
        
        private bool? _currentMobileMode;
        private bool _isPhoneOpen;
        
        public bool IsMobilePhoneOpen => _currentMobileMode == true && _isPhoneOpen;
        public bool IsMobileLayout => _currentMobileMode == true;

        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _root.pickingMode = PickingMode.Ignore;
            
            _phoneOverlay = _root.Q<VisualElement>("PhoneOverlay");
            _phoneToggleButton = _root.Q<Button>("PhoneToggleButton");

            if (_phoneOverlay == null || _phoneToggleButton == null)
            {
                Debug.LogError("Phone UI elements are missing.", this);
                enabled = false;
                return;
            }

            _phoneToggleButton.clicked += TogglePhone;
            
            _currentMobileMode = null;
            UpdateUI();
        }

        private void OnDisable()
        {
            if (_phoneToggleButton != null)
            {
                _phoneToggleButton.clicked -= TogglePhone;
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
    }
}
