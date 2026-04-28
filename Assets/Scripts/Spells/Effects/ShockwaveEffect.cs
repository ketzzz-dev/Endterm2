using UnityEngine;

[CreateAssetMenu(fileName = "ShockwaveEffect", menuName = "Spells/Effects/ShockwaveEffect")]
public class ShockwaveEffect : SpellEffect
{
    [SerializeField] private float radius = 4f;
    [SerializeField] private float force = 12f;

    public override void Cast(SpellCastContext context)
    {
        var hits = Physics2D.OverlapCircleAll(context.origin, radius);

        foreach (var hit in hits)
        {
            if (hit == null || !hit.CompareTag("Enemy"))
                continue;

            if (!hit.attachedRigidbody)
                continue;

            var away = (Vector2)hit.transform.position - (Vector2)context.origin;
            if (away.sqrMagnitude < 0.0001f)
                away = Vector2.up;

            var falloff = 1f - Mathf.Clamp01(away.magnitude / radius);
            var impulse = force * Mathf.Max(0.25f, falloff);

            hit.attachedRigidbody.AddForce(away.normalized * impulse, ForceMode2D.Impulse);
        }
    }
}