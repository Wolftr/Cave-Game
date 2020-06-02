using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//This code is pretty disorganized. I should probably fix that


public class PlayerController : MonoBehaviour
{

    private float movementInputDirection;
    private float verticalInputDirection;

    public int facingDirection = 1;

    private bool isFacingRight = true;
    private bool isWalking;
    private bool isGrounded;
    private bool canJump;
    private bool isTouchingWallLeft;
    private bool isTouchingWallRight;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool canGrab;
    private bool isGrabbing;
    private bool isClimbing;
    private bool canWallJump;
    private bool isChargingRight;

    private Rigidbody2D rb;
    private Animator anim;

    public float movementSpeed;
    public float jumpForce;
    public float jumpCount;
    public float groundCheckRadius;
    public float wallCheckDistance;
    public float wallSlideSpeed;
    public float variableJumpHightMultiplier;
    public float wallJumpForceX;
    public float wallJumpForceY;

    public Transform groundCheck;
    public Transform wallCheckLeft;
    public Transform wallCheckRight;

    public LayerMask WhatIsGround;

    // Start is called before the first frame update
    void Start()
    {

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {

        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
        CheckIfWallSliding();
        Grab();

    }

    private void FixedUpdate()
    {

        ApplyMovement();
        CheckSurroundings();

    }

    //Checks if wall sliding
    private void CheckIfWallSliding()
    {

        if (isTouchingWall && !isGrounded && rb.velocity.y < 0 && movementInputDirection != 0)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }

    }

    //Code for Grabbing
    private void Grab()
    {

        if (isTouchingWall && canGrab)
        {

            isGrabbing = true;
            isWallSliding = false;

        }
        else if (isTouchingWall && isGrabbing && verticalInputDirection != 0)
        {

            isWallSliding = false;
            isClimbing = true;

        }
        if (isTouchingWall && isGrabbing && verticalInputDirection != 0)
        {

            isWallSliding = false;
            isClimbing = true;

        }
        else if (isTouchingWall && canGrab)
        {

            isGrabbing = true;
            isWallSliding = false;

        }
        else
        {

            isGrabbing = false;
            isClimbing = false;

        }

    }

    //Collision detection
    private void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, WhatIsGround);

        isTouchingWallLeft = Physics2D.Raycast(wallCheckLeft.position, transform.right, (-facingDirection * wallCheckDistance), WhatIsGround);

        isTouchingWallRight = Physics2D.Raycast(wallCheckRight.position, transform.right, (facingDirection * wallCheckDistance), WhatIsGround);

        if (isTouchingWallLeft | isTouchingWallRight)
        {

            isTouchingWall = true;

        }
        else
        {

            isTouchingWall = false;

        }

    }


    //Updates animations
    private void UpdateAnimations()
    {

        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y);

    }

    //Checks the player's input
    private void CheckInput()
    {

        movementInputDirection = Input.GetAxisRaw("Horizontal");
        verticalInputDirection = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Jump"))
        { 

            Jump();

        }

        if (Input.GetButtonUp("Jump"))
        {

            if (jumpCount > 0)
            {

                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHightMultiplier);

            }

        }

        canGrab = Input.GetButton("Grab");

    }

    //Checks which way the player is moving
    private void CheckMovementDirection()
    {

        if (isFacingRight && movementInputDirection < 0)
        {

            Flip();

        }
        else if (!isFacingRight && movementInputDirection > 0)
        {

            Flip();

        }

        if (rb.velocity.x != 0)
        {

            isWalking = true;

        }
        else
        {

            isWalking = false;

        }

    }

    //Checks if player can jump
    private void CheckIfCanJump()
    {

        if (jumpCount >= 1)
        {

            canJump = true;

        }
        else if (isGrounded && rb.velocity.y <= 0.01)
        {

            if (jumpCount < 1)
            {

                jumpCount = 1;

            }
                
        }
        else
        {

            canJump = false;

        }

        if (isTouchingWall && !isGrounded)
        {

            canWallJump = true;

        }
        else
        {

            canWallJump = false;

        }

    }

    //Makes player jump 
    private void Jump()
    { 

        if (canJump)
        {

            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpCount = jumpCount - 1;

        }
        else if (canWallJump) //Wall jump code
        {

            if (movementInputDirection != 0)
            {

                if (isTouchingWallLeft)
                {

                    rb.velocity = new Vector2(wallJumpForceX, wallJumpForceY);
                    Flip();

                }

                if (isTouchingWallRight)
                {

                    rb.velocity = new Vector2(-wallJumpForceX, wallJumpForceY);
                    Flip();

                }

            }
            else if (movementInputDirection == 0)
            {

                if (isTouchingWallLeft)
                {

                    rb.velocity = new Vector2((wallJumpForceX/2), wallJumpForceY);
                    Flip();

                }

                if (isTouchingWallRight)
                {

                    rb.velocity = new Vector2((-wallJumpForceX/2), wallJumpForceY);
                    Flip();

                }

            }
            
        }

    //Bugs: 
    //
    //Wall jumping does not push you away from the wall
    //Fix: add something to keep the "rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);" from running for a period of time after a wall jump
    //Problem: how do I add a command that will make the script freeze for x seconds before continuing?

    }

    //WIP code. I plan to make the player gain a charge while moving in a direction. If the player changes direction the charge is reset.
    //The higher the charge the more midair jumps they have. If they are in midair the charge is locked. 
    //Currently it has no functionality for whatever reason even though it should work and that annoys me slightly but whatever
    private void JumpCharge()
    {

        if (isGrounded)
        {

            if (isFacingRight)
            {

                if (!isChargingRight)
                {

                    isChargingRight = true;
                    jumpCount = 1.0f;

                }

            }

            if (!isFacingRight)
            {

                if (isChargingRight)
                {

                    isChargingRight = false;
                    jumpCount = 1.0f;

                }

            }

            if (movementInputDirection != 0)
            {

                jumpCount = jumpCount + 0.01f;

            }

        }

    }

    //Manages the different movement states the player can be in (Walking, Grabbing/Climbing, Wall Sliding etc.)
    //They aren't all here (example: jump) and I don't know if I'll be moving them
    private void ApplyMovement()
    {

        if (!isGrabbing)
        {

             rb.gravityScale = 4;
             rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);
        
        }
        else if (isGrabbing)
        {
            canWallJump = false;
            rb.gravityScale = 0;
            rb.velocity = new Vector2(movementSpeed * movementInputDirection, (movementSpeed / 2) * verticalInputDirection);

        }

        if (isWallSliding)
        {
            if(rb.velocity.y < -wallSlideSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            }

        }

    }

    //Makes the sprite flip directions
    private void Flip()
    {
        if (!isWallSliding)
        {

            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);

            if (isFacingRight)
            {

                facingDirection = 1;

            }
            else
            {

                facingDirection = -1;

            }

        }

    }

    //Draws collision detection
    private void OnDrawGizmos()
    {

        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.DrawLine(wallCheckLeft.position, new Vector2(wallCheckLeft.position.x - wallCheckDistance, wallCheckLeft.position.y));

        Gizmos.DrawLine(wallCheckRight.position, new Vector2(wallCheckRight.position.x + wallCheckDistance, wallCheckRight.position.y));

    }

}
