using System.Collections.Generic;
using UnityEngine;

public struct SpellCastContext
{
    public Vector3 origin;
    public Vector3 target;
}

public abstract class SpellEffect : ScriptableObject
{
    public abstract void Cast(SpellCastContext context);
}

[CreateAssetMenu(fileName = "SpellDefinition", menuName = "Spells/SpellDefinition")]
public class SpellDefinition : ScriptableObject
{
    public string symbolId;

    public float manaCost;
    public float cooldown;

    public List<SpellEffect> effects;

    public void Cast(SpellCastContext context)
    {
        foreach (var effect in effects)
        {
            effect.Cast(context);
        }
    }
}
