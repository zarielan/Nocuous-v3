using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System;

/* 
 *  The main script file when the level loads. Everything in the map is generated from this file.
 */
public class LevelScript : MonoBehaviour
{
    /* CONSTANTS */
    public static Quaternion NO_ROTATION = new Quaternion(0, 0, 0, 0);

    // Defines the lowest and highest x and y coordinate in the level.
    // TODO un-hardcode this
    public static int LOWER_BOUND = 0;
    public static int UPPER_BOUND = 7;
    
    /* Game Objects (and other public stuff) */
    public GameObject PF_Room;
    public GameObject PF_Player;
    public GameObject PF_Fire;
    public GameObject PF_Gas;
    public GameObject PF_Hole;
    public GameObject PF_FireExtinguisher;
    public GameObject PF_Plank;

    public Camera PF_Camera;

    public TextAsset levelFile;

    /* Class variables */
    private string[] levelData;
    private int levelHeight = 0;
    private int levelWidth = 0;

    private Dictionary<Vector3, GameObject> Rooms;
    private GameObject Player;
    private Camera Camera;

    void Start()
    {
        Rooms = new Dictionary<Vector3, GameObject>();

        levelData = readFile(levelFile);

        // Read the data from the map file and creates the rooms. 
        // Also add the elements to each room (fire, gas, hole)
        for (int y = 0; y < levelHeight; y++)
        {
            for (int x = 0; x < levelWidth; x++)
            {
                int roomX = x * 2;
                int roomY = y * 2;
                var roomPosition = new Vector3(roomX, roomY, 0);

                GameObject Room = Instantiate(PF_Room, roomPosition, NO_ROTATION) as GameObject;
                Rooms.Add(roomPosition, Room);

                var type = levelData[y][x];
                if (type == 'F')
                {
                    CreateElementInRoom(PF_Fire, Room);
                }
                else if (type == 'G')
                {
                    CreateElementInRoom(PF_Gas, Room);
                }
                else if (type == 'H')
                {
                    CreateElementInRoom(PF_Hole, Room);
                }
            }
        }

        // Create the player
        Player = Instantiate(PF_Player, new Vector3(0, UPPER_BOUND - 1, 0), NO_ROTATION);
        Player.SendMessage("SetLevelInstance", this);

        // Create the camera
        Camera = Instantiate(PF_Camera, Player.transform.position, NO_ROTATION);
        Camera.SendMessage("SetPlayer", Player);

        AddItems();
    }

    private string[] readFile(TextAsset file)
    {
        string[] lines = Regex.Split(file.text, Environment.NewLine);
        
        foreach (var l in lines)
            print(l);

        // The list is reversed because in the game, the y-axis points upwards, but we're
        // reading the file from top to bottom
        Array.Reverse(lines);
        levelHeight = lines.Length;
        levelWidth = lines[0].Length;
        UPPER_BOUND = levelWidth * 2 - 1;

        return lines;
    }

    /*
     *  On each turn of the player, somethings can happen.
     */
    public void OnTurn()
    {
        // 50% Chance of spreading gas or fire.
        if (UnityEngine.Random.Range(0, 2) == 1)
        {
            // Hashset that will contain places we'll put gas on. To prevent duplicates.
            var addGas = new HashSet<GameObject>();
            // For fire
            var addFire = new HashSet<GameObject>();

            // Iterate through all rooms to find gases or fire
            foreach (var r in Rooms)
            {
                // Check for each room's children
                Transform roomTransform = r.Value.transform;
                foreach (Transform t in roomTransform)
                {
                    // If the room has gas in it
                    if (t.name == GetPrefabName(PF_Gas))
                    {
                        // Find a neighbor.
                        var possibleNeighbors = GetRoomNeighbors(r.Key);
                        int index = UnityEngine.Random.Range(0, possibleNeighbors.Count);
                        Vector3 next = possibleNeighbors[index];

                        GameObject Room = Rooms[next];
                        Transform roomChildren = Room.transform;
                        bool hasGas = false;

                        // Check if that neighbor has no gas in it
                        foreach (Transform c in roomChildren)
                        {
                            if (c.name == GetPrefabName(PF_Gas))
                            {
                                hasGas = true;
                                break;
                            }
                        }

                        // If it doesn't, spread gas there!
                        if (!hasGas)
                            addGas.Add(Room);
                    }
                    // If the room has fire in it
                    else if (t.name == GetPrefabName(PF_Fire))
                    {
                        // Find a neighbor.
                        var possibleNeighbors = GetRoomNeighbors(r.Key);
                        int index = UnityEngine.Random.Range(0, possibleNeighbors.Count);
                        Vector3 next = possibleNeighbors[index];

                        GameObject Room = Rooms[next];
                        Transform roomChildren = Room.transform;
                        bool hasFire = false;

                        // Check if that neighbor has no fire in it
                        foreach (Transform c in roomChildren)
                        {
                            if (c.name == GetPrefabName(PF_Fire))
                            {
                                hasFire = true;
                                break;
                            }
                        }

                        // If it doesn't, spread gas there!
                        if (!hasFire)
                            addFire.Add(Room);
                    }
                }
            }

            // Set gas to these rooms
            addGas = new HashSet<GameObject>(addGas);
            foreach (GameObject r in addGas)
                CreateElementInRoom(PF_Gas, r);

            // Set fire to these rooms
            addFire = new HashSet<GameObject>(addFire);
            foreach (GameObject r in addFire)
                CreateElementInRoom(PF_Fire, r);

            // If the room has two elements or more in it, when need to do something
            foreach (var r in Rooms)
            {
                bool hasGas = false;
                bool hasFire = false;
                bool hasHole = false;

                // Check for each room's children
                Transform roomTransform = r.Value.transform;
                foreach (Transform t in roomTransform)
                {
                    if (t.name == GetPrefabName(PF_Gas))
                        hasGas = true;
                    if (t.name == GetPrefabName(PF_Fire))
                        hasFire = true;
                    if (t.name == GetPrefabName(PF_Hole))
                        hasHole = true;
                }

                // Gas and Fire
                if (hasGas && hasFire && !hasHole)
                {
                    Destroy(r.Value.transform.Find(GetPrefabName(PF_Gas)).gameObject);
                    Destroy(r.Value.transform.Find(GetPrefabName(PF_Fire)).gameObject);
                    CreateElementInRoom(PF_Hole, r.Value);
                    //TODO explode
                }

                // Hole and Fire
                if (!hasGas && hasFire && hasHole)
                {
                    Destroy(r.Value.transform.Find(GetPrefabName(PF_Fire)).gameObject);
                }

                // All
                if (hasGas && hasFire && hasHole)
                {
                    Destroy(r.Value.transform.Find(GetPrefabName(PF_Gas)).gameObject);
                    Destroy(r.Value.transform.Find(GetPrefabName(PF_Fire)).gameObject);
                    //TODO explode
                }
            }
        }
    }

    private List<Vector3> GetRoomNeighbors(Vector3 Current)
    {
        float x = Current.x;
        float y = Current.y;

        List<Vector3> output = new List<Vector3>();

        if (x + 2 <= UPPER_BOUND)
            output.Add(new Vector3(x + 2, y));
        if (x - 2 >= LOWER_BOUND)
            output.Add(new Vector3(x - 2, y));
        if (y + 2 <= UPPER_BOUND)
            output.Add(new Vector3(x, y + 2));
        if (y - 2 >= LOWER_BOUND)
            output.Add(new Vector3(x, y - 2));

        return output;
    }

    /*
     *  Add the element to the room specified, given the prefab.
     */
    private void CreateElementInRoom(GameObject prefab, GameObject room)
    {
        GameObject elem = Instantiate(prefab, room.transform.position, NO_ROTATION);
        elem.transform.parent = room.transform;
    }

    private void AddItems()
    {
        var possibleRooms = new List<GameObject>();

        foreach (var r in Rooms)
        {
            bool add = true;

            Transform children = r.Value.transform;
            foreach (Transform c in children)
            {
                if (c.name == GetPrefabName(PF_Hole) || c.name == GetPrefabName(PF_Fire) || c.name == GetPrefabName(PF_Gas))
                {
                    add = false;
                    break;
                }
            }

            if (add)
                possibleRooms.Add(r.Value);
        }

        int randomRoom = UnityEngine.Random.Range(0, possibleRooms.Count);
        CreateElementInRoom(PF_FireExtinguisher, possibleRooms[randomRoom]);
        possibleRooms.RemoveAt(randomRoom);

        randomRoom = UnityEngine.Random.Range(0, possibleRooms.Count);
        CreateElementInRoom(PF_Plank, possibleRooms[randomRoom]);
    }

    public Dictionary<Vector3, GameObject> GetRooms()
    {
        return Rooms;
    }

    public string GetPrefabName(GameObject prefab)
    {
        return prefab.name + "(Clone)";
    }
}
