using UnityEngine;
using UnityEngine.UI;

namespace Room.UI
{
    [RequireComponent(typeof(CanvasScaler))]
    public class OrientationCanvasScaler : MonoBehaviour
    {
        [SerializeField] bool portraitMode = true;
        [SerializeField, Range(0f, 1f)] float portraitMatch = 0f;
        [SerializeField] Vector2 portraitReferenceResolution = new Vector2(1080f, 1920f);

        CanvasScaler scaler;
        float landscapeMatch;
        Vector2 landscapeReferenceResolution;
        bool? appliedPortrait;

        void Awake()
        {
            scaler = GetComponent<CanvasScaler>();
            landscapeMatch = scaler.matchWidthOrHeight;
            landscapeReferenceResolution = scaler.referenceResolution;
            Apply();
        }

        void Update()
        {
            Apply();
        }

        void Apply()
        {
            bool portrait = portraitMode && Screen.height > Screen.width;
            if (appliedPortrait == portrait) return;

            appliedPortrait = portrait;
            scaler.matchWidthOrHeight = portrait ? portraitMatch : landscapeMatch;
            scaler.referenceResolution = portrait ? portraitReferenceResolution : landscapeReferenceResolution;
        }
    }
}
