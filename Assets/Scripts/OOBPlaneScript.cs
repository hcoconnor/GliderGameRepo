using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OOBPlaneScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        playerControllerScript pcScript =
            other.transform.root.GetComponent<playerControllerScript>();

        if(pcScript != null)
        {
            //other is player
            pcScript.resetToCheckPoint();
            Debug.Log("plane enter");
        }
    }
}
