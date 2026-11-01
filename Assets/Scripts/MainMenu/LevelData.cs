using System.Collections.Generic;
using UnityEngine;

namespace MainMenu
{
    // A level: a name and the chapters inside it.
    [CreateAssetMenu(fileName = "NewLevel", menuName = "Levels/Level")]
    public class LevelData : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField] private List<ChapterData> chapters = new();

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public IReadOnlyList<ChapterData> Chapters => chapters;
    }
}
