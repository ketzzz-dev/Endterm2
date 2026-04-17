using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Chaser : Enemy
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float acceleration = 10f;

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

        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void FixedUpdate()
    {
        if (player == null)
            return;

        var direction = (player.position - transform.position).normalized;
        var targetVelocity = isDying ? Vector3.zero : direction * moveSpeed;
        var accelerationFactor = 1f - Mathf.Exp(-acceleration * Time.fixedDeltaTime);

        rigidbody.linearVelocity = Vector2.Lerp(rigidbody.linearVelocity, targetVelocity, accelerationFactor);
    }
}