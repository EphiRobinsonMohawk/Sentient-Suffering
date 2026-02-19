using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController3D : MonoBehaviour
{
    public bool doesSwitchingCameraResetAngle1 = true;

    public Transform cameraAnchor;
    public Transform cameraAnchor2;

    public Transform carTransform;

    public float mouseSensitivity = 180f;
    public float distanceFromAnchor = 10f;

    public float minPitch = -10f;
    public float maxPitch = 65f;

    public bool viewingAngle2 = false;

    private float yaw = 0f;
    private float pitch = 20f;

    private float defaultPitch;

    private float lastCarYaw;

    void Start()
    {
        yaw = carTransform.eulerAngles.y;
        pitch = 20f;

        defaultPitch = pitch;

        lastCarYaw = carTransform.eulerAngles.y;
    }

    void LateUpdate()
    {
        ControlInput();

        if (!viewingAngle2)
        {
            ApplyCarRotationOffset();
            HandleMouseOrbit();
            PositionOrbitCamera();
        }
        else
        {
            PositionLockedCamera();
        }
    }

    void ControlInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCameraAngle();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            viewingAngle2 = !viewingAngle2;
            if (doesSwitchingCameraResetAngle1) ResetCameraAngle();
        }
    }

    void ResetCameraAngle()
    {
        yaw = carTransform.eulerAngles.y;
        pitch = defaultPitch;
    }

    void ApplyCarRotationOffset()
    {
        float currentCarYaw = carTransform.eulerAngles.y;
        float delta = currentCarYaw - lastCarYaw;

        yaw += delta;
        lastCarYaw = currentCarYaw;
    }

    void HandleMouseOrbit()
    {
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            yaw += mouseX * mouseSensitivity * Time.deltaTime;
            pitch -= mouseY * mouseSensitivity * Time.deltaTime;

            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
    }

    void PositionOrbitCamera()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 offset = rotation * new Vector3(0f, 0f, -distanceFromAnchor);

        transform.position = cameraAnchor.position + offset;

        transform.LookAt(cameraAnchor.position);
    }

    void PositionLockedCamera()
    {
        transform.position = cameraAnchor2.position;
        transform.rotation = cameraAnchor2.rotation;
    }
}
