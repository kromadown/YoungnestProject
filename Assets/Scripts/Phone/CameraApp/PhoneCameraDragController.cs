using UnityEngine;
using UnityEngine.UIElements;

namespace Phone.CameraApp
{
    [RequireComponent(typeof(UIDocument))]
    public class PhoneCameraDragController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Camera phoneCamera;

        [Header("Settings")]
        [SerializeField, Min(0.01f)] private float returnDuration = 0.5f;
        
        private VisualElement _phoneFrame;
        private VisualElement _cameraScreen;
        private Image _cameraPreview;
        
        private bool _isDragging;
        private Vector2 _lastPointerPosition;
        private int _activePointerId = -1;

        private Vector2 _phoneOffset;
        private Vector2 _returnStartOffset;

        private bool _isReturning;

        private float _returnElapsedTime = 0f;
        
        private void OnEnable()
        {
            if (!TryAssignReferences())
            {
                enabled = false;
                return;
            }
            
            RegisterCallbacks();
        }

        private bool TryAssignReferences()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            
            _phoneFrame = root.Q<VisualElement>("PhoneFrame");
            _cameraScreen = root.Q<VisualElement>("CameraScreen");
            
            _cameraPreview = root.Q<Image>("CameraPreview");

            if (_phoneFrame == null || 
                _cameraScreen == null || 
                _cameraPreview == null || 
                mainCamera == null ||
                phoneCamera == null)
            {
                Debug.LogError("Phone Camera Drag Controller has missing references or UI elements.", this);
                return false;
            }

            return true;
        }

        private void RegisterCallbacks()
        {
            if (_cameraPreview == null) return;
            
            _cameraPreview.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _cameraPreview.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _cameraPreview.RegisterCallback<PointerUpEvent>(OnPointerUp);
            _cameraPreview.RegisterCallback<PointerCaptureOutEvent>(OnLostPointer);
        }

        private void OnDisable()
        {
            UnRegisterCallbacks();
            
            if (_cameraPreview != null &&
                _activePointerId != -1 &&
                _cameraPreview.HasPointerCapture(_activePointerId))
            {
                _cameraPreview.ReleasePointer(_activePointerId);
            }
            
            _isDragging = false;
            _activePointerId = -1;
        }
        
        private void UnRegisterCallbacks()
        {
            if (_cameraPreview == null) return;
            
            _cameraPreview.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            _cameraPreview.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            _cameraPreview.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            _cameraPreview.UnregisterCallback<PointerCaptureOutEvent>(OnLostPointer);
        }

        private void Update()
        {
            if (!_isReturning) return;
            
            _returnElapsedTime += Time.deltaTime;
            
            var t = _returnElapsedTime / returnDuration;
            var smoothedT = Mathf.SmoothStep(0, 1, t);
            _phoneOffset = Vector2.Lerp(_returnStartOffset, Vector2.zero, smoothedT);
            
            _phoneFrame.style.translate = new Translate(_phoneOffset.x, _phoneOffset.y);

            if (t >= 1)
            {
                _isReturning = false;
            }
        }

        private void LateUpdate()
        {
            if (_cameraPreview == null || _cameraPreview.panel == null) return;
            if (_cameraScreen.resolvedStyle.display == DisplayStyle.None) return;

            UpdateCameraPosition();
        }

        private void UpdateCameraPosition()
        {
            var panelBounds = _cameraPreview.panel.visualTree.worldBound;
            var previewBounds = _cameraPreview.worldBound;

            if (panelBounds.width <= 0 || panelBounds.height <= 0 || previewBounds.width <= 0 || previewBounds.height <= 0) return;
            
            var viewPortX = (previewBounds.center.x - panelBounds.xMin) / panelBounds.width;
            var viewPortY = 1f - (previewBounds.center.y - panelBounds.yMin) / panelBounds.height;

            const float depth = 1f;

            var worldAtScreenCenter = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, depth));
            var worldAtPhoneCenter = mainCamera.ViewportToWorldPoint(new Vector3(viewPortX, viewPortY, depth));

            var worldOffset = worldAtPhoneCenter - worldAtScreenCenter;
            
            phoneCamera.transform.SetPositionAndRotation(
                mainCamera.transform.position + worldOffset, 
                mainCamera.transform.rotation
            );

            phoneCamera.orthographic = true;
            phoneCamera.orthographicSize = mainCamera.orthographicSize * (previewBounds.height / panelBounds.height);
            phoneCamera.aspect = previewBounds.width / previewBounds.height;
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;
            
            CancelReturn(false);
            
            _isDragging = true;
            
            _lastPointerPosition = new Vector2(evt.position.x, evt.position.y);
            
            _activePointerId = evt.pointerId;
            
            _cameraPreview.CapturePointer(evt.pointerId);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_isDragging) return;
            if (evt.pointerId != _activePointerId) return;
            
            Vector2 currentPosition = evt.position;
            var delta = currentPosition - _lastPointerPosition;

            _lastPointerPosition = currentPosition;

            _phoneOffset += delta;
            _phoneFrame.style.translate = new Translate(
                _phoneOffset.x,
                _phoneOffset.y
                );
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_isDragging) return;
            if (evt.pointerId != _activePointerId) return;
            if (evt.button != 0) return;
            
            _isDragging = false;
            _activePointerId = -1;
            
            _cameraPreview.ReleasePointer(evt.pointerId);
        }

        private void OnLostPointer(PointerCaptureOutEvent evt)
        {
            if (_activePointerId != evt.pointerId) return;
            
            _isDragging = false;
            _activePointerId = -1;
        }
        
        public void ResetPhonePosition()
        {
            _returnStartOffset = _phoneOffset;
            _returnElapsedTime = 0f;
            _isReturning = true;
        }

        public void CancelReturn(bool resetPosition = true)
        {
            if (resetPosition)
            {
                _phoneOffset = Vector2.zero;
                _phoneFrame.style.translate = new Translate(_phoneOffset.x,  _phoneOffset.y);
            }
            
            _isReturning = false;
        }
    }
}
