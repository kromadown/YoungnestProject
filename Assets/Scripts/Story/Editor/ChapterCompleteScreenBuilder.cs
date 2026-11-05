using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Story.Editor
{
    // Tools > Youngnest > Add Chapter Complete Screen To Scene
    // Creates Assets/UI/ChapterComplete/ChapterCompleteScreen.prefab (if missing) and adds it to the open scene.
    // Restyle the prefab by hand afterwards.
    public static class ChapterCompleteScreenBuilder
    {
        private const string Folder = "Assets/UI/ChapterComplete";
        private const string PrefabPath = Folder + "/ChapterCompleteScreen.prefab";
        private const string Title = "Chapter Complete Screen";
        private const int UILayer = 5;

        private static readonly Color Ink = new(0.07f, 0.07f, 0.07f);
        private static readonly Color Grey = new(0.33f, 0.33f, 0.33f);
        private static readonly Color ButtonColor = new(0.91f, 0.91f, 0.91f);
        private static readonly Color SidePanelColor = new(0.97f, 0.97f, 0.97f);

        [MenuItem("Tools/Youngnest/Add Chapter Complete Screen To Scene")]
        public static void AddToScene()
        {
            if (Resources.Load<TMP_Settings>("TMP Settings") == null)
            {
                EditorUtility.DisplayDialog(Title, "TextMesh Pro Essentials are missing. Import them first.", "OK");
                return;
            }

            if (Object.FindAnyObjectByType<ChapterCompleteScreen>(FindObjectsInactive.Include) != null)
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null) UpgradePrefab();
                EditorUtility.DisplayDialog(Title, "This scene already has a Chapter Complete screen (prefab is up to date).", "OK");
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null) prefab = CreatePrefab();
            else UpgradePrefab();

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(instance, Title);
            Selection.activeGameObject = instance;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("Chapter Complete screen added. Save the scene to keep it.");
        }

        private static GameObject CreatePrefab()
        {
            Directory.CreateDirectory(Folder);

            var root = new GameObject("ChapterCompleteScreen", typeof(RectTransform), typeof(Canvas),
                typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster)) { layer = UILayer };

            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50; // above the dialogue box (10)
            var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Full-screen white panel; it also blocks clicks on the room behind it.
            var panel = CreateUI("Panel", root.transform);
            Stretch(panel, 0, 0, 0, 0);
            AddImage(panel, Color.white, true);

            var content = CreateUI("Content", panel.transform);
            Stretch(content, 86, 50, 86, 58);

            // Header: "First night   Chapter 1 complete" on one line.
            var header = CreateUI("Header", content.transform);
            AnchorTop(header, 96);
            var headerLayout = header.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            headerLayout.spacing = 38;
            headerLayout.childAlignment = TextAnchor.LowerLeft;
            headerLayout.childControlWidth = true;
            headerLayout.childControlHeight = true;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childForceExpandHeight = false;
            var title = CreateText(header.transform, "TitleText", "First night", 80, true, Ink, TextAlignmentOptions.BottomLeft);
            var subtitle = CreateText(header.transform, "SubtitleText", "Chapter 1 complete", 50, false, Grey, TextAlignmentOptions.BottomLeft);

            // Body: choice cards on the left, what you learned on the right.
            var body = CreateUI("Body", content.transform);
            Stretch(body, 0, 128, 0, 126);

            var list = CreateUI("ChoiceList", body.transform);
            SetAnchors(list, new Vector2(0, 0), new Vector2(0.6f, 1), Vector2.zero, new Vector2(-30, 0));
            AddVerticalLayout(list, 19, new RectOffset(0, 0, 0, 0));

            var card = CreateUI("ChoiceCardTemplate", list.transform);
            AddImage(card, Color.white, false);
            var outline = card.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = Ink;
            outline.effectDistance = new Vector2(3, -3);
            AddVerticalLayout(card, 8, new RectOffset(38, 38, 26, 26));
            CreateText(card.transform, "Text", "You <b>bought the home insurance</b>.", 44, false, Ink, TextAlignmentOptions.MidlineLeft);
            AddSubText(card.transform);

            var learned = CreateUI("LearnedSection", body.transform);
            SetAnchors(learned, new Vector2(0.62f, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            AddImage(learned, SidePanelColor, false);
            AddVerticalLayout(learned, 19, new RectOffset(38, 38, 30, 30));
            CreateText(learned.transform, "LearnedHeader", "What you learned", 38, false, Grey, TextAlignmentOptions.TopLeft);
            var learnList = CreateUI("LearnList", learned.transform);
            AddVerticalLayout(learnList, 6, new RectOffset(0, 0, 0, 0));
            var learnItem = CreateText(learnList.transform, "LearnItemTemplate", "• Learning goal", 38, false, Ink, TextAlignmentOptions.TopLeft);

            // Buttons, styled like the main menu buttons.
            var back = CreateButton(content.transform, "BackButton", "Back to level select");
            AnchorBottom(back.gameObject, new Vector2(0, 0), 460, 104);
            var next = CreateButton(content.transform, "NextButton", "Next chapter");
            AnchorBottom(next.gameObject, new Vector2(1, 0), 460, 104);

            var screen = root.AddComponent<ChapterCompleteScreen>();
            var so = new SerializedObject(screen);
            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("subtitleText").objectReferenceValue = subtitle;
            so.FindProperty("choiceList").objectReferenceValue = list.GetComponent<RectTransform>();
            so.FindProperty("choiceCardTemplate").objectReferenceValue = card;
            so.FindProperty("learnedSection").objectReferenceValue = learned;
            so.FindProperty("learnList").objectReferenceValue = learnList.GetComponent<RectTransform>();
            so.FindProperty("learnItemTemplate").objectReferenceValue = learnItem;
            so.FindProperty("backButton").objectReferenceValue = back;
            so.FindProperty("nextButton").objectReferenceValue = next;
            so.ApplyModifiedPropertiesWithoutUndo();

            card.SetActive(false);
            learnItem.gameObject.SetActive(false);
            panel.SetActive(false);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        // The small italic "other option" line under a choice card's sentence.
        private static void AddSubText(Transform card)
        {
            var sub = CreateText(card, "SubText", "If you hadn't, you'd have kept $1,500 this month.", 34, false, Grey,
                TextAlignmentOptions.MidlineLeft);
            sub.fontStyle = FontStyles.Italic;
        }

        // Adds anything an older prefab is missing (currently the SubText line), keeping your edits.
        private static void UpgradePrefab()
        {
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var card = root.GetComponentInChildren<ChapterCompleteScreen>(true) != null
                    ? FindDeep(root.transform, "ChoiceCardTemplate")
                    : null;
                if (card == null || card.Find("SubText") != null) return;

                var layout = card.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
                if (layout != null) layout.spacing = 8;
                AddSubText(card);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log("Chapter Complete screen prefab updated with the other-option line.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static Transform FindDeep(Transform parent, string name)
        {
            foreach (var child in parent.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name) return child;
            }
            return null;
        }

        private static GameObject CreateUI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = UILayer };
            go.transform.SetParent(parent, false);
            return go;
        }

        private static UnityEngine.UI.Image AddImage(GameObject go, Color color, bool raycastTarget)
        {
            var image = go.AddComponent<UnityEngine.UI.Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
        }

        private static void AddVerticalLayout(GameObject go, float spacing, RectOffset padding)
        {
            var layout = go.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size, bool bold,
            Color color, TextAlignmentOptions alignment)
        {
            var go = CreateUI(name, parent);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.richText = true;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static UnityEngine.UI.Button CreateButton(Transform parent, string name, string label)
        {
            var go = CreateUI(name, parent);
            var image = AddImage(go, ButtonColor, true);
            var button = go.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image;

            var text = CreateText(go.transform, "Text", label, 44, false, Ink, TextAlignmentOptions.Center);
            Stretch(text.gameObject, 0, 0, 0, 0);
            return button;
        }

        private static void Stretch(GameObject go, float left, float bottom, float right, float top)
        {
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void SetAnchors(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void AnchorTop(GameObject go, float height)
        {
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0, height);
        }

        private static void AnchorBottom(GameObject go, Vector2 corner, float width, float height)
        {
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = corner;
            rect.anchorMax = corner;
            rect.pivot = corner;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(width, height);
        }
    }
}
