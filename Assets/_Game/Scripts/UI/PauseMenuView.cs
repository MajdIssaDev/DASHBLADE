using Dashblade.Flow;
using UnityEngine;
using UnityEngine.UI;

namespace Dashblade.UI
{
    /// <summary>
    /// The pause panel (F11), on the "Pause Canvas" in Arena.unity. A listener only: it follows
    /// <see cref="RunPause.StateChanged"/>, showing the 70% black layer over the whole 16:9 picture and the centred
    /// panel on either its buttons view ("PAUSED", Resume, Controls, Menu, Quit) or its Controls view ("CONTROLS",
    /// the three control lines, Back), and hides both while playing. It passes every button click to
    /// <see cref="RunPause"/>, which decides whether it counts. Freezing, scene loading and quitting are not done here.
    /// </summary>
    /// <remarks>
    /// Built like the ending panel: a Screen Space - Camera canvas on the letterboxed Main Camera, scaled from its
    /// 1920 × 1080 layout, sorting at <see cref="UiSorting.PauseOrder"/> (above the HUD, which is darkened). The texts
    /// and their layout are set in the scene. No game script refers to this component.
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class PauseMenuView : MonoBehaviour
    {
        [Tooltip("The run's pause, whose state this panel shows.")]
        [SerializeField] private RunPause pause;

        [Tooltip("Supplies Dim Strength, the opacity of the black layer (the same as the ending's).")]
        [SerializeField] private EndingConfig config;

        [Tooltip("Holds the black layer and the panel; hidden while playing.")]
        [SerializeField] private GameObject overlay;

        [Tooltip("The full-picture black layer over the arena and the HUD.")]
        [SerializeField] private Image dim;

        [Tooltip("The \"PAUSED\" title and the four buttons.")]
        [SerializeField] private GameObject buttonsView;

        [Tooltip("The \"CONTROLS\" title, the three control lines and Back.")]
        [SerializeField] private GameObject controlsView;

        [SerializeField] private Button resumeButton;

        [SerializeField] private Button controlsButton;

        [SerializeField] private Button menuButton;

        [SerializeField] private Button quitButton;

        [SerializeField] private Button backButton;

        private void Awake()
        {
            if (pause == null || config == null || overlay == null || dim == null || buttonsView == null
                || controlsView == null || resumeButton == null || controlsButton == null || menuButton == null
                || quitButton == null || backButton == null)
            {
                Debug.LogError($"{nameof(PauseMenuView)} on '{name}' needs its pause, config, overlay, views and buttons assigned.", this);
                enabled = false;
                return;
            }

            overlay.SetActive(false);
            resumeButton.onClick.AddListener(OnResumeClicked);
            controlsButton.onClick.AddListener(OnControlsClicked);
            menuButton.onClick.AddListener(OnMenuClicked);
            quitButton.onClick.AddListener(OnQuitClicked);
            backButton.onClick.AddListener(OnBackClicked);
        }

        private void OnEnable()
        {
            if (pause == null)
            {
                return;
            }

            pause.StateChanged += Show;
            Show(pause.State);
        }

        private void OnDisable()
        {
            if (pause != null)
            {
                pause.StateChanged -= Show;
            }
        }

        private void Show(PauseState state)
        {
            if (state == PauseState.Playing)
            {
                overlay.SetActive(false);
                return;
            }

            dim.color = new Color(0f, 0f, 0f, config.DimStrength);
            buttonsView.SetActive(state == PauseState.Buttons);
            controlsView.SetActive(state == PauseState.Controls);
            overlay.SetActive(true);
        }

        private void OnResumeClicked()
        {
            pause.Resume();
        }

        private void OnControlsClicked()
        {
            pause.ShowControls();
        }

        private void OnMenuClicked()
        {
            pause.Choose(MenuChoice.Menu);
        }

        private void OnQuitClicked()
        {
            pause.Choose(MenuChoice.Quit);
        }

        private void OnBackClicked()
        {
            pause.Back();
        }
    }
}
