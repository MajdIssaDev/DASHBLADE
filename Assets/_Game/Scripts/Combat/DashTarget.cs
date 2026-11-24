using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dashblade.Combat
{
    /// <summary>
    /// Marks something the dash can destroy: the F02 dummies, F04's Charger and Turret, and Turret projectiles.
    /// When killed it turns off its collision at once, raises <see cref="Killed"/>, fades its sprite to transparent
    /// over the shared Fade Time, then removes its GameObject. A dead target cannot be killed again.
    /// Destroying an enemy is a kill for the dash; destroying a projectile is not (<see cref="CountsAsKill"/>).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DashTarget : MonoBehaviour
    {
        private static readonly List<Collider2D> ColliderBuffer = new List<Collider2D>();

        [SerializeField] private DeathFadeConfig fadeConfig;

        [Tooltip("Sprite faded to transparent when this target is destroyed.")]
        [SerializeField] private SpriteRenderer sprite;

        [Tooltip("On for enemies: destroying this makes the dash ready again at once. Off for projectiles: destroying only those is a miss.")]
        [SerializeField] private bool countsAsKill = true;

        private readonly DeathFadeTimer fade = new DeathFadeTimer();
        private Color aliveColor;
        private int killFrame = -1;

        /// <summary>True from the moment this target is destroyed, through its fade.</summary>
        public bool IsDead => fade.IsStarted;

        /// <summary>True if destroying this target counts as a kill for the dash (enemies); false for projectiles.</summary>
        public bool CountsAsKill => countsAsKill;

        /// <summary>
        /// While false, <see cref="Kill"/> does nothing: the dash passes through without destroying it (for example an
        /// enemy that has not started yet, such as one still fading in). On by default.
        /// </summary>
        public bool IsDestroyable { get; set; } = true;

        /// <summary>Raised once, at the moment this target is destroyed (its collision is already off).</summary>
        public event Action<DashTarget> Killed;

        private float FadeTime => fadeConfig != null ? fadeConfig.FadeTime : 0f;

        private void Reset()
        {
            TryGetComponent(out sprite);
        }

        private void Awake()
        {
            if (fadeConfig == null)
            {
                Debug.LogError($"{nameof(DashTarget)} on '{name}' has no {nameof(DeathFadeConfig)} assigned.", this);
            }

            if (sprite != null)
            {
                aliveColor = sprite.color;
            }
        }

        /// <summary>
        /// Destroys this target. Returns true if it was alive and destroyable (it is destroyed now), false if it was
        /// already dead or is not destroyable. Whether that is a kill for the dash is <see cref="CountsAsKill"/>.
        /// </summary>
        public bool Kill()
        {
            if (!IsDestroyable || !fade.TryStart())
            {
                return false;
            }

            killFrame = Time.frameCount;

            GetComponents(ColliderBuffer);
            foreach (Collider2D hitbox in ColliderBuffer)
            {
                hitbox.enabled = false;
            }

            ColliderBuffer.Clear();

            Killed?.Invoke(this);
            ApplyFade();
            return true;
        }

        private void Update()
        {
            // This frame's time passed before the kill, so the fade starts counting on the next frame.
            if (!fade.IsStarted || Time.frameCount == killFrame)
            {
                return;
            }

            fade.Tick(Time.deltaTime);
            ApplyFade();
        }

        private void ApplyFade()
        {
            float fadeTime = FadeTime;
            if (sprite != null)
            {
                Color color = aliveColor;
                color.a *= fade.Alpha(fadeTime);
                sprite.color = color;
            }

            if (fade.IsFinished(fadeTime))
            {
                Destroy(gameObject);
            }
        }
    }
}
