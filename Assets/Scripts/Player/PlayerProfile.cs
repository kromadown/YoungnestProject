using System;
using UnityEngine;

namespace Player
{
    // The player's chosen name, saved between sessions.
    // Dialogue writes {player} wherever the player's name should appear.
    public static class PlayerProfile
    {
        public const string NameToken = "{player}";
        public const int MaxNameLength = 12;

        // Used when no name has been entered yet (the name entry is off, or a scene is tested directly).
        public const string DefaultName = "You";

        private const string NameKey = "player_name";
        private const string MoneyKey = "player_money";
        private const string CharacterKey = "player_character";

        public static event Action<string> NameChanged;

        // Which character the player picked on the character select screen (0 = first in the list).
        public static int CharacterIndex => PlayerPrefs.GetInt(CharacterKey, 0);

        // False until the player has picked a character (e.g. right after New Game).
        public static bool HasCharacter => PlayerPrefs.HasKey(CharacterKey);

        public static void SetCharacter(int index)
        {
            PlayerPrefs.SetInt(CharacterKey, Mathf.Max(0, index));
            PlayerPrefs.Save();
        }

        public static void ClearCharacter()
        {
            PlayerPrefs.DeleteKey(CharacterKey);
            PlayerPrefs.Save();
        }
        public static event Action<int> MoneyChanged;

        public static string Name => PlayerPrefs.GetString(NameKey, "");
        public static int Money => PlayerPrefs.GetInt(MoneyKey, 0);

        public static bool HasName => !string.IsNullOrWhiteSpace(Name);

        public static void SetName(string name)
        {
            name = (name ?? "").Trim();
            if (name.Length > MaxNameLength) name = name.Substring(0, MaxNameLength);

            PlayerPrefs.SetString(NameKey, name);
            PlayerPrefs.Save();
            NameChanged?.Invoke(name);
        }

        public static void ClearName()
        {
            PlayerPrefs.DeleteKey(NameKey);
            PlayerPrefs.Save();
            NameChanged?.Invoke("");
        }

        public static void SetMoney(int amount)
        {
            amount = Mathf.Max(0, amount);
            PlayerPrefs.SetInt(MoneyKey, amount);
            PlayerPrefs.Save();
            MoneyChanged?.Invoke(amount);
        }

        public static void ClearMoney()
        {
            PlayerPrefs.DeleteKey(MoneyKey);
            PlayerPrefs.Save();
            MoneyChanged?.Invoke(0);
        }

        public static void AddMoney(int amount)
        {
            SetMoney(Money + amount);
        }

        public static bool TrySpend(int amount)
        {
            if (amount < 0 || amount > Money) return false;
            SetMoney(Money - amount);
            return true;
        }

        // Replaces {player} with the player's name.
        public static string FillName(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Replace(NameToken, HasName ? Name : DefaultName);
        }
    }
}
