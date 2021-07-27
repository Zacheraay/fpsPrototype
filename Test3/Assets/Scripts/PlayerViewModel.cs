using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerViewModel : MonoBehaviour
{
    private Vector3 previousPosition;
    private Vector3 playerVelocity;
    private float previousVelocity;
    private float playerAcceleration;

    private float previousHorizontalRotation;
    private float previousVerticalRotation;

    [SerializeField]
    private GameObject gunModel;
    private Vector3 gunModelPosition;
    [SerializeField]
    private GameObject playerCamera;
    [SerializeField]
    private GameObject playerHead;

    private float distanceFromStep;
    private float stepSway = 0f;
    private float swayRate = 0f;
    private bool step = false;

    public bool ADS = false;
    private float swayMultiplier;

    private void Start()
    {
        previousPosition = transform.position;

        previousHorizontalRotation = playerCamera.transform.rotation.eulerAngles.y;
        previousVerticalRotation = playerHead.transform.localRotation.x;

        gunModelPosition = gunModel.transform.localPosition;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (ADS)
            {
                ADS = false;
                swayMultiplier = -1f;
            }
            else
            {
                ADS = true;
                swayMultiplier = -0.25f;
            }
        }
    }
    private void FixedUpdate()
    {
        playerVelocity = transform.position - previousPosition;
        playerAcceleration = Mathf.Abs(playerVelocity.magnitude) - previousVelocity;

        Step();

        AimDownSights();
        gunModel.transform.localPosition = ViewModelSway();

        previousPosition = transform.position;
        previousVelocity = Mathf.Abs(playerVelocity.magnitude);
    }

    private Vector3 ViewModelSway()
    {
        if(distanceFromStep > 0 && !step)
        {
            stepSway += Mathf.Abs(playerVelocity.magnitude) * 0.007f;
        }

        if(distanceFromStep == 0 && step)
        {
            if(stepSway > 0)
            {
                swayRate -= (Mathf.Abs(playerVelocity.magnitude) + 0.1f) * 0.007f;
            }
            if(stepSway < 0)
            {
                swayRate += 0.01f;
                if(stepSway + swayRate > 0f)
                {
                    swayRate = 0f - stepSway;
                    step = false;
                }
            }
            stepSway += swayRate;
        }

        float deltaHorizontalRotation = Mathf.DeltaAngle(previousHorizontalRotation, playerCamera.transform.rotation.eulerAngles.y); 
        previousHorizontalRotation = playerCamera.transform.rotation.eulerAngles.y;
        deltaHorizontalRotation = Mathf.Clamp(deltaHorizontalRotation, -18f, 18f);
        Vector3 horizontalRotationSway = new Vector3(deltaHorizontalRotation * swayMultiplier * 0.01f, 0f, 0f);

        float deltaVerticalRotation = Mathf.DeltaAngle(previousVerticalRotation, playerHead.transform.localRotation.x);
        previousVerticalRotation = playerHead.transform.localRotation.x;
        deltaVerticalRotation = Mathf.Clamp(deltaVerticalRotation, -2f, 2f);
        Vector3 verticalRotationSway = new Vector3(0f, deltaVerticalRotation * swayMultiplier * 2f, 0f);

        Vector3 camRotation = new Vector3(0f, playerCamera.transform.rotation.eulerAngles.y, 0f);
        Vector3 horizontalVelocity = new Vector3(playerVelocity.x, 0f, playerVelocity.z);
        Vector3 orientatedVelocity = Quaternion.Euler(-camRotation) * new Vector3(playerVelocity.x, 0f, playerVelocity.z).normalized * horizontalVelocity.magnitude * swayMultiplier;
        Vector3 velocitySway = gunModelPosition + orientatedVelocity + Vector3.down * playerVelocity.y * 0.25f;

        Vector3 sway = Vector3.Lerp(gunModel.transform.localPosition, Vector3.up * stepSway + horizontalRotationSway + verticalRotationSway + velocitySway, 0.5f);
        
        return sway;
    }

    private void Step()
    {
        Vector3 playerMovement = playerVelocity * Time.fixedDeltaTime;

        float distanceTraveled = Mathf.Sqrt(Mathf.Pow(playerMovement.x, 2) + Mathf.Pow(playerMovement.y, 2) + Mathf.Pow(playerMovement.z, 2));

        if(!step)
        {
            distanceFromStep += distanceTraveled;
        }
        
        if(distanceFromStep >= 0.05f)
        {
            distanceFromStep = 0f;
            step = true;
        }
        else if(distanceFromStep > 0 && Mathf.Abs(playerVelocity.magnitude) <= 0.03f && playerAcceleration < 0)
        {
            distanceFromStep = 0f;
            step = true;
        }
    }

    private void AimDownSights()
    {
        if(ADS)
        {
            gunModelPosition = new Vector3(-0.257f, 0.128f, 0f);
        }
        else
        {
            gunModelPosition = new Vector3(0f, 0f, 0f);
        }
    }
}
