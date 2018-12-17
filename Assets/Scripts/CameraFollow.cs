using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private GameObject player;

    private Vector3 velocity = Vector3.zero;
    private const float fixedZ = -10;

    void LateUpdate()
    {
        Vector3 newPos = Vector3.SmoothDamp(transform.position, player.transform.position, ref velocity, 0.3f);
        newPos.z = fixedZ;
        transform.position = newPos;
    }

    public void SetPlayer(GameObject Pl)
    {
        player = Pl;
    }
}
