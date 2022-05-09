using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(playerControllerScript))]
class PlayerControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Label(" ");

        if (GUILayout.Button("Enable Jetpack"))
        {
            playerControllerScript script = (playerControllerScript)target;
            script.enableJetpack(true);
        }

        if (GUILayout.Button("Disable Jetpack"))
        {
            playerControllerScript script = (playerControllerScript)target;
            script.enableJetpack(false);
        }

    }
}
