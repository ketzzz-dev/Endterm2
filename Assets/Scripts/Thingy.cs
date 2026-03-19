using UnityEngine;

public class Thingy : MonoBehaviour
{
	public float playerSpeed = 10f;
	public float jumpForce = 10f;
	public bool isGrounded;

	//Uncomment the bool you will be using 
	public bool isJumping;
	public bool isRunning;

	public Rigidbody2D rigid;
	public SpriteRenderer sr;

	public Animator animator;


	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		rigid = GetComponent<Rigidbody2D>();
		sr = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();

	}

	// Update is called once per frame
	void Update()
	{

		//Returns a value of 1 and 0 
		float movementSpeed = Input.GetAxis("Horizontal");
		Debug.Log(movementSpeed);

		// {Parameter Name, {Parameter Value}}
		// Movement Animation 
		animator.SetFloat("movement", Mathf.Abs(movementSpeed));



		rigid.linearVelocity = new Vector2(playerSpeed * movementSpeed, rigid.linearVelocity.y);


		//Uncomment this if you choose the idle -> walk -> run
		if(Input.GetKey(KeyCode.LeftShift)){
			isRunning = true;
			playerSpeed += 5;
		}
		else
		{
			isRunning = false;
			playerSpeed = 10f;
		}

		//Uncomment this if you choose the idle -> run -> jump
		if(Input.GetKeyDown(KeyCode.Space) && isGrounded){
			rigid.linearVelocity = new Vector2(rigid.linearVelocityX, jumpForce);
			isGrounded = false;
		}

		//Flipping Mechanics
		if (movementSpeed < -0.01)
		{
			sr.flipX = true;
		}
		else if (movementSpeed > 0.01)
		{
			sr.flipX = false;
		}
	}
	
	private void OnCollisionEnter2D(Collision2D col)
	{
		if (col.gameObject.CompareTag("Ground"))
		{
			isGrounded = true;
		}
	}
}


// IF YOU WANT TO PROVE YOURSELF WORTHY YOU NEED TO CHALLENGE YOURSELF HARD
    // bobo