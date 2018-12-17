using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    public GameObject PF_Door_H;
    public GameObject PF_Door_V;

    enum Direction { UP, DOWN, LEFT, RIGHT }

    private Direction currentDirection;

    private const float SPEED = 30f;

    private Vector3 newPosition;
    private Vector3 vel;
    private Quaternion newRotation;

    private bool isMoving = false;

    void Start()
    {
        currentDirection = Direction.DOWN;
        newPosition = transform.position;
    }
	
	void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentDirection == Direction.RIGHT)
            {
                Move(2, 0);
            }
            else
            {
                currentDirection = Direction.RIGHT;
                newRotation = Quaternion.Euler(0, 0, 90);
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentDirection == Direction.LEFT)
            {
                Move(-2, 0);
            }
            else
            {
                currentDirection = Direction.LEFT;
                newRotation = Quaternion.Euler(0, 0, 270);
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentDirection == Direction.UP)
            {
                Move(0, 2);
            }
            else
            {
                currentDirection = Direction.UP;
                newRotation = Quaternion.Euler(0, 0, 180);
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentDirection == Direction.DOWN)
            {
                Move(0, -2);
            }
            else
            {
                currentDirection = Direction.DOWN;
                newRotation = Quaternion.Euler(0, 0, 0);
            }
        }

        isMoving = transform.position != newPosition;
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * SPEED);
        transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * SPEED);
    }
    
    private void Move(int x, int y)
    {
        int newX = (int)Math.Round(transform.position.x, 0) + x;
        int newY = (int)Math.Round(transform.position.y, 0) + y;

        if (newX >= LevelScript.LOWER_BOUND && newX <= LevelScript.UPPER_BOUND && newY >= LevelScript.LOWER_BOUND && newY <= LevelScript.UPPER_BOUND && !isMoving)
        {
            /* Player Moves!! */

            newPosition = new Vector3(newX, newY, 0);

            /* Door Animation */

            // Horizontal:
            if (x != 0)
            {
                GameObject Door = Instantiate(PF_Door_H) as GameObject;
                Door.transform.position = new Vector3((int)Math.Round(transform.position.x, 0) + x / 2, (int)Math.Round(transform.position.y, 0) - 0.25f, 0);
                Door.SendMessage("Clockwise", x > 1);
            }
            // Vertical:
            else
            {
                GameObject Door = Instantiate(PF_Door_V) as GameObject;
                Door.transform.position = new Vector3((int)Math.Round(transform.position.x, 0) - 0.25f, (int)Math.Round(transform.position.y, 0) + y / 2, 0);
                Door.SendMessage("Clockwise", y < 1);
            }
        }
    }
}
