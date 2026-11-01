using System.Collections.Generic;
using Player;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MainMenu.Editor
{
    // Adds anything the main menu is missing and wires it up, leaving existing UI and styling alone:
    // the name entry panel, the Continue button and the New Game confirmation.
    // Open MainMenu.unity and run Tools > Youngnest > Update Main Menu. Safe to run again.
    public static class MainMenuUpdater
    {
        private const string Title = "Update Main Menu";
        private const int UILayer = 5;
        private static readonly Color Black = new(0.07f, 0.07f, 0.07f);
        private static readonly Color Grey = new(0.35f, 0.35f, 0.35f);
        private static readonly Color ErrorRed = new(0.64f, 0.18f, 0.18f);
        private static readonly Color ButtonColor = new(0.91f, 0.91f, 0.91f);

        [MenuItem("Tools/Youngnest/Update Main Menu")]
        public static void Run()
        {
            var scene = EditorSceneManager.GetActiveScene();
            var menu = Object.FindAnyObjectByType<MainMenuController>(FindObjectsInactive.Include);
            if (!scene.path.EndsWith("MainMenu.unity") || menu == null)
            {
                EditorUtility.DisplayDialog(Title, "Open Assets/Scenes/MainMenu.unity first.", "OK");
                return;
            }

            var menuSo = new SerializedObject(menu);
            var newGame = menuSo.FindProperty("newGameButton").objectReferenceValue as UnityEngine.UI.Button;
            if (newGame == null)
            {
                EditorUtility.DisplayDialog(Title, "The Main Menu Controller has no New Game Button assigned.", "OK");
                return;
            }

            var added = new List<string>();
            var canvas = menu.transform;

            if (menuSo.FindProperty("nameEntry").objectReferenceValue == null || canvas.Find("NamePanel") == null)
            {
                AddNameEntry(canvas, menuSo);
                added.Add("name entry");
            }

            if (menuSo.FindProperty("continueButton").objectReferenceValue == null)
            {
                AddContinueButton(newGame, menuSo);
                added.Add("Continue button");
            }

            if (menuSo.FindProperty("newGameConfirmPanel").objectReferenceValue == null)
            {
                AddNewGameConfirm(canvas, newGame, menuSo);
                added.Add("New Game confirmation");
            }

            menuSo.ApplyModifiedPropertiesWithoutUndo();

            if (added.Count == 0)
            {
                EditorUtility.DisplayDialog(Title, "The main menu is already up to date.", "OK");
                return;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Main menu updated ({string.Join(", ", added)}) and scene saved.");
        }

        // "What's your name?" panel, shown the first time New Game is pressed.
        private static void AddNameEntry(Transform canvas, SerializedObject menuSo)
        {
            var old = canvas.Find("NamePanel");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            foreach (var oldScreen in canvas.GetComponents<NameEntryScreen>()) Undo.DestroyObjectImmediate(oldScreen);

            var panel = CreateUI("NamePanel", canvas);
            Stretch(panel);
            var mainPanel = canvas.Find("MainPanel");
            if (mainPanel != null) panel.transform.SetSiblingIndex(mainPanel.GetSiblingIndex() + 1);
            Undo.RegisterCreatedObjectUndo(panel, Title);
            AddVerticalLayout(panel, 16);

            var question = CreateText(panel.transform, "QuestionText", "What's your name?", 48, true, Black);
            SetSize(question.gameObject, 800, 70);

            var inputGo = TMP_DefaultControls.CreateInputField(new TMP_DefaultControls.Resources());
            inputGo.name = "NameInput";
            inputGo.transform.SetParent(panel.transform, false);
            foreach (var child in inputGo.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = UILayer;
            SetSize(inputGo, 480, 72);
            var input = inputGo.GetComponent<TMP_InputField>();
            input.pointSize = 32;
            input.characterLimit = PlayerProfile.MaxNameLength;
            input.textComponent.alignment = TextAlignmentOptions.Center;
            if (input.placeholder is TMP_Text placeholder)
            {
                placeholder.text = PlayerProfile.DefaultName;
                placeholder.alignment = TextAlignmentOptions.Center;
            }

            var error = CreateText(panel.transform, "ErrorText", "Enter a name first.", 24, false, ErrorRed);
            SetSize(error.gameObject, 600, 32);

            var row = CreateButtonRow(panel.transform);
            var back = CreateButton(row, "BackButton", "Back");
            var cont = CreateButton(row, "ContinueButton", "Continue");

            var hint = CreateText(panel.transform, "HintText", $"Up to {PlayerProfile.MaxNameLength} letters.", 22, false, Grey);
            SetSize(hint.gameObject, 600, 32);

            var screen = Undo.AddComponent<NameEntryScreen>(canvas.gameObject);
            var so = new SerializedObject(screen);
            so.FindProperty("namePanel").objectReferenceValue = panel;
            so.FindProperty("nameInput").objectReferenceValue = input;
            so.FindProperty("errorMessage").objectReferenceValue = error.gameObject;
            so.FindProperty("backButton").objectReferenceValue = back;
            so.FindProperty("continueButton").objectReferenceValue = cont;
            so.ApplyModifiedPropertiesWithoutUndo();

            menuSo.FindProperty("nameEntry").objectReferenceValue = screen;

            error.gameObject.SetActive(false);
            panel.SetActive(false);
        }

        // A copy of the New Game button (so it keeps your styling), placed above it.
        private static void AddContinueButton(UnityEngine.UI.Button newGame, SerializedObject menuSo)
        {
            var copy = Object.Instantiate(newGame.gameObject, newGame.transform.parent);
            copy.name = "ContinueButton";
            copy.transform.SetSiblingIndex(newGame.transform.GetSiblingIndex());
            Undo.RegisterCreatedObjectUndo(copy, Title);
            SetButtonLabel(copy, "Continue");

            menuSo.FindProperty("continueButton").objectReferenceValue = copy.GetComponent<UnityEngine.UI.Button>();
        }

        // "Start a new game?" popup shown before New Game deletes a save.
        private static void AddNewGameConfirm(Transform canvas, UnityEngine.UI.Button newGame, SerializedObject menuSo)
        {
            var overlay = CreateUI("NewGameConfirmPanel", canvas);
            Stretch(overlay);
            overlay.transform.SetAsLastSibling();
            Undo.RegisterCreatedObjectUndo(overlay, Title);
            var dim = overlay.AddComponent<UnityEngine.UI.Image>();
            dim.color = new Color(0, 0, 0, 0.5f);

            var dialog = CreateUI("Dialog", overlay.transform);
            SetSize(dialog, 800, 340);
            dialog.AddComponent<UnityEngine.UI.Image>().color = Color.white;
            var layout = AddVerticalLayout(dialog, 30);
            layout.padding = new RectOffset(40, 40, 40, 40);

            var message = CreateText(dialog.transform, "MessageText",
                "Start a new game?\nYour name, chapter progress and choices will be deleted.", 30, false, Black);
            SetSize(message.gameObject, 700, 120);

            var row = CreateButtonRow(dialog.transform);
            var delete = CloneButton(newGame, row, "DeleteButton", "Delete and start");
            var cancel = CloneButton(newGame, row, "CancelButton", "Cancel");

            menuSo.FindProperty("newGameConfirmPanel").objectReferenceValue = overlay;
            menuSo.FindProperty("confirmDeleteButton").objectReferenceValue = delete;
            menuSo.FindProperty("confirmCancelButton").objectReferenceValue = cancel;

            overlay.SetActive(false);
        }

        private static UnityEngine.UI.Button CloneButton(UnityEngine.UI.Button source, Transform parent, string name, string label)
        {
            var copy = Object.Instantiate(source.gameObject, parent);
            copy.name = name;
            SetSize(copy, 300, 60);
            SetButtonLabel(copy, label);
            return copy.GetComponent<UnityEngine.UI.Button>();
        }

        private static void SetButtonLabel(GameObject button, string label)
        {
            var text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null) text.text = label;
        }

        private static Transform CreateButtonRow(Transform parent)
        {
            var row = CreateUI("ButtonRow", parent);
            SetSize(row, 640, 60);
            var rowLayout = row.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            rowLayout.spacing = 24;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = false;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;
            return row.transform;
        }

        private static GameObject CreateUI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = UILayer };
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void Stretch(GameObject go)
        {
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetSize(GameObject go, float width, float height)
        {
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        }

        private static UnityEngine.UI.VerticalLayoutGroup AddVerticalLayout(GameObject go, float spacing)
        {
            var layout = go.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = spacing;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return layout;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size, bool bold, Color color)
        {
            var go = CreateUI(name, parent);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static UnityEngine.UI.Button CreateButton(Transform parent, string name, string label)
        {
            var go = CreateUI(name, parent);
            SetSize(go, 200, 60);
            var image = go.AddComponent<UnityEngine.UI.Image>();
            image.color = ButtonColor;
            var button = go.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image;

            var text = CreateText(go.transform, "Text", label, 28, false, Black);
            Stretch(text.gameObject);
            return button;
        }
    }
}
