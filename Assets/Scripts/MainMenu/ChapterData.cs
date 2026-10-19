using System.Collections.Generic;
using UnityEngine;

namespace MainMenu
{
    [CreateAssetMenu(fileName = "NewChapter", menuName = "Chapters/Chapter")]
    public class ChapterData : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField] private List<SubChapterData> subChapters = new();

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public IReadOnlyList<SubChapterData> SubChapters => subChapters;
    }
}
