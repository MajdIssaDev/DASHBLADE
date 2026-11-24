namespace Dashblade.Flow
{
    /// <summary>
    /// The two scenes of the game flow (F06), as registered in the build scene list: MainMenu at index 0 (the first
    /// scene of a build) and Arena at index 1. Scenes are loaded by name, so the Arena also works when played directly.
    /// </summary>
    public static class GameScenes
    {
        public const string MainMenu = "MainMenu";

        public const string Arena = "Arena";

        /// <summary>
        /// The scene a choice loads: Play and Retry load the Arena (a fresh run), Menu loads the main menu.
        /// Returns null for Quit, which loads nothing.
        /// </summary>
        public static string SceneFor(MenuChoice choice)
        {
            switch (choice)
            {
                case MenuChoice.Play:
                case MenuChoice.Retry:
                    return Arena;

                case MenuChoice.Menu:
                    return MainMenu;

                default:
                    return null;
            }
        }
    }
}
