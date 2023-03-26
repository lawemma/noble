using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class handles the input and physics related to the movement of the player
public class PlayerMovement : MonoBehaviour
{
    // amount of force applied when jumping
    [SerializeField]
    float jumpForce;

    // amount of force applied when dashing
    [SerializeField]
    float dashForce;

    // the movement speed of the player
    [SerializeField]
    float playerSpeed;

    // bool that reflects if the player is touching the ground
    [SerializeField]
    bool isGrounded;

    // gets the direction the player is facing
    [SerializeField]
    bool isFlipped;

    // where the player loads in when the scene starts or when respawning
    [SerializeField]
    Vector2 spawnLocation;

    // vars that are initialized in start
    private Rigidbody2D rb;
    private int jumpCounter;
    private bool inputIsEnabled;
    private float dashTime;
    private float dashTimeDelta;
    private bool isFacingAndTouchingWall;

    // vars that are initialized elsewhere
    private Vector2 speed;
    private float horizontal;
    private bool canJump;
    private bool canDash;

    // start method
    private void Start()
    {
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        dashTime = 0.6f; // should last for approximately 60 frames
        dashTimeDelta = dashTime;
        jumpCounter = 0;
        jumpForce = 275f;
        playerSpeed = 3f;
        inputIsEnabled = true;
        isGrounded = true;
        isFlipped = false;
        isFacingAndTouchingWall = false;
        spawnLocation = this.transform.position;
    }

    // update function
    private void Update()
    {
        // check and see if the player is on the lowest platform
        if (this.transform.position.y <= 1.25f)
            isGrounded = true;

        // Check and see if the player can take in input

        if (isGrounded)
        {
            inputIsEnabled = true;
            isFacingAndTouchingWall = false;
        }

        // get the direction that the player is facing
        horizontal = Input.GetAxisRaw("Horizontal");

        // sets the status if the player can jump or not
        if (Input.GetKeyDown(KeyCode.X) && jumpCounter < 1)
        {
            canJump = true;
        }

        // half the y velocity of the player and set can jump to false
        if (Input.GetKeyUp(KeyCode.X))
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / 2);
            canJump = false;
        }

        // start dashing
        if (Input.GetKeyDown(KeyCode.Z) && jumpCounter == 0 && inputIsEnabled)
        {
            canDash = true;
        }

        // decrement dash time
        if (canDash)
            dashTimeDelta -= Time.deltaTime;

        // reset dash after dash time is less than or equal to 0
        if (dashTimeDelta <= 0 && isGrounded)
        {
            canDash = false;
            dashTimeDelta = dashTime;
        }

        // Stop dashing, when not holding down the dash button
        if ((!Input.GetKey(KeyCode.Z) && isGrounded))
        {
            canDash = false;
        }
    }

    // fixed update, physics operations done here
    private void FixedUpdate()
    {
        // set the direction the player is facing
        if (!isFlipped && horizontal == -1)
        {
            this.transform.Rotate(new Vector3(0, 180, 0));
            isFlipped = true;
            if (!inputIsEnabled)
                isFacingAndTouchingWall = !isFacingAndTouchingWall;
        }
        else if (isFlipped && horizontal == 1)
        {
            this.transform.Rotate(new Vector3(0, -180, 0));
            isFlipped = false;
            if (!inputIsEnabled)
                isFacingAndTouchingWall = !isFacingAndTouchingWall;
        }

        // get the player speed
        if (canDash)
        {
            speed = new Vector2(horizontal * playerSpeed * 2, rb.velocity.y);
        }
        else if (inputIsEnabled || !isFacingAndTouchingWall)
        {
            speed = new Vector2(horizontal * playerSpeed, rb.velocity.y);
        }
        else
        {
            speed = new Vector2(0, rb.velocity.y);
        }

        rb.velocity = speed;

        // Do jump if the player can jump
        if (canJump)
        {
            rb.gravityScale = 1;
            rb.AddForce(new Vector2(0, jumpForce));
            jumpCounter++;
            canJump = false;
        }
    }

    // check for collisions
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            isFacingAndTouchingWall = true;
            canDash = false;
        }

        if (collision.gameObject.CompareTag("Platform"))
        {
            jumpCounter = 0;
            if (inputIsEnabled)
                isGrounded = true;
        }
    }

    // check for continous collisions
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall") && !isGrounded)
        {
            inputIsEnabled = false;
        }
    }

    // check for the end of collisions
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
            isGrounded = false;
        else if (collision.gameObject.CompareTag("Wall"))
        {
            inputIsEnabled = true;
            isFacingAndTouchingWall = false;
        }
    }

    // check if entering certain triggers
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Trap") || collider.gameObject.CompareTag("Enemy"))
        {
            rb.velocity = new Vector2(0, 0);
            if (isFlipped)
            {
                this.transform.Rotate(new Vector3(0, -180, 0));
                isFlipped = false;
            }
            this.transform.position = spawnLocation;
        }
    }

    // return true if is grounded, else return false
    public bool GetIsGrounded()
    {
        return isGrounded;
    }

    // return true if can dash, else return false
    public bool GetIfCanDash()
    {
        return canDash;
    }
}
