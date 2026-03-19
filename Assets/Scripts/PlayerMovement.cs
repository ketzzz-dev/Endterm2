using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;

    private Rigidbody2D rigidbody;
    
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        var moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        var targetVelocity = moveInput.normalized * speed;
        
        var isMoving = moveInput.sqrMagnitude > 0f;
        
        var accelerationRate = isMoving ? acceleration : deceleration;
        var accelerationFactor = 1f - Mathf.Exp(-accelerationRate * Time.fixedDeltaTime);
        
        rigidbody.linearVelocity = Vector3.Lerp(rigidbody.linearVelocity, targetVelocity, accelerationFactor);
    }
}
