using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetpackTriggerScript : MonoBehaviour
{
    bool triggered = false;
    private void OnTriggerEnter(Collider other)
    {
        playerControllerScript pcScript =
            other.transform.root.GetComponent<playerControllerScript>();

        if (!triggered && pcScript != null)
        {
            triggered = true;
            Debug.Log("triggered");
            pcScript.enableJetpack();
        }

    }
}
