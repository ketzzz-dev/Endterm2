using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;

    private new Rigidbody2D rigidbody;
    private Animator animator;
    
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        var moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        var targetVelocity = moveInput.normalized * speed;
        
        var isMoving = moveInput.sqrMagnitude > 0f;
        
        var accelerationRate = isMoving ? acceleration : deceleration;
        var t = 1f - Mathf.Exp(-accelerationRate * Time.fixedDeltaTime);
        
        rigidbody.linearVelocity = Vector2.Lerp(rigidbody.linearVelocity, targetVelocity, t);

        if (isMoving)
        {
            animator.SetFloat("LastDirectionX", rigidbody.linearVelocity.x);
            animator.SetFloat("LastDirectionY", rigidbody.linearVelocity.y);
        }

        animator.SetBool("IsRunning", rigidbody.linearVelocity.sqrMagnitude > 0.1f);
    }
}
