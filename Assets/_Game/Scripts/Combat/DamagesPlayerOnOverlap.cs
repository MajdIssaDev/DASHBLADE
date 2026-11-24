using System.Collections.Generic;
using UnityEngine;

namespace Dashblade.Combat
{
    /// <summary>
    /// Hurts the player every frame its collider overlaps the player (the F03 hazard zone now; F04's charging Charger
    /// and projectiles later). Each landed hit costs exactly 1 heart; <see cref="PlayerHealth"/> decides whether a hit
    /// lands (not while dashing, protected or defeated). Turn this component off to make the object harmless.
    /// </summary>
    /// <remarks>
    /// The overlap is asked of the physics world directly each Update rather than read from trigger callbacks:
    /// the dash places the player's body by position each frame, so the frame a dash ends inside danger is seen on
    /// that very frame, and a player standing still (a sleeping body) is still found when protection runs out.
    /// Runs just after <see cref="PlayerHealth"/>, which runs its protection timer first.
    /// The player is found by its <see cref="PlayerHealth"/> component, never by tag or name.
    /// </remarks>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(PlayerHealth.ExecutionOrder + 1)]
    public sealed class DamagesPlayerOnOverlap : MonoBehaviour
    {
        private readonly List<Collider2D> overlaps = new List<Collider2D>();
        private Collider2D hitbox;

        private void Awake()
        {
            if (!TryGetComponent(out hitbox))
            {
                Debug.LogError($"{nameof(DamagesPlayerOnOverlap)} on '{name}' needs a {nameof(Collider2D)} on the same GameObject.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            if (!hitbox.enabled)
            {
                return;
            }

            int count = hitbox.Overlap(ContactFilter2D.noFilter, overlaps);
            for (int i = 0; i < count; i++)
            {
                if (overlaps[i] != null && overlaps[i].TryGetComponent(out PlayerHealth health))
                {
                    health.TryHit();
                }
            }

            overlaps.Clear();
        }
    }
}
