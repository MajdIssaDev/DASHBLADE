using UnityEngine;

namespace Dashblade.Player
{
    /// <summary>
    /// Turns the four screen-relative movement directions into a move direction.
    /// Each flag should already combine both key sets (for example W or Up Arrow),
    /// so holding W and Up Arrow together counts once.
    /// </summary>
    public static class MoveInputRules
    {
        /// <summary>
        /// Returns a unit vector (or zero). Opposite directions cancel on their axis,
        /// and diagonals are normalised so they are not faster than straight moves.
        /// </summary>
        public static Vector2 Direction(bool up, bool down, bool left, bool right)
        {
            float x = (right ? 1f : 0f) - (left ? 1f : 0f);
            float y = (up ? 1f : 0f) - (down ? 1f : 0f);
            var direction = new Vector2(x, y);
            return direction == Vector2.zero ? Vector2.zero : direction.normalized;
        }
    }
}
