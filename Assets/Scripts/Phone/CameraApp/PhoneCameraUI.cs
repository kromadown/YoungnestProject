using UnityEngine;
using UnityEngine.UIElements;

namespace Phone.CameraApp
{
    [RequireComponent(typeof(UIDocument))]
    public class PhoneCameraUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PhoneCameraDetector cameraDetector;
        [SerializeField] private RenderTexture cameraRenderTexture;
        
        private VisualElement _homeScreen;
        private VisualElement _cameraScreen;

        private Button _cameraButton;
        private Button _cameraShutterButton;
        private Button _backButton;
        
        private Image _cameraPreview;
        

        private void OnEnable()
        {
            if (!TryAssignReferences())
            {
                enabled = false;
                return;
            }
            
            SubscribeEvents();
            
            _cameraPreview.image = cameraRenderTexture;
            _cameraScreen.style.display = DisplayStyle.None;
        }

        private bool TryAssignReferences()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _homeScreen = root.Q<VisualElement>("HomeScreen");
            _cameraScreen = root.Q<VisualElement>("CameraScreen");
            
            _cameraButton = root.Q<Button>("CameraButton");
            _cameraShutterButton = root.Q<Button>("CameraShutterButton");
            _backButton = root.Q<Button>("CameraBackButton");
            
            _cameraPreview = root.Q<Image>("CameraPreview");
            
            if (_homeScreen == null || 
                _cameraScreen == null ||
                _cameraButton == null ||
                _cameraShutterButton == null ||
                _backButton == null ||
                _cameraPreview == null ||
                cameraRenderTexture == null ||
                cameraDetector == null
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
            _cameraShutterButton.clicked += TakePicture;
            _backButton.clicked += GoBack;
        }
        
        private void OnDisable()
        {
            UnSubscribeEvents();
        }

        private void UnSubscribeEvents()
        {
            if (_cameraButton != null)
            {
                _cameraButton.clicked -= OpenCamera;
            }

            if (_cameraShutterButton != null)
            {
                _cameraShutterButton.clicked -= TakePicture;
            }

            if (_backButton != null)
            {
                _backButton.clicked -= GoBack;
            }
        }
        
        private void OpenCamera()
        {
            _homeScreen.style.display = DisplayStyle.None;
            _cameraScreen.style.display = DisplayStyle.Flex;
        }

        private void TakePicture()
        {
            Debug.Log("Picture Taken!");
            cameraDetector.TakePicture();
        }
        
        private void GoBack()
        {
            _cameraScreen.style.display = DisplayStyle.None;
            _homeScreen.style.display = DisplayStyle.Flex;
        }
    }
}
