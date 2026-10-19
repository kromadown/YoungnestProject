using UnityEngine;

namespace MainMenu
{
    // Saves which subchapters are finished and remembers which one the player picked.
    public static class ChapterProgress
    {
        private const string CompletedKeyPrefix = "subchapter_completed_";

        // Set by the chapter select before the game scene loads.
        public static SubChapterData CurrentSubChapter { get; set; }

        public static bool IsCompleted(SubChapterData subChapter)
        {
            return subChapter != null && PlayerPrefs.GetInt(CompletedKeyPrefix + subChapter.Id, 0) == 1;
        }

        // Call this from the game when the player finishes a subchapter.
        public static void MarkCompleted(SubChapterData subChapter)
        {
            if (subChapter == null) return;

            PlayerPrefs.SetInt(CompletedKeyPrefix + subChapter.Id, 1);
            PlayerPrefs.Save();
        }

        public static void MarkCurrentCompleted()
        {
            MarkCompleted(CurrentSubChapter);
        }
    }
}
