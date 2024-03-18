using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class WallRunning : NetworkBehaviour
{
    [Header("Wallrun")]
    public LayerMask wallLayer;
    public LayerMask groundLayer;
    public float wallRunForce;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    public float maxWallRunTime;
    private bool exitingWall;
    public float exitingWallTime;
    private float exitingWallTimer;

    [Header("Input")]
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    [Header("References")]
    public Transform orientation;
    private NewPlayerController pc;
    private Rigidbody rb;

    public bool useGravity;
    public float gravityCounterForce;
    // Start is called before the first frame update
    void Start()
    {
        if (!IsOwner) return;
        rb = GetComponent<Rigidbody>();
        pc = rb.GetComponent<NewPlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        PlayerInput();
        WallCheck();
        State(); 
        }
    private void FixedUpdate()
    {
        if (!IsOwner) return;
        if (pc.isWallRunning)
        {
            Movement();
        }
    }
    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }
    private void WallCheck()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance);

    }
    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, groundLayer);
    }

    private void State()
    {
        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !exitingWall)
        {
            if (!pc.isWallRunning)
                StartWallRun();
            if(Input.GetKeyDown(KeyCode.Space)) WallJump();
        }
        else if(exitingWall)
        {
            if (pc.isWallRunning)
                StopWallRun();
            if (exitingWallTimer > 0)
                exitingWallTimer -= Time.deltaTime;
            if(exitingWallTimer <= 0)
                exitingWall = false;

        }
        else
        {
            if (pc.isWallRunning)
                StopWallRun();
        }
            

    }
    private void StartWallRun()
    {
        pc.isWallRunning = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
    }
    private void StopWallRun()
    {
        pc.isWallRunning = false;

    }
    private void Movement()
    {
        rb.useGravity = useGravity;
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal,transform.up);
        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude) 
                wallForward = -wallForward;
        // forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        // push against wall
        if(!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            rb.AddForce(-wallNormal *100,ForceMode.Force);

        // Weaken gravity

        if(useGravity)
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);



    }
    private void WallJump()
    {
        exitingWall = true;
        exitingWallTimer = exitingWallTime;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        // add force
        rb.velocity = new Vector3(rb.velocity.x, 0f,rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

    }
}

