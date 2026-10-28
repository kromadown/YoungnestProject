using UnityEngine;
using UnityEngine.UIElements;

namespace Phone.Camera
{
    [RequireComponent(typeof(UIDocument))]
    public class PhoneCameraUI : MonoBehaviour
    {
        private VisualElement _homeScreen;
        private VisualElement _cameraScreen;

        private Button _cameraButton;
        private Button _backButton;

        private void OnEnable()
        {
            if (!TryAssignReferences())
            {
                enabled = false;
                return;
            }
            
            SubscribeEvents();
            
            _cameraScreen.style.display = DisplayStyle.None;
        }

        private bool TryAssignReferences()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _homeScreen = root.Q<VisualElement>("HomeScreen");
            _cameraScreen = root.Q<VisualElement>("CameraScreen");
            
            _cameraButton = root.Q<Button>("CameraButton");
            _backButton = root.Q<Button>("CameraBackButton");
            
            if (_homeScreen == null || 
                _cameraScreen == null ||
                _cameraButton == null ||
                _backButton == null
                )
            {
                Debug.LogError("Phone Camera UI has missing references or UI elements.", this);
                return false;
            }
            
            return true;
        }

        private void SubscribeEvents()
        {
            _cameraButton.clicked += OpenCamera;
            _backButton.clicked += GoBack;
        }
        
        private void OnDisable()
        {
            UnSubscribeEvents();
        }

        private void UnSubscribeEvents()
        {
            _cameraButton.clicked -= OpenCamera;
            _backButton.clicked -= GoBack;
        }
        
        private void OpenCamera()
        {
            _homeScreen.style.display = DisplayStyle.None;
            _cameraScreen.style.display = DisplayStyle.Flex;
        }
        
        private void GoBack()
        {
            _cameraScreen.style.display = DisplayStyle.None;
            _homeScreen.style.display = DisplayStyle.Flex;
        }
    }
}
