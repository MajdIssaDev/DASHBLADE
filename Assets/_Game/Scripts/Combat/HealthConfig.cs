using UnityEngine;

namespace Dashblade.Combat
{
    /// <summary>
    /// Tuning numbers for the player's hearts and damage protection (F03). The asset lives at
    /// Assets/_Game/Config/HealthConfig.asset. Every hit costs exactly 1 heart; that is a rule, not a number here.
    /// </summary>
    [CreateAssetMenu(fileName = "HealthConfig", menuName = "Dashblade/Health Config")]
    public sealed class HealthConfig : ScriptableObject
    {
        [Tooltip("Hearts the player starts with, and has again after a restore.")]
        [Min(1)]
        [SerializeField] private int maxHearts = 3;

        [Tooltip("Seconds after a hit during which further hits are ignored. The circle blinks for this long.")]
        [Min(0f)]
        [SerializeField] private float protectionTime = 0.75f;

        [Tooltip("Seconds between the circle switching between half-transparent and solid while protected.")]
        [Min(0f)]
        [SerializeField] private float blinkInterval = 0.1f;

        [Tooltip("Seconds the player takes to fade out after losing the last heart.")]
        [Min(0f)]
        [SerializeField] private float defeatFadeTime = 0.3f;

        public int MaxHearts => maxHearts;
        public float ProtectionTime => protectionTime;
        public float BlinkInterval => blinkInterval;
        public float DefeatFadeTime => defeatFadeTime;
    }
}
