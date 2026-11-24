using UnityEngine;
using UnityEngine.InputSystem;

namespace Dashblade.Player
{
    /// <summary>
    /// Thin scene wrapper: turns the player to face the mouse cursor every frame.
    /// Rotation Z = 0 means the nose points +X. The current aim is exposed through <see cref="AimDirection"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerAim : MonoBehaviour
    {
        [SerializeField] private PlayerConfig config;

        [Tooltip("Camera used to convert the mouse's screen position into world units.")]
        [SerializeField] private Camera aimCamera;

        private readonly AimTracker tracker = new AimTracker();

        /// <summary>Unit vector the player is currently facing, in world space. Defaults to +X.</summary>
        public Vector2 AimDirection => tracker.Direction;

        /// <summary>
        /// While true, the cursor is ignored and the body keeps its current rotation (the dash points it along its path).
        /// When it turns false, the next Update snaps the facing to the cursor again.
        /// </summary>
        public bool IsLocked { get; set; }

        private void Awake()
        {
            if (config == null)
            {
                Debug.LogError($"{nameof(PlayerAim)} on '{name}' has no {nameof(PlayerConfig)} assigned.", this);
                enabled = false;
                return;
            }

            if (aimCamera == null)
            {
                Debug.LogError($"{nameof(PlayerAim)} on '{name}' has no aim camera assigned.", this);
                enabled = false;
                return;
            }

            ApplyFacing();
        }

        private void Update()
        {
            if (IsLocked)
            {
                return;
            }

            // Recalculated every frame, so the nose keeps pointing at a still cursor while the player moves.
            RefreshFacing();
        }

        /// <summary>
        /// Recalculates the aim from the cursor right now and turns the body to it. Update calls this every frame;
        /// the dash also calls it at the click, so it uses this frame's cursor rather than last frame's.
        /// </summary>
        public void RefreshFacing()
        {
            if (config == null || aimCamera == null)
            {
                return;
            }

            tracker.Update(transform.position, ReadCursorWorld(), config.AimDeadZone);
            ApplyFacing();
        }

        private Vector2? ReadCursorWorld()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return null;
            }

            // Screen pixels to world units. Camera.ScreenToWorldPoint accounts for the camera's viewport rect,
            // so this stays correct under the letterbox and extrapolates past the picture's edges.
            Vector2 screen = mouse.position.ReadValue();
            Vector3 world = aimCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 0f));
            return new Vector2(world.x, world.y);
        }

        private void ApplyFacing()
        {
            transform.rotation = Quaternion.Euler(0f, 0f, tracker.AngleDegrees);
        }
    }
}
