using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class SpellCaster : MonoBehaviour
{
    [SerializeField] private List<SpellDefinition> spells;

    private readonly Dictionary<string, SpellDefinition> spellMap = new();
    private readonly Dictionary<string, float> cooldowns = new();

    private PlayerStats playerStats;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        foreach (var spell in spells)
        {
            if (spell == null || string.IsNullOrWhiteSpace(spell.symbolId))
                continue;

            spellMap[NormalizeKey(spell.symbolId)] = spell;
        }
    }

    private void OnEnable()
    {
        SymbolInput.OnSymbolRecognized += TryCast;
    }

    private void OnDisable()
    {
        SymbolInput.OnSymbolRecognized -= TryCast;
    }

    private void TryCast(string symbolId, Vector3 position)
    {
        var key = NormalizeKey(symbolId);

        if (!spellMap.TryGetValue(key, out var spell))
        {
            Debug.LogWarning($"No spell found for symbol: {symbolId}");
            return;
        }

        if (!CanCast(spell))
        {
            Debug.Log($"Spell {spell.symbolId} cannot be cast right now.");
            return;
        }

        spell.Cast(new SpellCastContext
        {
            origin = transform.position,
            target = position,
            caster = gameObject,
            casterStats = playerStats
        });

        ApplyCosts(spell);
    }

    private bool CanCast(SpellDefinition spell)
    {
        if (spell == null)
            return false;

        var key = NormalizeKey(spell.symbolId);

        if (cooldowns.TryGetValue(key, out var cooldownEndTime) && Time.time < cooldownEndTime)
            return false;

        if (playerStats.currentMana < spell.manaCost)
        {
            Debug.Log("Not enough mana to cast the spell.");
            return false;
        }

        return true;
    }

    private void ApplyCosts(SpellDefinition spell)
    {
        if (spell == null)
            return;

        var key = NormalizeKey(spell.symbolId);
        cooldowns[key] = Time.time + spell.cooldown;
        playerStats.SpendMana(spell.manaCost);
    }

    private static string NormalizeKey(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}