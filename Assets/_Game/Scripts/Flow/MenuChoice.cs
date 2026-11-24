namespace Dashblade.Flow
{
    /// <summary>What a menu or ending-panel button asks for (F06).</summary>
    public enum MenuChoice
    {
        /// <summary>Main menu: start a fresh run (loads the Arena).</summary>
        Play,

        /// <summary>Ending panel: start a fresh run (reloads the Arena).</summary>
        Retry,

        /// <summary>Ending panel: back to the main menu.</summary>
        Menu,

        /// <summary>Main menu or ending panel: stop Play Mode in the Editor, close the application in a build.</summary>
        Quit,
    }
}
