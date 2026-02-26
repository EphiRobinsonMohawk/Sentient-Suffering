using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Car Parts"), Tooltip("Assign Us PLEASE")]
    public GameObject headLightL;
    public GameObject headLightR;
    public GameObject flapTop;
    public GameObject flapBottom;
    public GameObject leftDoor;
    public GameObject rightDoor;
    public GameObject leftWheel;
    public GameObject rightWheel;
    public GameObject turnLightLD;
    public GameObject turnLightLB;
    public GameObject turnLightRD;
    public GameObject turnLightRB;
    public GameObject brakeLightCD;
    public GameObject brakeLightCB;
    public GameObject brakeLightLRD;
    public GameObject brakeLightLRB;
    public GameObject reverseLightD;
    public GameObject reverseLightB;
    public GameObject headLightD;
    public GameObject headLightB;



    [Header("Car State Variables")]
    public bool isTurning;
    public bool isAccelerating;
    public bool headLightOn = false;
    public bool hazardsOn = false;
    public bool isBraking = false;
    public bool isReversing = false;
    public bool isTurningLeft = false;
    public bool rightDoorOpen = false;
    public bool leftDoorOpen = false;

    [Header("Car Door Variables")]
    public float maxOpenAngle = 90f;
    public float openSpeed = 20f;
    public float closeSpeed = 20f;
    public float leftDoorClosedY = 0f;
    public float rightDoorClosedY = 0f;
    public float leftDoorY = 0f;
    public float rightDoorY = 0f;

    [Header("Rotation/Wheel Variables")]
    public float reverseThreshhold = 1f;
    public float rotationMax = 35f;
    public float rotationSpeed = 100f;
    public float returnSpeed = 80f;
    public float baseWheelRotation = -90f;
    public float maxTurnRate = 100f;
    public float turnRateChangeSpeed = 300f;
    public float wheelSpinMaxSpeed = 1200f;

    [Header("Acceleration Variables")]
    public float brakeSpeed = 45f;
    public float speedToZero = 15f;
    public float maxAcceleration = 25f;
    public float maxDeceleration = 20f;
    public float accelerationSpeed = 20f;
    public float reverseAccelerationSpeed = 8f;


    private float wheelTurnAmount = 0f;
    private float currentTurnRate = 0f;
    private float currentVelocity = 0f;
    private float currentRotation = 0f;
    private Rigidbody caRB;

    [Header("Transforms For Spinnning Wheels")]
    public Transform frontLeftSteerPivot;
    public Transform frontRightSteerPivot;
    public Transform frontLeftWheelMesh;
    public Transform frontRightWheelMesh;
    public Transform backWheelsMesh;
    public Transform car;

    [Header("Headlight + Flap Variables")]
    public float maxHeadlightAngle = 45f;
    public float headlightOpenSpeed = 20f;
    public float headlightCloseSpeed = 20f;

    public float maxFlapAngle = 45f;
    public float flapOpenSpeed = 20f;
    public float flapCloseSpeed = 20f;

    public float headLightClosedX = 0f;
    public float headLightX = 0f;

    public float flapTopClosedX = 0f;
    public float flapBottomClosedX = 0f;
    public float flapTopX = 0f;
    public float flapBottomX = 0f;

    private float wheelSpinAngle = 0f;

    [Header("Ground + Air Control")]
    public float groundRayStart = 0.6f;
    public float groundRayLength = 1.6f;
    public LayerMask groundMask = ~0;

    public float extraGravity = 35f;
    public float downforce = 60f;
    public float lateralGrip = 8f;

    public float groundDrag = 0.2f;
    public float brakeDrag = 2.0f;
    public float airDrag = 0.02f;

    [Header("Rotation Clamp")]
    public float maxPitchDegrees = 30f;
    public float yawDamp = 3.5f; // kills tiny unwanted yaw when steering is neutral

    private bool grounded;
    private bool wasGrounded;
    private RaycastHit groundHit;

    [Header("Auto Level Pitch (X)")]
    public float pitchReturnSpeedGround = 360f; // deg/sec
    public float pitchReturnSpeedAir = 540f;    // deg/sec

    [Header("Slope Launch Control")]
    public float extraGravityAir = 20f;         // downward accel while airborne
    public float maxUpVelGrounded = 1.5f;       // caps upward pop from slope collisions

    [Header("Air Control")]
    public float airControlStrength = 6f;       // how fast air steering nudges horizontal velocity
    public float maxDepenetration = 6f;

    [Header("Yaw Wobble Fix")]
    public float yawStabilizeWhenStraight = 8f;   // higher = stronger yaw damping
    public float yawInputDeadzone = 0.02f;        // treat tiny steering as zero

    private Quaternion frontLeftBaseRotation;
    private Quaternion frontRightBaseRotation;
    private Quaternion frontLeftPivotBaseRotation;
    private Quaternion frontRightPivotBaseRotation;
    private Quaternion backBaseRotation;
    private Quaternion headLightBaseRotation;
    private Quaternion flapTopBaseRotation;
    private Quaternion flapBottomBaseRotation;


    private void Awake()
    {
        caRB = GetComponent<Rigidbody>();

        caRB.interpolation = RigidbodyInterpolation.Interpolate;
        caRB.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Roll locked by physics, pitch clamped by code, yaw unlimited
        caRB.constraints = RigidbodyConstraints.FreezeRotationZ;
        caRB.maxDepenetrationVelocity = maxDepenetration;
        caRB.angularDrag = 1.5f;
        headLightBaseRotation = headLightL.transform.localRotation;

        flapTopBaseRotation = flapTop.transform.localRotation;
        flapBottomBaseRotation = flapBottom.transform.localRotation;
        headLightClosedX = headLightL.transform.localEulerAngles.x;
        headLightX = headLightClosedX;
        flapTopClosedX = flapTop.transform.localEulerAngles.x;
        flapBottomClosedX = flapBottom.transform.localEulerAngles.x;
        flapTopX = flapTopClosedX;
        flapBottomX = flapBottomClosedX;
        leftDoorClosedY = leftDoor.transform.localEulerAngles.y;
        rightDoorClosedY = rightDoor.transform.localEulerAngles.y;
        leftDoorY = leftDoorClosedY;
        rightDoorY = rightDoorClosedY;
        frontLeftBaseRotation = frontLeftWheelMesh.localRotation;
        frontRightBaseRotation = frontRightWheelMesh.localRotation;
        frontLeftPivotBaseRotation = frontLeftSteerPivot.localRotation;
        frontRightPivotBaseRotation = frontRightSteerPivot.localRotation;
        backBaseRotation = backWheelsMesh.localRotation;
    }

    private void Update()
    {
        if (currentVelocity < -reverseThreshhold) isReversing = true;
        else isReversing = false;
        ControlInput();
        ControlDoors();
        ControlLights();
        ControlHeadlights();
        ControlFlaps();
    }

    private void FixedUpdate()
    {
        GroundCheck();

        if (isTurning) ApplyWheelRotation();
        else ResetWheels();

        ApplyBodyRotation();

        // ---- One movement decision per physics tick ----
        if (grounded)
        {
            if (isBraking) Brake();
            else if (isAccelerating) ApplyMovement();
            else SlowDownCar();
        }
        else
        {
            // Air: preserve momentum; allow gentle control only when accelerating
            if (isAccelerating) ApplyMovementAir();
            caRB.drag = airDrag;
        }

        ApplyWheelSpin();
        StabilizeYaw();
        StabilizeOnTerrain();     // stick + grip + rotation leveling/clamp
    }

    void ControlInput()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            leftDoorOpen = !leftDoorOpen;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("Honk!");
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            rightDoorOpen = !rightDoorOpen;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            headLightOn = !headLightOn;
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            hazardsOn = !hazardsOn;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            isBraking = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isBraking = false;
        }

        if (Input.GetKey(KeyCode.A))
        {
            TurnWheels(true);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            TurnWheels(false);
        }

        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
        {
            isTurning = false;
        }

        if (Input.GetKey(KeyCode.W))
        {
            Accelerate(true);
        }

        else if (Input.GetKey(KeyCode.S))
        {
            Accelerate(false);
        }

        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S))
        {
            isAccelerating = false;
        }

    }

    void StabilizeYaw()
    {
        // When wheels are basically centered, kill leftover yaw spin
        if (Mathf.Abs(wheelTurnAmount) < yawInputDeadzone && !isTurning)
        {
            Vector3 av = caRB.angularVelocity;
            av.y = Mathf.MoveTowards(av.y, 0f, yawStabilizeWhenStraight * Time.fixedDeltaTime);
            caRB.angularVelocity = av;
        }
    }

    void GroundCheck()
    {
        wasGrounded = grounded;

        Vector3 rayStart = caRB.position + Vector3.up * groundRayStart;
        grounded = Physics.Raycast(rayStart, Vector3.down, out groundHit, groundRayLength, groundMask);

        if (!wasGrounded && grounded)
        {
            Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            currentVelocity = Vector3.Dot(caRB.velocity, flatForward);
        }
    }


    void ApplyMovementAir()
    {
        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

        Vector3 vel = caRB.velocity;
        Vector3 horiz = new Vector3(vel.x, 0f, vel.z);
        Vector3 targetHoriz = flatForward * currentVelocity;

        horiz = Vector3.MoveTowards(horiz, targetHoriz, airControlStrength * Time.fixedDeltaTime);

        vel.x = horiz.x;
        vel.z = horiz.z;
        caRB.velocity = vel;
    }



    void ControlHeadlights()
    {
        if (headLightOn)
        {
            headLightX = Mathf.MoveTowardsAngle
            (
                headLightX,
                maxHeadlightAngle,
                headlightOpenSpeed * Time.deltaTime
            );
        }
        else
        {
            headLightX = Mathf.MoveTowardsAngle
            (
                headLightX,
                0f,
                headlightCloseSpeed * Time.deltaTime
            );
        }

        Quaternion rotationOffset = Quaternion.Euler(headLightX, 0f, 0f);

        headLightL.transform.localRotation = headLightBaseRotation * rotationOffset;
        headLightR.transform.localRotation = headLightBaseRotation * rotationOffset;
    }

    void ControlFlaps()
    {
        if (hazardsOn)
        {
            flapTopX = Mathf.MoveTowardsAngle
            (
                flapTopX,
                maxFlapAngle,
                flapOpenSpeed * Time.deltaTime
            );

            flapBottomX = Mathf.MoveTowardsAngle
            (
                flapBottomX,
                -maxFlapAngle,
                flapOpenSpeed * Time.deltaTime
            );
        }
        else
        {
            flapTopX = Mathf.MoveTowardsAngle
            (
                flapTopX,
                0f,
                flapCloseSpeed * Time.deltaTime
            );

            flapBottomX = Mathf.MoveTowardsAngle
            (
                flapBottomX,
                0f,
                flapCloseSpeed * Time.deltaTime
            );
        }

        Quaternion topOffset = Quaternion.Euler(flapTopX, 0f, 0f);
        Quaternion bottomOffset = Quaternion.Euler(flapBottomX, 0f, 0f);

        flapTop.transform.localRotation = flapTopBaseRotation * topOffset;
        flapBottom.transform.localRotation = flapBottomBaseRotation * bottomOffset;
    }

    void ControlDoors()
    {
        if (leftDoorOpen)
        {
            leftDoorY = Mathf.MoveTowardsAngle
            (
                leftDoorY,
                leftDoorClosedY + maxOpenAngle,
                openSpeed * Time.deltaTime
            );
        }
        else if (!leftDoorOpen)
        {
            leftDoorY = Mathf.MoveTowardsAngle
            (
                leftDoorY,
                leftDoorClosedY,
                closeSpeed * Time.deltaTime
            );
        }

        leftDoor.transform.localRotation = Quaternion.Euler
        (
            0f,
            leftDoorY,
            0f
        );



        if (rightDoorOpen)
        {
            rightDoorY = Mathf.MoveTowardsAngle
            (
                rightDoorY,
                rightDoorClosedY - maxOpenAngle,
                openSpeed * Time.deltaTime
            );
        }
        else if (!rightDoorOpen)
        {
            rightDoorY = Mathf.MoveTowardsAngle
            (
                rightDoorY,
                rightDoorClosedY,
                closeSpeed * Time.deltaTime
            );
        }

        rightDoor.transform.localRotation = Quaternion.Euler
        (
            0f,
            rightDoorY,
            0f
        );
    }




    void ControlLights()
    {
        // HEADLIGHTS
        headLightD.SetActive(!headLightOn);
        headLightB.SetActive(headLightOn);

        // CENTER BRAKE
        brakeLightCD.SetActive(!isBraking);
        brakeLightCB.SetActive(isBraking);

        // LEFT/RIGHT BRAKE
        brakeLightLRD.SetActive(!isBraking);
        brakeLightLRB.SetActive(isBraking);

        // REVERSE
        reverseLightD.SetActive(!isReversing);
        reverseLightB.SetActive(isReversing);

        // TURN LEFT
        if (!hazardsOn) turnLightLD.SetActive(!(isTurning && isTurningLeft));
        if (!hazardsOn) turnLightLB.SetActive(isTurning && isTurningLeft);

        // TURN RIGHT
        if (!hazardsOn) turnLightRD.SetActive(!(isTurning && !isTurningLeft));
        if (!hazardsOn) turnLightRB.SetActive(isTurning && !isTurningLeft);

        // HAZARDS override turn signals
        if (hazardsOn)
        {
            turnLightLD.SetActive(false);
            turnLightLB.SetActive(true);

            turnLightRD.SetActive(false);
            turnLightRB.SetActive(true);
        }
    }

    void StabilizeOnTerrain()
    {
        if (grounded)
        {
            caRB.drag = isBraking ? brakeDrag : groundDrag;

            Vector3 stickDir = -groundHit.normal;

            // Stick to ground (normal-based feels better on slopes)
            caRB.AddForce(stickDir * extraGravity, ForceMode.Acceleration);

            // Speed-based downforce (also along normal)
            float speed = caRB.velocity.magnitude;
            caRB.AddForce(stickDir * (downforce * speed), ForceMode.Acceleration);

            // Lateral grip on the ground plane
            Vector3 rightOnGround = Vector3.ProjectOnPlane(transform.right, groundHit.normal).normalized;
            Vector3 lateralVel = Vector3.Project(caRB.velocity, rightOnGround);
            caRB.AddForce(-lateralVel * lateralGrip, ForceMode.Acceleration);

            // Kill slope “launch pop”
            Vector3 v = caRB.velocity;
            if (v.y > maxUpVelGrounded) v.y = maxUpVelGrounded;
            caRB.velocity = v;
        }
        else
        {
            caRB.drag = airDrag;
            caRB.AddForce(Vector3.down * extraGravityAir, ForceMode.Acceleration);
        }

        // ---- Auto-level + clamp rotation ----
        Vector3 e = caRB.rotation.eulerAngles;

        float x = NormalizeAngle(e.x);
        float y = e.y;

        float returnSpeed = grounded ? pitchReturnSpeedGround : pitchReturnSpeedAir;

        // Return X toward 0 like wheel reset
        x = Mathf.MoveTowardsAngle(x, 0f, returnSpeed * Time.fixedDeltaTime);

        // Clamp pitch and hard zero roll
        x = Mathf.Clamp(x, -maxPitchDegrees, maxPitchDegrees);

        caRB.MoveRotation(Quaternion.Euler(x, y, 0f));
    }

    float NormalizeAngle(float a)
    {
        while (a > 180f) a -= 360f;
        while (a < -180f) a += 360f;
        return a;
    }


    void TurnWheels(bool turningLeft)
    {
        isTurningLeft = turningLeft;
        isTurning = true;

        float targetRotation = 0f;

        if (turningLeft == true)
        {
            targetRotation = -rotationMax;
        }
        if (turningLeft == false)
        {
            targetRotation = rotationMax;
        }
        currentRotation = Mathf.MoveTowards
        (
            currentRotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        wheelTurnAmount = Mathf.Clamp
        (
            currentRotation / rotationMax,
            -1f,
            1f
        );
    }

    void ResetWheels()
    {
        currentRotation = Mathf.MoveTowards
        (
            currentRotation,
            0f,
            returnSpeed * Time.deltaTime
        );

        wheelTurnAmount = Mathf.Clamp
        (
            currentRotation / rotationMax,
            -1f,
            1f
        );

        ApplyWheelRotation();
    }

    void ApplyWheelRotation()
    {
        frontLeftSteerPivot.localRotation = frontLeftPivotBaseRotation * Quaternion.Euler
        (
            0f,
            currentRotation,
            0f
        );

        frontRightSteerPivot.localRotation = frontRightPivotBaseRotation * Quaternion.Euler
        (
            0f,
            currentRotation,
            0f
        );
    }



    void Accelerate(bool movingForward)
    {
        isAccelerating = true;

        if (movingForward)
        {
            currentVelocity = Mathf.MoveTowards
            (
            currentVelocity,
            maxAcceleration,
            accelerationSpeed * Time.deltaTime
            );
        }

        else if (!movingForward)
        {
            currentVelocity = Mathf.MoveTowards
            (
            currentVelocity,
            -maxDeceleration,
            reverseAccelerationSpeed * Time.deltaTime
            );
        }
    }

    void ApplyMovement()
    {
        Vector3 moveDir;

        if (grounded)
        {
            // Drive along the slope plane
            moveDir = Vector3.ProjectOnPlane(transform.forward, groundHit.normal).normalized;
        }
        else
        {
            // Fallback: flat forward
            moveDir = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        }

        Vector3 desired = moveDir * currentVelocity;

        // Keep current vertical velocity so gravity/jumps feel normal
        caRB.velocity = new Vector3(desired.x, caRB.velocity.y, desired.z);
    }

    void SlowDownCar()
    {
        currentVelocity = Mathf.MoveTowards
        (
            currentVelocity,
            0f,
            speedToZero * Time.deltaTime
        );

        ApplyMovement();
    }

    void Brake()
    {
        currentVelocity = Mathf.MoveTowards
        (
            currentVelocity,
            0f,
            brakeSpeed * Time.deltaTime
        );

        ApplyMovement();
    }

    void ApplyBodyRotation()
    {
        float speedFactor = Mathf.Clamp01
        (
            Mathf.Abs(currentVelocity) / maxAcceleration
        );

        float movementDirection = 1f;

        if (currentVelocity < 0f)
        {
            movementDirection = -1f;
        }

        float targetTurnRate = maxTurnRate * wheelTurnAmount * speedFactor * movementDirection;

        currentTurnRate = Mathf.MoveTowards
        (
            currentTurnRate,
            targetTurnRate,
            turnRateChangeSpeed * Time.fixedDeltaTime
        );

        Quaternion turnRotation = Quaternion.Euler
        (
            0f,
            currentTurnRate * Time.fixedDeltaTime,
            0f
        );

        caRB.MoveRotation
        (
            caRB.rotation * turnRotation
        );
    }

    void ApplyWheelSpin()
    {
        float speedFactor = Mathf.Clamp01
        (
            Mathf.Abs(currentVelocity) / maxAcceleration
        );

        float spinDirection = 1f;

        if (currentVelocity < 0f)
        {
            spinDirection = -1f;
        }

        float spinStep = wheelSpinMaxSpeed * speedFactor * spinDirection * Time.fixedDeltaTime;

        wheelSpinAngle = wheelSpinAngle + spinStep;

        Quaternion spinRotation = Quaternion.Euler
        (

            wheelSpinAngle,
            0f,
            0f
        );

        frontLeftWheelMesh.localRotation = frontLeftBaseRotation * spinRotation;
        frontRightWheelMesh.localRotation = frontRightBaseRotation * spinRotation;
        backWheelsMesh.localRotation = backBaseRotation * spinRotation;
    }
}
