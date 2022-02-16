using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlScript : MonoBehaviour
{

    public Transform playerCamTrans;

    public float camSensitivity;


    Rigidbody playerRB;
    Transform playerTrans;



    bool flying = false;

    // Start is called before the first frame update
    void Start()
    {
        playerTrans = transform;
    }

    void Update()
    {
        float mouseX = camSensitivity * Input.GetAxis("Mouse X");
        float mouseY = -camSensitivity * Input.GetAxis("Mouse Y");

        if (flying)
        {
            //gliding cam controls

        }
        else
        {
            //walking cam controls

            playerTrans.Rotate(0, mouseX, 0);
            playerCamTrans.Rotate(mouseY, 0, 0);
            //clamp vertical camera angle
            playerCamTrans.localEulerAngles = new Vector3(
                Mathf.Clamp(playerCamTrans.localEulerAngles.x, 5, 60), 
                0,
                0
                );


        }

    }

    void FixedUpdate()
    {
        




    }
}
