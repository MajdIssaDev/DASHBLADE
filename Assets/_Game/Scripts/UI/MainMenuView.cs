using Dashblade.Flow;
using UnityEngine;
using UnityEngine.UI;

namespace Dashblade.UI
{
    /// <summary>
    /// The main menu's buttons (F06), on the "Menu Canvas" in MainMenu.unity. A listener only: it passes Play and Quit
    /// clicks to <see cref="MainMenuFlow.Choose"/>, which decides whether they count and carries them out. The title,
    /// controls and goal lines are plain texts in the scene.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MainMenuView : MonoBehaviour
    {
        [SerializeField] private MainMenuFlow flow;

        [SerializeField] private Button playButton;

        [SerializeField] private Button quitButton;

        private void Awake()
        {
            if (flow == null || playButton == null || quitButton == null)
            {
                Debug.LogError($"{nameof(MainMenuView)} on '{name}' needs its flow and its Play and Quit buttons assigned.", this);
                enabled = false;
                return;
            }

            playButton.onClick.AddListener(OnPlayClicked);
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnPlayClicked()
        {
            flow.Choose(MenuChoice.Play);
        }

        private void OnQuitClicked()
        {
            flow.Choose(MenuChoice.Quit);
        }
    }
}
