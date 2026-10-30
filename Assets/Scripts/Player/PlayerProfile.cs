using System;
using UnityEngine;

namespace Player
{
    public static class PlayerProfile
    {
        const string NameKey = "player_name";
        const string MoneyKey = "player_money";

        public static event Action<string> NameChanged;
        public static event Action<int> MoneyChanged;

        public static string Name => PlayerPrefs.GetString(NameKey, "");
        public static int Money => PlayerPrefs.GetInt(MoneyKey, 0);

        public static void SetName(string name)
        {
            name = name == null ? "" : name.Trim();
            PlayerPrefs.SetString(NameKey, name);
            PlayerPrefs.Save();
            NameChanged?.Invoke(name);
        }

        public static void SetMoney(int amount)
        {
            amount = Mathf.Max(0, amount);
            PlayerPrefs.SetInt(MoneyKey, amount);
            PlayerPrefs.Save();
            MoneyChanged?.Invoke(amount);
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
    }
}
