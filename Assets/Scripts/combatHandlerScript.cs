using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;

public class combatHandlerScript : MonoBehaviour
{

    public bool inCombat;
    public genericCharacterScript[] combatInvolved;
    public int[] turns;
    public int turnsMade;
    public int currentTurn;

    public bool checkingTurn;

    public bool signalPlayerTurn;

    public GameObject InputText;

    public GameObject Map;
    public MapScript mapData;
    public playerScript player;

    public GameObject openAIHandlerObj;
    public OpenAIHandler openAIHandler;

    public List<Message> actionList;

    // Start is called before the first frame update
    void Start()
    {
        openAIHandlerObj = GameObject.Find("OpenAIHandler");
        openAIHandler = openAIHandlerObj.GetComponent<OpenAIHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        if(mapData!=null)
            checkEnemiesOnMap();
        if (inCombat)
        {
            combatInvolved = getAllParticipants();
            checkTurn();
        }
        else
            signalPlayerTurn = true;
        if(mapData==null)
            mapData = Map.GetComponent<MapScript>();
        if (player == null)
            player = mapData.playerStats;
        InputText.SetActive(!inCombat);
    }

    public void addToActions(string text)
    {
        actionList.Add(new Message(Role.System, text));
    }

    public void sendList()
    {
        Debug.Log($"{actionList.Count} actions in list");
        if (actionList.Count > 0)
        {
            actionList.Add(new Message(Role.System, $"Please summarize these last {actionList.Count} actions to the player in a few sentences."));
            openAIHandler.sendCombatActions(actionList);
            actionList.Clear();
        }
    }
    public bool checkEnemiesOnMap()
    {
        for(int i = 0; i < mapData.gridLength; i++)
        {
            for (int j = 0; j < mapData.gridHeight; j++)
            {
                if (mapData.objects[i, j] != null && mapData.objects[i, j].GetComponent<genericEnemyScript>())
                {
                    if(!inCombat)
                        startCombat();
                    return true;
                }
            }
        }
        if (inCombat)
        {
            actionList.Add(new Message(Role.System, $"All enemy combatants have been defeated."));
            sendList();
            inCombat = false;
        }
        return false;
    }

    public genericCharacterScript[] getAllParticipants()
    {
        genericCharacterScript[] temp = new genericCharacterScript[mapData.gridLength * mapData.gridHeight];
        int iterator = 0;
        foreach(GameObject obj in mapData.objects)
        {
            if(obj != null && obj.GetComponent<genericCharacterScript>())
            {
                temp[iterator++] = obj.GetComponent<genericCharacterScript>();
            }
        }
        genericCharacterScript[] result = new genericCharacterScript[iterator];
        for(int i = 0; i < iterator; i++)
        {
            result[i] = temp[i];
        }

        bool swapped;
        for(int i = 0; i < result.Length-1; i++)
        {
            swapped = false;
            for (int j = 0; j < result.Length-1; j++)
            {
                if (result[j].initiative < result[j+1].initiative)
                {
                    genericCharacterScript temp1 = result[j];
                    result[j] = result[j + 1];
                    result[j + 1] = temp1;
                    swapped = true;
                }
            }
            if (swapped == false)
                break;
        }
        return result;
    }

    public void startCombat()
    {
        inCombat = true;
        combatInvolved = getAllParticipants();
        currentTurn = 0;
        turnsMade = combatInvolved[currentTurn].speed;
        checkTurn();
    }

    public void checkTurn()
    {
        if (currentTurn >= combatInvolved.Length)
            currentTurn = 0;
        if (combatInvolved[currentTurn].signalUsedTurn == true)
        {
            turnsMade--;
            combatInvolved[currentTurn].signalUsedTurn = false;
        }
        if (turnsMade <= 0)
        {
            combatInvolved = getAllParticipants();
            if (combatInvolved[currentTurn] != player)
                combatInvolved[currentTurn].gameObject.GetComponent<genericAIScript>().signalTurn = false;
            currentTurn++;
            sendList();
            nextTurn();
            checkingTurn = false;
        }
        if (combatInvolved[currentTurn] == player)
        {
            signalPlayerTurn = true;
            checkingTurn = false;
        }
        else
        {
            combatInvolved[currentTurn].gameObject.GetComponent<genericAIScript>().signalTurn = true;
            signalPlayerTurn = false;
            checkingTurn = false;
        }
    }

    public void nextTurn()
    {
        if (currentTurn >= combatInvolved.Length)
        {
            currentTurn = 0;
            loopEffects();
        }
        turnsMade = combatInvolved[currentTurn].speed;
        turnEffects();
    }

    public void turnEffects()
    {
        //effects happening per turn...
    }

    public void loopEffects()
    {
        //bleeding, poison, other effects...
    }
}
