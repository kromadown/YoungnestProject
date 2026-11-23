using System;
using System.Collections.Generic;
using Dialogue;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace Phone.Messages
{
    [RequireComponent(typeof(UIDocument))]
    public class PhoneMessagesUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MessageManager messageManager;
        [SerializeField] private VisualTreeAsset contactRowTemplate;
        [SerializeField] private VisualTreeAsset messageBubbleTemplate;
        [SerializeField] private PhoneDialogueController phoneDialogueController;
        
        private VisualElement _homeScreen;
        private VisualElement _messagesScreen;
        private VisualElement _replyArea;

        private Button _messagesButton;
        private Button _backButton;
        private Label _headerTitle;

        private Label _homeUnreadBadge;

        private ScrollView _contactsList;
        private ScrollView _conversationList;
        private ScrollView _replyList;

        private PhoneContact _currentContact;
        private bool _isMessagesOpen;

        private void OnEnable()
        {
            if (!TryAssignReferences())
            {
                enabled = false;
                return;
            }

            SubscribeEvents();

            UpdateHomeUnreadBadge();
            
            _messagesScreen.style.display = DisplayStyle.None;
        }
        
        private bool TryAssignReferences()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _homeScreen = root.Q<VisualElement>("HomeScreen");
            _messagesScreen = root.Q<VisualElement>("MessagesScreen");
            _replyArea = root.Q<VisualElement>("ReplyArea");

            _messagesButton = root.Q<Button>("MessagesButton");
            _backButton = root.Q<Button>("MessagesBackButton");
            _headerTitle = root.Q<Label>("MessagesHeaderTitle");

            _contactsList = root.Q<ScrollView>("ContactsList");
            _conversationList = root.Q<ScrollView>("ConversationList");
            _replyList = root.Q<ScrollView>("ReplyList");
            
            _homeUnreadBadge = root.Q<Label>("HomeUnreadBadge");
            
            if (messageManager == null ||
                contactRowTemplate == null ||
                messageBubbleTemplate == null ||
                _homeScreen == null ||
                _messagesScreen == null ||
                _messagesButton == null ||
                _backButton == null ||
                _headerTitle == null ||
                _contactsList == null ||
                _conversationList == null ||
                _homeUnreadBadge == null ||
                _replyArea == null ||
                _replyList == null ||
                phoneDialogueController == null)
            {
                Debug.LogError("Phone Messages UI has missing references or UI elements.", this);
                return false;
            }
            
            return true;
        }

        private void SubscribeEvents()
        {
            if (_messagesButton != null)
            {
                _messagesButton.clicked += OpenMessages;
            }

            if (_backButton != null)
            {
                _backButton.clicked += GoBack;
            }

            if (messageManager != null)
            {
                messageManager.ConversationChanged += OnConversationChanged;
            }

            if (phoneDialogueController != null)
            {
                phoneDialogueController.RepliesChanged += OnRepliesChanged;
            }
        }

        private void OnDisable()
        {
            UnSubscribeEvents();
        }

        private void UnSubscribeEvents()
        {
            if (_messagesButton != null)
            {
                _messagesButton.clicked -= OpenMessages;
            }

            if (_backButton != null)
            {
                _backButton.clicked -= GoBack;
            }
            
            if (messageManager != null)
            {
                messageManager.ConversationChanged -= OnConversationChanged;
            }
            
            if (phoneDialogueController != null)
            {
                phoneDialogueController.RepliesChanged -= OnRepliesChanged;
            }
        }

        private void OpenMessages()
        {
            _isMessagesOpen = true;
            
            _homeScreen.style.display = DisplayStyle.None;
            _messagesScreen.style.display = DisplayStyle.Flex;

            ShowContacts();
        }

        private void ShowContacts()
        {
            HideReplyOptions();
            
            _currentContact = null;
            _headerTitle.text = "Messages";
            
            _contactsList.style.display = DisplayStyle.Flex;
            _conversationList.style.display = DisplayStyle.None;

            _contactsList.contentContainer.Clear();

            foreach (var contact in messageManager.Contacts)
            {
                var row = contactRowTemplate.Instantiate();
                
                var button = row.Q<Button>("ContactButton");
                
                var name = row.Q<Label>("ContactName");
                
                var avatarInitial = row.Q<Label>("AvatarInitial");
                avatarInitial.text = string.IsNullOrWhiteSpace(contact.DisplayName)
                    ? "?" : contact.DisplayName.Substring(0, 1).ToUpperInvariant();
                
                var preview = row.Q<Label>("ContactPreview");
                var unreadLabel = row.Q<Label>("UnreadCount");

                var messages = messageManager.GetConversation(contact);
                var unread = messageManager.GetUnreadCount(contact);
                
                name.text = contact.DisplayName;

                preview.text = messages.Count > 0 ? messages[^1].Text : "";

                unreadLabel.text = unread.ToString();
                unreadLabel.style.display = unread > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                
                button.clicked += () => ShowConversation(contact);
                
                _contactsList.contentContainer.Add(row);
            }
        }

        private void ShowConversation(PhoneContact contact)
        {
            HideReplyOptions();
            
            _currentContact = contact;
            _headerTitle.text = contact.DisplayName;
            
            messageManager.MarkAsRead(contact);
            
            UpdateHomeUnreadBadge();
            
            _contactsList.style.display = DisplayStyle.None;
            _conversationList.style.display = DisplayStyle.Flex;

            RenderConversation();
            RefreshReplyOptions();
        }

        private void RenderConversation()
        {
            _conversationList.contentContainer.Clear();

            VisualElement lastBubble = null;
            
            foreach (var message in messageManager.GetConversation(_currentContact))
            {
                var bubble = messageBubbleTemplate.Instantiate();
                
                var messageRow = bubble.Q<VisualElement>("MessageRow");
                messageRow.EnableInClassList("outgoing-message", message.IsFromPlayer);
                
                bubble.Q<Label>("MessageText").text = message.Text;
                
                _conversationList.contentContainer.Add(bubble);
                lastBubble = bubble;
            }

            if (lastBubble == null) return;
            
            var target = lastBubble;
            _conversationList.schedule.Execute(() =>
            {
                if (target.parent != null)
                {
                    _conversationList.ScrollTo(target);
                }
            });
        }
        
        private void OnConversationChanged(PhoneContact sender)
        {
            if (_isMessagesOpen)
            {
                if (_currentContact == sender)
                {
                    messageManager.MarkAsRead(sender);
                    RenderConversation();
                }
                else if (_currentContact == null)
                {
                    ShowContacts();
                }
            }
            
            UpdateHomeUnreadBadge();
        }

        private void GoBack()
        {
            if (_currentContact != null)
            {
                ShowContacts();
                return;
            }

            _isMessagesOpen = false;
            _messagesScreen.style.display = DisplayStyle.None;
            _homeScreen.style.display = DisplayStyle.Flex;
        }

        private void UpdateHomeUnreadBadge()
        {
            var totalUnread = 0;

            foreach (var contact in messageManager.Contacts)
            {
                totalUnread += messageManager.GetUnreadCount(contact);
            }
            
            _homeUnreadBadge.text = totalUnread > 99 ? "99+" : totalUnread.ToString();
            
            _homeUnreadBadge.style.display = totalUnread > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        private void OnRepliesChanged(PhoneContact contact)
        {
            if (_isMessagesOpen && _currentContact == contact)
            {
                RefreshReplyOptions();
            }
        }

        private void RefreshReplyOptions()
        {
            if (_currentContact == null)
            {
                HideReplyOptions();
                return;
            }
            
            var contact =  _currentContact;
            
            ShowReplyOptions(
                phoneDialogueController.GetReplies(contact),
                choice => phoneDialogueController.SelectReply(contact, choice)
                );
        }
        
        public void ShowReplyOptions(
            IReadOnlyList<DialogueChoice> choices, 
            Action<DialogueChoice> onSelected)
        {
            _replyList.contentContainer.Clear();

            if (choices == null || choices.Count == 0)
            {
                HideReplyOptions();
                return;
            }

            foreach (var choice in choices)
            {
                if (string.IsNullOrWhiteSpace(choice.text)) continue;

                var selectedChoice = choice;

                var button = new Button(() => onSelected?.Invoke(selectedChoice))
                {
                    text = selectedChoice.text
                };
                
                button.AddToClassList("reply-choice");
                
                _replyList.contentContainer.Add(button);
            }
            
            _replyArea.style.display = 
                _replyList.contentContainer.childCount > 0 
                ? DisplayStyle.Flex 
                : DisplayStyle.None;
        }

        private void HideReplyOptions()
        {
            _replyArea.style.display = DisplayStyle.None;
            _replyList.contentContainer.Clear();
        }
    }
}