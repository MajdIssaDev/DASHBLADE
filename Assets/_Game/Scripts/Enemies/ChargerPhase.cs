namespace Dashblade.Enemies
{
    /// <summary>Where a Charger is in its attack cycle.</summary>
    public enum ChargerPhase
    {
        /// <summary>Just spawned; waiting out the First Attack Delay. Solid and harmless.</summary>
        WaitingForFirstAttack,

        /// <summary>The lane is shown: tracking the player first, then locked. Solid and harmless.</summary>
        Warning,

        /// <summary>Running along the locked lane until the wrapper reports the wall. Hurts on contact.</summary>
        Charging,

        /// <summary>Stopped after a charge, waiting out the Rest Time. Solid and harmless.</summary>
        Resting,

        /// <summary>The player is defeated; this Charger never attacks again. Solid and harmless.</summary>
        Idle,

        /// <summary>Killed by a dash; everything has stopped. Harmless.</summary>
        Destroyed,
    }
}
