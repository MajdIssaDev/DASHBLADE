using UnityEngine;

namespace Dashblade.Combat
{
    /// <summary>
    /// Tuning numbers for the player's dash (F02; Start Lockout from F06 criterion 14). The asset lives at Assets/_Game/Config/DashConfig.asset.
    /// </summary>
    [CreateAssetMenu(fileName = "DashConfig", menuName = "Dashblade/Dash Config")]
    public sealed class DashConfig : ScriptableObject
    {
        [Tooltip("How far an unblocked dash travels, in world units.")]
        [Min(0f)]
        [SerializeField] private float distance = 5f;

        [Tooltip("Seconds an unblocked dash takes. The speed is Distance / Duration; a dash cut short by a wall keeps that speed.")]
        [Min(0f)]
        [SerializeField] private float duration = 0.15f;

        [Tooltip("Seconds the dash stays unavailable after a dash that destroys nothing, counted from the end of that dash.")]
        [Min(0f)]
        [SerializeField] private float missCooldown = 1f;

        [Tooltip("Diameter in world units of the circle swept along the dash path that destroys enemies.")]
        [Min(0f)]
        [SerializeField] private float hitWidth = 1.25f;

        [Tooltip("Seconds at the start of every run, counted from the first frame the run is shown, during which a left-click does not start a dash (it is dropped, not saved for later). The dash still shows as ready. 0 means no lockout.")]
        [Min(0f)]
        [SerializeField] private float startLockout = 0.5f;

        public float Distance => distance;
        public float Duration => duration;
        public float MissCooldown => missCooldown;
        public float HitWidth => hitWidth;
        public float HitRadius => hitWidth * 0.5f;
        public float StartLockout => startLockout;
    }
}
