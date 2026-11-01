using System;
using System.Collections.Generic;
using UnityEngine;

namespace Story
{
    // Remembers the choices the player made, e.g. "insurance" = "skipped".
    // Saved with PlayerPrefs, so choices carry over between scenes and sessions.
    public static class StoryChoices
    {
        private const string KeyPrefix = "story_choice_";
        private const string KeyListKey = "story_choice_keys";

        // Raised when a choice is remembered: (key, value).
        public static event Action<string, string> Changed;

        public static string Get(string key)
        {
            return string.IsNullOrEmpty(key) ? "" : PlayerPrefs.GetString(KeyPrefix + key, "");
        }

        public static bool Is(string key, string value)
        {
            return !string.IsNullOrEmpty(key) && Get(key) == value;
        }

        public static void Set(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) return;

            PlayerPrefs.SetString(KeyPrefix + key, value ?? "");

            var keys = LoadKeys();
            if (!keys.Contains(key))
            {
                keys.Add(key);
                PlayerPrefs.SetString(KeyListKey, string.Join("|", keys));
            }

            PlayerPrefs.Save();
            Changed?.Invoke(key, value ?? "");
        }

        // Forgets every remembered choice, e.g. when starting a new game.
        public static void ClearAll()
        {
            foreach (var key in LoadKeys())
            {
                PlayerPrefs.DeleteKey(KeyPrefix + key);
            }

            PlayerPrefs.DeleteKey(KeyListKey);
            PlayerPrefs.Save();
        }

        private static List<string> LoadKeys()
        {
            var saved = PlayerPrefs.GetString(KeyListKey, "");
            return saved.Length == 0 ? new List<string>() : new List<string>(saved.Split('|'));
        }
    }
}
