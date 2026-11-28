using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearnMore
{
    public class LearnMorePopup : MonoBehaviour
    {
        private const string SeenKeyPrefix = "learn_more_seen_page_";

        [SerializeField] private LearnMoreBook book;
        [SerializeField, Min(1)] private int currentChapter = 1;
        [SerializeField] private Button openButton;
        [SerializeField] private GameObject newDot;
        [SerializeField] private GameObject popup;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button backdropButton;
        [SerializeField] private RectTransform tabBar;
        [SerializeField] private LearnMoreTab tabTemplate;
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private RectTransform list;
        [SerializeField] private TMP_Text subtitle;
        [SerializeField] private LearnMoreEntryView entryTemplate;

        private readonly List<LearnMoreTab> _tabs = new();
        private readonly List<LearnMoreEntryView> _views = new();
        private int _page;

        public event Action Closed;

        public bool IsOpen => popup != null && popup.activeSelf;
        public int CurrentChapter => currentChapter;

        private void Awake()
        {
            if (openButton != null) openButton.onClick.AddListener(Open);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (backdropButton != null) backdropButton.onClick.AddListener(Close);
            if (tabTemplate != null) tabTemplate.gameObject.SetActive(false);
            if (entryTemplate != null) entryTemplate.gameObject.SetActive(false);
            if (popup != null) popup.SetActive(false);
            RefreshDot();
        }

        public void SetChapter(int chapter)
        {
            currentChapter = Mathf.Max(1, chapter);
            if (IsOpen)
            {
                BuildTabs();
                ShowPage(_page);
            }
            RefreshDot();
        }

        public void Open()
        {
            if (popup == null) return;

            int unseen = FirstUnseenPage();
            popup.SetActive(true);
            BuildTabs();
            ShowPage(unseen >= 0 ? unseen : _page);
        }

        public void Close()
        {
            if (!IsOpen) return;

            popup.SetActive(false);
            Closed?.Invoke();
        }

        public void ResetSeen()
        {
            if (book != null)
                for (int i = 0; i < book.Pages.Count; i++) PlayerPrefs.DeleteKey(SeenKeyPrefix + i);
            RefreshDot();
            for (int i = 0; i < _tabs.Count; i++) _tabs[i].SetNew(IsUnseen(i));
        }

        public void ShowPage(int index)
        {
            if (book == null || book.Pages.Count == 0) return;

            _page = Mathf.Clamp(index, 0, book.Pages.Count - 1);
            MarkSeen(_page);
            for (int i = 0; i < _tabs.Count; i++)
            {
                _tabs[i].SetSelected(i == _page);
                _tabs[i].SetNew(IsUnseen(i));
            }

            var page = book.Pages[_page];
            if (subtitle != null)
            {
                subtitle.text = page.subtitle;
                subtitle.gameObject.SetActive(!string.IsNullOrWhiteSpace(page.subtitle));
            }

            ClearViews();
            if (entryTemplate == null || list == null) return;

            var shown = page.entries
                .Where(e => e.chapter <= currentChapter)
                .OrderBy(e => e.chapter == currentChapter ? 0 : 1)
                .ThenBy(e => e.chapter);

            foreach (var entry in shown)
            {
                var view = Instantiate(entryTemplate, list);
                view.gameObject.SetActive(true);
                view.Show(entry, entry.chapter == currentChapter);
                _views.Add(view);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(list);
            Canvas.ForceUpdateCanvases();
            if (scroll != null) scroll.verticalNormalizedPosition = 1f;
        }

        private void BuildTabs()
        {
            foreach (var tab in _tabs) Destroy(tab.gameObject);
            _tabs.Clear();
            if (book == null || tabTemplate == null || tabBar == null) return;

            for (int i = 0; i < book.Pages.Count; i++)
            {
                int index = i;
                var page = book.Pages[i];
                var tab = Instantiate(tabTemplate, tabBar);
                tab.gameObject.SetActive(true);
                tab.Setup(page.title, IsUnseen(i), () => ShowPage(index));
                _tabs.Add(tab);
            }
        }

        private void ClearViews()
        {
            foreach (var view in _views)
            {
                view.gameObject.SetActive(false);
                Destroy(view.gameObject);
            }
            _views.Clear();
        }

        private bool IsUnseen(int page)
        {
            return book != null && page >= 0 && page < book.Pages.Count &&
                   book.Pages[page].HasEntriesFor(currentChapter) &&
                   PlayerPrefs.GetInt(SeenKeyPrefix + page, 0) < currentChapter;
        }

        private int FirstUnseenPage()
        {
            if (book == null) return -1;

            for (int i = 0; i < book.Pages.Count; i++)
                if (IsUnseen(i)) return i;
            return -1;
        }

        private void MarkSeen(int page)
        {
            if (!IsUnseen(page)) return;

            PlayerPrefs.SetInt(SeenKeyPrefix + page, currentChapter);
            PlayerPrefs.Save();
            RefreshDot();
        }

        private void RefreshDot()
        {
            if (newDot != null) newDot.SetActive(FirstUnseenPage() >= 0);
        }
    }
}
