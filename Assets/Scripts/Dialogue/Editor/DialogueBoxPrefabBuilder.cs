using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Dialogue.Editor
{
    // Creates Assets/UI/Dialogue/DialogueBox.prefab. Run once from Tools > Youngnest > Create Dialogue Box Prefab,
    // then restyle the prefab by hand (swap in hand-drawn sprites, fonts, etc.).
    public static class DialogueBoxPrefabBuilder
    {
        private const string Folder = "Assets/UI/Dialogue";
        private const string PrefabPath = Folder + "/DialogueBox.prefab";
        private const int UILayer = 5;

        [MenuItem("Tools/Youngnest/Create Dialogue Box Prefab")]
        public static void Build()
        {
            if (Resources.Load<TMP_Settings>("TMP Settings") == null)
            {
                EditorUtility.DisplayDialog("Create Dialogue Box Prefab",
                    "TextMesh Pro Essentials are missing. Import them from Window > TextMeshPro > Import TMP Essential Resources, then run this again.", "OK");
                return;
            }

            if (File.Exists(PrefabPath) && !EditorUtility.DisplayDialog("Create Dialogue Box Prefab",
                    $"{PrefabPath} already exists. Replace it? Any edits made to it will be lost.", "Replace", "Cancel"))
            {
                return;
            }

            Directory.CreateDirectory(Folder);

            var root = new GameObject("DialogueBox", typeof(RectTransform), typeof(Canvas),
                typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster)) { layer = UILayer };

            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var dialogueRoot = CreateUI("DialogueRoot", root.transform);
            var rootRect = dialogueRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            // Main box along the bottom, left of the phone.
            var box = CreateUI("Box", dialogueRoot.transform);
            var boxRect = box.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0, 0);
            boxRect.anchorMax = new Vector2(0.62f, 0);
            boxRect.pivot = new Vector2(0, 0);
            boxRect.offsetMin = new Vector2(60, 40);
            boxRect.offsetMax = new Vector2(0, 210);
            var boxImage = AddOutlinedImage(box, true);
            var button = box.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = boxImage;
            button.transition = UnityEngine.UI.Selectable.Transition.None;

            var body = CreateText(box.transform, "BodyText", "Dialogue text goes here.", 32, false, TextAlignmentOptions.MidlineLeft);
            var bodyRect = body.rectTransform;
            bodyRect.anchorMin = Vector2.zero;
            bodyRect.anchorMax = Vector2.one;
            bodyRect.offsetMin = new Vector2(50, 25);
            bodyRect.offsetMax = new Vector2(-110, -25);

            var next = CreateText(box.transform, "NextIndicator", ">", 56, true, TextAlignmentOptions.Center);
            var nextRect = next.rectTransform;
            nextRect.anchorMin = new Vector2(1, 0);
            nextRect.anchorMax = new Vector2(1, 1);
            nextRect.pivot = new Vector2(1, 0.5f);
            nextRect.anchoredPosition = new Vector2(-20, 0);
            nextRect.sizeDelta = new Vector2(80, 0);

            // Name tag sitting on the top-left edge of the box.
            var nameTag = CreateUI("NameTag", dialogueRoot.transform);
            var tagRect = nameTag.GetComponent<RectTransform>();
            tagRect.anchorMin = new Vector2(0, 0);
            tagRect.anchorMax = new Vector2(0, 0);
            tagRect.pivot = new Vector2(0, 0);
            tagRect.anchoredPosition = new Vector2(90, 185);
            tagRect.sizeDelta = new Vector2(240, 56);
            AddOutlinedImage(nameTag, false);

            var name = CreateText(nameTag.transform, "NameText", "Jessie", 30, false, TextAlignmentOptions.Center);
            var nameRect = name.rectTransform;
            nameRect.anchorMin = Vector2.zero;
            nameRect.anchorMax = Vector2.one;
            nameRect.offsetMin = new Vector2(12, 0);
            nameRect.offsetMax = new Vector2(-12, 0);

            var manager = root.AddComponent<DialogueManager>();
            var so = new SerializedObject(manager);
            so.FindProperty("dialogueRoot").objectReferenceValue = dialogueRoot;
            so.FindProperty("nameTag").objectReferenceValue = nameTag;
            so.FindProperty("nameText").objectReferenceValue = name;
            so.FindProperty("bodyText").objectReferenceValue = body;
            so.FindProperty("advanceButton").objectReferenceValue = button;
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);

            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            Debug.Log($"Created {PrefabPath}. Drag it into your scene to use it.");
        }

        private static GameObject CreateUI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = UILayer };
            go.transform.SetParent(parent, false);
            return go;
        }

        private static UnityEngine.UI.Image AddOutlinedImage(GameObject go, bool raycastTarget)
        {
            var image = go.AddComponent<UnityEngine.UI.Image>();
            image.color = Color.white;
            image.raycastTarget = raycastTarget;

            var outline = go.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(4, -4);
            return image;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size, bool bold,
            TextAlignmentOptions alignment)
        {
            var go = CreateUI(name, parent);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            tmp.color = Color.black;
            tmp.alignment = alignment;
            tmp.raycastTarget = false;
            return tmp;
        }
    }
}
