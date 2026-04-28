using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    public static GameLoop Instance { get; private set; }

    [Header("Wave Settings")]
    [SerializeField] private int wavesPerLevel = 4;
    [SerializeField] private float countdownBeforeWave = 3f;

    [Header("Enemy Scaling")]
    [Tooltip("Enemies spawned on wave 1 of level 1.")]
    [SerializeField] private int baseEnemyCount = 5;
    [Tooltip("Additional enemies added per wave within a level.")]
    [SerializeField] private int enemiesPerWaveIncrement = 3;
    [Tooltip("Additional enemies added per level.")]
    [SerializeField] private int enemiesPerLevelIncrement = 5;

    [Header("References")]
    [SerializeField] private EnemySpawner spawner;


    // ── Public events for HUD and other systems ──────────────────────────────
    public static event Action<int, int> OnWaveStarted;   // (waveNumber, levelNumber)
    public static event Action<int>      OnWaveCompleted; // (waveNumber)
    public static event Action<int>      OnLevelCompleted;// (levelNumber)
    public static event Action<float>    OnCountdownTick; // seconds remaining

    // ── Read-only state for HUD queries ──────────────────────────────────────
    public int CurrentWave  { get; private set; }
    public int CurrentLevel { get; private set; } = 1;
    public int EnemiesRemaining { get; private set; }

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()  => Enemy.OnEnemyDied += HandleEnemyDied;
    private void OnDisable() => Enemy.OnEnemyDied -= HandleEnemyDied;

    private void Start() => StartCoroutine(RunGameLoop());

    // ── Main loop ─────────────────────────────────────────────────────────────

    private IEnumerator RunGameLoop()
    {
        while (true)
        {
            for (CurrentWave = 1; CurrentWave <= wavesPerLevel; CurrentWave++)
            {
                // 1. Count down before the wave starts
                yield return StartCoroutine(Countdown(countdownBeforeWave));

                // 2. Run the wave — coroutine doesn't return until all enemies die
                yield return StartCoroutine(RunWave());
            }

            // 3. Level complete (spell selection already happened after the last wave)
            OnLevelCompleted?.Invoke(CurrentLevel);

            yield return new WaitForSeconds(3f);

            CurrentLevel++;
        }
    }

    // ── Wave phase ────────────────────────────────────────────────────────────

    private IEnumerator RunWave()
    {
        // Calculate how many enemies to spawn, scaling with both wave and level
        int count = baseEnemyCount
                  + (CurrentWave - 1) * enemiesPerWaveIncrement
                  + (CurrentLevel - 1) * enemiesPerLevelIncrement;

        EnemiesRemaining = count;
        OnWaveStarted?.Invoke(CurrentWave, CurrentLevel);

        var totalWaves = (CurrentLevel - 1) * wavesPerLevel + CurrentWave;

        spawner.SpawnWave(count, totalWaves);

        // Simply wait — HandleEnemyDied decrements EnemiesRemaining
        yield return new WaitUntil(() => EnemiesRemaining <= 0);

        OnWaveCompleted?.Invoke(CurrentWave);
    }

    private void HandleEnemyDied(Enemy enemy)
    {
        EnemiesRemaining = Mathf.Max(0, EnemiesRemaining - 1);
    }
    
    // ── Countdown helper ──────────────────────────────────────────────────────

    private IEnumerator Countdown(float duration)
    {
        float remaining = duration;
        while (remaining > 0f)
        {
            OnCountdownTick?.Invoke(remaining);
            yield return new WaitForSeconds(1f);
            remaining -= 1f;
        }
    }
}