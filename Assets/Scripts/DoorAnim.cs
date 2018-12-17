using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DoorAnim : MonoBehaviour
{
    private bool reverse = false;
    private float clockwise = 1f;

    // Use this for initialization
    void Start()
    {
    }

    public void Clockwise(bool clock)
    {
        clockwise = clock ? 1f : -1f;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.rotation == Quaternion.Euler(0f, 0f, 180))
        {
            reverse = true;
        }

        if (reverse && Math.Abs(transform.rotation.w) == 1)
            Destroy(this.gameObject);

        if (reverse)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, 0f, 0), Time.deltaTime * 8f);
        else
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0f, 0f, 180 * clockwise), 60f);
    }
}
