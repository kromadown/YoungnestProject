using UnityEngine;

namespace Phone.CameraApp
{
    [DefaultExecutionOrder(100)]
    public class PhoneCameraDetector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera phoneCamera;
        
        [Header("Detection")]
        [SerializeField] private LayerMask detectionLayer;

        private PhotoTarget _currentTarget;
        private bool _isCameraActive;

        private void LateUpdate()
        {
            if (!_isCameraActive) return;

            SetCurrentTarget(DetectPhotoTarget());
        }

        public void SetCameraActive(bool active)
        {
            _isCameraActive = active;

            if (!active) SetCurrentTarget(null);
        }

        public void TakePicture()
        {
            var target = DetectPhotoTarget();
            
            if (target == null)
            {
                Debug.Log("Took a picture of nothing!");
                return;
            }
            
            target.OnPhotographed();
        }

        private PhotoTarget DetectPhotoTarget()
        {
            var hit = DetectedPhotoObject().collider;
            if (hit == null) return null;

            return hit.GetComponentInParent<PhotoTarget>();
        }

        private void SetCurrentTarget(PhotoTarget target)
        {
            if (_currentTarget == target) return;

            if (_currentTarget != null)
            {
                _currentTarget.SetHighlighted(false);
            }

            _currentTarget = target;

            if (_currentTarget != null)
            {
                _currentTarget.SetHighlighted(true);
            }
        }

        private RaycastHit2D DetectedPhotoObject()
        {
            var ray = phoneCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            return Physics2D.GetRayIntersection(ray, Mathf.Infinity, detectionLayer);
        }

        private void OnDisable()
        {
            SetCameraActive(false);
        }
    }
}
