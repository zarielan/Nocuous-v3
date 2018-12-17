using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject player;

    private Vector3 velocity = Vector3.zero;
    private float fixedZ;

    private void Start()
    {
        fixedZ = transform.position.z;
    }

    void LateUpdate()
    {
        Vector3 newPos = Vector3.SmoothDamp(transform.position, player.transform.position, ref velocity, 0.1f);
        newPos.z = fixedZ;
        transform.position = newPos;
    }
}
