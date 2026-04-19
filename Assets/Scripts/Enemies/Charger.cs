using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Charger : Enemy
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float acceleration = 10f;

    [Header("Charging")]
    [SerializeField] private float chargeSpeed = 10f;
    [SerializeField] private float chargeCooldown = 10f;

    private bool isCharging;
    private float chargeTimer;

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

        chargeTimer = chargeCooldown;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected override void Update()
    {
        base.Update();

        if (chargeTimer > 0f)
            chargeTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (player == null)
            return;

        var direction = (player.position - transform.position).normalized;
        var targetPosition = player.position - direction * 0.5f;

        var targetVelocity = (isDying || isCharging) ? Vector3.zero : (targetPosition - transform.position).normalized * moveSpeed;
        var accelerationFactor = 1f - Mathf.Exp(-acceleration * Time.fixedDeltaTime);

        rigidbody.linearVelocity = Vector2.Lerp(rigidbody.linearVelocity, targetVelocity, accelerationFactor);

        if (!isCharging && chargeTimer <= 0f)
        {
            animator.SetTrigger("Charge");

            isCharging = true;
            chargeTimer = chargeCooldown;
        }
    }

    private void OnChargeAnimationCharge()
    {
        if (player == null)
            return;

        var direction = (player.position - transform.position).normalized;
        
        rigidbody.linearVelocity = direction * chargeSpeed;
    }

    private void OnChargeAnimationEnd()
    {
        isCharging = false;
    }
}
