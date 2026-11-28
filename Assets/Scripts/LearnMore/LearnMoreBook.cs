using System;
using System.Collections.Generic;
using UnityEngine;

namespace LearnMore
{
    [CreateAssetMenu(fileName = "LearnMoreBook", menuName = "Youngnest/Learn More Book")]
    public class LearnMoreBook : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            [Min(1)] public int chapter = 1;
            public string title;
            [TextArea(3, 12)] public string body;
        }

        [Serializable]
        public class Page
        {
            public string title;
            [TextArea(2, 4)] public string subtitle;
            public List<Entry> entries = new();

            public bool HasEntriesFor(int chapter)
            {
                return entries.Exists(e => e.chapter == chapter);
            }
        }

        [SerializeField] private List<Page> pages = new();

        public IReadOnlyList<Page> Pages => pages;

        public bool HasEntriesFor(int chapter)
        {
            return pages.Exists(p => p.HasEntriesFor(chapter));
        }
    }
}
