namespace Dashblade.Enemies
{
    /// <summary>
    /// Whether an enemy's body blocks the walking player right now. An enemy that should be solid (started, not
    /// charging, alive) blocks like a wall, except that once the two bodies overlap for any reason it lets the player
    /// through until they have fully separated; then it blocks again.
    /// </summary>
    public sealed class EnemySolidity
    {
        /// <summary>True while the enemy's body blocks the player. Starts false.</summary>
        public bool IsBlocking { get; private set; }

        /// <summary>
        /// Updates and returns <see cref="IsBlocking"/>.
        /// </summary>
        /// <param name="shouldBeSolid">The enemy's state wants it solid (not charging, not dormant, not dead).</param>
        /// <param name="overlapDepth">How deep the enemy and player bodies overlap, in units; 0 or less when apart.</param>
        /// <param name="tolerance">
        /// Overlap a blocking enemy ignores: the physics solver lets a body pressing against a wall sink in very
        /// slightly, which is not an overlap.
        /// </param>
        public bool Update(bool shouldBeSolid, float overlapDepth, float tolerance)
        {
            if (!shouldBeSolid)
            {
                IsBlocking = false;
            }
            else if (!IsBlocking)
            {
                // Solid again only once the two have fully separated.
                IsBlocking = !(overlapDepth > 0f);
            }
            else if (overlapDepth > tolerance)
            {
                IsBlocking = false;
            }

            return IsBlocking;
        }
    }
}
