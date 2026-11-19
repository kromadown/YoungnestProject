using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Phone.Objectives
{
    [RequireComponent(typeof(UIDocument))]
    public class PhoneNotesUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ObjectiveManager objectiveManager;

        private ScrollView _notesList;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _notesList = root.Q<ScrollView>("NotesList");

            if (objectiveManager == null || _notesList == null)
            {
                Debug.LogError("PhoneNotesUI is missing references", this);
                
                enabled = false;
                return;
            }

            objectiveManager.ObjectivesChanged += RefreshNotes;

            RefreshNotes();
        }

        private void OnDisable()
        {
            if (objectiveManager != null) objectiveManager.ObjectivesChanged -= RefreshNotes;
        }

        private void RefreshNotes()
        {
            _notesList.contentContainer.Clear();

            var visibleCount = 0;

            foreach (var progress in objectiveManager.Objectives)
            {
                if (progress.Status == ObjectiveStatus.Inactive) continue;
                
                AddObjective(progress);
                visibleCount++;
            }

            if (visibleCount == 0)
            {
                AddEmptyMessage();
            }
        }

        private void AddObjective(ObjectiveProgress progress)
        {
            var completed = progress.Status == ObjectiveStatus.Completed;
            var label = new Label
            {
                text = completed ? $"✓ {progress.Definition.Title}" : $"- {progress.Definition.Title}"
            };

            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.marginBottom = 6;
            label.style.flexShrink = 1;
            
            label.style.opacity = completed ? 0.6f : 1f;
            
            _notesList.contentContainer.Add(label);
        }

        private void AddEmptyMessage()
        {
            var label = new Label("No notes yet.")
            {
                style =
                {
                    opacity = 0.6f,
                    whiteSpace = WhiteSpace.Normal
                }
            };

            _notesList.contentContainer.Add(label);
        }
    }
}
