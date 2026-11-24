using UnityEngine;

namespace Dashblade.Enemies
{
    /// <summary>
    /// Tuning numbers for the Charger (F04). The asset lives at Assets/_Game/Config/ChargerConfig.asset.
    /// </summary>
    [CreateAssetMenu(fileName = "ChargerConfig", menuName = "Dashblade/Charger Config")]
    public sealed class ChargerConfig : ScriptableObject
    {
        [Tooltip("Seconds from the Charger starting (Play Mode start for a placed Charger) to its first warning.")]
        [Min(0f)]
        [SerializeField] private float firstAttackDelay = 1f;

        [Tooltip("Seconds the lane is shown before the charge. The bar reaches the wall at the end of this time.")]
        [Min(0.01f)]
        [SerializeField] private float warningTime = 1f;

        [Tooltip("The last seconds of the warning during which the lane stops following the player. At most Warning Time.")]
        [Min(0f)]
        [SerializeField] private float lockTime = 0.4f;

        [Tooltip("Units per second along the locked lane during the charge.")]
        [Min(0.01f)]
        [SerializeField] private float chargeSpeed = 15f;

        [Tooltip("Seconds the Charger rests after a charge before the next warning.")]
        [Min(0f)]
        [SerializeField] private float restTime = 1.5f;

        public float FirstAttackDelay => firstAttackDelay;
        public float WarningTime => warningTime;
        public float LockTime => Mathf.Min(lockTime, warningTime);
        public float ChargeSpeed => chargeSpeed;
        public float RestTime => restTime;
    }
}
