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

        private void Update()
        {
            #if UNITY_EDITOR
                UpdateUI();
            #endif
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
            #if UNITY_EDITOR
                return previewMobileInEditor;
            #else
                return Application.isMobilePlatform;
            #endif
        }
    }
}
