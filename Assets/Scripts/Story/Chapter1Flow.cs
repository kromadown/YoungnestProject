using System.Collections;
using System.Globalization;
using Dialogue;
using Phone.Messages;
using Phone.Objectives;
using Player;
using Room.Interaction;
using Room.Rendering;
using UnityEngine;

namespace Story
{
    public class Chapter1Flow : MonoBehaviour
    {
        private const string InsuranceKey = "insurance";
        private const string Bought = "bought";
        private const string LearnMoreKey = "insurance_learn";

        [Header("1.1 A Message from Mom")]
        [SerializeField, Min(0)] private float startDelay = 1f;
        [SerializeField] private DialogueData arrival;
        [SerializeField] private MessageManager messageManager;
        [SerializeField] private PhoneMessageSequence momMessage;
        [SerializeField] private string momMessageCue = "mom_message";
        [Tooltip("Seconds to wait after the player has opened Mom's chat (and all her messages are in) before Mom Reply plays, so they can read it.")]
        [SerializeField, Min(0)] private float momReplyDelay = 4f;
        [SerializeField] private DialogueData momReply;
        [SerializeField, Min(0)] private int momAllowance = 15000;
        [SerializeField] private DialogueData insuranceChoice;
        [SerializeField, Min(0)] private int insurancePrice = 1500;
        [SerializeField] private DialogueData insuranceBought;
        [SerializeField] private string learnMoreUrl = "https://jessiehungyolo.github.io/youngnest-docs/home-insurance.html";

        [Header("1.2 A Friend Arrives with Coffee")]
        [SerializeField, Min(0)] private float knockDelay = 3f;
        [SerializeField] private DialogueData knock;
        [SerializeField] private Interactable door;
        [SerializeField] private SpriteRenderer doorRenderer;
        [SerializeField] private Sprite doorOpenSprite;
        [SerializeField, Min(0.1f)] private float knockFocusSize = 3f;
        [SerializeField] private GameObject friend;
        [SerializeField] private DialogueData friendArrives;

        [Header("1.3 The Vlog Camera Turns On")]
        [SerializeField] private DialogueData lookForCamera;
        [SerializeField] private Interactable vlogCamera;
        [SerializeField] private string recordingText = "It's still recording.";
        [SerializeField] private DialogueData chat;

        [Header("1.4 The Coffee Spills on the Laptop")]
        [SerializeField] private DialogueData spill;
        [SerializeField] private string spillCue = "coffee_spill";
        [SerializeField] private GameObject spilledCoffee;
        [SerializeField] private SpriteRenderer laptopRenderer;
        [SerializeField] private Sprite brokenLaptopSprite;
        [SerializeField] private CameraFocus cameraFocus;
        [SerializeField, Min(0.1f)] private float spillFocusSize = 2.5f;
        [SerializeField] private DialogueData insuredOutcome;
        [SerializeField] private DialogueData uninsuredOutcome;
        [Tooltip("Paid from the wallet when the player has no insurance. Shown on the Chapter Complete screen as {laptop_cost}.")]
        [SerializeField, Min(0)] private int laptopRepairCost = 12000;
        [SerializeField] private string friendLeavesCue = "friend_leaves";

        [Header("1.5 The Reveal")]
        [SerializeField] private DialogueData insuredEnding;
        [SerializeField] private DialogueData uninsuredEnding;
        
        [Header("Notes App Objectives")]
        [SerializeField] private ObjectiveManager objectiveManager;
        [SerializeField] private ObjectiveData readTextObjective;
        [SerializeField] private ObjectiveData insuranceObjective;
        [SerializeField] private ObjectiveData doorObjective;
        [SerializeField] private ObjectiveData cameraObjective;
        

        private bool _insured;
        private bool _momMessageSent;
        private Sprite _doorClosedSprite;

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
                ActivateObjective(readTextObjective);
                SendMomMessage();
                yield return new WaitUntil(MomMessageRead);
                
                CompleteObjective(readTextObjective);
                yield return new WaitForSeconds(momReplyDelay);

                yield return Play(momReply, momMessage.Sender);
            }
            
            PlayerProfile.AddMoney(momAllowance);

            ActivateObjective(insuranceObjective);
            yield return AskInsurance();

            yield return new WaitForSeconds(knockDelay);
            FocusOnDoor();
            yield return Play(knock);
            if (cameraFocus != null) cameraFocus.Release();
            
            ActivateObjective(doorObjective);

            if (door != null) door.SetPulse(true);
            yield return WaitForClick(door);

            if (door != null)
            {
                door.SetPulse(false);
                CompleteObjective(doorObjective);
            }
            
            if (doorRenderer != null && doorOpenSprite != null)
            {
                _doorClosedSprite = doorRenderer.sprite;
                doorRenderer.sprite = doorOpenSprite;
            }
            if (friend != null) friend.SetActive(true);
            yield return Play(friendArrives);

            yield return Play(lookForCamera);
            
            ActivateObjective(cameraObjective);
            
            if (vlogCamera != null) vlogCamera.SetPulse(true);
            yield return WaitForClick(vlogCamera);
            
            StartRecording();

            if (vlogCamera != null) CompleteObjective(cameraObjective);
            
            yield return Play(chat);

            DialogueManager.CueFired += OnCue;
            yield return Play(spill);
            DialogueManager.CueFired -= OnCue;
            BreakLaptop();
            if (cameraFocus != null) cameraFocus.Release();

            DialogueManager.CueFired += OnCue;
            yield return Play(_insured ? insuredOutcome : uninsuredOutcome);
            DialogueManager.CueFired -= OnCue;
            if (!_insured) PayForLaptop();
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
                else if (choice.rememberKey == LearnMoreKey && !string.IsNullOrWhiteSpace(learnMoreUrl))
                    Application.OpenURL(learnMoreUrl);
            }

            var choicesOnly = ChoicesOnly(insuranceChoice);
            manager.ChoiceMade += OnChoice;
            yield return Play(insuranceChoice);
            while (picked == null) yield return Play(choicesOnly);
            manager.ChoiceMade -= OnChoice;
            Destroy(choicesOnly);
            
            CompleteObjective(insuranceObjective);

            _insured = picked == Bought;
            if (!_insured) yield break;

            if (!PlayerProfile.TrySpend(insurancePrice))
                Debug.LogWarning($"Bought insurance but the wallet has only {PlayerProfile.Money}.", this);
            yield return Play(insuranceBought);
        }

        private static DialogueData ChoicesOnly(DialogueData dialogue)
        {
            var copy = Instantiate(dialogue);
            JsonUtility.FromJsonOverwrite("{\"lines\":[]}", copy);
            return copy;
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
            else if (cue == spillCue)
            {
                BreakLaptop();
                FocusOnSpill();
            }
            else if (cue == friendLeavesCue) FriendLeaves();
        }

        private void SendMomMessage()
        {
            if (_momMessageSent || messageManager == null || momMessage == null) return;

            _momMessageSent = true;
            messageManager.DeliverSequence(momMessage);
        }

        private void StartRecording()
        {
            StoryChoices.Set("vlog_recording", "true");
            if (vlogCamera == null) return;

            vlogCamera.SetPulse(false);
            vlogCamera.AppendClickText(recordingText);
            if (vlogCamera.GetComponent<RecordingLight>() == null)
                vlogCamera.gameObject.AddComponent<RecordingLight>();
        }

        // No insurance: the player pays for the broken laptop themselves.
        // The amount is remembered so the Chapter Complete screen can show it.
        private void PayForLaptop()
        {
            if (!PlayerProfile.TrySpend(laptopRepairCost))
                Debug.LogWarning($"Laptop costs {laptopRepairCost} but the wallet has only {PlayerProfile.Money}.", this);

            StoryChoices.Set("laptop_cost", laptopRepairCost.ToString("N0", CultureInfo.InvariantCulture));
        }

        private void FriendLeaves()
        {
            if (friend != null) friend.SetActive(false);
            if (doorRenderer != null && _doorClosedSprite != null) doorRenderer.sprite = _doorClosedSprite;
        }

        private void FocusOnDoor()
        {
            if (cameraFocus == null) return;

            Vector3 point = doorRenderer != null ? doorRenderer.bounds.center
                : door != null ? door.transform.position
                : cameraFocus.transform.position;
            cameraFocus.FocusOn(point, knockFocusSize);
        }

        private void FocusOnSpill()
        {
            if (cameraFocus == null) return;

            Vector3 point = laptopRenderer != null ? laptopRenderer.bounds.center
                : spilledCoffee != null ? spilledCoffee.transform.position
                : cameraFocus.transform.position;
            cameraFocus.FocusOn(point, spillFocusSize);
        }

        private void BreakLaptop()
        {
            if (spilledCoffee != null) spilledCoffee.SetActive(true);
            if (laptopRenderer != null && brokenLaptopSprite != null) laptopRenderer.sprite = brokenLaptopSprite;
        }

        private static IEnumerator Play(DialogueData dialogue, PhoneContact contact = null)
        {
            if (dialogue == null) yield break;
            
            var router = DialogueRouter.Instance;

            if (router == null)
            {
                Debug.LogWarning("Chapter1Flow: DialogueRouter is missing.");
                yield break;
            }

            if (dialogue.Presentation == DialoguePresentation.Phone && contact == null)
            {
                Debug.LogWarning($"Phone dialogue '{dialogue.name}' is missing a contact.");
                yield break;
            }

            bool done = false;
            router.Play(dialogue, () => done = true, contact);
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

        private void ActivateObjective(ObjectiveData objective)
        {
            if (objectiveManager == null || objective == null) return;

            objectiveManager.ActivateObjective(objective);
        }

        private void CompleteObjective(ObjectiveData objective)
        {
            if (objectiveManager == null || objective == null) return;

            objectiveManager.CompleteObjective(objective);
        }
    }
}
