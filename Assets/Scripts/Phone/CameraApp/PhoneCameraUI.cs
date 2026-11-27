using UnityEngine;
using UnityEngine.UIElements;

namespace Phone.CameraApp
{
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(PhoneCameraDragController))]
    public class PhoneCameraUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PhoneCameraDetector cameraDetector;
        [SerializeField] private RenderTexture cameraRenderTexture;
        
        private PhoneCameraDragController _dragController;
        
        private VisualElement _cameraScreen;
        
        private Button _cameraShutterButton;
        
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
            
            SetCameraOpen(false);
        }

        private bool TryAssignReferences()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            
            _cameraScreen = root.Q<VisualElement>("CameraScreen");
            
            _cameraShutterButton = root.Q<Button>("CameraShutterButton");
            
            _cameraPreview = root.Q<Image>("CameraPreview");

            _dragController = GetComponent<PhoneCameraDragController>();
            
            if (_cameraScreen == null ||
                _cameraShutterButton == null ||
                _cameraPreview == null ||
                cameraRenderTexture == null ||
                cameraDetector == null ||
                _dragController == null
                )
            {
                Debug.LogError("Phone Camera UI has missing references or UI elements.", this);
                return false;
            }
            
            return true;
        }

        private void SubscribeEvents()
        {
            _cameraShutterButton.clicked += TakePicture;
        }
        
        private void OnDisable()
        {
            cameraDetector?.SetCameraActive(false);
            UnSubscribeEvents();
        }

        private void UnSubscribeEvents()
        {
            if (_cameraShutterButton != null)
            {
                _cameraShutterButton.clicked -= TakePicture;
            }
        }
        
        public void OpenCamera()
        {
            _dragController.CancelReturn();
            _dragController.ResetMobileCameraView();
            
            SetCameraOpen(true);
        }

        private void TakePicture()
        {
            Debug.Log("Picture Taken!");
            cameraDetector.TakePicture();
        }
        
        public void CloseCamera()
        {
            SetCameraOpen(false);
            
            _dragController.ResetPhonePosition();
        }

        private void SetCameraOpen(bool isOpen)
        {
            _cameraScreen.style.display = isOpen ? DisplayStyle.Flex : DisplayStyle.None;
            
            cameraDetector.SetCameraActive(isOpen);
        }
    }
}