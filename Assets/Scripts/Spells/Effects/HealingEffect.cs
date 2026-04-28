using UnityEngine;

[CreateAssetMenu(fileName = "HealingEffect", menuName = "Spells/Effects/HealingEffect")]
public class HealingEffect : SpellEffect
{
    [SerializeField] private float totalHeal = 50f;
    [SerializeField] private float duration = 5f;

    public override void Cast(SpellCastContext context)
    {
        if (context.casterStats == null)
            return;

        context.casterStats.StartHealingOverTime(totalHeal, duration);
    }
}