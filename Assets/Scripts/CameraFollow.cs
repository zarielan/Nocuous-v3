using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *  Code to follow the player
 */ 
public class CameraFollow : MonoBehaviour
{
    private GameObject player;

    private Vector3 velocity = Vector3.zero;

    // Since the camera is viewing a 2D screen, its Z axis is fixed.
    private const float fixedZ = -10;

    void LateUpdate()
    {
        // Smooth damp to create a spring/smooth camera follow
        Vector3 newPos = Vector3.SmoothDamp(transform.position, player.transform.position, ref velocity, 0.3f);
        newPos.z = fixedZ;
        transform.position = newPos;
    }

    public void SetPlayer(GameObject Pl)
    {
        player = Pl;
    }
}
