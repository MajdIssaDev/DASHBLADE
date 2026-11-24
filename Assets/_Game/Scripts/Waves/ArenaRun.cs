using System;
using System.Collections.Generic;
using Dashblade.Combat;
using Dashblade.Common;
using Dashblade.Enemies;
using UnityEngine;

namespace Dashblade.Waves
{
    /// <summary>
    /// Thin scene wrapper for one run of the arena (F05), the "Run" object in Arena.unity. The wave rules live in
    /// <see cref="WaveRun"/> and the layout choice in <see cref="LayoutPicker"/>; this component carries them out in
    /// the scene: it places the run's pillars, creates each wave's enemies fading in (dormant meanwhile), starts their
    /// attack cycles when the fade-in ends, counts their deaths, removes the projectiles and restores the hearts when
    /// a wave is cleared, and announces wave changes, victory and defeat.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A fresh run starts whenever the scene loads: the layout is picked and the pillars placed in Awake, and nothing
    /// is kept in static state, so reloading the scene is a full restart.
    /// </para>
    /// <para>
    /// Projectiles: every Turret this run creates reports each shot through <see cref="Turret.Fired"/>, and the run
    /// keeps those shots in a list. Clearing a wave removes every shot still in the list, including shots from
    /// Turrets that have died since, without searching the scene.
    /// </para>
    /// <para>
    /// Deaths come from each enemy's <see cref="DashTarget.Killed"/> event and defeat from
    /// <see cref="PlayerHealth.Defeated"/>; both are handled inside the frame they happen. The timers run after the
    /// player, the dash and the damage sources, so a wave spawned or started this frame is first updated next frame.
    /// </para>
    /// <para>Freezing time (Time.timeScale = 0) freezes the run with everything else.</para>
    /// </remarks>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(PlayerHealth.ExecutionOrder + 2)]
    public sealed class ArenaRun : MonoBehaviour
    {
        [SerializeField] private WaveConfig waveConfig;

        [SerializeField] private ArenaConfig arenaConfig;

        [Tooltip("The player the enemies attack, whose defeat ends the run and whose hearts are restored between waves.")]
        [SerializeField] private PlayerHealth player;

        [SerializeField] private Charger chargerPrefab;

        [SerializeField] private Turret turretPrefab;

        [Tooltip("A 2 Ã— 2 solid square; placed once per pillar of the run's layout.")]
        [SerializeField] private GameObject pillarPrefab;

        [Tooltip("The wave being played or about to appear. Shown for checking; not editable.")]
        [ReadOnlyInInspector]
        [SerializeField] private int currentWave;

        [Tooltip("Enemies of the current wave not yet destroyed (0 during a pause). Shown for checking; not editable.")]
        [ReadOnlyInInspector]
        [SerializeField] private int enemiesLeft;

        [Tooltip("Playing, Victory or Defeat. Shown for checking; not editable.")]
        [ReadOnlyInInspector]
        [SerializeField] private RunState runState;

        [Tooltip("The pillar layout this run uses. Shown for checking; not editable.")]
        [ReadOnlyInInspector]
        [SerializeField] private ArenaLayout layout;

        private readonly List<FadingSprite> fadingSprites = new List<FadingSprite>();
        private readonly List<Action> pendingStarts = new List<Action>();
        private readonly List<Projectile> shots = new List<Projectile>();
        private readonly List<SpriteRenderer> spriteBuffer = new List<SpriteRenderer>();

        private WaveRun run;
        private WaveDefinition[] waves;

        /// <summary>The wave being played or about to appear, 1-based (1 during the first pause).</summary>
        public int CurrentWave => run != null ? run.CurrentWave : 0;

        /// <summary>Enemies of the current wave not yet destroyed; 0 during a pause.</summary>
        public int EnemiesLeft => run != null ? run.EnemiesLeft : 0;

        /// <summary>How many waves the run has.</summary>
        public int WaveCount => run != null ? run.WaveCount : 0;

        public RunState RunState => run != null ? run.RunState : RunState.Playing;

        /// <summary>The pillar layout of this run (picked in Awake).</summary>
        public ArenaLayout Layout => layout;

        /// <summary>
        /// Raised with the new number the moment a wave other than the last is cleared (Current Wave then names the
        /// next wave, which appears after the Wave Pause). Not raised for the first wave: read <see cref="CurrentWave"/>.
        /// </summary>
        public event Action<int> CurrentWaveChanged;

        /// <summary>Raised once, the moment the last enemy of the final wave starts dying. Run State is Victory.</summary>
        public event Action VictoryReached;

        /// <summary>Raised once, the moment the player loses the last heart. Run State is Defeat.</summary>
        public event Action DefeatReached;

        private void Awake()
        {
            if (waveConfig == null || arenaConfig == null || player == null
                || chargerPrefab == null || turretPrefab == null || pillarPrefab == null)
            {
                Debug.LogError($"{nameof(ArenaRun)} on '{name}' needs its configs, player, Charger, Turret and pillar prefabs assigned.", this);
                enabled = false;
                return;
            }

            try
            {
                waves = waveConfig.BuildWaves();
                run = new WaveRun(waveConfig.WavePause, waveConfig.SpawnFadeTime, waves);
            }
            catch (ArgumentException exception)
            {
                Debug.LogError($"{nameof(ArenaRun)} on '{name}': invalid {nameof(WaveConfig)} ({exception.Message}).", this);
                run = null;
                enabled = false;
                return;
            }

            run.WaveSpawning += OnWaveSpawning;
            run.FadeInFinished += OnFadeInFinished;
            run.ClearProjectilesRequested += ClearProjectiles;
            run.RestoreHeartsRequested += OnRestoreHeartsRequested;
            run.CurrentWaveChanged += OnCurrentWaveChanged;
            run.VictoryReached += OnVictoryReached;
            run.DefeatReached += OnDefeatReached;
            player.Defeated += OnPlayerDefeated;

            // Once per run, kept for all three waves.
            layout = LayoutPicker.Pick(arenaConfig.ForceLayout, n => UnityEngine.Random.Range(0, n));
            PlacePillars();
            ShowState();
        }

        private void OnDestroy()
        {
            if (player != null)
            {
                player.Defeated -= OnPlayerDefeated;
            }
        }

        private void Update()
        {
            run.Tick(Time.deltaTime);
            ApplyFade();
            ShowState();
        }

        private void PlacePillars()
        {
            foreach (Vector2 centre in arenaConfig.PillarCentres(layout))
            {
                GameObject pillar = Instantiate(pillarPrefab, new Vector3(centre.x, centre.y, 0f), Quaternion.identity, transform);
                pillar.name = $"{pillarPrefab.name} {layout} ({centre.x:0.##}, {centre.y:0.##})";
            }
        }

        private void OnWaveSpawning(int wave)
        {
            foreach (WaveEnemy entry in waves[wave - 1].Enemies)
            {
                var position = new Vector3(entry.X, entry.Y, 0f);
                switch (entry.Kind)
                {
                    case EnemyKind.Charger:
                        Charger charger = Instantiate(chargerPrefab, position, Quaternion.identity);
                        charger.Initialise(player);
                        Track(charger.gameObject, charger.Target, charger.StartCycle);
                        break;

                    case EnemyKind.Turret:
                        Turret turret = Instantiate(turretPrefab, position, Quaternion.identity);
                        turret.Initialise(player);
                        turret.Fired += OnShotFired;
                        Track(turret.gameObject, turret.Target, turret.StartCycle);
                        break;
                }
            }

            // Fully transparent at once (or further along, if this frame's step went past the end of the pause).
            ApplyFade();
        }

        /// <summary>
        /// Remembers a new, dormant enemy: its death counts for the wave, its visible sprites fade in, and its cycle
        /// starts when the fade-in ends.
        /// </summary>
        private void Track(GameObject enemy, DashTarget target, Action startCycle)
        {
            target.Killed += OnEnemyKilled;
            pendingStarts.Add(startCycle);

            // Only what is drawn now (the body); hidden warning sprites keep their own colours. The colour the death
            // fade uses was read in Awake, before this, so it stays fully opaque.
            enemy.GetComponentsInChildren(spriteBuffer);
            foreach (SpriteRenderer sprite in spriteBuffer)
            {
                if (sprite.enabled)
                {
                    fadingSprites.Add(new FadingSprite(sprite));
                }
            }

            spriteBuffer.Clear();
        }

        private void ApplyFade()
        {
            float progress = run.FadeInProgress;
            foreach (FadingSprite fading in fadingSprites)
            {
                fading.Apply(progress);
            }
        }

        private void OnFadeInFinished(int wave)
        {
            foreach (FadingSprite fading in fadingSprites)
            {
                fading.Apply(1f);
            }

            fadingSprites.Clear();

            // From now solid, destroyable and attacking; the first-attack delay starts here. After a defeat the
            // enemies see the defeated player at once and stay idle (F04).
            foreach (Action startCycle in pendingStarts)
            {
                startCycle();
            }

            pendingStarts.Clear();
        }

        private void OnEnemyKilled(DashTarget target)
        {
            target.Killed -= OnEnemyKilled;
            run.ReportEnemyDestroyed();
            ShowState();
        }

        private void OnShotFired(Projectile shot)
        {
            shots.RemoveAll(s => s == null);
            shots.Add(shot);
        }

        /// <summary>Removes every projectile at once, with no fade: hidden now, destroyed at the end of the frame.</summary>
        private void ClearProjectiles()
        {
            foreach (Projectile shot in shots)
            {
                if (shot != null)
                {
                    // Inactive at once, so it cannot hit anything later this frame.
                    shot.gameObject.SetActive(false);
                    Destroy(shot.gameObject);
                }
            }

            shots.Clear();
        }

        private void OnRestoreHeartsRequested()
        {
            player.RestoreHearts();
        }

        private void OnPlayerDefeated()
        {
            run.ReportPlayerDefeated();
            ShowState();
        }

        private void OnCurrentWaveChanged(int wave)
        {
            ShowState();
            CurrentWaveChanged?.Invoke(wave);
        }

        private void OnVictoryReached()
        {
            ShowState();
            VictoryReached?.Invoke();
        }

        private void OnDefeatReached()
        {
            ShowState();
            DefeatReached?.Invoke();
        }

        private void ShowState()
        {
            if (run == null)
            {
                return;
            }

            currentWave = run.CurrentWave;
            enemiesLeft = run.EnemiesLeft;
            runState = run.RunState;
        }

        /// <summary>One sprite fading in: its alpha goes from 0 to its own alpha.</summary>
        private readonly struct FadingSprite
        {
            private readonly SpriteRenderer sprite;
            private readonly float fullAlpha;

            public FadingSprite(SpriteRenderer sprite)
            {
                this.sprite = sprite;
                fullAlpha = sprite.color.a;
            }

            public void Apply(float progress)
            {
                if (sprite == null)
                {
                    return;
                }

                Color color = sprite.color;
                color.a = fullAlpha * Mathf.Clamp01(progress);
                sprite.color = color;
            }
        }
    }
}
