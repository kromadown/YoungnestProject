using MainMenu;
using Story;

namespace Player
{
    // The player's whole save: name, character, money, finished chapters and remembered story choices.
    public static class SaveData
    {
        // A save exists once the player has entered a name.
        public static bool HasSave => PlayerProfile.HasName;

        // Deletes the whole save, e.g. when the player starts a new game.
        public static void DeleteAll()
        {
            PlayerProfile.ClearName();
            PlayerProfile.ClearCharacter();
            PlayerProfile.ClearMoney();
            ChapterProgress.ClearAll();
            StoryChoices.ClearAll();
        }
    }
}
