using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnEntry
    {
        public GameObject prefab;

        [Tooltip("This enemy type won't appear until this wave number.")]
        public int minWave = 1;

        [Tooltip("Higher weight = more likely to be chosen when eligible.")]
        [Min(0.01f)] public float weight = 1f;
    }

    [Header("Enemy Types")]
    [SerializeField] private List<SpawnEntry> spawnTable;

    [Header("Spawn Ring")]
    [Tooltip("Enemies spawn between these two radii around the player.")]
    [SerializeField] private float minRadius = 8f;
    [SerializeField] private float maxRadius = 12f;

    /// <summary>
    /// Spawns 'count' enemies appropriate for the given wave number.
    /// </summary>
    public void SpawnWave(int count, int waveNumber)
    {
        var player = PlayerReference.Instance;
        if (player == null) return;

        // Only use entries that have unlocked by this wave
        var eligible = spawnTable.FindAll(e => e.minWave <= waveNumber);
        if (eligible.Count == 0)
        {
            Debug.LogWarning($"No eligible enemy types for wave {waveNumber}.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            var entry = PickWeighted(eligible);
            var spawnPos = RandomRingPosition(player.transform.position);
            Instantiate(entry.prefab, spawnPos, Quaternion.identity);
        }
    }

    private SpawnEntry PickWeighted(List<SpawnEntry> entries)
    {
        float total = 0f;
        foreach (var e in entries) total += e.weight;

        float roll = Random.Range(0f, total);
        foreach (var e in entries)
        {
            roll -= e.weight;
            if (roll <= 0f) return e;
        }
        return entries[^1]; // safety fallback
    }

    private Vector2 RandomRingPosition(Vector2 center)
    {
        float angle  = Random.Range(0f, Mathf.PI * 2f);
        float radius = Random.Range(minRadius, maxRadius);
        return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    }
}