using UnityEngine;

[CreateAssetMenu(fileName = "ShieldEffect", menuName = "Spells/Effects/ShieldEffect")]
public class ShieldEffect : SpellEffect
{
    [SerializeField] private float shieldDuration = 30f;

    public override void Cast(SpellCastContext context)
    {
        if (context.casterStats == null)
            return;

        context.casterStats.ActivateShield(shieldDuration);
    }
}