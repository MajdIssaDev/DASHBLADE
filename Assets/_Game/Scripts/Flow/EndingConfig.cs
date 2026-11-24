using UnityEngine;

namespace Dashblade.Flow
{
    /// <summary>
    /// Tuning numbers for the end of a run (F06). The asset lives at Assets/_Game/Config/EndingConfig.asset.
    /// The ending rules themselves live in <see cref="EndingFlow"/>; this asset only feeds them and the panel.
    /// </summary>
    [CreateAssetMenu(fileName = "EndingConfig", menuName = "Dashblade/Ending Config")]
    public sealed class EndingConfig : ScriptableObject
    {
        [Tooltip("Seconds of game time between the run ending (victory or the last heart lost) and the ending panel appearing. The game runs normally meanwhile.")]
        [Min(0f)]
        [SerializeField] private float endingDelay = 1f;

        [Tooltip("Opacity of the black layer drawn over the arena while the ending panel shows (0 = none, 1 = solid black).")]
        [Range(0f, 1f)]
        [SerializeField] private float dimStrength = 0.7f;

        public float EndingDelay => endingDelay;

        public float DimStrength => dimStrength;
    }
}
