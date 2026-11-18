using System;
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
        private bool? _currentMobileMode;

        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _currentMobileMode = null;

            UpdateUI();
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
