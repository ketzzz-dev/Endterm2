using System.Collections.Generic;
using UnityEngine;

public class SpellCaster : MonoBehaviour
{
    [SerializeField] private List<SpellDefinition> spells;

    private readonly Dictionary<string, SpellDefinition> spellMap = new();

    private void Awake()
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

    public void TryCast(string symbolId)
    {
        if (spellMap.TryGetValue(symbolId, out var spell))
        {
            Debug.Log($"Casting spell: {symbolId}");
        }
        else
        {
            Debug.LogWarning($"No spell found for symbol: {symbolId}");
        }
    }
}
