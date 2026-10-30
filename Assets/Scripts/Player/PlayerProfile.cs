using UnityEngine;

namespace Player
{
    // The player's chosen name, saved between sessions.
    // Dialogue writes {player} wherever the player's name should appear.
    public static class PlayerProfile
    {
        public const string NameToken = "{player}";
        public const int MaxNameLength = 12;

        // Used when no name has been entered yet, e.g. when testing a scene directly.
        public const string DefaultName = "Jessie";

        private const string NameKey = "player_name";

        public static string Name => PlayerPrefs.GetString(NameKey, "");

        public static bool HasName => !string.IsNullOrWhiteSpace(Name);

        public static void SetName(string name)
        {
            name = (name ?? "").Trim();
            if (name.Length > MaxNameLength) name = name.Substring(0, MaxNameLength);

            PlayerPrefs.SetString(NameKey, name);
            PlayerPrefs.Save();
        }

        public static void ClearName()
        {
            PlayerPrefs.DeleteKey(NameKey);
            PlayerPrefs.Save();
        }

        // Replaces {player} with the player's name.
        public static string FillName(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Replace(NameToken, HasName ? Name : DefaultName);
        }
    }
}
