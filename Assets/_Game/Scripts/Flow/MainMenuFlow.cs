using UnityEngine;

namespace Dashblade.Flow
{
    /// <summary>
    /// Thin scene wrapper for the main menu's choices (F06), on the "Main Menu Flow" object in MainMenu.unity. The
    /// menu view passes button clicks to <see cref="Choose"/>; the first Play or Quit is carried out and every later
    /// click is ignored. This component knows nothing about the UI.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MainMenuFlow : MonoBehaviour
    {
        private readonly ChoiceLatch latch = new ChoiceLatch(MenuChoice.Play, MenuChoice.Quit);

        /// <summary>
        /// A main-menu button was clicked. Only the first Play or Quit is carried out (Play loads the Arena, Quit stops
        /// Play Mode or closes the game); returns false for every other call, which changes nothing.
        /// </summary>
        public bool Choose(MenuChoice choice)
        {
            if (!latch.TryChoose(choice))
            {
                return false;
            }

            SceneFlow.Carry(choice);
            return true;
        }
    }
}
