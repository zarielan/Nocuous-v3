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
    private Vector3 nextRoomPosition;

    private bool isMoving = false;
    private bool isPlaying = true;
    private int health = 2;

    void Start()
    {
        currentDirection = Direction.DOWN;
        newPosition = transform.position;

        nextRoomPosition = Vector3.zero;
    }
    
    void Update()
    {
        // The player turns depending on where you press the arrow key. If you press an arrow key and 
        // is already facing that direction, you get sent to that room.

        if (isPlaying)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
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
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
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
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
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
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
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
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                level.OnItemUse();
            }

            if (Input.GetMouseButtonDown(0))
            {
                level.OnMouseClick(Input.mousePosition);
            }
        }

        SetNewRoomPosition();

        // Makes the player move and rotate to its new position and rotation smoothly.
        isMoving = transform.position != newPosition;
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * SPEED);
        transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * SPEED);

        if (health <= 0 && isPlaying)
            level.OnLevelExit("Aww you died!");
    }
    
    /*
     *  On each player movement between rooms, it needs to check if the player can go there. This will also do the door slam animation.
     */ 
    private void Move(int x, int y)
    {
        int newX = (int)Math.Round(transform.position.x, 0) + x;
        int newY = (int)Math.Round(transform.position.y, 0) + y;

        // Check if the new position is within map boundaries and if you are allowed to move (moving/rotating animation is done)
        if (level.GetRooms().ContainsKey(new Vector3(newX, newY, 0)) && !isMoving)
        {            
            var next = new Vector3(newX, newY, 0);

            // Check if the new room isn't a hole.
            GameObject newRoom = level.GetRooms()[next];
            foreach (Transform t in newRoom.transform)
            {
                if (t.name == level.GetPrefabName(level.PF_Hole) && newRoom.transform.Find(level.GetPrefabName(level.PF_PlankBridge)) == null)
                    return;
            }
            
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

            // Alert the level that the player has moved. Sending in the current position before moving
            level.OnTurn(newPosition, next);

            // If so, then the player moves.
            newPosition = next;
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

    /*
     *  This is so much better than OnCollisionEnter2D holy shit...oh well, can't change the code na lol
     */ 
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If the player has entered the exit room,
        if (collision.gameObject.name == level.GetPrefabName(level.PF_ExitRoom))
        {
            // Start exiting the level
            level.OnLevelExit("You made it!");
        }
        else if (collision.gameObject.name == level.GetPrefabName(level.PF_Gas))
        {
            if (!level.checkedHealth)
            {
                print("Hit by gas. Decreasing health by 1 point");
                SetHealth(health - 1);
                level.checkedHealth = true;
            }
        }
        else if (collision.gameObject.name == level.GetPrefabName(level.PF_Fire))
        {
            if (!level.checkedHealth)
            {
                print("Hit by fire. You dead bro lmao");
                SetHealth(0);
                level.checkedHealth = true;
            }
        }
    }

    public void SetIsPlaying(bool playing)
    {
        isPlaying = playing;
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }

    private void SetNewRoomPosition()
    {
        int x = 0;
        int y = 0;
        if (currentDirection == Direction.DOWN)
        {
            x = 0;
            y = -2;
        }
        else if (currentDirection == Direction.UP)
        {
            x = 0;
            y = 2;
        }
        else if (currentDirection == Direction.LEFT)
        {
            x = -2;
            y = 0;
        }
        else if (currentDirection == Direction.RIGHT)
        {
            x = 2;
            y = 0;
        }
        nextRoomPosition = new Vector3((int)Math.Round(transform.position.x, 0) + x, (int)Math.Round(transform.position.y, 0) + y, 0);
    }

    public Vector3 GetNewRoomPosition()
    {
        return nextRoomPosition;
    }

    public void SetHealth(int x)
    {
        health = x;
        level.UpdateHealthBar(health);
    }

    public Vector3 GetNewPosition()
    {
        return newPosition;
    }

    public int GetHealth()
    {
        return health;
    }
}
