using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Sliding : NetworkBehaviour
{
    // References
    public Transform orientation;
    public Transform Player;
    private Rigidbody rb;
    private NewPlayerController pc;

    // Slide
    public float slideForce = 15f;
    private float slideScale = 0.5f;
    private float playerBaseScale;
    private float slideTimer;
    public float maxSlideTime = 1.5f;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.C;
    private float horizontalInput;
    private float verticalInput;

    private void Start()
    {
        if (!IsOwner) return;
        rb = GetComponent<Rigidbody>();
        pc = GetComponent<NewPlayerController>();
        playerBaseScale = Player.localScale.y;

    }
    private void Update()
    {
        if (!IsOwner) return;
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0) && pc.isGrounded)
        {
            StartSlide();

        }
        if (Input.GetKeyUp(slideKey) && pc.isSliding)
            StopSlide();
    }
    private void FixedUpdate()
    {
        if (!IsOwner) return;
        if (pc.isSliding) 
        {
            SlidingMovement(); 
        }
        
    }
    private void StartSlide()
    {
        pc.isSliding = true;
        Player.localScale = new Vector3(Player.localScale.x, slideScale, Player.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        slideTimer = maxSlideTime;


    }
    private void StopSlide()
    {
        pc.isSliding = false;

        Player.localScale = new Vector3(Player.localScale.x, playerBaseScale, Player.localScale.z);

    }
    
    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        // sliding normal
        if (!pc.OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }

        // sliding down a slope
        else
        {
            rb.AddForce(pc.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0)
            StopSlide();
    }
}



