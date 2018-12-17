using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System;

public class LevelScript : MonoBehaviour
{
    public static Quaternion NO_ROTATION = new Quaternion(0, 0, 0, 0);

    public GameObject PF_Room;

    public string levelFile;

    private string[] levelData;
    private int levelHeight = 0;
    private int levelWidth = 0;

	// Use this for initialization
	void Start()
    {
        levelData = readFile(levelFile);

        for (int y = 0; y < levelHeight * 2; y += 2)
        {
            for (int x = 0; x < levelWidth * 2; x += 2)
            {
                Instantiate(PF_Room, new Vector3(x, y, 0), NO_ROTATION);
            }
        }
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
        print(levelWidth + "," + levelHeight);

        return lines;
    }
}
