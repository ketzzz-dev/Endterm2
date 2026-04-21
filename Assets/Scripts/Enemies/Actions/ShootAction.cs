using UnityEngine;

public class ShootAction : EnemyAction
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;
    
    private void Shoot()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            var projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            
            if (projectile.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.linearVelocity = enemy.context.directionToPlayer * projectileSpeed;
            }
        }
    }
}