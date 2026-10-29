using UnityEngine;

namespace Phone.CameraApp
{
    public class PhoneCameraDetector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera phoneCamera;
        
        [Header("Detection")]
        [SerializeField] private LayerMask detectionLayer;

        public void TakePicture()
        {
            var hit = DetectedPhotoObject().collider;
            if (hit == null)
            {
                Debug.Log("Took a picture of nothing!");
                return;
            }

            var photoTarget = hit.GetComponentInParent<PhotoTarget>();
            if (photoTarget == null)
            {
                Debug.LogWarning("Took a picture of object but couldn't find PhotoTarget!");
                return;
            }
            
            photoTarget.OnPhotographed();
        }

        private RaycastHit2D DetectedPhotoObject()
        {
            var ray = phoneCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            return Physics2D.GetRayIntersection(ray, Mathf.Infinity, detectionLayer);
        }
    }
}
