using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    CameraMode cameraMode = CameraMode.FREE;
    public float mouseSentitivity = 100f; //sensitivity of the first person camera

    public Transform cameraBody; //the transform of the player root

    float xRotation = 0f;

    public List<MAPFAgent> agents;
    public int currentAgentNum = 0;
    public Transform currentAgentTransform;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; //lock the cursor to the game window
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(cameraMode == CameraMode.FREE)
            {
                currentAgentTransform = agents[0].transform;
                currentAgentNum = 0;
                cameraBody.localPosition = new Vector3(0, 10, -14);
                cameraBody.localRotation = Quaternion.Euler(30, 0, 0);
                transform.localRotation = Quaternion.Euler(0, 0, 0);
                cameraMode = CameraMode.FOLLOW;
            }
            else
            {
                transform.position = cameraBody.position;
                cameraBody.localPosition = Vector3.zero;
                cameraMode = CameraMode.FREE;
            }
        }

        if (cameraMode == CameraMode.FREE)
        {
            //get mouse input and apply sensitivity along with scaling by time
            float mouseX = Input.GetAxis("Mouse X") * mouseSentitivity * Time.deltaTime; //left and right movement of the mouse

            float mouseY = Input.GetAxis("Mouse Y") * mouseSentitivity * Time.deltaTime;//up and down movement of the mouse

            xRotation -=mouseY; //set the x rotation to be the negative of the y mouse input
            xRotation = Mathf.Clamp(xRotation, -90f, 90f); //clamp up and down rotation so the player cannot look past straight up or down

            //when looking left/right, rotate the whole player
            transform.Rotate(Vector3.up * mouseX);
            cameraBody.transform.localRotation = Quaternion.Euler(xRotation,0,0);

            if (Input.GetKey(KeyCode.W))
            {
                transform.Translate(Vector3.forward * Time.deltaTime * 50, cameraBody);
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.Translate(-Vector3.forward * Time.deltaTime * 50, cameraBody);
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.Translate(Vector3.right * Time.deltaTime * 50, cameraBody);
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.Translate(Vector3.left * Time.deltaTime * 50, cameraBody);
            }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                transform.Translate(Vector3.up * Time.deltaTime * 50);
            }
            if (Input.GetKey(KeyCode.LeftControl))
            {
                transform.Translate(-Vector3.up * Time.deltaTime * 50);
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                currentAgentNum++;
                currentAgentNum = currentAgentNum % agents.Count;
                currentAgentTransform = agents[currentAgentNum].transform;
            }
            transform.position = currentAgentTransform.position;
        }

    }
}

public enum CameraMode
{
    FREE,
    FOLLOW
}
