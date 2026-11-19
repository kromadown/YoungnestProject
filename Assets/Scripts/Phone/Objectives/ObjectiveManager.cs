using System;
using System.Collections.Generic;
using MainMenu;
using UnityEngine;

namespace Phone.Objectives
{
    public enum ObjectiveStatus
    {
        Inactive,
        Active,
        Completed
    }

    public class ObjectiveProgress
    {
        public ObjectiveData Definition { get; }
        public ObjectiveStatus Status { get; private set; }

        public ObjectiveProgress(ObjectiveData definition, ObjectiveStatus status)
        {
            Definition = definition;
            Status = status;
        }

        public void SetStatus(ObjectiveStatus status)
        {
            Status = status;
        }
    }
    
    public class ObjectiveManager : MonoBehaviour
    {
        [Header("Chapter testing")]
        [Tooltip("Only used for testing in the editor.")]
        [SerializeField] private ChapterData chapterForTesting;

        public event Action ObjectivesChanged;
        
        private readonly List<ObjectiveProgress> _objectives = new();
        
        public IReadOnlyList<ObjectiveProgress> Objectives => _objectives;
        public ChapterData CurrentChapter { get; private set; }

        private void Awake()
        {
            var chapter = 
                ChapterProgress.CurrentChapter != null
                ? ChapterProgress.CurrentChapter
                : chapterForTesting;

            if (chapter != null)
            {
                LoadChapter(chapter);
            }
            else
            {
                Debug.LogWarning("No chapter is assigned in the ObjectiveManager", this);
            }
        }

        public void LoadChapter(ChapterData chapter)
        {
            if (chapter == null)
            {
                Debug.LogWarning("Chapter is null", this);
                return;
            }
            
            CurrentChapter = chapter;
            _objectives.Clear();

            foreach (var entry in chapter.Objectives)
            {
                if (entry == null || entry.Objective == null) continue;

                if (FindObjective(entry.Objective) != null)
                {
                    Debug.LogWarning($"Duplicate objectives: {entry.Objective.name}", this); 
                    continue;
                }
                
                var status = 
                    entry.ActiveAtStart 
                    ? ObjectiveStatus.Active
                    : ObjectiveStatus.Inactive;
                
                _objectives.Add(new ObjectiveProgress(entry.Objective, status));
            }
            
            ObjectivesChanged?.Invoke();
            
            Debug.Log($"Started objectives chapter {chapter.name}" + $"Loaded {_objectives.Count} objectives");
        }

        public bool ActivateObjective(ObjectiveData objective)
        {
            return TryChangeStatus(objective, ObjectiveStatus.Inactive, ObjectiveStatus.Active);
        }

        public bool CompleteObjective(ObjectiveData objective)
        {
            return TryChangeStatus(objective, ObjectiveStatus.Active, ObjectiveStatus.Completed);
        }

        public bool IsCompleted(ObjectiveData objective)
        {
            var progress = FindObjective(objective);
            return progress != null && progress.Status == ObjectiveStatus.Completed;
        }

        private bool TryChangeStatus(
            ObjectiveData objective,
            ObjectiveStatus requiredStatus,
            ObjectiveStatus newStatus)
        {
            var progress = FindObjective(objective);

            if (progress == null)
            {
                Debug.LogWarning(
                    "Objective is not part of the current chapter: " + 
                    $"{objective?.name}", this);
                
                return false;
            }

            if (progress.Status != requiredStatus) return false;
            
            progress.SetStatus(newStatus);
            
            ObjectivesChanged?.Invoke();
            
            Debug.Log($"Objective '{objective.Title}' changed to {newStatus}");
            return true;
        }

        private ObjectiveProgress FindObjective(ObjectiveData objective)
        {
            foreach (var progress in _objectives)
            {
                if (progress.Definition == objective) return progress;
            }
            
            return null;
        }
    }
}
