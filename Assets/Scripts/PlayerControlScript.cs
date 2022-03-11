using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlScript : MonoBehaviour
{

    

    [Header("Walking")]
    public float maxWalkSpeed;
    //public float walkSpeedScale;
    public float walkAccelerationTime = 1; //in seconds
    public AnimationCurve walkCurve;
    public float jumpForce;


    [Header("Gliding")]
    public float maxRollSpeed;
    public float maxPitchSpeed;



    [Header("Other")]
    public Transform playerCamTrans;
    public float camSensitivity;

    public SphereCollider groundCheck;


    Rigidbody playerRB;
    Transform playerTrans;


    float moveVert = 0;
    float moveHorz = 0;

    Vector3 intendedMoveVec;
    float intendedMoveTime;
    Vector3 actualMoveVec;

    bool flying = true;

    // Start is called before the first frame update
    void Start()
    {
        playerTrans = transform;
        playerRB = GetComponent<Rigidbody>();
        actualMoveVec = new Vector3(0, 0, 0);

    }

    void Update()
    {
        float mouseX = camSensitivity * Input.GetAxis("Mouse X");
        float mouseY = -camSensitivity * Input.GetAxis("Mouse Y");

        if (flying)
        {
            //gliding cam controls


            

            playerCamTrans.Rotate(0, mouseX, 0);
            playerCamTrans.Rotate(mouseY, 0, 0);

            //clamp vertical camera angle
            float xRot;
            if(playerCamTrans.localEulerAngles.x < 180)
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

            Vector3 playerRot = playerTrans.rotation.eulerAngles;
            playerRot.x += -moveVert * maxPitchSpeed*Time.deltaTime;
            playerRot.z += -moveHorz * maxRollSpeed * Time.deltaTime;

            playerTrans.rotation = Quaternion.Euler(playerRot);






            Destroy(camDir.gameObject);


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

            Transform camDir = new GameObject().transform;
            camDir.rotation = Quaternion.Euler(0, playerCamTrans.rotation.eulerAngles.y, 0);

            Vector3 lastIntendedVec = intendedMoveVec;
            float lastIntendedMoveTime = intendedMoveTime;

            intendedMoveVec = Vector3.RotateTowards(new Vector3(0, 0, moveVert),
                                                    Mathf.Sign(moveVert) * camDir.forward,
                                                    Mathf.PI, 0);
            intendedMoveVec += Vector3.RotateTowards(new Vector3(moveHorz, 0, 0),
                                                    Mathf.Sign(moveHorz) * camDir.right,
                                                    Mathf.PI, 0);
            intendedMoveVec.Normalize();
            if(intendedMoveVec.magnitude == 0)
            {
                //no movement input; slow character
                intendedMoveTime = Mathf.Clamp(lastIntendedMoveTime - Time.deltaTime,0,walkAccelerationTime);
                
            }
            else
            {
                

                //there are movement inputs, speed up character in right direction.
                intendedMoveTime = Vector3.Project(lastIntendedVec, intendedMoveVec).magnitude;
                intendedMoveTime += Time.deltaTime;
                

                if(actualMoveVec.magnitude == 0)
                {
                    actualMoveVec = intendedMoveVec;
                }
                else
                {
                    //Vector3 lastMoveVec = actualMoveVec;
                   // Debug.Log(intendedMoveVec);
                    if(180 - Vector3.Angle(actualMoveVec,intendedMoveVec) <= 2.5)
                    {
                        //reverse speed
                        intendedMoveTime = Mathf.Clamp(intendedMoveTime - 2.5f*Time.deltaTime, 0, walkAccelerationTime);
                        //Debug.Log(intendedMoveTime);
                    }
                    else
                    {
                        //turn toward new direction
                        actualMoveVec = Vector3.RotateTowards(actualMoveVec, intendedMoveVec, Mathf.PI / 4 * Time.fixedDeltaTime, 0);
                    }
                    
                }
                intendedMoveVec *= Mathf.Clamp(intendedMoveTime, 0, walkAccelerationTime);

            }


            Destroy(camDir.gameObject);


            //float speed = Vector3.Project(actualMoveVec, lastMoveVec).magnitude;

            //calculate new speed to match acceleration curve of (percent of max speed) = x^2.5 (for 0<=x<=1)

            //float newSpeed = Mathf.Clamp(speed * walkSpeedScale,0,maxWalkSpeed);


            float newSpeed = maxWalkSpeed*walkCurve.Evaluate(intendedMoveTime/walkAccelerationTime);
                
                
                
            //Debug.Log(newSpeed);
            actualMoveVec.Normalize();
            actualMoveVec *= newSpeed;

            //face direction of movement

            Transform playerModelTrans = playerTrans.Find("playerModel").transform;
            playerModelTrans.rotation = Quaternion.RotateTowards(playerModelTrans.rotation, Quaternion.LookRotation(actualMoveVec),360* Time.deltaTime);
            Debug.Log(playerModelTrans.rotation);



            


            if (Input.GetButtonDown("Jump"))
            {
                //do jump if on ground; else switch to flying
                if (Physics.CheckSphere(groundCheck.transform.position,
                        groundCheck.radius,
                        LayerMask.GetMask("Ground")))
                        
                {
                    //is on ground, so jump

                    //playerRB.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                    playerRB.velocity += Vector3.up * jumpForce;

                    Debug.Log("jump");


                }
                else
                {
                    // is not on ground, so switch to flying
                    flying = true;
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

            //Debug.Log(moveVector);

            //if(actualMoveVec.magnitude == 0)
            //{
            //    playerRB.AddForce(playerRB.velocity * -5);
            //}
            //else
            //{
               //Debug.Log(actualMoveVec);
            //    playerRB.AddForce(actualMoveVec, ForceMode.Acceleration);
            //}

            Vector3 newVelocity = actualMoveVec;
            newVelocity.y += playerRB.velocity.y;

            playerRB.velocity = newVelocity;



        }



    }

    //float calcNewSpeed(float currentSpeed, float maxSpeed, float accel)
    //{
    //    //calculate new speed based on speed curve of (percent of max Speed) = x^(exp)
    //    //return (x+accel)^exp

    //    //float x = Mathf.Pow(currentSpeed/maxSpeed, 1 / exp);
    //    //float newX = Mathf.Clamp(x+accel,0,1);
    //    //float percentOfMaxSpeed = Mathf.Pow(newX,exp);
    //    //return percentOfMaxSpeed * maxSpeed;

        

    //    return maxSpeed *+ walkCurve.


    //}
}
