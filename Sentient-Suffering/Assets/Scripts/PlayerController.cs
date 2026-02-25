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
        if (isBraking) Brake();
    }

    private void FixedUpdate()
    {
        
        if (isTurning)
        {
            ApplyWheelRotation(); 
        }
        if (!isTurning) ResetWheels();

        ApplyBodyRotation();

        if (isAccelerating) ApplyMovement();
        if (!isAccelerating) SlowDownCar();

        ApplyWheelSpin();
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
        Vector3 movementDirection = transform.forward * currentVelocity;

        caRB.velocity = new Vector3
        (
            movementDirection.x,
            caRB.velocity.y,
            movementDirection.z
        );
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
