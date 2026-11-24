using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dashblade.Flow
{
    /// <summary>
    /// Carries out an accepted menu choice: loads the choice's scene, or quits. Holds no state. Game time is always
    /// set back to normal speed first, because <see cref="Time.timeScale"/> survives scene loads and the ending
    /// panel leaves it at 0: a run started after an ending must never begin frozen.
    /// </summary>
    public static class SceneFlow
    {
        public static void Carry(MenuChoice choice)
        {
            Time.timeScale = 1f;

            string scene = GameScenes.SceneFor(choice);
            if (scene != null)
            {
                // Loads at the end of this frame. The click that chose this was a release (buttons act on release),
                // so the new scene never sees that press as a new left-click.
                SceneManager.LoadScene(scene, LoadSceneMode.Single);
                return;
            }

            Quit();
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            // In the Editor, Quit stops Play Mode.
            UnityEditor.EditorApplication.isPlaying = false;
#else
            // In a build, the application closes without confirmation.
            Application.Quit();
#endif
        }
    }
}
