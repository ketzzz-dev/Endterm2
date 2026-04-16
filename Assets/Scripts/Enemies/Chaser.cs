using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Chaser : Enemy
{
    [SerializeField] private float moveSpeed = 3f;

    private new Rigidbody2D rigidbody;
    private Transform player;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void FixedUpdate()
    {
        if (player == null)
            return;

        Vector2 direction = (player.position - transform.position).normalized;

        rigidbody.linearVelocity = direction * moveSpeed;
    }
}