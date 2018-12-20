using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public GameObject PF_ExitRoom;
    public GameObject PF_PlankBridge;
    public GameObject PF_Selection;
    public GameObject PF_AnimExplode;

    public Camera PF_Camera;
    public Canvas UI_Canvas;

    private TextAsset levelFile;

    public AudioClip sfx_explode;
    public AudioClip sfx_hammer;
    public AudioClip sfx_door;

    /* Class variables */
    private string[] levelData;
    private int levelHeight = 0;
    private int levelWidth = 0;

    private Dictionary<Vector3, GameObject> Rooms;
    private Player player;
    private Camera Camera;
    private float fadeTime = 1f;
    private UIHandler UI;

    private GameObject selection;
    private int influencePart = 0;
    private Vector3 selectedToInfluence;
    private AudioSource sfx_player;

    void Start()
    {
        print("Starting level!");

        Rooms = new Dictionary<Vector3, GameObject>();

        levelFile = Resources.Load("Level" + StaticClass.LEVEL_NUMBER) as TextAsset;
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

        var Exit = Instantiate(PF_ExitRoom, new Vector3(levelWidth * 2, 0, 0), NO_ROTATION);
        Rooms.Add(Exit.transform.position, Exit);

        // Create the player
        var p = Instantiate(PF_Player, new Vector3(0, UPPER_BOUND - 1, 0), NO_ROTATION);
        player = p.GetComponent<Player>();
        player.SetLevelInstance(this);

        // Create the camera
        Camera = Instantiate(PF_Camera, player.transform.position, NO_ROTATION);
        Camera.SendMessage("SetPlayer", p);

        AddItems();

        UI = UI_Canvas.GetComponent<UIHandler>();
        UI.FadeToGame(fadeTime);

        sfx_player = GetComponent<AudioSource>();
    }

    private string[] readFile(TextAsset file)
    {
        string[] lines = Regex.Split(file.text, Environment.NewLine);
        
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
    public void OnTurn(Vector3 prev_playerPos, Vector3 newPos)
    {
        sfx_player.PlayOneShot(sfx_door);

        // 25% Chance of spreading gas or fire.
        if (UnityEngine.Random.Range(0, 4) == 1)
        {
            SpreadElements();
        }

        var plankBridge = Rooms[prev_playerPos].transform.Find(GetPrefabName(PF_PlankBridge));
        if (plankBridge != null)
            Destroy(plankBridge.gameObject);
    }

    private void SpreadElements()
    {
        // Hashset that will contain places we'll put gas on. To prevent duplicates.
        var addGas = new HashSet<GameObject>();
        // For fire
        var addFire = new HashSet<GameObject>();

        // Iterate through all rooms to find gases or fire
        foreach (var r in Rooms)
        {
            if (r.Value.name == GetPrefabName(PF_ExitRoom))
                continue;

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
                Explode(r.Value);
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
                Explode(r.Value);
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
    public void CreateElementInRoom(GameObject prefab, GameObject room)
    {
        GameObject elem = Instantiate(prefab, room.transform.position, NO_ROTATION);
        elem.transform.parent = room.transform;
    }

    private void AddItems()
    {
        var possibleRooms = new List<GameObject>();

        // Check each room. If it's an empty room, or not the player's spawn point, then add it to the list of
        // possible rooms to add items in.
        foreach (var r in Rooms)
        {
            if (r.Value.name == GetPrefabName(PF_ExitRoom))
                continue;

            bool add = true;

            // Check the room's children if it has any element in it
            Transform children = r.Value.transform;
            foreach (Transform c in children)
            {
                if (c.name == GetPrefabName(PF_Hole) || c.name == GetPrefabName(PF_Fire) || c.name == GetPrefabName(PF_Gas))
                {
                    add = false;
                    break;
                }
            }

            // Check if it's not the player's spawn point
            if (r.Value.transform.position == new Vector3(0, UPPER_BOUND - 1, 0))
                add = false;

            // If the room is sound, we can put an item there            
            if (add)
                possibleRooms.Add(r.Value);
        }

        // Add the fire extinguisher in a random room. Remove that room when it's time to add the plank.
        int randomRoom = UnityEngine.Random.Range(0, possibleRooms.Count);
        CreateElementInRoom(PF_FireExtinguisher, possibleRooms[randomRoom]);
        possibleRooms.RemoveAt(randomRoom);

        randomRoom = UnityEngine.Random.Range(0, possibleRooms.Count);
        CreateElementInRoom(PF_Plank, possibleRooms[randomRoom]);
    }

    public void OnItemGet(GameObject item)
    {
        // If the item is either a fire extinguisher or plank
        // (Idk why I have to check it again, but for safety measures I guess...)
        if (item.name == GetPrefabName(PF_FireExtinguisher) || item.name == GetPrefabName(PF_Plank))
        {
            // Add an image component to it, because the UI needs it
            Image img = item.AddComponent<Image>();
            img.sprite = item.GetComponent<SpriteRenderer>().sprite;

            // Remove everything that made it an item
            Destroy(item.GetComponent<SpriteRenderer>());
            Destroy(item.GetComponent<CircleCollider2D>());
            item.GetComponent<Item>().SetRotating(false);

            // Add it to the canvas
            item.transform.SetParent(UI_Canvas.transform);

            // Notify set canvas
            UI.OnAddChild(item);
        }
    }

    public Dictionary<Vector3, GameObject> GetRooms()
    {
        return Rooms;
    }

    public string GetPrefabName(GameObject prefab)
    {
        return prefab.name + "(Clone)";
    }

    /*
     *  Called when it's time to leave the level
     */
    public void OnLevelExit(string msg)
    {
        // Start fading out
        UI.FadeToBlack(fadeTime, msg);

        // Stop accepting inputs
        player.SetIsPlaying(false);
        UI.SetAcceptingInputs(false);

        // Switch the screen to the next level upon fading
        Invoke("ExitLevel", fadeTime + 0.1f);
    }

    /*
     *  In this case, the next level is just the same level but reset
     */ 
    private void ExitLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Update()
    {
        UI.SetGhostItemPosition(Camera.WorldToScreenPoint(player.GetNewRoomPosition()));
    }

    public void OnItemUse()
    {
        try
        {
            var item = UI.UseItem().transform.GetChild(0);

            var place = player.GetNewRoomPosition();
            var roomToPlace = Rooms[place];

            if (roomToPlace.name == GetPrefabName(PF_ExitRoom))
                return;

            var itemScript = item.GetComponent<Item>();
            var allUsedUp = itemScript.UseItem(this, roomToPlace);

            if (allUsedUp)
            {
                //Sfx
                if (item.name == GetPrefabName(PF_Plank))
                    sfx_player.PlayOneShot(sfx_hammer);

                if (item.name == GetPrefabName(PF_Plank))
                    UI.OnRemoveChild();
            }
        }
        // Silence the errors. Shhhh
        catch (ArgumentOutOfRangeException)
        {
            // If you're trying to use an item but you have no items
            return;
        }
        catch (KeyNotFoundException)
        {
            // If you're trying to place an item outside the map
            return;
        }
    }

    public void OnMouseClick(Vector3 mouse)
    {
        Vector3 world = Camera.ScreenToWorldPoint(mouse);
        int roomX = Convert.ToInt32(world.x / 2) * 2;
        int roomY = Convert.ToInt32(world.y / 2) * 2;
        Vector3 roomPosition = new Vector3(roomX, roomY, 0);
                
        try
        {
            GameObject roomSelected = Rooms[roomPosition];

            if (influencePart == 0)
            {
                if (roomSelected.transform.Find(GetPrefabName(PF_Gas)) != null)
                {
                    influencePart = 1;

                    if (selection == null)
                        selection = Instantiate(PF_Selection);

                    selection.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                    selection.transform.position = roomPosition;
                    selectedToInfluence = roomPosition;
                }
            }
            else if (influencePart == 1)
            {
                float distance = Vector3.Distance(roomPosition, selectedToInfluence);

                var previousRoom = Rooms[selectedToInfluence];

                if (distance < 2.5f && roomSelected != previousRoom)
                {
                    if (roomSelected.transform.Find(GetPrefabName(PF_Gas)) != null)
                    {
                        // It already has gas, don't do anything
                    }
                    else if (roomSelected.transform.Find(GetPrefabName(PF_Fire)) != null)
                    {
                        Explode(roomSelected);

                        // Remove the fire
                        Destroy(roomSelected.transform.Find(GetPrefabName(PF_Fire)).gameObject);
                        // Then put a hole there
                        CreateElementInRoom(PF_Hole, roomSelected);
                    }
                    else
                    {
                        CreateElementInRoom(PF_Gas, roomSelected);
                    }

                    // In the end, destroy the original gas
                    Destroy(previousRoom.transform.Find(GetPrefabName(PF_Gas)).gameObject);
                }

                influencePart = 0;
                selection.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
                SpreadElements();
                CheckHealth();
            }
        }
        catch (KeyNotFoundException)
        {
            return;
        }        
    }

    private void CheckHealth()
    {
        GameObject room = Rooms[player.GetNewPosition()];
        if (room.transform.Find(GetPrefabName(PF_Gas)) != null)
        {
            player.SetHealth(player.GetHealth() - 1);
        }
    }

    public void UpdateHealthBar(int health)
    {
        UI.SetHealthBar(health);
    }

    private void Explode(GameObject obj)
    {
        Instantiate(PF_AnimExplode, obj.transform);
        sfx_player.PlayOneShot(sfx_explode);
    }
}
