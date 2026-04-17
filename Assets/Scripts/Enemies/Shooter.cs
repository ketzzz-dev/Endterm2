using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Shooter : Enemy
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float acceleration = 10f;

    [Header("Shooting")]
    [SerializeField] private float orbitRadius = 2f;
    [SerializeField] private float shootCooldown = 2f;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float bulletDuration = 2f;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    private float shootTimer;

    private new Rigidbody2D rigidbody;
    private Transform player;

    protected override void Awake()
    {
        base.Awake();

        rigidbody = GetComponent<Rigidbody2D>();
    }
    protected override void Start()
    {
        base.Start();

        shootTimer = shootCooldown;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected override void Update()
    {
        base.Update();

        if (shootTimer > 0f)
            shootTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (player == null)
            return;

        var direction = (player.position - transform.position).normalized;
        var targetPosition = player.position - direction * orbitRadius;

        var targetVelocity = isDying ? Vector3.zero : (targetPosition - transform.position).normalized * moveSpeed;
        var accelerationFactor = 1f - Mathf.Exp(-acceleration * Time.fixedDeltaTime);

        rigidbody.linearVelocity = Vector2.Lerp(rigidbody.linearVelocity, targetVelocity, accelerationFactor);

        if (shootTimer <= 0f)
        {
            animator.SetTrigger("Shoot");

            shootTimer = shootCooldown;
        }
    }

    private void OnAttackAnimationShoot()
    {
        if (player == null)
            return;

        var direction = (player.position - firePoint.position).normalized;
        var bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        var rb = bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.linearVelocity = direction * bulletSpeed;

        Destroy(bullet, bulletDuration);
    }
}
