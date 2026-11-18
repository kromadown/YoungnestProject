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
        [SerializeField] private PhoneUIController phoneUIController;

        [Header("Animation")]
        [SerializeField, Min(0.01f)] private float returnDuration = 0.5f;

        [Header("Mobile Version Camera")] 
        [SerializeField, Range(1f, 5f)] private float mobileZoom = 1.75f;
        
        private VisualElement _phoneFrame;
        private VisualElement _cameraScreen;
        
        private Image _cameraPreview;

        private Vector2 _phoneOffset;
        private Vector2 _returnStartOffset;
        private Vector2 _lastPointerPosition;

        private Vector3 _mobilePanOffset;

        private bool _isReturning;
        private bool _isDragging;
        private bool IsMobile => phoneUIController.IsMobileLayout;
        
        private int _activePointerId = -1;

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
                phoneCamera == null ||
                phoneUIController == null)
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
            EndDrag(true);
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
            if (_isReturning)
            {
                UpdatePhoneReturn();
            }
        }

        private void UpdatePhoneReturn()
        {
            _returnElapsedTime += Time.deltaTime;
            
            var t = Mathf.Clamp01(_returnElapsedTime / returnDuration);
            var smoothedT = Mathf.SmoothStep(0, 1, t);
            var newPhoneOffset = Vector2.Lerp(_returnStartOffset, Vector2.zero, smoothedT);
            
            ApplyPhoneOffset(newPhoneOffset);

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

        #region Camera Positioning

            private void UpdateCameraPosition()
            {
                var panelBounds = _cameraPreview.panel.visualTree.worldBound;
                var previewBounds = _cameraPreview.worldBound;
    
                if (panelBounds.width <= 0 || 
                    panelBounds.height <= 0 || 
                    previewBounds.width <= 0 || 
                    previewBounds.height <= 0) 
                    return;
    
                var cameraPosition =
                    IsMobile
                        ? mainCamera.transform.position + _mobilePanOffset
                        : GetPCCameraPosition(panelBounds, previewBounds);
                
                phoneCamera.transform.SetPositionAndRotation(cameraPosition, mainCamera.transform.rotation);
                
                UpdateCameraProjection(panelBounds, previewBounds);
            }
    
            private Vector3 GetPCCameraPosition(Rect panelBounds, Rect previewBounds)
            {
                var viewPortX = (previewBounds.center.x - panelBounds.xMin) / panelBounds.width;
                var viewPortY = 1f - (previewBounds.center.y - panelBounds.yMin) / panelBounds.height;
    
                const float depth = 1f;
    
                var worldAtScreenCenter = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, depth));
                var worldAtPhoneCenter = mainCamera.ViewportToWorldPoint(new Vector3(viewPortX, viewPortY, depth));
    
                var worldOffset = worldAtPhoneCenter - worldAtScreenCenter;
                
                return mainCamera.transform.position + worldOffset;
            }
    
            private void UpdateCameraProjection(Rect panelBounds, Rect previewBounds)
            {
                phoneCamera.orthographic = true;
    
                var baseSize = mainCamera.orthographicSize * (previewBounds.height / panelBounds.height);
                phoneCamera.orthographicSize = IsMobile ? baseSize / mobileZoom : baseSize;
                phoneCamera.aspect = previewBounds.width / previewBounds.height;
            }

        #endregion
        
        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;

            if (!IsMobile)
            {
                CancelReturn(false);
            }
            
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

            if (IsMobile)
            {
                PanMobileCamera(delta);
                return;
            }
            
            ApplyPhoneOffset(_phoneOffset + delta);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_isDragging) return;
            if (evt.pointerId != _activePointerId) return;
            if (evt.button != 0) return;
            
            EndDrag(true);
        }

        private void OnLostPointer(PointerCaptureOutEvent evt)
        {
            if (_activePointerId != evt.pointerId) return;
            
            EndDrag(false);
        }

        private void EndDrag(bool releasePointerCapture)
        {
            var pointerId = _activePointerId;
            
            _activePointerId = -1;
            _isDragging = false;

            if (_cameraPreview == null || pointerId == -1) return;
            
            if (releasePointerCapture && _cameraPreview.HasPointerCapture(pointerId))
            {
                _cameraPreview.ReleasePointer(pointerId);
            }
        }

        private void ApplyPhoneOffset(Vector2 offset)
        {
            _phoneOffset = offset;
            
            _phoneFrame.style.translate = new Translate(_phoneOffset.x, _phoneOffset.y);
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
                ApplyPhoneOffset(Vector2.zero);
            }
            
            _isReturning = false;
        }

        private void PanMobileCamera(Vector2 delta)
        {
            var previewHeight = _cameraPreview.worldBound.height;
            
            if (previewHeight <= 0) return;
            
            var worldUnitsPerPixel = (phoneCamera.orthographicSize * 2f) / previewHeight;
            var movement = 
                    -mainCamera.transform.right * (delta.x * worldUnitsPerPixel)
                    +mainCamera.transform.up * (delta.y * worldUnitsPerPixel);

            _mobilePanOffset += movement;
        }

        public void ResetMobileCameraView()
        {
            _mobilePanOffset = Vector3.zero;
        }
    }
}