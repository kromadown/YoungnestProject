using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearnMore
{
    public class LearnMoreTab : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text label;
        [SerializeField] private GameObject newDot;
        [SerializeField] private Color selectedColor = new(0.1843f, 0.2824f, 0.3451f);
        [SerializeField] private Color normalColor = new(0.93f, 0.94f, 0.95f);
        [SerializeField] private Color selectedTextColor = Color.white;
        [SerializeField] private Color normalTextColor = new(0.1843f, 0.2824f, 0.3451f);

        public void Setup(string title, bool hasNew, Action onClick)
        {
            if (label != null) label.text = title;
            if (newDot != null) newDot.SetActive(hasNew);
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onClick());
            }
        }

        public void SetNew(bool hasNew)
        {
            if (newDot != null) newDot.SetActive(hasNew);
        }

        public void SetSelected(bool selected)
        {
            if (background != null) background.color = selected ? selectedColor : normalColor;
            if (label != null) label.color = selected ? selectedTextColor : normalTextColor;
        }
    }
}
