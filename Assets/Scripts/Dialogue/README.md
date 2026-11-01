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

Write `{player}` wherever the player's name should appear, in the **Speaker** or the **Text**.
The game swaps it for the name the player typed after New Game (or "Jessie" if no name is saved yet).
For example, Speaker `{player}`, or Mom saying `{player}? Are you okay?`.

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

## Choices and branches

A dialogue can end with options for the player. The picked option decides what plays next,
and can be remembered so later scenes react to it.

Prefabs made with **Create Dialogue Box Prefab** already include the option list. For an older prefab, run that command and choose **Update**: it adds the option list and keeps your art.

In a dialogue asset, under **Choices (optional)**:
- **Choice Prompt**: speaker and text shown above the options, e.g. `{player}` / "About the home insurance...".
- **Choices**: one entry per option:
  - **Text**: what the option says.
  - **Next Dialogue**: what plays if it's picked (leave empty to end the conversation).
  - **Remember Key / Value**: optional, e.g. `insurance` / `skipped`.

The player clicks an option or presses 1-9. See `Assets/Dialogues/Examples/Example_Insurance_Ask`.

Reacting to remembered choices later:
- **Dialogue Trigger > Only If**: plays only if the choice matches, e.g. `insurance` = `skipped`.
- **Choice Reaction** component: runs its **On Match** list when the choice is made, and when a scene starts if it
  was made earlier (show/hide objects, send a phone message, play a trigger).
- Code: `Story.StoryChoices.Get("insurance")`.

New Game clears all remembered choices (the player's name is kept).

## Changing the scene during a dialogue (cues)

To change visuals at a specific line (swap a sprite, show an object, play an animation):

1. In the dialogue asset, type a name in that line's **Cue** field, e.g. `laptop_off`.
2. In the scene, add a **Dialogue Cue** component to any object, set **Cue Name** to the same name,
   and add what should happen to **On Cue** (e.g. the laptop's SpriteRenderer > `sprite` = the dark screen).
3. When that line appears, every Dialogue Cue with that name runs.

Before or after a dialogue, use the Dialogue Trigger's **On Finished**, Interactable's **On Clicked**,
or a **Choice Reaction** instead. See the `laptop_off` cue in `Example_Day1_Coffee`.

## Changing the look

Edit the `DialogueBox` prefab directly: swap the box/name tag images for hand-drawn sprites,
change fonts and sizes, move it around. Typing speed is **Characters Per Second** on the
Dialogue Manager component (0 = show the whole line at once).
