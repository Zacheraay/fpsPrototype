using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //references
    public CharacterController controller;
    private Transform body;
    private Transform head;
    [SerializeField]
    private Transform bodyHitbox;
    [SerializeField]
    private Transform headHitbox;

    //physics checks
    public LayerMask groundMask;
    private bool groundCheck;
    private float groundDistance = 0.2f;
    private float fallDistance;

    //wasd movement
    Vector3 movementInputs;
    Vector3 currentVelocity;
    private bool restoreSpeed;
    public float maxGroundVelocity;
    private float _maxGroundVelocity;
    public float groundAcceleration;
    public float maxAirVelocity;
    public float airAcceleration;
    public float friction;

    //jump
    private bool jump;
    private bool canJump;
    private bool isJumping;
    private float jumpHeight = 0.75f;

    //walk
    private bool walk;

    //crouch
    private bool crouch;
    private bool isCrouched;
    private float previousHeight = 0.7f;

    //gravity
    Vector3 velocity;
    private float gravity = -15;

    //height change
    private float height = 0.7f;
    private float _height = 0.7f;
    private float impactHeight = 0f;
    private float recoverSpeed;

    //movement inaccuracy
    private PlayerShoot playerShoot;

    //--------------------------------------------------------------------------
    // Purpose: get references
    //--------------------------------------------------------------------------
    void Start()
    {
        body = transform.Find("Body");
        head = transform.Find("Head");

        _maxGroundVelocity = maxGroundVelocity;

        playerShoot = GetComponent<PlayerShoot>();
    }

    //--------------------------------------------------------------------------
    // Purpose: call all frame-dependent functions
    //--------------------------------------------------------------------------
    void Update()
    {
        GetInputs();
    }

    //--------------------------------------------------------------------------
    // Purpose: call all frame-independent functions
    //--------------------------------------------------------------------------
    void FixedUpdate()
    {
        MovePlayer();

        SnapToGround();

        LandingImpact();

        ImpactEffect();

        PlayerWalk();

        PlayerCrouch();

        PlayerJump();

        SendInaccuracy();

        previousHeight = _height;
    }

    //--------------------------------------------------------------------------
    // Purpose: retrieve any user inputs related to player movement
    //--------------------------------------------------------------------------
    private void GetInputs()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        movementInputs = new Vector3(x, 0f, z);
        
        if (Input.GetButtonDown("Jump") || Input.GetAxis("Mouse ScrollWheel") > 0f && canJump)
        {
            jump = true;
        }

        walk = Input.GetKey("left shift");

        crouch = Input.GetKey("left ctrl");
    }

    //--------------------------------------------------------------------------
    // Purpose: move character controller horizontally
    //--------------------------------------------------------------------------
    private void MovePlayer()
    {
        Vector3 movement;

        groundCheck = Physics.CheckSphere(transform.position, groundDistance, groundMask);

        if (groundCheck)
        {
            PlayerVelocity();

            movement = MoveGround();
        }
        else
        {
            movement = MoveAir();
        }

        controller.Move(movement * Time.fixedDeltaTime);

        currentVelocity = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
    }

    //--------------------------------------------------------------------------
    // Purpose: bring player back to full speed
    //--------------------------------------------------------------------------
    private void PlayerVelocity()
    {
        if(restoreSpeed)
        {
            if(_maxGroundVelocity < maxGroundVelocity)
            {
                _maxGroundVelocity += 0.1f;
            }
            else
            {
                _maxGroundVelocity = maxGroundVelocity;
            }
        }

        if(_maxGroundVelocity >= maxGroundVelocity / 2)
        {
            canJump = true;
        }
    }

    //--------------------------------------------------------------------------
    // Purpose: move character controller vertically
    //--------------------------------------------------------------------------
    private void PlayerJump()
    {
        bool jumpCheck = Physics.CheckSphere(transform.position, groundDistance + 0.07f, groundMask);

        velocity = ApplyGravity();

        if(groundCheck && velocity.y < 0)
        {
            velocity.y = -1f;

            isJumping = false;
        }

        if(jump && jumpCheck && canJump)
        {
            isJumping = true;
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        jump = false;

        controller.Move(velocity * Time.fixedDeltaTime);
    }

    //--------------------------------------------------------------------------
    // Purpose: bring player to walking speed
    //--------------------------------------------------------------------------
    private void PlayerWalk()
    {
        float walkSpeed = 3f;

        restoreSpeed = true;

        if(walk && !crouch)
        {
            if(_maxGroundVelocity > walkSpeed)
            {
                _maxGroundVelocity -= 0.5f;
            }
            else
            {
                _maxGroundVelocity = walkSpeed;
            }

            restoreSpeed = false;
        }
    }

    //--------------------------------------------------------------------------
    // Purpose: crouch the player and bring down the speed
    //--------------------------------------------------------------------------
    private void PlayerCrouch()
    {
        float crouchSpeed = 2f;

        restoreSpeed = true;

        if(crouch)
        {
            ChangeHeight(height / 2, 0.02f);

            if(_maxGroundVelocity > crouchSpeed)
            {
                _maxGroundVelocity -= 0.5f;
            }
            else
            {
                _maxGroundVelocity = crouchSpeed;
            }
            if (_height == height / 2)
            {
                isCrouched = true;
            }
            else
            {
                isCrouched = false;
            }

            restoreSpeed = false;
        }
        else if(impactHeight == 0)
        {
            ChangeHeight(height, 0.02f);
        }

        if(_height != height)
        {
            if (isJumping)
            {
                //crouch jump
                Vector3 heightChange = new Vector3(0f, previousHeight - _height, 0f);

                //x2 bc changeheight brings player down so player doesnt freefall when crouching
                controller.Move(heightChange * 2);
            }
        }
    }

    //--------------------------------------------------------------------------
    // Purpose: apply gravity to player
    //--------------------------------------------------------------------------
    private Vector3 ApplyGravity()
    {
        velocity.y += gravity * Time.fixedDeltaTime;

        return velocity;
    }

    //--------------------------------------------------------------------------
    // Purpose: applies friction when player is on ground
    //--------------------------------------------------------------------------
    private Vector3 MoveGround()
    {
        float currentSpeed = currentVelocity.magnitude;

        if(currentSpeed != 0)
        {
            float speedLoss = currentSpeed * friction * Time.fixedDeltaTime;
            currentVelocity *= Mathf.Max(currentSpeed - speedLoss, 0) / currentSpeed;
        }

        return Accelerate(groundAcceleration, _maxGroundVelocity);
    }

    //--------------------------------------------------------------------------
    // Purpose: doesn't apply friction when player is in the air
    //--------------------------------------------------------------------------
    private Vector3 MoveAir()
    {
        return Accelerate(airAcceleration, maxAirVelocity);
    }

    //--------------------------------------------------------------------------
    // Purpose: calculate strafe movement
    //--------------------------------------------------------------------------
    private Vector3 Accelerate(float acceleration, float maxVelocity)
    {
        //global wish movement direction based on orientation and inputs
        Vector3 wishDirection = (transform.right * movementInputs.x + transform.forward * movementInputs.z).normalized;

        //projection of the velocity onto wish direction (would normally require dividing by wishDirection.magnitude, but wishDirection is normalized)
        float velocityProjection = Vector3.Dot(currentVelocity, wishDirection);

        //change in velocity based on wish direction
        Vector3 deltaVelocity = wishDirection * acceleration * Time.fixedDeltaTime;

        //sets return velocity equal to something so there is no errors
        Vector3 returnVelocity = currentVelocity;

        if (velocityProjection > maxVelocity)
        {
            //if the projection of the velocity is greater than max velocity, dont limit the velocity, just dont increase it
            returnVelocity = currentVelocity;
        }
        else if (velocityProjection + deltaVelocity.magnitude >= maxVelocity)
        {
            //if the velocity projection isnt greater than the max velocity, but adding deltavelocity will make it greater, truncate deltavelocity
            deltaVelocity = (maxVelocity - velocityProjection) * wishDirection;
            returnVelocity = currentVelocity + deltaVelocity;
        }
        else if (velocityProjection + deltaVelocity.magnitude < maxVelocity)
        {
            //if velocity projetion and deltavelocity dont exceed max velocity, add delta velocity to current velocity as normal
            returnVelocity = currentVelocity + deltaVelocity;
        }

        return returnVelocity;
    }

    //--------------------------------------------------------------------------
    // Purpose: vertically stretches and shrinks player for acrobatics
    //--------------------------------------------------------------------------
    private void ChangeHeight(float inputHeight, float speed)
    {
        //error corrections
        if(_height == inputHeight)
        {
            _height = inputHeight;
        }
        else if(height <= inputHeight && _height > inputHeight)
        {
            _height = inputHeight;
        }
        else if(height > inputHeight && _height < inputHeight)
        {
            _height = inputHeight;
        }

        //height change
        if (_height > inputHeight)
        {
            _height -= speed;
        }
        else if(_height < inputHeight)
        {
            _height += speed;
        }

        //apply
        body.localPosition = new Vector3(body.localPosition.x, _height, body.localPosition.z);
        body.localScale = new Vector3(body.localScale.x, _height, body.localScale.z);
        bodyHitbox.localPosition = body.localPosition;
        bodyHitbox.localScale = body.localScale;

        head.localPosition = new Vector3(head.localPosition.x, body.localPosition.y + _height + 0.15f, head.localPosition.z);
        headHitbox.localPosition = head.localPosition;

        Player _player = GameManager.GetPlayer(transform.name);
        _player.SyncScaleServerRpc(_height);
    }

    //--------------------------------------------------------------------------
    // Purpose: calculates movement penalty for falling
    //--------------------------------------------------------------------------
    private void LandingImpact()
    {
        //calculate distance player has fallen
        if(velocity.y < 0 && !groundCheck)
        {
            fallDistance -= velocity.y * Time.fixedDeltaTime;
        }
        else if(velocity.y > 0)
        {
            fallDistance = 0f;
        }
        
        //apply impact at threshold
        if(groundCheck && fallDistance > jumpHeight - 0.1f)
        {
            float curbMovement = Mathf.Max(0f, 0.3f - fallDistance / 5);

            _maxGroundVelocity = maxGroundVelocity * curbMovement;

            canJump = false;
            impactHeight = Mathf.Min(fallDistance * 0.5f, 0.15f);
            recoverSpeed = Mathf.Clamp(0.05f / fallDistance, 0.01f, 0.05f);
            fallDistance = 0f;
        }
    }

    //--------------------------------------------------------------------------
    // Purpose: applies landing impact penalty to player
    //--------------------------------------------------------------------------
    private void ImpactEffect()
    {
        if(impactHeight != 0)
        {

            if (_height <= height - impactHeight)
            {
                impactHeight = 0f;
            }

            ChangeHeight(height - impactHeight, recoverSpeed);
        }
    }

    //--------------------------------------------------------------------------
    // Purpose: keeps player on the ground when going down a slope
    //--------------------------------------------------------------------------
    private void SnapToGround()
    {
        RaycastHit hit;

        if(!isJumping)
        {
            if(Physics.Raycast(transform.position, Vector3.down, out hit, jumpHeight / 2, groundMask))
            {
                controller.Move(Vector3.down * hit.distance * 10 * Time.fixedDeltaTime);
            }
        }
    }

    private void SendInaccuracy()
    {
        float inaccuracy = currentVelocity.magnitude * 0.03f;
        if(isJumping)
        {
            inaccuracy += 0.15f;
        }
        else if(_height != previousHeight)
        {
            inaccuracy += 0.03f;
        }

        playerShoot.UpdateInaccuracy(inaccuracy, isCrouched);
    }
}