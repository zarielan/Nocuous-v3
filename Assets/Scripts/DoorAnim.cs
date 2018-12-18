using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
 *  When the door game object is made, it does the door slam animation.
 */
public class DoorAnim : MonoBehaviour
{
    private bool reverse = false;
    
    // If the door should animate clockwise or not
    private float clockwise = 1f;
    
    public void Clockwise(bool clock)
    {
        clockwise = clock ? 1f : -1f;
    }

    void Update()
    {
        // If the first part of the door slam has been reached (it has done a 180) flip it
        if (transform.rotation == Quaternion.Euler(0f, 0f, 180))
            reverse = true;

        // If the door is on reverse now and has went back to its original rotation (it has done its second part)
        // then the door slam animation is finished. Destroy it.
        if (reverse && Math.Abs(transform.rotation.w) == 1)
            Destroy(this.gameObject);

        // Rotate the door depending on which part it's in. !reverse is the first part.
        if (reverse)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, 0f, 0), Time.deltaTime * 8f);
        else
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0f, 0f, 180 * clockwise), 60f);
    }
}
