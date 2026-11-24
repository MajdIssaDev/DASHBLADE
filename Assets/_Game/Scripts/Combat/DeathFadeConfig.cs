using UnityEngine;

namespace Dashblade.Combat
{
    /// <summary>
    /// Tuning numbers for how a destroyed enemy disappears (F02). The asset lives at
    /// Assets/_Game/Config/DeathFadeConfig.asset and is shared by every <see cref="DashTarget"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "DeathFadeConfig", menuName = "Dashblade/Death Fade Config")]
    public sealed class DeathFadeConfig : ScriptableObject
    {
        [Tooltip("Seconds a destroyed enemy takes to fade to transparent before it is removed.")]
        [Min(0f)]
        [SerializeField] private float fadeTime = 0.3f;

        public float FadeTime => fadeTime;
    }
}
