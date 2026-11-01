using System.Collections.Generic;
using UnityEngine;

namespace MainMenu
{
    // A chapter inside a level: the scene it plays and what the preview shows.
    [CreateAssetMenu(fileName = "NewChapter", menuName = "Levels/Chapter")]
    public class ChapterData : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private string sceneName;
        [SerializeField] private Sprite screenshot;
        [SerializeField, TextArea(3, 6)] private string description;
        [SerializeField] private List<string> learningGoals = new();
        [SerializeField] private bool unlockedByDefault;

        public string Id => string.IsNullOrEmpty(id) ? name : id;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public string SceneName => sceneName;
        public Sprite Screenshot => screenshot;
        public string Description => description;
        public IReadOnlyList<string> LearningGoals => learningGoals;
        public bool UnlockedByDefault => unlockedByDefault;
    }
}
