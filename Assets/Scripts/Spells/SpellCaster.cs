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
            if (spell != null && !string.IsNullOrEmpty(spell.symbolId))
                spellMap[spell.symbolId.ToLowerInvariant()] = spell;
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
        if (spellMap.TryGetValue(symbolId, out var spell))
        {
            if (!CanCast(spell))
            {
                Debug.Log($"Spell {spell.symbolId} is on cooldown.");

                return;
            }

            Debug.Log($"Casting spell: {symbolId} at {position}");

            spell.Cast(new SpellCastContext
            {
                origin = transform.position,
                target = position
            });

            StartCooldown(spell);
        }
        else
        {
            Debug.LogWarning($"No spell found for symbol: {symbolId}");
        }
    }

    private bool CanCast(SpellDefinition spell)
    {
        if (spell == null)
            return false;

        if (cooldowns.TryGetValue(spell.symbolId, out var cooldownEndTime))
        {
            if (Time.time < cooldownEndTime)
                return false;
        }
        if (playerStats.currentMana < spell.manaCost)
        {
            Debug.Log("Not enough mana to cast the spell.");

            return false;
        }

        return true;
    }

    private void StartCooldown(SpellDefinition spell)
    {
        if (spell != null)
            cooldowns[spell.symbolId] = Time.time + spell.cooldown;
    }
}
