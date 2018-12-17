using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System;

public class LevelScript : MonoBehaviour
{
    public static Quaternion NO_ROTATION = new Quaternion(0, 0, 0, 0);
    public static int LOWER_BOUND = 0;
    public static int UPPER_BOUND = 7;

    public GameObject PF_Room;
    public GameObject PF_Player;
    public Camera PF_Camera;

    public string levelFile;

    private string[] levelData;
    private int levelHeight = 0;
    private int levelWidth = 0;

    private Dictionary<Vector3, GameObject> Rooms;
    private GameObject Player;
    private Camera Camera;

	// Use this for initialization
	void Start()
    {
        Rooms = new Dictionary<Vector3, GameObject>();

        levelData = readFile(levelFile);

        for (int y = 0; y < levelHeight * 2; y += 2)
        {
            for (int x = 0; x < levelWidth * 2; x += 2)
            {
                GameObject Room = Instantiate(PF_Room, new Vector3(x, y, 0), NO_ROTATION) as GameObject;
                Rooms.Add(Room.transform.position, Room);
            }
        }

        Player = Instantiate(PF_Player, new Vector3(0, UPPER_BOUND - 1, 0), NO_ROTATION);
        Camera = Instantiate(PF_Camera, Player.transform.position, NO_ROTATION);
        Camera.SendMessage("SetPlayer", Player);
	}
	
	// Update is called once per frame
	void Update()
    {
		
	}

    private string[] readFile(string file)
    {
        string[] lines = System.IO.File.ReadAllLines("Assets/Levels/" + file);
        levelHeight = lines.Length;
        levelWidth = lines[0].Length;
        UPPER_BOUND = levelWidth * 2 - 1;
        print(levelWidth + "," + levelHeight);

        return lines;
    }
}
