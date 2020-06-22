// Some stupid rigidbody based movement by Dani

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    // TOGGLE
    public bool disabled = false;

    //Assingables
    public Transform playerCam;
    public Transform orientation;

    //Other
    private Rigidbody rb;
    private Player player;

    //Rotation and look
    private float xRotation;
    private float sensitivity = 50f;
    private float sensMultiplier = 1f;

    //Movement
    public float moveSpeed = 4;
    public float maxSpeed = 5;

    public float crouchSpeed = 2;

    //public float walkSpeed = 2;           In case we want walking again
    public float airFriction = 0.01f;
    public float friction = 0.1f;
    public bool grounded;
    public LayerMask whatIsGround;

    private bool moving;

    // Stair logic
    // The maximum a player can set upwards in units when they hit a wall that's potentially a step
    public float maxStepHeight = 0.4f;

    // How much to overshoot into the direction a potential step in units when testing. High values prevent player from walking up tiny steps but may cause problems.
    public float stepSearchOvershoot = 0.01f;

    private List<ContactPoint> allCPs = new List<ContactPoint>();
    private Vector3 lastVelocity;

    public float counterMovement = 0.175f;
    private float threshold = 0.01f;
    public float maxSlopeAngle = 50f;
    private bool isTooSteepSlope;

    //Crouch & Slide
    private Vector3 playerScale;

    //Jumping
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
    public float jumpForce = 200f;

    public float maxGroundSnapHeight = 0.05f;

    public float maxSafeVelocity = 7f;

    //Input
    float x, y;
    bool jumping, sprinting, crouching; //, walking
    bool jumpin; // jumping but its a continous var

    //Sliding
    private Vector3 normalVector = Vector3.up;
    private Vector3 wallNormalVector;

    // Audio
    public AudioSource audioSrc;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        player = GetComponent<Player>();
    }

    void Start() {
        playerScale = transform.localScale;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void FixedUpdate() {
        Movement();
    }

    private void Update() {
        if (!disabled) {
            MyInput();
            Look();
        }

        // No thx
    }

    private void MyInput() {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
        jumping = Input.GetButton("Jump");
        HandleMovement(KeyCode.LeftControl, StartCrouch, StopCrouch, ref crouching);
        //HandleMovement(KeyCode.LeftShift, () => maxSpeed -= walkSpeed, () => maxSpeed += walkSpeed, ref walking);
    }

    private void StartCrouch() {
        maxSpeed -= crouchSpeed;
        //transform.localScale = crouchScale;
        var camTransform = GameManager.instance.playerCamera.transform;
        camTransform.position = new Vector3(camTransform.position.x, camTransform.position.y - 0.5f, camTransform.position.z);
        // if (rb.velocity.magnitude > 0.5f) {
        //     if (grounded) {
        //         rb.AddForce(orientation.transform.forward * slideForce);
        //     }
        // }
    }

    private void StopCrouch() {
        maxSpeed += crouchSpeed;
        //transform.localScale = playerScale;
        var camTransform = GameManager.instance.playerCamera.transform;
        camTransform.position = new Vector3(camTransform.position.x, camTransform.position.y + 0.5f, camTransform.position.z);
    }

    private void Movement() {
        Vector3 _velocity = rb.velocity;

        // Air friction
        rb.velocity -= rb.velocity * airFriction;

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement();

        if (!disabled) {
            //If holding jump && ready to jump, then jump
            if (readyToJump && jumping) Jump();

            //If sliding down a ramp, add force down so player stays grounded and also builds speed
            //if (crouching && grounded && readyToJump) {
            //    rb.AddForce(Vector3.down * (Time.deltaTime * 3000));
            //    return;
            //}

            float actualX = x;
            float actualY = y;

            //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
            if (x > 0 && xMag > maxSpeed) actualX = 0;
            if (x < 0 && xMag < -maxSpeed) actualX = 0;
            if (y > 0 && yMag > maxSpeed) actualY = 0;
            if (y < 0 && yMag < -maxSpeed) actualY = 0;

            //Some multipliers
            float multiplier = 1f, multiplierV = 1f;

            // Movement in air
            if (!grounded) {
                multiplier = 0.5f;
                multiplierV = 0.5f;
            }

            // Movement while sliding
            //if (grounded && crouching) multiplierV = 0f;

            Vector3 velf = orientation.transform.forward;
            Vector3 velr = orientation.transform.right;
            if (grounded && !jumping) {
                velf = Vector3.ProjectOnPlane(orientation.transform.forward, normalVector);
                velr = Vector3.ProjectOnPlane(orientation.transform.right, normalVector);
            }

            // If it's a steep slope, fuck it hard
            //Apply forces to move player
            //if (!isTooSteepSlope) {
            rb.AddForce(velf * (actualY * moveSpeed * Time.deltaTime * multiplier * multiplierV));
            rb.AddForce(velr * (actualX * moveSpeed * Time.deltaTime * multiplier));
            //rb.velocity += (velf * (actualY * moveSpeed * multiplier * multiplierV));
            //rb.velocity += (velr * (actualX * moveSpeed * multiplier));
            //}
            // Play sound effects
            if ((!Util.isEqual(x, 0) || !Util.isEqual(y, 0)) && grounded) {
                moving = true;
            }
            else {
                moving = false;
            }

            if (moving && !crouching /*&& !walking*/) AudioManager.instance.playFootstep();
        }

        //I have no fucking idea why but clamp small movements
        if (Util.isEqual(rb.velocity.x, 0)) {
            rb.velocity = new Vector3(0, rb.velocity.y, rb.velocity.z);
        }

        if (Util.isEqual(rb.velocity.y, 0)) {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        }

        if (Util.isEqual(rb.velocity.z, 0)) {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, 0);
        }

        // if (grounded && !jumping) {
        //     rb.velocity = Vector3.ProjectOnPlane(rb.velocity, normalVector);
        // }

        if (!disabled) {
            //Filter through the ContactPoints to see if we're grounded and to see if we can step up
            bool areWeGrounded = FindGround(out var groundCP, allCPs);

            Vector3 stepUpOffset = default;
            bool stepUp = false;
            if (areWeGrounded)
                stepUp = FindStep(out stepUpOffset, allCPs, groundCP, rb.velocity);

            //Steps
            if (stepUp) {
                rb.position += stepUpOffset;
                rb.velocity = lastVelocity;
            }

            //Debug.Log($"{lastVelocity}, {rb.velocity}");
            if (lastVelocity.y < 0 && Util.isEqual(rb.velocity.y, 0)) {
                ApplyFallDamage();
            }

            // snappy snappy ground
            if (!jumping && !jumpin) {
                //if (!grounded && !jumping && !jumpin) {
                Debug.Log("AA");
                float halfheight = rb.GetComponent<Collider>().bounds.extents.y;
                //Debug.Log(halfheight);
                if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y - halfheight + 0.01f, transform.position.z), Vector3.down, out var hitInfo, maxGroundSnapHeight, whatIsGround)) {
                    //Debug.Log(hitInfo.rigidbody.gameObject.name);
                    Debug.Log("BBB");
                    if (hitInfo.distance > 0.01f) {
                            Debug.Log("CCC");
                            teleportToFeetPos(hitInfo.point, hitInfo.normal);
                    }
                    //rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y - 5, rb.velocity.z);
                }
            }


            //Debug.Log(
            //    $"{grounded}, {moving}, {normalVector}, {lastVelocity}, STDATA {stepUp}, {areWeGrounded}, {isTooSteepSlope}");
        }

        allCPs.Clear();
        lastVelocity = _velocity;
    }

    /// <summary>
    /// Teleports the character into the specified position for its feet.
    /// </summary>
    private void teleportToFeetPos(Vector3 feetPos, Vector3 normal) {
        Debug.Log("DDD");
        float halfheight = rb.GetComponent<Collider>().bounds.extents.y;
        if (normal != Vector3.up) {
            //transform.position = feetPos + new Vector3(0, halfheight, 0);
            //rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y - 5, rb.velocity.z);
            rb.velocity = Vector3.ProjectOnPlane(rb.velocity, normal);
            rb.velocity = new Vector3(rb.velocity.x, -2f, rb.velocity.z);
            //rb.MovePosition(feetPos + new Vector3(0, halfheight, 0));
        }
        else {
            rb.velocity = new Vector3(rb.velocity.x, -2f, rb.velocity.z);
            //rb.MovePosition(feetPos + new Vector3(0, halfheight, 0));
        }
    }

    private void Jump() {
        if (grounded && readyToJump) {
            readyToJump = false;

            //Add jump forces
            rb.AddForce(Vector2.up * (jumpForce * 2.0f));
            //rb.AddForce(normalVector * (jumpForce * 0.5f));

            //If jumping while falling, reset y velocity.
            //Vector3 vel = rb.velocity;
            //if (rb.velocity.y < 0.5f)
            //    rb.velocity = new Vector3(vel.x, 0, vel.z);
            //else if (rb.velocity.y > 0)
            //    rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);
            jumpin = true;
            StartCoroutine(ResetJump());
        }
    }

    private IEnumerator ResetJump() {
        yield return new WaitForSeconds(jumpCooldown);
        readyToJump = true;
    }

    private float desiredX;

    private void Look() {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;

        //Find current look rotation
        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;

        //Rotate, and also make sure we dont over- or under-rotate.
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Perform the rotations
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, 0);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);
    }

    private void CounterMovement() {
        if (!grounded || jumping) return;

        //Slow down sliding
        // if (crouching) {
        //     rb.AddForce(-rb.velocity.normalized * (moveSpeed * Time.deltaTime * slideCounterMovement));
        //     return;
        // }

        if (grounded) {
            rb.velocity -= rb.velocity * friction;
        }

        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > maxSpeed) {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    /// <summary>
    /// Find the velocity relative to where the player is looking
    /// Useful for vectors calculations regarding movement and limiting movement
    /// </summary>
    /// <returns></returns>
    public Vector2 FindVelRelativeToLook() {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        //float magnitude = rb.velocity.magnitude;
        float magnitude = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;
        float yMag = magnitude * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitude * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v) {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private void ApplyFallDamage() {
        float yVel = -lastVelocity.y;
        yVel -= maxSafeVelocity;
        yVel = Math.Max(yVel, 0);
        player.damage((int) Math.Round(yVel * 5)); // TODO apply some logic, fuck knows
    }

    private bool cancellingGrounded;

    /// <summary>
    /// Handle ground detection
    /// </summary>
    void OnCollisionStay(Collision col) {
        // Add all contacts
        allCPs.AddRange(col.contacts);

        //Make sure we are only checking for walkable layers
        int layer = col.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        bool isOnGround = false;
        List<Vector3> normals = new List<Vector3>();
        // Iterate through every collision in a physics update
        // If all normals are up, we are on ground; if at least one normal is to the side, it is a fucking slope
        // In that case, just use the not-upwards one.
        for (int i = 0; i < col.contactCount; i++) {
            Vector3 normal_ = col.contacts[i].normal;
            Debug.DrawLine(col.contacts[i].point, col.contacts[i].point + col.contacts[i].normal, Color.green, 2,
                false);

            //FLOOR
            if (IsFloor(normal_)) {
                isOnGround = true;
            }

            normals.Add(normal_);
        }

        bool isOnSlope = false;
        bool steepSlope = false;
        Vector3 normal2 = default;
        Vector3 tempNormal = default;
        foreach (var normal in normals) {
            if (normal != Vector3.up) {
                isOnSlope = true;
                normal2 = normal;
            }

            //Debug.Log(Vector3.Angle(Vector3.up, normal));
            if (Vector3.Angle(Vector3.up, normal) > maxSlopeAngle) {
                //rb.velocity = new Vector3(0, 0, 0);
                steepSlope = true;
                // isTooSteepSlope = true;
            }

            tempNormal = normal;
        }

        if (!steepSlope) {
            // Reset the bullshit counter
        }

        if (!isOnSlope) {
            normal2 = tempNormal; // just set it to the last one fuck it
        }

        if (isOnGround) {
            grounded = true;
            jumpin = false;
            cancellingGrounded = false;
            normalVector = normal2;
            rb.useGravity = false;
            CancelInvoke(nameof(StopGrounded));
        }

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 1f;
        if (!cancellingGrounded) {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    void OnCollisionEnter(Collision col) {
        allCPs.AddRange(col.contacts);
    }

    private void StopGrounded() {
        grounded = false;
        rb.useGravity = true;
    }

    /// Finds the MOST grounded (flattest y component) ContactPoint
    /// \param allCPs List to search
    /// \param groundCP The contact point with the ground
    /// \return If grounded
    bool FindGround(out ContactPoint groundCP, List<ContactPoint> allCPs) {
        groundCP = default;
        var found = false;
        foreach (ContactPoint cp in allCPs) {
            //Pointing with some up direction
            if (!Util.isEqual(cp.normal.y, 0) && (found == false || cp.normal.y > groundCP.normal.y)) {
                groundCP = cp;
                found = true;
            }
        }

        return found;
    }

    /// Find the first step up point if we hit a step
    /// \param allCPs List to search
    /// \param stepUpOffset A Vector3 of the offset of the player to step up the step
    /// \return If we found a step
    bool FindStep(out Vector3 stepUpOffset, List<ContactPoint> allCPs, ContactPoint groundCP, Vector3 currVelocity) {
        stepUpOffset = default;

        //No chance to step if the player is not moving
        //if (!moving) return false;
        //if (Util.isEqual(rb.velocity.x, 0) && Util.isEqual(rb.velocity.y, 0) && Util.isEqual(rb.velocity.z, 0)) {
        //    return false;
        //}

        foreach (ContactPoint cp in allCPs) {
            bool test = ResolveStepUp(out stepUpOffset, cp, groundCP);
            if (test)
                return test;
        }

        return false;
    }

    /// Takes a contact point that looks as though it's the side face of a step and sees if we can climb it
    /// \param stepTestCP ContactPoint to check.
    /// \param groundCP ContactPoint on the ground.
    /// \param stepUpOffset The offset from the stepTestCP.point to the stepUpPoint (to add to the player's position so they're now on the step)
    /// \return If the passed ContactPoint was a step
    bool ResolveStepUp(out Vector3 stepUpOffset, ContactPoint stepTestCP, ContactPoint groundCP) {
        ShittyUtil.DrawBox(
            new Vector3(rb.position.x, rb.position.y + maxStepHeight + 0.01f, rb.position.z) +
            orientation.forward * 0.05f,
            rb.GetComponent<Collider>().bounds.extents, Quaternion.identity, Color.red);
        stepUpOffset = default;
        Collider stepCol = stepTestCP.otherCollider;

        // (1) Check if the contact point normal matches that of a step (y close to 0)
        if (!Util.isEqual(stepTestCP.normal.y, 0)) {
            return false;
        }

        // (2) Make sure the contact point is low enough to be a step
        if (!(stepTestCP.point.y - groundCP.point.y < maxStepHeight)) {
            return false;
        }

        // (3) Check to see if there's actually a place to step in front of us
        //Fires one Raycast
        RaycastHit hitInfo;
        float stepHeight = groundCP.point.y + maxStepHeight + 0.0001f;
        Vector3 stepTestInvDir = new Vector3(-stepTestCP.normal.x, 0, -stepTestCP.normal.z).normalized;
        Vector3 origin = new Vector3(stepTestCP.point.x, stepHeight, stepTestCP.point.z) +
                         (stepTestInvDir * stepSearchOvershoot);
        Vector3 direction = Vector3.down;
        if (!(stepCol.Raycast(new Ray(origin, direction), out hitInfo, maxStepHeight))) {
            return false;
        }

        // (4) Check if it's not a fucking wall
        if (Physics.OverlapBox(
            new Vector3(rb.position.x, rb.position.y + maxStepHeight + 0.01f, rb.position.z) +
            orientation.forward * 0.05f,
            rb.GetComponent<Collider>().bounds.extents, Quaternion.identity, whatIsGround).Length > 0) {
            // just please don't
            return false;
        }

        //We have enough info to calculate the points
        Vector3 stepUpPoint = new Vector3(stepTestCP.point.x, hitInfo.point.y + 0.0001f, stepTestCP.point.z) +
                              (stepTestInvDir * stepSearchOvershoot);
        Vector3 stepUpPointOffset = stepUpPoint - new Vector3(stepTestCP.point.x, groundCP.point.y, stepTestCP.point.z);

        //We passed all the checks! Calculate and return the point!
        stepUpOffset = stepUpPointOffset;
        return true;
    }

    void HandleMovement(KeyCode key, Action trueEvent, Action falseEvent, ref bool moveVar) {
        moveVar = Input.GetKey(key);
        if (Input.GetKeyDown(key)) trueEvent();
        if (Input.GetKeyUp(key)) falseEvent();
    }
}
