using System;
using System.Collections.Generic;
using Dashblade.Common;
using Dashblade.Player;
using UnityEngine;

namespace Dashblade.Combat
{
    /// <summary>
    /// Thin scene wrapper for the player's hearts (F03). Damage sources such as <see cref="DamagesPlayerOnOverlap"/>
    /// call <see cref="TryHit"/>; the rules live in <see cref="HeartsRules"/>. While protected only the white circle
    /// blinks. On the last heart the player stops, its walking, aim and dash are turned off, its collision goes, and
    /// the body and nose fade out. Listeners (HUD, ending screen, enemies) use <see cref="HeartsChanged"/> and
    /// <see cref="Defeated"/>; this component knows nothing about them.
    /// </summary>
    /// <remarks>
    /// Frame order: <see cref="PlayerDash"/> (-10) moves or ends the dash first, then this Update (-5) runs the
    /// protection timer, then damage sources (-4) attempt their hits from their Update, so a dash that ends inside
    /// danger is hit on that same frame and protection leftovers carry over without drift. The blink is drawn in
    /// LateUpdate, after every hit of the frame. <see cref="PlayerMovement"/> and <see cref="PlayerAim"/> (0) run
    /// after the hits, so on the frame of defeat they are already off.
    /// </remarks>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(ExecutionOrder)]
    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerDash))]
    [RequireComponent(typeof(PlayerMovement), typeof(PlayerAim))]
    public sealed class PlayerHealth : MonoBehaviour
    {
        /// <summary>After <see cref="PlayerDash"/> (-10), before damage sources and walking/aim.</summary>
        public const int ExecutionOrder = -5;

        private static readonly List<Collider2D> ColliderBuffer = new List<Collider2D>();

        [SerializeField] private HealthConfig config;

        [Tooltip("The white circle: blinks while protected and fades out on defeat.")]
        [SerializeField] private SpriteRenderer bodySprite;

        [Tooltip("The nose: never blinks (it keeps showing dash readiness), but fades out with the body on defeat.")]
        [SerializeField] private SpriteRenderer noseSprite;

        [Tooltip("Hearts left. Shown for checking; set by the hearts rules, not editable.")]
        [ReadOnlyInInspector]
        [SerializeField] private int currentHearts;

        // Named for its Inspector label, "Protected".
        [Tooltip("On while hits are ignored after a hit. Shown for checking; not editable.")]
        [ReadOnlyInInspector]
        [SerializeField] private bool @protected;

        [Tooltip("On once the last heart is lost. Shown for checking; not editable.")]
        [ReadOnlyInInspector]
        [SerializeField] private bool defeated;

        private readonly DeathFadeTimer defeatFade = new DeathFadeTimer();
        private HeartsRules rules;
        private Rigidbody2D body;
        private PlayerDash dash;
        private PlayerMovement movement;
        private PlayerAim aim;
        private int defeatFrame = -1;

        /// <summary>Hearts left, from <see cref="MaxHearts"/> down to 0.</summary>
        public int Hearts => rules != null ? rules.Hearts : 0;

        public int MaxHearts => rules != null ? rules.MaxHearts : 0;

        public bool IsProtected => rules != null && rules.IsProtected;

        public bool IsDefeated => rules != null && rules.IsDefeated;

        /// <summary>Raised with the new value whenever <see cref="Hearts"/> changes (a hit or a restore).</summary>
        public event Action<int> HeartsChanged;

        /// <summary>Raised once, when the last heart is lost. The player is already stopped and without collision.</summary>
        public event Action Defeated;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            dash = GetComponent<PlayerDash>();
            movement = GetComponent<PlayerMovement>();
            aim = GetComponent<PlayerAim>();

            if (config == null)
            {
                Debug.LogError($"{nameof(PlayerHealth)} on '{name}' has no {nameof(HealthConfig)} assigned.", this);
                enabled = false;
                return;
            }

            if (bodySprite == null || noseSprite == null)
            {
                Debug.LogError($"{nameof(PlayerHealth)} on '{name}' needs its body and nose {nameof(SpriteRenderer)}s assigned.", this);
            }

            rules = new HeartsRules(config.MaxHearts, config.ProtectionTime);
            rules.HeartsChanged += OnHeartsChanged;
            rules.Defeated += OnDefeated;
            ShowState();
        }

        private void Update()
        {
            rules.Tick(Time.deltaTime);

            // The frame of defeat already passed when the defeat happened, so the fade counts from the next frame.
            if (defeatFade.IsStarted && Time.frameCount != defeatFrame)
            {
                defeatFade.Tick(Time.deltaTime);
            }

            ShowState();
        }

        private void LateUpdate()
        {
            if (rules.IsDefeated)
            {
                float alpha = defeatFade.Alpha(config.DefeatFadeTime);
                SetAlpha(bodySprite, alpha);
                SetAlpha(noseSprite, alpha);
            }
            else
            {
                // Only the circle; the nose's colour belongs to the dash-readiness display.
                SetAlpha(bodySprite, rules.BodyAlpha(config.BlinkInterval));
            }
        }

        /// <summary>
        /// Something dangerous touches the player this frame. Costs exactly 1 heart unless the player is dashing,
        /// protected or defeated; any number of calls in one frame cost at most 1 heart. Call it from Update
        /// (damage sources run after this component), not from physics callbacks. Returns true if a heart was lost.
        /// </summary>
        public bool TryHit()
        {
            if (rules == null || !enabled)
            {
                return false;
            }

            bool landed = rules.TryHit(dash.IsInvulnerable);
            ShowState();
            return landed;
        }

        /// <summary>
        /// Sets the hearts back to the maximum (F05 calls this between waves). Does nothing at full hearts or once
        /// defeated; a running protection keeps its remaining time. Returns true if the hearts changed.
        /// </summary>
        public bool RestoreHearts()
        {
            if (rules == null)
            {
                return false;
            }

            bool restored = rules.Restore();
            ShowState();
            return restored;
        }

        [ContextMenu("Restore Hearts")]
        private void RestoreHeartsFromMenu()
        {
            RestoreHearts();
        }

        private void OnHeartsChanged(int hearts)
        {
            ShowState();
            HeartsChanged?.Invoke(hearts);
        }

        private void OnDefeated()
        {
            defeatFrame = Time.frameCount;
            defeatFade.TryStart();

            // Disabled rather than locked: nothing can unlock them again. The dash hands the body back to physics
            // if it is ever turned off mid-dash.
            dash.enabled = false;
            movement.enabled = false;
            aim.enabled = false;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;

            GetComponents(ColliderBuffer);
            foreach (Collider2D hitbox in ColliderBuffer)
            {
                hitbox.enabled = false;
            }

            ColliderBuffer.Clear();

            ShowState();
            Defeated?.Invoke();
        }

        private void ShowState()
        {
            currentHearts = rules.Hearts;
            @protected = rules.IsProtected;
            defeated = rules.IsDefeated;
        }

        private static void SetAlpha(SpriteRenderer sprite, float alpha)
        {
            if (sprite == null)
            {
                return;
            }

            Color color = sprite.color;
            color.a = alpha;
            sprite.color = color;
        }
    }
}
