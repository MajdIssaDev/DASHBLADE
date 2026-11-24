using UnityEngine;

namespace Dashblade.Player
{
    /// <summary>
    /// Tuning numbers for the player (F01). The asset lives at Assets/_Game/Config/PlayerConfig.asset.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Dashblade/Player Config")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [Tooltip("Movement speed in world units per second, along any direction (diagonals included).")]
        [Min(0f)]
        [SerializeField] private float moveSpeed = 6f;

        [Tooltip("If the cursor is closer than this many world units to the player's centre, the facing keeps its last value.")]
        [Min(0f)]
        [SerializeField] private float aimDeadZone = 0.5f;

        public float MoveSpeed => moveSpeed;
        public float AimDeadZone => aimDeadZone;
    }
}
