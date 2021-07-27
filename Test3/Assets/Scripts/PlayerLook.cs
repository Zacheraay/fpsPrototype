using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public PauseMenu pauseMenu;

    public Transform playerBody;

    public float mouseSensitivity;

    private float xRotation = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        mouseSensitivity = pauseMenu.sensitivitySlider.value;

        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        playerBody.Rotate(Vector3.up * mouseX);

        xRotation = transform.localEulerAngles.x - mouseY;
        if(xRotation < 270f)
        {
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        }
        else
        {
            xRotation = Mathf.Clamp(xRotation, 270f, 450f);
        }

        transform.localEulerAngles = new Vector3(xRotation, 0f, 0f);

    }
}