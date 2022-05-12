using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class playerControllerScript: MonoBehaviour
{

    


    [Header("Walking")]
    public float maxWalkSpeed;
    public float sprintSpeed;
    //public float walkSpeedScale;
    public float walkAccelerationTime = 1; //in seconds
    public float turnSpeed = 1; // in second to do full circle
    public AnimationCurve walkCurve;
    public AnimationCurve jumpCurve;
    


    [Header("Gliding")]
    public float maxRollSpeed;
    public float maxPitchSpeed;
    [RangeAttribute(0,1)]
    public float minGravity;
    

    [Header("Jetpack")]

    public GameObject jetpackObj;
    public float jetpackPower;
    public ParticleSystem[] jetPackFires;
    public Animator wingsAnim;
    public float maxFuel;
    public float refuelRate;
    public Slider fuelSlider;
    private float currentFuel;
    private bool jetpackEnabled;


    [Header("Other")]
    public Transform playerCamTrans;
    public float camSensitivity;

    public SphereCollider groundCheck;
    public float isGroundedMax;
    public float camMaxDist;



    
    

    CharacterController playerCC;
    Transform playerTrans;
    Transform playerModel;


    float moveVert = 0;
    float moveHorz = 0;

    float speed = 0;
    float speedTime = 0; //used with walk acceleration curve to create walk speed curve

    float jumpTime = -1; //used with jump curve
    float currentJumpHeight = 0;

    const float gravity = 9.81f;

    float isGrounded = 0; //is short timer, will be false after not been on ground for isGroundedMax seconds.
    bool flying = false;

    //for checkpoints
    GameObject currentCheckPoint = null;

    // Start is called before the first frame update
    void Start()
    {
        playerTrans = transform;
        playerCC = GetComponent<CharacterController>();
        //actualMoveVec = new Vector3(0, 0, 0);
        playerModel = transform.Find("playerModel");

        enableJetpack(jetpackEnabled);

        currentFuel = maxFuel;
    }

    // Update is called once per frame
    void Update()
    {

        if (!PauseMenu.isPaused)
        {
            float mouseX = camSensitivity * Input.GetAxis("Mouse X");
            float mouseY = -camSensitivity * Input.GetAxis("Mouse Y");




            Vector3 velocity = Vector3.zero;

            if (playerCC.isGrounded)
            {
                isGrounded = isGroundedMax;
            }
            else
            {
                isGrounded -= Time.deltaTime;
                isGrounded = Mathf.Clamp(isGrounded, 0, isGroundedMax);
            }

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

                Vector3 playerRot = Vector3.zero;
                playerRot.x += -moveVert * maxPitchSpeed * Time.deltaTime;
                playerRot.z += -moveHorz * maxRollSpeed * Time.deltaTime;

                //playerModel.localRotation = Quaternion.Euler(playerRot);

                //playerModel.RotateAround(playerModel.up, playerRot.x);
                playerModel.Rotate(playerRot);

                Destroy(camDir.gameObject);

                //angle between x and xz plane
                float xAngleGrav = playerModel.rotation.eulerAngles.x;

                //need to make x angle fit {0,90}, and then need that range to fit {minGravity,1}

                if (180 <= xAngleGrav && xAngleGrav <= 360)
                {
                    xAngleGrav -= 180;
                }
                if (90 <= xAngleGrav && xAngleGrav <= 180)
                {
                    xAngleGrav = 180 - xAngleGrav;
                }
                //now xAngle is between 0 and 90



                float gravModifier = (xAngleGrav / (90 / 1 - minGravity)) + minGravity;
                //gravModifeir is in range {minGravity,1}



                //determine speed

                //angle between x and xz plane
                float xAngleSp = playerModel.rotation.eulerAngles.x;

                //need to make x angle fit {-90,90}, and then need that range to fit {-1,1}
                if (180 <= xAngleSp && xAngleSp <= 360)
                {
                    xAngleSp -= 360;
                }
                //xAngleSp new in range {-180,180}

                if (90 <= Mathf.Abs(xAngleSp) && Mathf.Abs(xAngleSp) <= 180)
                {
                    xAngleSp = Mathf.Sign(xAngleSp) * 180 - xAngleSp;
                }
                //xAngleSp now in range {-90,90}

                xAngleSp /= 90;

                //xAngleSp now in range {-1,1}

                speed += xAngleSp * gravity * Time.deltaTime;
                speed = Mathf.Clamp(speed, 0, Mathf.Infinity);
                //Debug.Log("Angle: "+ xAngleSp + " speed: " + speed);



                //use jetpack
                if (Input.GetButton("Sprint") && jetpackEnabled && currentFuel > 0)
                {
                    speed += jetpackPower * Time.deltaTime;
                    currentFuel -= Time.deltaTime;
                    Mathf.Clamp(currentFuel, 0, maxFuel);
                    fuelSlider.value = currentFuel / maxFuel;
                    turnOnJetpack();
                }
                else
                {
                    turnOffJetpack();
                }




                //Debug.Log("grav " + gravModifier +" "+ gravModifier * Vector3.down * gravity * Time.deltaTime);
                velocity = playerModel.forward * speed * Time.deltaTime + gravModifier * Vector3.down * gravity * Time.deltaTime;

                //playerCC.Move(velocity);




                //speed = velocity.magnitude;


                //Debug.Log(velocity.magnitude);






                // if player 'jump' while flying or hit ground, switch to walking controls
                //if (Input.GetButtonDown("Jump") || Physics.CheckSphere(groundCheck.transform.position,
                //            groundCheck.radius,
                //            LayerMask.GetMask("Ground")))
                if (Input.GetButtonDown("Jump") || Convert.ToBoolean(isGrounded))
                {
                    switchToWalking();
                }
                //Debug.Log(velocity / Time.deltaTime);

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
                    xRot = Mathf.Clamp(playerCamTrans.localEulerAngles.x, 0, 60);
                }
                else
                {
                    xRot = Mathf.Clamp(playerCamTrans.localEulerAngles.x, 345, 360);
                }


                playerCamTrans.localEulerAngles = new Vector3(
                    xRot,
                    playerCamTrans.localEulerAngles.y,
                    0
                    );

                ////move cam closer if clipping

                //RaycastHit hit;
                //Debug.DrawRay(playerCamTrans.position, -playerCamTrans.forward*camMaxDist, Color.red, .5f);
                //if (Physics.Raycast(playerCamTrans.position, -playerCamTrans.forward, out hit, camMaxDist, LayerMask.GetMask("Player")))
                //{
                //    Transform actualCam = playerCamTrans.GetComponentInChildren<Camera>().transform;
                //    actualCam.localPosition = new Vector3(0, 0, -hit.distance*camMaxDist);
                //    //Debug.Log("clipping: "+hit.distance);
                //}
                //else
                //{
                //    Transform actualCam = playerCamTrans.GetComponentInChildren<Camera>().transform;
                //    actualCam.localPosition = new  Vector3(0, 0, -camMaxDist);
                //    Debug.Log("not clipping");
                //}


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
                if (playerInputVec.magnitude == 0)
                {
                    //no player input, slow character
                    speedTime = Mathf.Clamp(speedTime - Time.deltaTime, 0, walkAccelerationTime);
                }
                else if (180 - Vector3.Angle(playerModel.forward, playerInputVec) <= 2.5 && speedTime != 0)
                {
                    //reverse speed
                    speedTime = Mathf.Clamp(speedTime - 1.5f * Time.deltaTime, 0, walkAccelerationTime);
                    //Debug.Log(intendedMoveTime);

                    //face direction of movement
                    //playerModel.rotation = Quaternion.RotateTowards(playerModel.rotation, Quaternion.LookRotation(playerInputVec), turnSpeed * 360 * Time.deltaTime);

                }
                else
                {
                    Vector3 lastFacing = playerModel.forward;

                    //face direction of movement
                    playerModel.rotation = Quaternion.RotateTowards(playerModel.rotation, Quaternion.LookRotation(playerInputVec), 360 / turnSpeed * Time.deltaTime);


                    //get new speedTime
                    speedTime = Vector3.Project(lastFacing * speedTime, playerModel.forward).magnitude;
                    speedTime += Time.deltaTime;



                }

                speed = walkCurve.Evaluate(speedTime / walkAccelerationTime) * maxWalkSpeed;

                if (Input.GetButton("Sprint"))
                {
                    //Debug.Log((speed / Time.deltaTime));
                    speed += sprintSpeed;
                }
                else
                {

                }

                velocity = playerModel.forward * speed * Time.deltaTime;







                //Debug.Log(velocity/Time.deltaTime);



                //Debug.Log(playerCC.isGrounded);





                if (Input.GetButtonDown("Jump"))
                {
                    //do jump if on ground; else switch to flying
                    //if (Physics.CheckSphere(groundCheck.transform.position,
                    //        groundCheck.radius,
                    //        LayerMask.GetMask("Ground")))
                    if (Convert.ToBoolean(isGrounded))
                    {
                        //is on ground, so jump

                        //playerRB.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                        //playerRB.velocity += Vector3.up * jumpForce;

                        jumpTime = 0;
                        isGrounded = 0;


                        float frameHeight = jumpCurve.Evaluate(jumpTime) - currentJumpHeight;
                        velocity += playerModel.up * frameHeight;

                        currentJumpHeight += frameHeight;


                        //Debug.Log("jump");


                    }
                    else
                    {
                        // is not on ground, so switch to flying
                        switchToFlying();
                        //Debug.Log("not jump");
                    }

                }

                //contiue jumping if currently jumping
                if (jumpTime >= 0)
                {
                    jumpTime += Time.deltaTime;
                    float frameHeight = jumpCurve.Evaluate(jumpTime) - currentJumpHeight;
                    velocity += playerModel.up * frameHeight;

                    currentJumpHeight += frameHeight;

                    if (jumpTime >= jumpCurve[jumpCurve.length - 1].time)
                    {
                        //jump over
                        jumpTime = -1;
                        currentJumpHeight = 0;
                    }


                }
                else if (!Convert.ToBoolean(isGrounded))
                {
                    //if not on ground, add gravity
                    velocity += Vector3.down * gravity * Time.deltaTime;
                }

                //Debug.Log(velocity);



            }

            //refuel jetpack
            if (Convert.ToBoolean(isGrounded) && currentFuel < maxFuel)
            {
                //Debug.Log("refueling");
                currentFuel += refuelRate * Time.deltaTime;
                Mathf.Clamp(currentFuel, 0, maxFuel);
                fuelSlider.value = currentFuel / maxFuel;
            }

            //actually move player
            playerCC.Move(velocity);
        }

       
    }
    void switchToFlying()
    {
        //make model fly

        Transform playerModel = playerTrans.Find("playerModel/Capsule").transform;
        playerModel.localRotation = Quaternion.Euler(90, 0, 0);

        wingsAnim.SetTrigger("Extend");

        flying = true;

    }

    void switchToWalking()
    {
        //make model walk
        Transform playerCapsule = playerTrans.Find("playerModel/Capsule").transform;
        //playerModel.rotation = Quaternion.Euler(0, 0, 0);
        playerCapsule.localRotation = Quaternion.Euler(0, 0, 0);
        wingsAnim.SetTrigger("Close");

        turnOffJetpack();

        flying = false;

    }

    public void setCheckPoint(GameObject checkPoint)
    {
        if (Convert.ToBoolean(isGrounded))
        {
            //disable currentCheckPoint OOBPlane
            if(currentCheckPoint != null)
            {
                currentCheckPoint.transform.Find("OOBPlane").gameObject.SetActive(false);
            }
        
            currentCheckPoint = checkPoint;
            currentCheckPoint.transform.Find("OOBPlane").gameObject.SetActive(true);
        
            //Debug.Log(currentCheckPoint);
        }
        
    }

    public void resetToCheckPoint()
    {
        playerCC.enabled = false;
        transform.position = currentCheckPoint.transform.position;
        playerCC.enabled = true;
        speed = 0;
        speedTime = 0;
        if (flying)
        {
            switchToWalking();
        }
        

    }

    public void enableJetpack(bool value = true)
    {
        jetpackEnabled = value;
        jetpackObj.SetActive(jetpackEnabled);
        fuelSlider.gameObject.SetActive(jetpackEnabled);
    }
    

    public void turnOnJetpack()
    {
        foreach(ParticleSystem ps in jetPackFires)
        {
            if (!ps.isPlaying)
            {
                ps.Play();
            }
        }
    }

    public void turnOffJetpack()
    {
        foreach (ParticleSystem ps in jetPackFires)
        {
            if (!ps.isStopped)
            {
                ps.Stop();
            }
        }
    }

}
