namespace Dashblade.Enemies
{
    /// <summary>Where a Turret is in its attack cycle.</summary>
    public enum TurretPhase
    {
        /// <summary>Just spawned; waiting out the First Attack Delay.</summary>
        WaitingForFirstAttack,

        /// <summary>The ring is shown and closing; the shot is fired the moment it closes.</summary>
        Warning,

        /// <summary>A shot has just been fired; waiting out the Rest Time.</summary>
        Resting,

        /// <summary>The player is defeated; this Turret never fires again.</summary>
        Idle,

        /// <summary>Killed by a dash; everything has stopped.</summary>
        Destroyed,
    }
}
