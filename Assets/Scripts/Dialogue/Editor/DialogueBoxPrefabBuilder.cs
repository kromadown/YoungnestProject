using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Dialogue.Editor
{
    // Creates Assets/UI/Dialogue/DialogueBox.prefab from Tools > Youngnest > Create Dialogue Box Prefab,
    // including the (hidden) choice list. Restyle the prefab by hand afterwards.
    // If the prefab already exists, "Update" adds anything missing (e.g. the choice list) and keeps your edits.
    public static class DialogueBoxPrefabBuilder
    {
        private const string Folder = "Assets/UI/Dialogue";
        private const string PrefabPath = Folder + "/DialogueBox.prefab";
        private const string Title = "Create Dialogue Box Prefab";
        private const int UILayer = 5;

        [MenuItem("Tools/Youngnest/Create Dialogue Box Prefab")]
        public static void Build()
        {
            if (Resources.Load<TMP_Settings>("TMP Settings") == null)
            {
                EditorUtility.DisplayDialog(Title,
                    "TextMesh Pro Essentials are missing. Import them from Window > TextMeshPro > Import TMP Essential Resources, then run this again.", "OK");
                return;
            }

            if (File.Exists(PrefabPath))
            {
                var answer = EditorUtility.DisplayDialogComplex(Title,
                    $"{PrefabPath} already exists.\n\nUpdate: add anything missing (like the choice list) and keep your edits.\n" +
                    "Replace: build a fresh box; your edits will be lost.",
                    "Update", "Cancel", "Replace");

                if (answer == 0) UpdateExisting();
                if (answer != 2) return;
            }

            CreateFresh();
        }

        private static void CreateFresh()
        {
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

            AddChoiceList(box.transform, manager);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);

            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            Debug.Log($"Created {PrefabPath}. Drag it into your scene to use it.");
        }

        // Adds what's missing to the existing prefab, leaving everything else (art, fonts, layout) as is.
        private static void UpdateExisting()
        {
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var manager = root.GetComponent<DialogueManager>();
                var box = root.transform.Find("DialogueRoot/Box");
                if (manager == null || box == null)
                {
                    EditorUtility.DisplayDialog(Title,
                        "Couldn't find DialogueManager or DialogueRoot/Box in the prefab. Use Replace to build a fresh one.", "OK");
                    return;
                }

                if (box.Find("ChoicePanel") != null)
                {
                    EditorUtility.DisplayDialog(Title, "The DialogueBox prefab is already up to date.", "OK");
                    return;
                }

                AddChoiceList(box, manager);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log($"Updated {PrefabPath}: added the choice list.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // The prompt and option buttons shown when a dialogue has choices. Hidden the rest of the time.
        private static void AddChoiceList(Transform box, DialogueManager manager)
        {
            var panel = CreateUI("ChoicePanel", box);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = new Vector2(50, 20);
            panelRect.offsetMax = new Vector2(-40, -20);
            AddVerticalLayout(panel, 10);

            var prompt = CreateText(panel.transform, "PromptText", "About the home insurance...", 30, false, TextAlignmentOptions.MidlineLeft);

            var list = CreateUI("ChoiceList", panel.transform);
            AddVerticalLayout(list, 6);

            var template = CreateUI("ChoiceButtonTemplate", list.transform);
            var image = template.AddComponent<UnityEngine.UI.Image>();
            image.color = Color.white;
            var button = template.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color(0.92f, 0.92f, 0.92f);
            colors.selectedColor = Color.white;
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            button.colors = colors;

            var row = template.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            row.padding = new RectOffset(8, 8, 4, 4);
            row.spacing = 12;
            row.childAlignment = TextAnchor.MiddleLeft;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;

            var arrow = CreateText(template.transform, "Arrow", ">", 30, true, TextAlignmentOptions.Center);
            var arrowLayout = arrow.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            arrowLayout.minWidth = 30;
            arrowLayout.preferredWidth = 30;

            var label = CreateText(template.transform, "Label", "Option text", 26, false, TextAlignmentOptions.MidlineLeft);
            label.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().flexibleWidth = 1;

            template.SetActive(false);
            panel.SetActive(false);

            var so = new SerializedObject(manager);
            so.FindProperty("choicePanel").objectReferenceValue = panel;
            so.FindProperty("choicePromptText").objectReferenceValue = prompt;
            so.FindProperty("choiceList").objectReferenceValue = list.GetComponent<RectTransform>();
            so.FindProperty("choiceButtonTemplate").objectReferenceValue = button;
            var nextIndicator = box.Find("NextIndicator");
            so.FindProperty("nextIndicator").objectReferenceValue = nextIndicator != null ? nextIndicator.gameObject : null;
            so.FindProperty("boxRect").objectReferenceValue = box as RectTransform;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateUI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = UILayer };
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void AddVerticalLayout(GameObject go, float spacing)
        {
            var layout = go.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
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
