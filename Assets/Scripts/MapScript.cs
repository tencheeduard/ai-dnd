using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapScript : MonoBehaviour
{
    public int gridLength;
    public int gridHeight;
    public GameObject map;

    public GameObject player;
    public playerScript playerStats;
    [HideInInspector] public mapObjectScript playerMapObject;

    [HideInInspector] public Renderer mapRenderer;
    [HideInInspector] public Vector3 size;

    public bool signalDialogue = false;
    
    public GameObject[,] objects;

    [HideInInspector] public GameObject selectedObject = null;
    [HideInInspector] public bool selectedMoving;

    [HideInInspector] public Vector3 LerpInitialPos = Vector3.zero;
    [HideInInspector] public Vector3 LerpFinalPos = Vector3.zero;

    [HideInInspector] public int LerpTimeElapsed;
    [HideInInspector] public int LerpTime;

    public GameObject combatHandlerObj;
    [HideInInspector] public combatHandlerScript combatHandler;

    // Start is called before the first frame update
    void Start()
    {
        LerpTimeElapsed = 0;
        objects = new GameObject[gridLength, gridHeight];
        mapRenderer = map.GetComponent<Renderer>();
        size = mapRenderer.bounds.size;

        combatHandler = combatHandlerObj.GetComponent<combatHandlerScript>();

        player = PlaceObject("Figurine", 2, 1);
        player.transform.SetParent(transform);

        playerStats = player.GetComponent<playerScript>();
        playerMapObject = player.GetComponent<mapObjectScript>();

        for(int i = 0; i < gridLength; i++)
        {
            PlaceObject("Wall", 0, i);
            PlaceObject("Wall", i, 0);
            PlaceObject("Wall", 7, i);
            if (i != 3)
                PlaceObject("Wall", i, 7);
            else
                PlaceObject("Door", i, 7);
        }
        PlaceObject("Table", 2, 2);
        PlaceObject("Table", 4, 2);
        PlaceObject("Table", 2, 4);
        PlaceObject("Table", 4, 4);

        player = PlaceObject("GenericFriendly", 2, 3);
        objects[2, 3].GetComponent<genericCharacterScript>().charName = "Jared";
        objects[2, 3].GetComponent<genericCharacterScript>().description = "Dark hair, brown eyes and wearing shining armor. He is visibly intoxicated.";
        objects[2, 3].GetComponent<genericCharacterScript>().description = "Jared is a knight of the Holy Order of the Three Faces";

        player = PlaceObject("GenericFriendly", 4, 3);
        objects[4, 3].GetComponent<genericCharacterScript>().charName = "Xavier";
        objects[4, 3].GetComponent<genericCharacterScript>().description = "Blonde hair, blue eyes and wearing peasants' clothing. He is visibly intoxicated.";
        objects[4, 3].GetComponent<genericCharacterScript>().description = "Xavier is a poor peasant from the farming region of Antacania";
        PlaceObject("InvisibleWall", 2, 6);
        PlaceObject("InvisibleWall", 3, 6);
        PlaceObject("InvisibleWall", 4, 6);
    }

    // Update is called once per frame
    void Update()
    {

        //Handle Lerp
        if (selectedObject != null && selectedMoving == true)
        {
            LerpTimeElapsed++;
            selectedObject.transform.position = Vector3.Lerp(LerpInitialPos, LerpFinalPos, (float)LerpTimeElapsed / LerpTime);
        }
        else if (selectedMoving == false)
        {
            LerpInitialPos = Vector3.zero;
            LerpFinalPos = Vector3.zero;
        }
        if (LerpTimeElapsed == LerpTime)
        {
            selectedMoving = false;
        }
    }

    public void MoveObject(GameObject obj, int i, int j)
    {
        obj.transform.position = new Vector3(0f, obj.GetComponent<BoxCollider>().size.y / 2 + transform.position.y, 0f) + calculatePos(i, j);
    }

    public void MoveObjectLerp(GameObject obj, int i, int j)
    {
        selectedObject = obj;
        mapObjectScript MOS = obj.GetComponent<mapObjectScript>();
        objects[MOS.MapPosI, MOS.MapPosJ] = null;
        objects[i, j] = obj;
        MOS.MapPosI = i;
        MOS.MapPosJ = j;
        selectedMoving = true;
        LerpTimeElapsed = 0;
        LerpInitialPos = obj.transform.position;
        LerpFinalPos = new Vector3(0f, selectedObject.GetComponent<BoxCollider>().size.y / 2 + transform.position.y, 0f) + calculatePos(i, j);
    }

    public GameObject PlaceObject(string objname, int i, int j)
    {
        GameObject placedObject;
        placedObject = PlaceObject(Resources.Load($"Prefabs/{objname}") as GameObject, i, j);
        return placedObject;
    }

    public GameObject PlaceObject(GameObject obj, int i, int j)
    {
        GameObject placedObject;
        placedObject = Instantiate(obj, new Vector3(0f, obj.GetComponent<BoxCollider>().size.y / 2 + transform.position.y, 0f) + calculatePos(i, j), Quaternion.identity);
        mapObjectScript MOS = placedObject.GetComponent<mapObjectScript>();
        MOS.MapPosI = i;
        MOS.MapPosJ = j;
        placedObject.transform.SetParent(transform);
        objects[i, j] = placedObject;
        return placedObject;
    }

    public Vector3 calculatePos(int i, int j)
    {
        return new Vector3(transform.position.x + size.x / gridLength * i + size.x / gridLength / 2, 0f, transform.position.z - size.z / gridHeight * j - size.z / gridHeight / 2);
    }

    public int[] calcWorldToMapPos(Vector3 worldPos)
    {
        int[] mapPos = new int[2];

        float currentCompare = size.x/-2;
        for (mapPos[0] = 0; mapPos[0] < gridLength; mapPos[0]++)
        {
            if (worldPos.x > currentCompare && worldPos.x < currentCompare + size.x / gridLength)
                break;
            else
                currentCompare += size.x / gridLength;
        }
        currentCompare = size.z/2;
        
        for (mapPos[1] = 0; mapPos[1] < gridHeight; mapPos[1]++)
        {
            if (worldPos.z < currentCompare && worldPos.z > currentCompare - size.z / gridHeight)
                break;
            else
                currentCompare -= size.z / gridHeight;
        }
        return mapPos;
    }

    public bool isValidTile(int i, int j)
    {
        return (objects[i, j] == null || (objects[i, j] != null && objects[i, j].GetComponent<mapObjectScript>().obstacle == false));
    }

    public bool isValidMove(int fromI, int fromJ, int toI, int toJ)
    {
        return (isValidTile(toI, toJ) && (fromI!=toI || fromJ!=toJ) && (Mathf.Abs(fromI - toI) <= 1 && Mathf.Abs(fromJ - toJ) <= 1));
    }

    public void handleClick(Vector3 worldPos)
    {
        if (combatHandler.signalPlayerTurn == true)
        {
            int[] coords = calcWorldToMapPos(worldPos);
            int i = coords[0];
            int j = coords[1];
            playerStats.interactWith(i, j);
        }
        else
            Debug.Log("Not Player's turn!");
    }
}
