using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject leftWheel;
    public GameObject rightWheel;

    public bool isTurning;
    public bool isAccelerating;

    public float rotationMax = 35;
    public float rotationSpeed = 120f;
    public float returnSpeed = 160f;
    public float baseWheelRotation = -90f;
    public float steeringStrength = 2.5f;

    public float maxAcceleration = 25f;
    public float maxDeceleration = 15f;
    public float accelerationSpeed = 25f;
    public float decelerationSpeed = 30f;

    private float currentVelocity = 0f;
    private float currentRotation = 0f;
    private Rigidbody caRB;

    private void Awake()
    {
        caRB = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        ControlInput();
    }

    private void FixedUpdate()
    {
        if (isTurning)
        {
            ApplyWheelRotation(); 
            ApplyBodyRotation();
        }
        if (isAccelerating) ApplyMovement();
        if (!isTurning) ResetWheels();
        if (!isAccelerating) SlowDownCar();
    }

    void ControlInput()
    {
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

    void TurnWheels(bool turningLeft)
    {
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

    }

    void ResetWheels()
    {
        currentRotation = Mathf.MoveTowards
        (
            currentRotation,
            0f,
            returnSpeed * Time.deltaTime
        );

        ApplyWheelRotation();
    }

    void ApplyWheelRotation()
    {
        rightWheel.transform.localRotation = Quaternion.Euler
        (
            0f,
            baseWheelRotation + currentRotation,
            0f
        );

        leftWheel.transform.localRotation = Quaternion.Euler
        (
            0f,
            baseWheelRotation + currentRotation,
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
            decelerationSpeed * Time.deltaTime
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
            decelerationSpeed * Time.deltaTime
        );

        ApplyMovement();
    }

    void ApplyBodyRotation()
    {
        float turnAmount = currentRotation * steeringStrength * Time.fixedDeltaTime;

        Quaternion turnRotation = Quaternion.Euler
        (
            0f,
            turnAmount,
            0f
        );

        caRB.MoveRotation
        (
            caRB.rotation * turnRotation
        );
    }


}
