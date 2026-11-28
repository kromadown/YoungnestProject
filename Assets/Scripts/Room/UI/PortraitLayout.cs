using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Room.UI
{
    public class PortraitLayout : MonoBehaviour
    {
        [Serializable]
        public class RectOverride
        {
            public RectTransform target;
            public Vector2 anchorMin;
            public Vector2 anchorMax;
            public Vector2 offsetMin;
            public Vector2 offsetMax;

            [NonSerialized] public Vector2 landscapeAnchorMin;
            [NonSerialized] public Vector2 landscapeAnchorMax;
            [NonSerialized] public Vector2 landscapeOffsetMin;
            [NonSerialized] public Vector2 landscapeOffsetMax;
        }

        [Serializable]
        public class FontOverride
        {
            public TMP_Text target;
            public float fontSize;

            [NonSerialized] public float landscapeFontSize;
        }

        [SerializeField] bool portraitMode = true;
        [SerializeField] List<RectOverride> rects = new();
        [SerializeField] List<FontOverride> fonts = new();

        bool? appliedPortrait;

        void Awake()
        {
            foreach (var r in rects)
            {
                if (r.target == null) continue;
                r.landscapeAnchorMin = r.target.anchorMin;
                r.landscapeAnchorMax = r.target.anchorMax;
                r.landscapeOffsetMin = r.target.offsetMin;
                r.landscapeOffsetMax = r.target.offsetMax;
            }

            foreach (var f in fonts)
                if (f.target != null) f.landscapeFontSize = f.target.fontSize;

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

            foreach (var r in rects)
            {
                if (r.target == null) continue;
                r.target.anchorMin = portrait ? r.anchorMin : r.landscapeAnchorMin;
                r.target.anchorMax = portrait ? r.anchorMax : r.landscapeAnchorMax;
                r.target.offsetMin = portrait ? r.offsetMin : r.landscapeOffsetMin;
                r.target.offsetMax = portrait ? r.offsetMax : r.landscapeOffsetMax;
            }

            foreach (var f in fonts)
                if (f.target != null) f.target.fontSize = portrait ? f.fontSize : f.landscapeFontSize;
        }
    }
}
