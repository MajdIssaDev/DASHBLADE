namespace Dashblade.Waves
{
    /// <summary>Where the current wave is in its cycle.</summary>
    public enum WavePhase
    {
        /// <summary>Wave Pause: empty arena (or only dying enemies) before the current wave appears.</summary>
        Pause,

        /// <summary>The current wave's enemies are fading in: harmless, not solid, not destroyable.</summary>
        FadingIn,

        /// <summary>The current wave's enemies are active; the wave ends when Enemies Left reaches 0.</summary>
        Fighting,

        /// <summary>The run has ended (Victory or Defeat). Nothing more ever spawns.</summary>
        Over,
    }
}
