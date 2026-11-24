using Dashblade.Flow;
using UnityEngine;
using UnityEngine.UI;

namespace Dashblade.UI
{
    /// <summary>
    /// The victory and defeat panel (F06), on the "Ending Canvas" in Arena.unity. A listener only: it waits for
    /// <see cref="RunEnding.EndingShown"/>, then shows the 70% black layer over the whole 16:9 picture and the centred
    /// panel with the result's texts, and passes Retry, Menu and Quit clicks back to <see cref="RunEnding.Choose"/>.
    /// Freezing, scene loading and quitting are not done here.
    /// </summary>
    /// <remarks>
    /// The canvas is Screen Space - Camera on the letterboxed Main Camera, so it is drawn and clicked inside the 16:9
    /// picture only, and is scaled from its 1920 × 1080 layout. It sorts at Order in Layer
    /// <see cref="UiSorting.EndingOrder"/> on the Default layer, above every sprite; a HUD drawn below that order is
    /// darkened under the black layer.
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class EndingPanelView : MonoBehaviour
    {
        private const string VictoryTitle = "VICTORY";
        private const string VictoryMessage = "All three waves cleared.";
        private const string DefeatTitle = "DEFEAT";
        private const string DefeatMessage = "You lost all three hearts.";

        [Tooltip("The run's ending, whose result this panel shows.")]
        [SerializeField] private RunEnding ending;

        [Tooltip("Supplies Dim Strength, the opacity of the black layer.")]
        [SerializeField] private EndingConfig config;

        [Tooltip("Holds the black layer and the panel; hidden until the ending shows.")]
        [SerializeField] private GameObject overlay;

        [Tooltip("The full-picture black layer over the arena.")]
        [SerializeField] private Image dim;

        [SerializeField] private Text title;

        [SerializeField] private Text message;

        [SerializeField] private Button retryButton;

        [SerializeField] private Button menuButton;

        [SerializeField] private Button quitButton;

        private void Awake()
        {
            if (ending == null || config == null || overlay == null || dim == null || title == null || message == null
                || retryButton == null || menuButton == null || quitButton == null)
            {
                Debug.LogError($"{nameof(EndingPanelView)} on '{name}' needs its ending, config, overlay, texts and buttons assigned.", this);
                enabled = false;
                return;
            }

            overlay.SetActive(false);
            retryButton.onClick.AddListener(OnRetryClicked);
            menuButton.onClick.AddListener(OnMenuClicked);
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnEnable()
        {
            if (ending == null)
            {
                return;
            }

            ending.EndingShown += Show;
            if (ending.IsShowing)
            {
                Show(ending.Result);
            }
        }

        private void OnDisable()
        {
            if (ending != null)
            {
                ending.EndingShown -= Show;
            }
        }

        private void Show(EndingResult result)
        {
            bool victory = result == EndingResult.Victory;
            title.text = victory ? VictoryTitle : DefeatTitle;
            message.text = victory ? VictoryMessage : DefeatMessage;
            dim.color = new Color(0f, 0f, 0f, config.DimStrength);
            overlay.SetActive(true);
        }

        private void OnRetryClicked()
        {
            ending.Choose(MenuChoice.Retry);
        }

        private void OnMenuClicked()
        {
            ending.Choose(MenuChoice.Menu);
        }

        private void OnQuitClicked()
        {
            ending.Choose(MenuChoice.Quit);
        }
    }
}
