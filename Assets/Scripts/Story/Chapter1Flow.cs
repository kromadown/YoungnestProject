using System.Collections;
using Dialogue;
using Phone.Messages;
using Player;
using Room.Interaction;
using UnityEngine;

namespace Story
{
    public class Chapter1Flow : MonoBehaviour
    {
        private const string InsuranceKey = "insurance";
        private const string Bought = "bought";

        [Header("1.1 A Message from Mom")]
        [SerializeField, Min(0)] private float startDelay = 1f;
        [SerializeField] private DialogueData arrival;
        [SerializeField] private MessageManager messageManager;
        [SerializeField] private PhoneMessageSequence momMessage;
        [SerializeField] private string momMessageCue = "mom_message";
        [SerializeField] private DialogueData momReply;
        [SerializeField, Min(0)] private int momAllowance = 15000;
        [SerializeField] private DialogueData insuranceChoice;
        [SerializeField, Min(0)] private int insurancePrice = 1500;

        [Header("1.2 A Friend Arrives with Coffee")]
        [SerializeField, Min(0)] private float knockDelay = 3f;
        [SerializeField] private DialogueData knock;
        [SerializeField] private Interactable door;
        [SerializeField] private SpriteRenderer doorRenderer;
        [SerializeField] private Sprite doorOpenSprite;
        [SerializeField] private GameObject friend;
        [SerializeField] private DialogueData friendArrives;

        [Header("1.3 The Vlog Camera Turns On")]
        [SerializeField] private DialogueData lookForCamera;
        [SerializeField] private Interactable vlogCamera;
        [SerializeField] private DialogueData chat;

        [Header("1.4 The Coffee Spills on the Laptop")]
        [SerializeField] private DialogueData spill;
        [SerializeField] private string spillCue = "coffee_spill";
        [SerializeField] private GameObject spilledCoffee;
        [SerializeField] private SpriteRenderer laptopRenderer;
        [SerializeField] private Sprite brokenLaptopSprite;
        [SerializeField] private DialogueData insuredOutcome;
        [SerializeField] private DialogueData uninsuredOutcome;

        [Header("1.5 The Reveal")]
        [SerializeField] private DialogueData insuredEnding;
        [SerializeField] private DialogueData uninsuredEnding;

        private bool _insured;
        private bool _momMessageSent;

        private void Start()
        {
            StartCoroutine(Run());
        }

        private void OnDisable()
        {
            DialogueManager.CueFired -= OnCue;
        }

        private IEnumerator Run()
        {
            yield return new WaitForSeconds(startDelay);
            DialogueManager.CueFired += OnCue;
            yield return Play(arrival);
            DialogueManager.CueFired -= OnCue;

            if (messageManager != null && momMessage != null)
            {
                SendMomMessage();
                yield return new WaitUntil(MomMessageRead);
            }

            yield return Play(momReply);
            PlayerProfile.AddMoney(momAllowance);

            yield return AskInsurance();

            yield return new WaitForSeconds(knockDelay);
            yield return Play(knock);
            yield return WaitForClick(door);
            if (doorRenderer != null && doorOpenSprite != null) doorRenderer.sprite = doorOpenSprite;
            if (friend != null) friend.SetActive(true);
            yield return Play(friendArrives);

            yield return Play(lookForCamera);
            yield return WaitForClick(vlogCamera);
            StoryChoices.Set("vlog_recording", "true");
            yield return Play(chat);

            DialogueManager.CueFired += OnCue;
            yield return Play(spill);
            DialogueManager.CueFired -= OnCue;
            BreakLaptop();

            yield return Play(_insured ? insuredOutcome : uninsuredOutcome);
            yield return Play(_insured ? insuredEnding : uninsuredEnding);

            StoryChoices.Set("chapter1", "done");

            // Counts this chapter as finished (Level Select unlocks the next one) and shows the Chapter Complete screen.
            ChapterCompleteScreen.Finish();
        }

        private IEnumerator AskInsurance()
        {
            var manager = DialogueManager.Instance;
            if (insuranceChoice == null || manager == null) yield break;

            string picked = null;
            void OnChoice(DialogueChoice choice)
            {
                if (choice.rememberKey == InsuranceKey) picked = choice.rememberValue;
            }

            manager.ChoiceMade += OnChoice;
            while (picked == null) yield return Play(insuranceChoice);
            manager.ChoiceMade -= OnChoice;

            _insured = picked == Bought;
            if (_insured && !PlayerProfile.TrySpend(insurancePrice))
                Debug.LogWarning($"Bought insurance but the wallet has only {PlayerProfile.Money}.", this);
        }

        private bool MomMessageRead()
        {
            var sender = momMessage.Sender;
            return messageManager.GetConversation(sender).Count >= momMessage.Messages.Count &&
                   messageManager.GetUnreadCount(sender) == 0;
        }

        private void OnCue(string cue)
        {
            if (cue == momMessageCue) SendMomMessage();
            else if (cue == spillCue) BreakLaptop();
        }

        private void SendMomMessage()
        {
            if (_momMessageSent || messageManager == null || momMessage == null) return;

            _momMessageSent = true;
            messageManager.DeliverSequence(momMessage);
        }

        private void BreakLaptop()
        {
            if (spilledCoffee != null) spilledCoffee.SetActive(true);
            if (laptopRenderer != null && brokenLaptopSprite != null) laptopRenderer.sprite = brokenLaptopSprite;
        }

        private static IEnumerator Play(DialogueData dialogue)
        {
            var manager = DialogueManager.Instance;
            if (dialogue == null || manager == null) yield break;

            bool done = false;
            manager.Play(dialogue, () => done = true);
            yield return new WaitUntil(() => done);
        }

        private static IEnumerator WaitForClick(Interactable target)
        {
            if (target == null)
            {
                yield return new WaitForSeconds(1f);
                yield break;
            }

            bool clicked = false;
            void OnClicked() => clicked = true;

            target.Clicked += OnClicked;
            yield return new WaitUntil(() => clicked);
            target.Clicked -= OnClicked;
        }
    }
}
