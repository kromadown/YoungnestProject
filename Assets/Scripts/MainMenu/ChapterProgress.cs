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
        private const string StartedListKey = "chapter_started_ids";

        // Set by the level select before the chapter's scene loads.
        public static ChapterData CurrentChapter { get; private set; }

        // All levels, in order, as the level select knows them. Used to find the next chapter.
        public static IReadOnlyList<LevelData> CurrentLevels { get; private set; }

        // True once the player has started playing any chapter in this save.
        public static bool HasStarted => PlayerPrefs.GetInt(StartedKey, 0) == 1;

        // Call this when a chapter is about to be played.
        public static void StartChapter(ChapterData chapter, IReadOnlyList<LevelData> levels = null)
        {
            CurrentChapter = chapter;
            if (levels != null) CurrentLevels = levels;
            PlayerPrefs.SetInt(StartedKey, 1);

            if (chapter != null)
            {
                var ids = LoadIds(StartedListKey);
                if (!ids.Contains(chapter.Id))
                {
                    ids.Add(chapter.Id);
                    PlayerPrefs.SetString(StartedListKey, string.Join("|", ids));
                }
            }

            PlayerPrefs.Save();
        }

        // True once the player has pressed Play on this chapter at least once in this save.
        public static bool IsStarted(ChapterData chapter)
        {
            return chapter != null && LoadIds(StartedListKey).Contains(chapter.Id);
        }

        // The chapter's number inside its level (1, 2, 3...), or 0 if it isn't in any level.
        public static int GetChapterNumber(ChapterData chapter, IReadOnlyList<LevelData> levels)
        {
            if (chapter == null || levels == null) return 0;

            foreach (var level in levels)
            {
                if (level == null) continue;

                for (var i = 0; i < level.Chapters.Count; i++)
                {
                    if (level.Chapters[i] == chapter) return i + 1;
                }
            }

            return 0;
        }

        // The chapter that comes after this one, across levels, or null if it's the last.
        public static ChapterData FindNextChapter(ChapterData chapter, IReadOnlyList<LevelData> levels)
        {
            if (chapter == null || levels == null) return null;

            var found = false;
            foreach (var level in levels)
            {
                if (level == null) continue;

                foreach (var other in level.Chapters)
                {
                    if (other == null) continue;
                    if (found) return other;
                    if (other == chapter) found = true;
                }
            }

            return null;
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

            var ids = LoadIds(CompletedListKey);
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
            foreach (var id in LoadIds(CompletedListKey))
            {
                PlayerPrefs.DeleteKey(CompletedKeyPrefix + id);
            }

            PlayerPrefs.DeleteKey(CompletedListKey);
            PlayerPrefs.DeleteKey(StartedListKey);
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

        // A chapter the player started but hasn't finished (the first one, in order). False if there's none.
        public static bool TryFindInProgress(IReadOnlyList<LevelData> levels, out LevelData level, out ChapterData chapter)
        {
            return TryFindFirst(levels, c => IsStarted(c) && !IsCompleted(c), out level, out chapter);
        }

        // The next chapter to play: the first one, in order, that isn't finished. False once everything is finished.
        public static bool TryFindNextToPlay(IReadOnlyList<LevelData> levels, out LevelData level, out ChapterData chapter)
        {
            return TryFindFirst(levels, c => !IsCompleted(c), out level, out chapter);
        }

        private static bool TryFindFirst(IReadOnlyList<LevelData> levels, System.Func<ChapterData, bool> match,
            out LevelData level, out ChapterData chapter)
        {
            foreach (var l in levels)
            {
                if (l == null) continue;

                foreach (var c in l.Chapters)
                {
                    if (c == null || !match(c)) continue;

                    level = l;
                    chapter = c;
                    return true;
                }
            }

            level = null;
            chapter = null;
            return false;
        }

        private static List<string> LoadIds(string listKey)
        {
            var saved = PlayerPrefs.GetString(listKey, "");
            return saved.Length == 0 ? new List<string>() : new List<string>(saved.Split('|'));
        }
    }
}
