namespace Dashblade.Player
{
    /// <summary>
    /// Blocks movement after the window loses focus, until a movement key is pressed again.
    /// Only a key going down counts as a press; simply holding a key does not unblock it.
    /// </summary>
    public sealed class FocusMoveGate
    {
        public bool IsBlocked { get; private set; }

        public void OnFocusLost()
        {
            IsBlocked = true;
        }

        /// <summary>Call once per frame.</summary>
        /// <param name="anyMovementKeyPressedThisFrame">True if any movement key went down this frame.</param>
        public void Update(bool anyMovementKeyPressedThisFrame)
        {
            if (IsBlocked && anyMovementKeyPressedThisFrame)
            {
                IsBlocked = false;
            }
        }
    }
}
