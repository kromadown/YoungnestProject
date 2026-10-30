using System.Collections.Generic;
using UnityEngine;

namespace MainMenu
{
    // Saves which chapters are finished, decides what's unlocked,
    // and remembers which chapter the player picked.
    public static class ChapterProgress
    {
        private const string CompletedKeyPrefix = "chapter_completed_";
        private const string CompletedListKey = "chapter_completed_ids";
        private const string StartedKey = "chapter_progress_started";

        // Set by the level select before the chapter's scene loads.
        public static ChapterData CurrentChapter { get; private set; }

        // True once the player has started playing any chapter in this save.
        public static bool HasStarted => PlayerPrefs.GetInt(StartedKey, 0) == 1;

        // Call this when a chapter is about to be played.
        public static void StartChapter(ChapterData chapter)
        {
            CurrentChapter = chapter;
            PlayerPrefs.SetInt(StartedKey, 1);
            PlayerPrefs.Save();
        }

        public static bool IsCompleted(ChapterData chapter)
        {
            return chapter != null && PlayerPrefs.GetInt(CompletedKeyPrefix + chapter.Id, 0) == 1;
        }

        // Call this from the game when the player finishes a chapter.
        public static void MarkCompleted(ChapterData chapter)
        {
            if (chapter == null) return;

            PlayerPrefs.SetInt(CompletedKeyPrefix + chapter.Id, 1);

            var ids = LoadCompletedIds();
            if (!ids.Contains(chapter.Id))
            {
                ids.Add(chapter.Id);
                PlayerPrefs.SetString(CompletedListKey, string.Join("|", ids));
            }

            PlayerPrefs.Save();
        }

        public static void MarkCurrentCompleted()
        {
            MarkCompleted(CurrentChapter);
        }

        // Forgets all progress, e.g. when starting a new game.
        public static void ClearAll()
        {
            foreach (var id in LoadCompletedIds())
            {
                PlayerPrefs.DeleteKey(CompletedKeyPrefix + id);
            }

            PlayerPrefs.DeleteKey(CompletedListKey);
            PlayerPrefs.DeleteKey(StartedKey);
            PlayerPrefs.Save();
            CurrentChapter = null;
        }

        // A chapter is unlocked if it's marked as unlocked by default
        // or the chapter before it (across all levels) is completed.
        public static bool IsChapterUnlocked(ChapterData chapter, IReadOnlyList<LevelData> levels)
        {
            if (chapter.UnlockedByDefault) return true;

            ChapterData previous = null;
            foreach (var level in levels)
            {
                if (level == null) continue;

                foreach (var other in level.Chapters)
                {
                    if (other == null) continue;
                    if (other == chapter)
                    {
                        // The very first chapter of the game is always playable.
                        return previous == null || IsCompleted(previous);
                    }
                    previous = other;
                }
            }

            return false;
        }

        // A level is unlocked once any of its chapters is.
        public static bool IsLevelUnlocked(LevelData level, IReadOnlyList<LevelData> levels)
        {
            foreach (var chapter in level.Chapters)
            {
                if (chapter != null && IsChapterUnlocked(chapter, levels)) return true;
            }

            return false;
        }

        public static int CountCompleted(LevelData level)
        {
            var count = 0;
            foreach (var chapter in level.Chapters)
            {
                if (IsCompleted(chapter)) count++;
            }

            return count;
        }

        // The chapter the player is up to: the first one, in order, that isn't finished yet.
        // Returns false before the player has started playing, and once everything is finished.
        public static bool TryFindInProgress(IReadOnlyList<LevelData> levels, out LevelData level, out ChapterData chapter)
        {
            if (HasStarted)
            {
                foreach (var l in levels)
                {
                    if (l == null) continue;

                    foreach (var c in l.Chapters)
                    {
                        if (c == null || IsCompleted(c)) continue;

                        level = l;
                        chapter = c;
                        return true;
                    }
                }
            }

            level = null;
            chapter = null;
            return false;
        }

        private static List<string> LoadCompletedIds()
        {
            var saved = PlayerPrefs.GetString(CompletedListKey, "");
            return saved.Length == 0 ? new List<string>() : new List<string>(saved.Split('|'));
        }
    }
}
