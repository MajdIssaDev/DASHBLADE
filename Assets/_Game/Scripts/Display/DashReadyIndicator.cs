using Dashblade.Combat;
using UnityEngine;

namespace Dashblade.Display
{
    /// <summary>
    /// Placeholder dash-readiness display (until the F07 HUD): tints a sprite, the player's nose, red while the dash
    /// is ready and grey while it is not. It only listens to <see cref="PlayerDash.ReadinessChanged"/>; the dash
    /// knows nothing about it.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DashReadyIndicator : MonoBehaviour
    {
        [SerializeField] private PlayerDash dash;
        [SerializeField] private SpriteRenderer target;
        [SerializeField] private Color readyColor = new Color(0.85f, 0.1f, 0.1f, 1f);
        [SerializeField] private Color notReadyColor = new Color(0.55f, 0.55f, 0.55f, 1f);

        private void OnEnable()
        {
            if (dash == null || target == null)
            {
                Debug.LogError($"{nameof(DashReadyIndicator)} on '{name}' needs a {nameof(PlayerDash)} and a target {nameof(SpriteRenderer)}.", this);
                enabled = false;
                return;
            }

            dash.ReadinessChanged += Show;
            Show(dash.IsReady);
        }

        private void OnDisable()
        {
            if (dash != null)
            {
                dash.ReadinessChanged -= Show;
            }
        }

        private void Show(bool isReady)
        {
            target.color = isReady ? readyColor : notReadyColor;
        }
    }
}
