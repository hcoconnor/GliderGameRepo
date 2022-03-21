using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerControllerScript: MonoBehaviour
{
    [Header("Walking")]
    public float maxWalkSpeed;
    //public float walkSpeedScale;
    public float walkAccelerationTime = 1; //in seconds
    public float turnSpeed = 1; // in second to do full circle
    public AnimationCurve walkCurve;
    public float jumpForce;


    [Header("Gliding")]
    public float maxRollSpeed;
    public float maxPitchSpeed;
    [RangeAttribute(0,1)]
    public float minGravity;


    [Header("Other")]
    public Transform playerCamTrans;
    public float camSensitivity;

    public SphereCollider groundCheck;



    CharacterController playerCC;
    Transform playerTrans;
    Transform playerModel;


    float moveVert = 0;
    float moveHorz = 0;

    float speed = 0;
    float speedTime = 0; //used with walk acceleration curve to create walk speed curve

    const float gravity = 9.81f;


    bool flying = false;

    // Start is called before the first frame update
    void Start()
    {
        playerTrans = transform;
        playerCC = GetComponent<CharacterController>();
        //actualMoveVec = new Vector3(0, 0, 0);
        playerModel = transform.Find("playerModel");

    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = camSensitivity * Input.GetAxis("Mouse X");
        float mouseY = -camSensitivity * Input.GetAxis("Mouse Y");

        Vector3 velocity = Vector3.zero;

        if (flying)
        {
            //gliding cam controls




            playerCamTrans.Rotate(0, mouseX, 0);
            playerCamTrans.Rotate(mouseY, 0, 0);

            //clamp vertical camera angle
            float xRot;
            if (playerCamTrans.localEulerAngles.x < 180)
            {
                xRot = Mathf.Clamp(playerCamTrans.localEulerAngles.x, 0, 60);
            }
            else
            {
                xRot = Mathf.Clamp(playerCamTrans.localEulerAngles.x, 300, 360);
            }


            playerCamTrans.localEulerAngles = new Vector3(
                xRot,
                playerCamTrans.localEulerAngles.y,
                0
                );



            //gliding moving controls
            moveVert = Input.GetAxis("Vertical");
            moveHorz = Input.GetAxis("Horizontal");

            Transform camDir = new GameObject().transform;
            camDir.rotation = Quaternion.Euler(0, playerCamTrans.rotation.eulerAngles.y, 0);

            Vector3 playerRot = playerTrans.localRotation.eulerAngles;
            playerRot.x += -moveVert * maxPitchSpeed * Time.deltaTime;
            playerRot.z += -moveHorz * maxRollSpeed * Time.deltaTime;

            playerTrans.rotation = Quaternion.Euler(playerRot);

            Destroy(camDir.gameObject);

            //angle between x and xz plane
            float xAngle = playerTrans.rotation.eulerAngles.x;

            //need to make x angle fit {0,90}, and then need that range to fit {minGravity,1}

            if(180 <= xAngle && xAngle <= 360)
            {
                xAngle -= 180;
            }
            if(90<= xAngle && xAngle <= 180)
            {
                xAngle -= 90;
            }
            //now xAngle is between 0 and 90

            
            float gravModifier = (xAngle / (90 / 1 - minGravity)) + minGravity;
            //gravModifeir is in range {minGravity,1}


            velocity = playerTrans.forward * speed + gravModifier * Vector3.down * gravity * Time.deltaTime;

            playerCC.Move(velocity);

            speed = velocity.magnitude;

            







            // if player 'jump' while flying or hit ground, switch to walking controls
            if (Input.GetButtonDown("Jump") || Physics.CheckSphere(groundCheck.transform.position,
                        groundCheck.radius,
                        LayerMask.GetMask("Ground")))
            {
                switchToWalking();
            }
            Debug.Log(velocity / Time.deltaTime);

        }
        else
        {
            //walking cam controls

            playerCamTrans.Rotate(0, mouseX, 0);
            playerCamTrans.Rotate(mouseY, 0, 0);

            //clamp vertical camera angle

            //clamp vertical camera angle
            float xRot;
            if (playerCamTrans.localEulerAngles.x < 180)
            {
                xRot = Mathf.Clamp(playerCamTrans.localEulerAngles.x, 5, 60);
            }
            else
            {
                xRot = 5;
            }


            playerCamTrans.localEulerAngles = new Vector3(
                xRot,
                playerCamTrans.localEulerAngles.y,
                0
                );


            //walking moving controls
            moveVert = Input.GetAxis("Vertical");
            moveHorz = Input.GetAxis("Horizontal");

            Transform camDir = new GameObject().transform;
            camDir.rotation = Quaternion.Euler(0, playerCamTrans.rotation.eulerAngles.y, 0);

            Vector3 playerInputVec = Vector3.RotateTowards(new Vector3(0, 0, moveVert),
                                                    Mathf.Sign(moveVert) * camDir.forward,
                                                    Mathf.PI, 0);
            playerInputVec += Vector3.RotateTowards(new Vector3(moveHorz, 0, 0),
                                                    Mathf.Sign(moveHorz) * camDir.right,
                                                    Mathf.PI, 0);

            Destroy(camDir.gameObject);


            playerInputVec.Normalize();

            //determine current speed
            if(playerInputVec.magnitude == 0)
            {
                //no player input, slow character
                speedTime = Mathf.Clamp(speedTime - Time.deltaTime, 0, walkAccelerationTime);
            }
            //else if (180 - Vector3.Angle(playerModel.forward, playerInputVec) <= 2.5)
            //{
            //    //reverse speed
            //    speedTime = Mathf.Clamp(speedTime - 1.5f * Time.deltaTime, 0, walkAccelerationTime);
            //    //Debug.Log(intendedMoveTime);

            //    //face direction of movement
            //    playerModel.rotation = Quaternion.RotateTowards(playerModel.rotation, Quaternion.LookRotation(playerInputVec), turnSpeed * 360 * Time.deltaTime);

            //}
            else
            {
                Vector3 lastFacing = playerModel.forward;

                //face direction of movement
                playerModel.rotation = Quaternion.RotateTowards(playerModel.rotation, Quaternion.LookRotation(playerInputVec),  360/turnSpeed * Time.deltaTime);


                //get new speedTime
                speedTime = Vector3.Project(lastFacing*speedTime,playerModel.forward).magnitude;
                speedTime += Time.deltaTime;

                

            }

            speed = walkCurve.Evaluate(speedTime/walkAccelerationTime)*maxWalkSpeed;

            
            velocity = playerModel.forward * speed * Time.deltaTime;
            
                

            if (!playerCC.isGrounded)
            {
                //if not on ground, add gravity
                velocity += Vector3.down * gravity * Time.deltaTime;
            }



            Debug.Log(velocity/Time.deltaTime);



            Debug.Log(playerCC.isGrounded);
            


            if (Input.GetButtonDown("Jump"))
            {
                //do jump if on ground; else switch to flying
                if (Physics.CheckSphere(groundCheck.transform.position,
                        groundCheck.radius,
                        LayerMask.GetMask("Ground")))
                {
                    //is on ground, so jump

                    //playerRB.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                    //playerRB.velocity += Vector3.up * jumpForce;
                    
                    Debug.Log("jump");


                }
                else
                {
                    // is not on ground, so switch to flying
                    switchToFlying();
                    Debug.Log("not jump");
                }

            }


            


        }

        playerCC.Move(velocity);

       
    }
    void switchToFlying()
    {
        //make model fly

        Transform playerModel = playerTrans.Find("playerModel/Capsule").transform;
        playerModel.localRotation = Quaternion.Euler(90, 0, 0);


        flying = true;

    }

    void switchToWalking()
    {
        //make model walk
        Transform playerModel = playerTrans.Find("playerModel/Capsule").transform;
        playerTrans.rotation = Quaternion.Euler(0, 0, 0);
        playerModel.localRotation = Quaternion.Euler(0, 0, 0);


        flying = false;

    }
}
