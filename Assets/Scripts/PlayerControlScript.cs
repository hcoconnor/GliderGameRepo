using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlScript : MonoBehaviour
{

    public Transform playerCamTrans;

    public float camSensitivity;
    public float playerWalkSpeed;
    public float jumpForce;

    public SphereCollider groundCheck;


    Rigidbody playerRB;
    Transform playerTrans;


    float moveVert = 0;
    float moveHorz = 0;



    bool flying = false;

    // Start is called before the first frame update
    void Start()
    {
        playerTrans = transform;
        playerRB = GetComponent<Rigidbody>();
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

            playerCamTrans.Rotate(0, mouseX, 0);
            playerCamTrans.Rotate(mouseY, 0, 0);

            //clamp vertical camera angle
            playerCamTrans.localEulerAngles = new Vector3(
                Mathf.Clamp(playerCamTrans.localEulerAngles.x, 5, 60),
                playerCamTrans.localEulerAngles.y,
                0
                );


            //walking moving controls
            moveVert = Input.GetAxis("Vertical");
            moveHorz = Input.GetAxis("Horizontal");

            if (Input.GetButtonDown("Jump"))
            {
                //do jump if on ground; else switch to flying
                if (Physics.CheckSphere(groundCheck.transform.position,
                        groundCheck.radius,
                        LayerMask.NameToLayer("Ground")))
                {
                    //is on ground, so jump

                    playerRB.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                    Debug.Log("jump");
                }
                else
                {
                    // is not on ground, so switch to flying
                    Debug.Log("not jump");
                }

            }
        }

    }

    void FixedUpdate()
    {

        //apply movement forces

        if (flying)
        {
            //flying movement forces

        }
        else
        {
            //walking movement forces


            //Matrix4x4 camRotMat = Matrix4x4.Rotate(Quaternion.Euler(0,playerCamTrans.rotation.y,0));
            //Vector3 moveVector = camRotMat.MultiplyPoint3x4(new Vector3(moveVert, 0, moveHorz));
            //Debug.Log(camRotMat);
            Transform camDir = new GameObject().transform;
            camDir.rotation = Quaternion.Euler(0, playerCamTrans.rotation.eulerAngles.y, 0);

            Vector3 moveVector = Vector3.RotateTowards(new Vector3(0, 0, moveVert),
                                                    Mathf.Sign(moveVert)*camDir.forward,
                                                    Mathf.PI,0);
            moveVector += Vector3.RotateTowards(new Vector3(moveHorz, 0,0),
                                                    Mathf.Sign(moveHorz) * camDir.right,
                                                    Mathf.PI, 0);
            
            //Debug.Log(moveVector);
            playerRB.MovePosition(playerTrans.position+moveVector.normalized * Time.fixedDeltaTime*playerWalkSpeed);
            Destroy(camDir.gameObject);


            
        }



    }
}
