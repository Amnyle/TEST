using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Netcode;
using UnityEngine;

public class NewPlayerController : NetworkBehaviour
{
    // Callables
    public Rigidbody rb;
    public float debug = 0f;


    //Movement
    [Header("Movement")]
    private float moveSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float wallRunSpeed;


    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    public Transform orientation;
    


    //Ground
    [Header("GroundCheck")]
    public LayerMask groundLayer;
    public bool isGrounded;
    public float groundDrag;

    [Header("Jump")]
    public float jumpForce;
    float playerJumpInput;
    public bool readyToJump;
    public float airMult;

    [Header("Crouch")]
    public bool crouchInput;
    public float crouchSpeed;
    private float crouchScale = 0.5f;
    private float playerBaseScale;

    [Header("Slope")]
    private float maxSlopeAngle = 45f;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Input")]


    //Player
    public float playerHeight;

    public MovementState currState;

    public enum MovementState
    {
        walking,
        sprinting,
        wallrunning,
        crouching,
        sliding,
        air

    }
    public bool isSliding;
    public bool isWallRunning;

    


    // Start is called before the first frame update
    void Start()
    {
        if (!IsOwner) return;
        //Init player
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        playerBaseScale = transform.localScale.y;

    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        PlayerInput();
        StateHandler();
        SpeedControl();
        GroundCheck();
        Drag();

        
    }
    //FixedUpdate is called once every time
    private void FixedUpdate()
    {
        if (!IsOwner) return;
        MovePlayer();
        Jump();
    }

    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        playerJumpInput = Input.GetAxisRaw("Jump");
        if (Input.GetKeyDown(KeyCode.LeftControl) && isGrounded)
            Crouch(true);
        if (Input.GetKeyUp(KeyCode.LeftControl) && isGrounded)
            Crouch(false);


    }
    private void StateHandler()
    {
        // Wallrun
        if(isWallRunning)
        {
            currState = MovementState.wallrunning;
            desiredMoveSpeed = wallRunSpeed;
        }
        // Slide
        else if(isSliding)
        {
            currState = MovementState.sliding;
            if (OnSlope() && rb.velocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;
            else
                desiredMoveSpeed = sprintSpeed;

        }
        // Crouch
        else if (Input.GetKey(KeyCode.LeftControl) && isGrounded)
        {
            currState = MovementState.crouching ;
            desiredMoveSpeed = crouchSpeed;
        }
        // Sprint
        else if (isGrounded && Input.GetKey(KeyCode.LeftShift))
        {
            currState = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;

        }
        // walk
        else if (isGrounded)
        {
            currState = MovementState.walking;
            desiredMoveSpeed = walkSpeed;

        }
        // air
        else
     
        {
            currState = MovementState.air;

        }
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }
        lastDesiredMoveSpeed = desiredMoveSpeed;

    }
    private void MovePlayer()
    {
        //Movement
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope())
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);
            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            debug++;
        }

        else if (isGrounded) // On ground movement
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10);

        }
        else if (!isGrounded) // On air movement
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10* airMult);

        }
        if (!isWallRunning) rb.useGravity = !OnSlope();
        
    }
    private void SpeedControl()
    {
        //Liming speed on slope
        if(OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed) 
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        //Limiting maximum Velocity
        else
        {
            //Limiting maximum Velocity
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }

    }


    private void GroundCheck()
    {
        // Checks if player is grounded 
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);
    }
    private void Drag()
    {
        // Apply drag based on ground state
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0f;
        }
    }
    private void Jump()
    {
        if (playerJumpInput == 1 && readyToJump && isGrounded)
        {
            exitingSlope = true;
            readyToJump = false;

            // Reset y velocity
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

            // Perform the jump

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            Invoke(nameof(ResetJump), 0.25f) ;//JumpCooldown
        }
    }
    private void ResetJump() 
    {
        // Reset jump availability
        readyToJump = true;
        exitingSlope = false;
    }
    private void Crouch(bool crouch)
    {
        if (crouch)
        {
            transform.localScale = new Vector3(transform.localScale.x,crouchScale, transform.localScale.z);
            rb.AddForce(Vector3.down *5f, ForceMode.Impulse);
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, playerBaseScale, transform.localScale.z);
        }
    }
    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }
    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            time += Time.deltaTime;
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }
}
