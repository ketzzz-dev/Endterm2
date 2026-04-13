using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileEffect", menuName = "Spells/Effects/ProjectileEffect")]
public class ProjectileEffect : SpellEffect
{
    public GameObject projectilePrefab;

    public float projectileSpeed = 10f;
    public float projectileDuration = 2f;

    public override void Cast(SpellCastContext context)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile prefab is not assigned.");
            
            return;
        }

        Vector3 direction = (context.target - context.origin).normalized;
        GameObject projectile = Instantiate(projectilePrefab, context.origin, Quaternion.LookRotation(direction));
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = direction * projectileSpeed;
        }

        Destroy(projectile, projectileDuration);
    }
}
