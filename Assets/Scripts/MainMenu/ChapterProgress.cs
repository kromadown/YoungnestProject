using System.Collections.Generic;
using UnityEngine;

namespace MainMenu
{
    // Saves which subchapters are finished, decides what's unlocked,
    // and remembers which one the player picked.
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

        // A subchapter is unlocked if it's marked as unlocked by default
        // or the subchapter before it (across all chapters) is completed.
        public static bool IsSubChapterUnlocked(SubChapterData subChapter, IReadOnlyList<ChapterData> chapters)
        {
            if (subChapter.UnlockedByDefault) return true;

            SubChapterData previous = null;
            foreach (var chapter in chapters)
            {
                if (chapter == null) continue;

                foreach (var other in chapter.SubChapters)
                {
                    if (other == null) continue;
                    if (other == subChapter)
                    {
                        // The very first subchapter of the game is always playable.
                        return previous == null || IsCompleted(previous);
                    }
                    previous = other;
                }
            }

            return false;
        }

        // A chapter is unlocked once any of its subchapters is.
        public static bool IsChapterUnlocked(ChapterData chapter, IReadOnlyList<ChapterData> chapters)
        {
            foreach (var subChapter in chapter.SubChapters)
            {
                if (subChapter != null && IsSubChapterUnlocked(subChapter, chapters)) return true;
            }

            return false;
        }

        public static int CountCompleted(ChapterData chapter)
        {
            var count = 0;
            foreach (var subChapter in chapter.SubChapters)
            {
                if (IsCompleted(subChapter)) count++;
            }

            return count;
        }
    }
}
