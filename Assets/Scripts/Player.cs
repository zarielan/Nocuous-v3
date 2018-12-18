using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
 *  The class that controls the player
 */ 
public class Player : MonoBehaviour
{
    /* Game Objects */
    public GameObject PF_Door_H;
    public GameObject PF_Door_V;

    /* Class variables */
    enum Direction { UP, DOWN, LEFT, RIGHT }

    private Direction currentDirection;

    private const float SPEED = 25f;

    private Vector3 newPosition;
    private Vector3 vel;
    private Quaternion newRotation;
    private LevelScript level;

    private bool isMoving = false;

    void Start()
    {
        currentDirection = Direction.DOWN;
        newPosition = transform.position;
    }
    
    void Update()
    {
        // The player turns depending on where you press the arrow key. If you press an arrow key and 
        // is already facing that direction, you get sent to that room.
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

        // Makes the player move and rotate to its new position and rotation smoothly.
        isMoving = transform.position != newPosition;
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * SPEED);
        transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * SPEED);
    }
    
    /*
     *  On each player movement between rooms, it needs to check if the player can go there. This will also do the door slam animation.
     */ 
    private void Move(int x, int y)
    {
        int newX = (int)Math.Round(transform.position.x, 0) + x;
        int newY = (int)Math.Round(transform.position.y, 0) + y;

        // Check if the new position is within map boundaries and if you are allowed to move (moving/rotating animation is done)
        if (newX >= LevelScript.LOWER_BOUND && newX <= LevelScript.UPPER_BOUND && newY >= LevelScript.LOWER_BOUND && newY <= LevelScript.UPPER_BOUND && !isMoving)
        {            
            var next = new Vector3(newX, newY, 0);

            // Check if the new room isn't a hole.
            GameObject newRoom = level.GetRooms()[next];
            foreach (Transform t in newRoom.transform)
            {
                if (t.name == level.GetPrefabName(level.PF_Hole))
                    return;
            }

            // If so, then the player moves.
            newPosition = next;

            // Do the door animation:
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

            // Alert the level that the player has moved
            level.OnTurn();
        }
    }

    public void SetLevelInstance(LevelScript lvlscr)
    {
        level = lvlscr;
    }

    /*
     *  If the player has collided on something, and if it's an item, let the level handle it
     */ 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if it's either the fire extinguisher or the plank
        if (collision.gameObject.name == level.GetPrefabName(level.PF_FireExtinguisher) || collision.gameObject.name == level.GetPrefabName(level.PF_Plank))
        {
            level.OnItemGet(collision.gameObject);
        }
    }
}
