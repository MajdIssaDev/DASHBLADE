using System;
using System.Collections.Generic;

namespace Dashblade.Waves
{
    /// <summary>
    /// The run's wave rules, advanced by game time. Every wave, wave 1 included, goes:
    /// Wave Pause (empty arena) → its enemies fade in for Spawn Fade Time → fighting until
    /// Enemies Left reaches 0 → cleared. A wave is cleared the moment its last enemy starts
    /// dying (<see cref="ReportEnemyDestroyed"/>), and at that moment, in this order, leftover
    /// projectiles are cleared and (except after the final wave) hearts are restored. After the
    /// final wave the run is a Victory; a defeated player makes it a Defeat. Both are final:
    /// nothing more ever spawns.
    /// <para>
    /// Current Wave is the wave being played or about to appear: a pause belongs to the wave
    /// that follows it, so Current Wave reads 1 during the first pause and becomes 2 the moment
    /// wave 1 is cleared.
    /// </para>
    /// <para>
    /// Time steps of any size are fine: a step that crosses the end of the pause and the end of
    /// the fade-in carries its leftover time on, so nothing drifts.
    /// </para>
    /// <para>
    /// Victory and Defeat on the same frame: whichever is reported first is final and the other
    /// report is ignored. A Victory means the last enemy is gone and the projectiles are cleared,
    /// so nothing can hurt the player any more and a later defeat report is ignored. A Defeat
    /// reported first stands (the player has already reached 0 hearts and faded out, F03), and a
    /// later kill no longer counts.
    /// </para>
    /// </summary>
    public sealed class WaveRun
    {
        // A boundary counts as reached when it is this close (seconds). It absorbs float
        // rounding, so for example 120 steps of 1/60 s reach 2 seconds.
        private const float BoundaryEpsilon = 0.00001f;

        private readonly float wavePause;
        private readonly float spawnFadeTime;
        private readonly int[] enemiesPerWave;

        /// <param name="wavePause">Seconds of empty arena before every wave, wave 1 included (spec: 2).</param>
        /// <param name="spawnFadeTime">Seconds a wave's enemies take to fade in (spec: 0.5). 0 means they appear at once.</param>
        /// <param name="enemiesPerWave">How many enemies each wave has, in order (spec: 3, 3, 4). At least one wave; each at least 1.</param>
        public WaveRun(float wavePause, float spawnFadeTime, IReadOnlyList<int> enemiesPerWave)
        {
            if (!(wavePause >= 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(wavePause), wavePause, "Must be 0 or more.");
            }

            if (!(spawnFadeTime >= 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(spawnFadeTime), spawnFadeTime, "Must be 0 or more.");
            }

            if (enemiesPerWave == null)
            {
                throw new ArgumentNullException(nameof(enemiesPerWave));
            }

            if (enemiesPerWave.Count == 0)
            {
                throw new ArgumentException("A run needs at least one wave.", nameof(enemiesPerWave));
            }

            this.enemiesPerWave = new int[enemiesPerWave.Count];
            for (int i = 0; i < enemiesPerWave.Count; i++)
            {
                if (enemiesPerWave[i] < 1)
                {
                    throw new ArgumentException($"Wave {i + 1} has {enemiesPerWave[i]} enemies; every wave needs at least 1.", nameof(enemiesPerWave));
                }

                this.enemiesPerWave[i] = enemiesPerWave[i];
            }

            this.wavePause = wavePause;
            this.spawnFadeTime = spawnFadeTime;
        }

        /// <param name="wavePause">Seconds of empty arena before every wave, wave 1 included (spec: 2).</param>
        /// <param name="spawnFadeTime">Seconds a wave's enemies take to fade in (spec: 0.5).</param>
        /// <param name="waves">The waves in order; only their enemy counts are used.</param>
        public WaveRun(float wavePause, float spawnFadeTime, IReadOnlyList<WaveDefinition> waves)
            : this(wavePause, spawnFadeTime, CountsOf(waves))
        {
        }

        /// <summary>
        /// The pause before a wave has ended: create that wave's enemies now (1-based wave
        /// number), in their fading-in state: harmless, not solid, not destroyable, no warning.
        /// </summary>
        public event Action<int> WaveSpawning;

        /// <summary>
        /// The wave's fade-in has ended: make its enemies solid and destroyable and start their
        /// attack cycles (their first-attack delay starts now). Also raised when the fade-in
        /// finishes after a defeat, so fading enemies do not stay half-visible.
        /// </summary>
        public event Action<int> FadeInFinished;

        /// <summary>A wave was cleared: remove every projectile at once, no fade. Raised first, for every wave.</summary>
        public event Action ClearProjectilesRequested;

        /// <summary>A wave other than the final one was cleared: restore hearts (F03). Raised after <see cref="ClearProjectilesRequested"/>.</summary>
        public event Action RestoreHeartsRequested;

        /// <summary>
        /// The given wave (1-based) was cleared. Announcement for later features (feedback,
        /// HUD); raised after the clear and restore requests, for every wave including the final one.
        /// </summary>
        public event Action<int> WaveCleared;

        /// <summary>Current Wave changed (F07 HUD), with the new number. Not raised for the initial 1.</summary>
        public event Action<int> CurrentWaveChanged;

        /// <summary>The final wave was cleared: Run State is now Victory (F06).</summary>
        public event Action VictoryReached;

        /// <summary>The player was defeated: Run State is now Defeat (F06).</summary>
        public event Action DefeatReached;

        /// <summary>How many waves the run has.</summary>
        public int WaveCount => enemiesPerWave.Length;

        /// <summary>The wave being played or about to appear, 1-based. 1 during the first pause.</summary>
        public int CurrentWave { get; private set; } = 1;

        /// <summary>Enemies of the current wave not yet destroyed. 0 during a pause; the wave's count from the moment it starts fading in.</summary>
        public int EnemiesLeft { get; private set; }

        public RunState RunState { get; private set; } = RunState.Playing;

        public WavePhase Phase { get; private set; } = WavePhase.Pause;

        /// <summary>Seconds spent in the current phase, including leftover carried over from the step that entered it.</summary>
        public float TimeInPhase { get; private set; }

        /// <summary>True while the current wave's enemies are fading in.</summary>
        public bool IsFadingIn => Phase == WavePhase.FadingIn;

        /// <summary>
        /// How far the current wave's fade-in has got: 0 during a pause, rising linearly from 0 to
        /// 1 while fading in, 1 once the enemies have fully appeared and after the run has ended.
        /// </summary>
        public float FadeInProgress
        {
            get
            {
                switch (Phase)
                {
                    case WavePhase.Pause:
                        return 0f;
                    case WavePhase.FadingIn:
                        if (!(spawnFadeTime > 0f))
                        {
                            return 1f;
                        }

                        float t = TimeInPhase / spawnFadeTime;
                        return t < 0f ? 0f : (t > 1f ? 1f : t);
                    default:
                        return 1f;
                }
            }
        }

        /// <summary>
        /// Advances the pause and fade-in timers by game time. Call once per frame. Steps of 0
        /// or less are ignored. One step may end the pause and the fade-in together; the events
        /// are raised in order inside this call.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (!(deltaTime > 0f) || Phase == WavePhase.Over)
            {
                return;
            }

            TimeInPhase += deltaTime;

            while (true)
            {
                switch (Phase)
                {
                    case WavePhase.Pause:
                        if (!Reached(wavePause))
                        {
                            return;
                        }

                        Enter(WavePhase.FadingIn, Leftover(wavePause));
                        EnemiesLeft = enemiesPerWave[CurrentWave - 1];
                        WaveSpawning?.Invoke(CurrentWave);
                        break;

                    case WavePhase.FadingIn:
                        if (!Reached(spawnFadeTime))
                        {
                            return;
                        }

                        Enter(RunState == RunState.Playing ? WavePhase.Fighting : WavePhase.Over, Leftover(spawnFadeTime));
                        FadeInFinished?.Invoke(CurrentWave);
                        break;

                    default:
                        // Fighting ends only through kills; Over never changes.
                        return;
                }
            }
        }

        /// <summary>
        /// An enemy of the current wave has started its death fade. Call once per enemy, at the
        /// moment it is destroyed. Ignored (returns false) unless the wave is being fought: fading
        /// enemies cannot be destroyed, and after Victory or Defeat nothing counts. When this was
        /// the last enemy the wave is cleared inside this call, exactly once.
        /// </summary>
        /// <returns>True if the kill was counted.</returns>
        public bool ReportEnemyDestroyed()
        {
            if (RunState != RunState.Playing || Phase != WavePhase.Fighting || EnemiesLeft <= 0)
            {
                return false;
            }

            EnemiesLeft--;
            if (EnemiesLeft == 0)
            {
                ClearWave();
            }

            return true;
        }

        /// <summary>
        /// The player reached 0 hearts. Run State becomes Defeat and no further wave appears. A
        /// fade-in already running finishes (raising <see cref="FadeInFinished"/>) so those enemies
        /// are fully visible; everything else stops at once. Ignored (returns false) after Victory
        /// or a previous Defeat.
        /// </summary>
        /// <returns>True if this report ended the run.</returns>
        public bool ReportPlayerDefeated()
        {
            if (RunState != RunState.Playing)
            {
                return false;
            }

            RunState = RunState.Defeat;
            if (Phase != WavePhase.FadingIn)
            {
                Enter(WavePhase.Over, 0f);
            }

            DefeatReached?.Invoke();
            return true;
        }

        private void ClearWave()
        {
            int clearedWave = CurrentWave;
            bool isFinalWave = clearedWave == enemiesPerWave.Length;

            // State first, so every subscriber below sees the run as it now is.
            if (isFinalWave)
            {
                RunState = RunState.Victory;
                Enter(WavePhase.Over, 0f);
            }
            else
            {
                CurrentWave = clearedWave + 1;
                Enter(WavePhase.Pause, 0f);
            }

            ClearProjectilesRequested?.Invoke();
            if (!isFinalWave)
            {
                RestoreHeartsRequested?.Invoke();
            }

            WaveCleared?.Invoke(clearedWave);

            if (isFinalWave)
            {
                VictoryReached?.Invoke();
            }
            else
            {
                CurrentWaveChanged?.Invoke(CurrentWave);
            }
        }

        private void Enter(WavePhase phase, float timeAlreadySpent)
        {
            Phase = phase;
            TimeInPhase = timeAlreadySpent;
        }

        private bool Reached(float duration) => TimeInPhase >= duration - BoundaryEpsilon;

        private float Leftover(float duration) => Math.Max(0f, TimeInPhase - duration);

        private static int[] CountsOf(IReadOnlyList<WaveDefinition> waves)
        {
            if (waves == null)
            {
                throw new ArgumentNullException(nameof(waves));
            }

            var counts = new int[waves.Count];
            for (int i = 0; i < waves.Count; i++)
            {
                if (waves[i] == null)
                {
                    throw new ArgumentException($"Wave {i + 1} is missing.", nameof(waves));
                }

                counts[i] = waves[i].Count;
            }

            return counts;
        }
    }
}
