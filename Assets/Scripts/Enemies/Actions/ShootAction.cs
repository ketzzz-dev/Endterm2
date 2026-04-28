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
            var projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.Euler(0f, 0f, Mathf.Atan2(enemy.context.directionToPlayer.y, enemy.context.directionToPlayer.x) * Mathf.Rad2Deg));
            
            if (projectile.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.linearVelocity = enemy.context.directionToPlayer * projectileSpeed;
            }

            Destroy(projectile, 5f);
        }
    }
}