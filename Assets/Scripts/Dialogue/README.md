# Dialogue system

Shows a character's name and their line in a box at the bottom of the screen, one line at a time.
Click the box or press Space/Enter to continue.

## One-time setup per scene

1. Drag `Assets/UI/Dialogue/DialogueBox.prefab` into the scene (only one per scene).
   - If the prefab doesn't exist yet, run **Tools > Youngnest > Create Dialogue Box Prefab** first.

## Making a dialogue

1. In the Project window, right-click > **Create > Dialogue > Dialogue**.
   Put it in `Assets/Dialogues/` and name it after the scenario, e.g. `Ch1_1_RentReminder`.
2. Add lines. Each line has:
   - **Speaker**: the name shown in the name tag. Leave empty for narration (the name tag hides).
   - **Text**: what they say.

See `Assets/Dialogues/Example_Day1_Coffee` for an example.

## Playing a dialogue (no code)

1. Add a **Dialogue Trigger** component to any GameObject.
2. Drag your dialogue asset into **Dialogue**.
3. Choose when it plays:
   - Tick **Play On Start** (with an optional **Delay** in seconds), or
   - Call `DialogueTrigger.Play()` from a Button's On Click or any other UnityEvent.
4. **On Finished** runs when the dialogue ends. Use it to chain things, e.g. another Dialogue Trigger's
   `Play()`, or `MessageManager.DeliverSequence` to send a phone message.

## Playing a dialogue (from code)

```csharp
DialogueManager.Instance.Play(myDialogue, () => Debug.Log("Dialogue finished"));
```

- If a dialogue is already playing, the new one waits its turn.
- `DialogueManager.Instance.IsPlaying` tells you if a dialogue is on screen.
- `DialogueStarted` / `DialogueEnded` events let other systems react (e.g. pause the phone).

## Changing the look

Edit the `DialogueBox` prefab directly: swap the box/name tag images for hand-drawn sprites,
change fonts and sizes, move it around. Typing speed is **Characters Per Second** on the
Dialogue Manager component (0 = show the whole line at once).
