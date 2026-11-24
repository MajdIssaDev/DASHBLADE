using UnityEngine;
using UnityEngine.InputSystem;

namespace Dashblade.Player
{
    /// <summary>
    /// Thin scene wrapper: reads WASD and the arrow keys, and drives the Rigidbody2D's linear velocity.
    /// Movement is screen-relative, starts and stops instantly, and leaves collision response to physics
    /// (so pushing into a wall settles at zero and a diagonal slides along a wall at its tangent component).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerConfig config;

        private readonly FocusMoveGate focusGate = new FocusMoveGate();
        private Rigidbody2D body;
        private Vector2 desiredVelocity;

        /// <summary>
        /// While true, walking input is ignored and this component leaves the Rigidbody2D alone (the dash drives it).
        /// When it turns false, the next Update applies the held keys at once, so walking resumes without a new press.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// While true (the F11 pause), walking input is ignored and the Rigidbody2D is left alone, exactly as while
        /// <see cref="IsLocked"/>; when both are false again the next Update applies the held keys at once. Separate
        /// from <see cref="IsLocked"/>, which the dash owns, so a pause during a dash never unlocks walking early.
        /// </summary>
        public bool IsPaused { get; set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            if (config == null)
            {
                Debug.LogError($"{nameof(PlayerMovement)} on '{name}' has no {nameof(PlayerConfig)} assigned.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            Vector2 direction = Vector2.zero;

            if (keyboard != null)
            {
                focusGate.Update(AnyMovementKeyPressedThisFrame(keyboard));
                if (!focusGate.IsBlocked)
                {
                    direction = MoveInputRules.Direction(
                        up: keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed,
                        down: keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed,
                        left: keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed,
                        right: keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed);
                }
            }

            if (IsLocked || IsPaused)
            {
                // Forget the pre-lock velocity, so the first unlocked frame always applies the held keys.
                desiredVelocity = Vector2.zero;
                return;
            }

            Vector2 velocity = direction * config.MoveSpeed;
            if (velocity != desiredVelocity)
            {
                // Apply on the frame the input changes, so start and stop are instant.
                desiredVelocity = velocity;
                body.linearVelocity = velocity;
            }
        }

        private void FixedUpdate()
        {
            if (IsLocked || IsPaused)
            {
                return;
            }

            // Re-applied before every physics step; the solver then removes any part that pushes into a wall.
            body.linearVelocity = desiredVelocity;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                return;
            }

            focusGate.OnFocusLost();
            desiredVelocity = Vector2.zero;
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }
        }

        private static bool AnyMovementKeyPressedThisFrame(Keyboard keyboard)
        {
            return keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame
                || keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame
                || keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame
                || keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame;
        }
    }
}
