using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearnMore
{
    public class LearnMoreEntryView : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private GameObject newTag;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text body;
        [SerializeField] private Color normalColor = new(0.96f, 0.97f, 0.98f);
        [SerializeField] private Color newColor = new(1f, 0.95f, 0.78f);

        public void Show(LearnMoreBook.Entry entry, bool isNew)
        {
            if (background != null) background.color = isNew ? newColor : normalColor;
            if (newTag != null) newTag.SetActive(isNew);
            if (title != null) title.text = entry.title;
            if (body != null) body.text = entry.body;
        }
    }
}
