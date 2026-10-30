using TMPro;
using UnityEngine;
using Player;

namespace Room.UI
{
    public class RoomHUD : MonoBehaviour
    {
        [SerializeField] TMP_Text nameText;
        [SerializeField] TMP_Text moneyText;
        [SerializeField] string moneyFormat = "$ {0}";
        [SerializeField] string fallbackName = "Player";

        void OnEnable()
        {
            PlayerProfile.NameChanged += ShowName;
            PlayerProfile.MoneyChanged += ShowMoney;
            ShowName(PlayerProfile.Name);
            ShowMoney(PlayerProfile.Money);
        }

        void OnDisable()
        {
            PlayerProfile.NameChanged -= ShowName;
            PlayerProfile.MoneyChanged -= ShowMoney;
        }

        void ShowName(string playerName)
        {
            if (nameText == null) return;
            nameText.text = string.IsNullOrEmpty(playerName) ? fallbackName : playerName;
        }

        void ShowMoney(int amount)
        {
            if (moneyText == null) return;
            moneyText.text = string.Format(moneyFormat, amount);
        }
    }
}
