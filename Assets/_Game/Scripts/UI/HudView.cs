using Dashblade.Combat;
using Dashblade.Waves;
using UnityEngine;
using UnityEngine.UI;

namespace Dashblade.UI
{
    /// <summary>
    /// The in-run HUD (F07), on the "HUD Canvas" in Arena.unity: a row over the top wall with the heart squares at the
    /// left, the wave text in the centre and the dash bar at the right. A listener only: hearts follow
    /// <see cref="PlayerHealth.HeartsChanged"/>, the text follows <see cref="ArenaRun.CurrentWaveChanged"/>, and the bar
    /// is redrawn each frame from <see cref="PlayerDash.RechargeProgress"/> (the dash announces no event while it
    /// recharges). What to show comes from <see cref="HudRules"/>. No game script refers to this component, so the game
    /// runs the same with the HUD object switched off.
    /// </summary>
    /// <remarks>
    /// The canvas is Screen Space - Camera on the letterboxed Main Camera, so it stays inside the 16:9 picture, and
    /// sorts at <see cref="UiSorting.HudOrder"/>, under the ending's black layer. It has no GraphicRaycaster and none of
    /// its graphics is a raycast target, so it never takes mouse input. The sizes and positions of the row, squares,
    /// text and bar are set on their RectTransforms in the scene (1920 × 1080 reference layout), not here. While the
    /// game is frozen (time scale 0) the values it reads do not change, so neither does the HUD.
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class HudView : MonoBehaviour
    {
        [Tooltip("The run whose current wave and wave count the centre text shows.")]
        [SerializeField] private ArenaRun run;

        [Tooltip("The player whose hearts the squares show.")]
        [SerializeField] private PlayerHealth health;

        [Tooltip("The player's dash, whose recharge the bar shows.")]
        [SerializeField] private PlayerDash dash;

        [Tooltip("The solid red insides of the heart squares, left to right. Each is shown while its heart is kept; the red outline around it always shows.")]
        [SerializeField] private Image[] heartFills;

        [SerializeField] private Text waveText;

        [Tooltip("The cyan fill of the dash bar. Its right edge (anchor max X) is moved from 0 to 1 to show the recharge, so it grows from the left.")]
        [SerializeField] private RectTransform dashBarFill;

        private float shownBarFill = -1f;

        private void Awake()
        {
            if (run == null || health == null || dash == null || heartFills == null || heartFills.Length == 0
                || waveText == null || dashBarFill == null)
            {
                Debug.LogError($"{nameof(HudView)} on '{name}' needs its run, player health, player dash, heart squares, wave text and dash bar fill assigned.", this);
                enabled = false;
                return;
            }

            foreach (Image fill in heartFills)
            {
                if (fill == null)
                {
                    Debug.LogError($"{nameof(HudView)} on '{name}' has an empty heart square slot.", this);
                    enabled = false;
                    return;
                }
            }
        }

        private void OnEnable()
        {
            if (run == null || health == null || dash == null)
            {
                return;
            }

            health.HeartsChanged += ShowHearts;
            run.CurrentWaveChanged += ShowWave;
            ShowAll();
        }

        private void Start()
        {
            // Every Awake has run by now, so the run and the hearts report their real starting values.
            ShowAll();
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.HeartsChanged -= ShowHearts;
            }

            if (run != null)
            {
                run.CurrentWaveChanged -= ShowWave;
            }
        }

        private void LateUpdate()
        {
            ShowDashBar(dash.RechargeProgress);
        }

        private void ShowAll()
        {
            ShowHearts(health.Hearts);
            ShowWave(run.CurrentWave);
            ShowDashBar(dash.RechargeProgress);
        }

        private void ShowHearts(int hearts)
        {
            for (int slot = 0; slot < heartFills.Length; slot++)
            {
                heartFills[slot].enabled = HudRules.IsHeartSolid(slot, hearts);
            }
        }

        private void ShowWave(int wave)
        {
            waveText.text = HudRules.WaveText(wave, run.WaveCount);
        }

        private void ShowDashBar(float rechargeProgress)
        {
            float fill = HudRules.DashBarFill(rechargeProgress);
            // Only on change, so a full or frozen bar does not rebuild the canvas every frame.
            if (fill == shownBarFill)
            {
                return;
            }

            shownBarFill = fill;
            dashBarFill.anchorMax = new Vector2(fill, dashBarFill.anchorMax.y);
        }
    }
}
