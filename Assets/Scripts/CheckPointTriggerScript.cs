using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointTriggerScript : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        playerControllerScript pcScript = 
            other.transform.root.GetComponent<playerControllerScript>();

        if (pcScript != null)
        {
            //other is player

            pcScript.setCheckPoint(transform.parent.gameObject);
            Debug.Log("trigger enter");
        }
        
    }

}
