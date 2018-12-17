using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System;

public class LevelScript : MonoBehaviour
{
    public string levelFile;

    private string[] levelData;

	// Use this for initialization
	void Start()
    {
        levelData = readFile(levelFile);
	}
	
	// Update is called once per frame
	void Update()
    {
		
	}

    private string[] readFile(string file)
    {
        string[] lines = System.IO.File.ReadAllLines("Assets/Levels/" + file);
        //print(lines);

        return lines;
    }
}
