using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class genericAIScript : MonoBehaviour
{

    public bool combatMode;

    public genericCharacterScript character;
    public combatHandlerScript combatHandler;

    public bool signalTurn;

    public bool hostile;

    public bool freeze;

    public GameObject openAIHandlerObj;
    public OpenAIHandler openAIHandler;

    // Start is called before the first frame update
    void Start()
    {
        openAIHandlerObj = GameObject.Find("OpenAIHandler");
        openAIHandler = openAIHandlerObj.GetComponent<OpenAIHandler>();
        character = gameObject.GetComponent<genericCharacterScript>();
        InvokeRepeating("makeDecision", 2f, 2f);
    }


    void Update()
    {
        if(combatHandler==null)
            combatHandler = character.combatHandler;
        if (combatHandler.inCombat == true)
            combatMode = true;
        if (combatMode == false)
            signalTurn = true;
    }
    // Decision Making
    
    public bool makeDecision()
    {
        if (combatMode && signalTurn && combatHandler.mapData.selectedMoving == false && openAIHandler.generatingText == false)   
        {
            Invoke("wait", 2f);
            int[,] sorrounding = sorroundingTiles();
            combatMode = combatHandler.inCombat;
            if (combatMode)
            {
                for (int i = 0; i < sorrounding.Length / 2; i++)
                {
                    if ((hostile && sorrounding[i, 0] < combatHandler.mapData.gridLength) && (sorrounding[i, 1] < combatHandler.mapData.gridHeight && (combatHandler.mapData.objects[sorrounding[i, 0], sorrounding[i, 1]] != null &&
                        combatHandler.mapData.objects[sorrounding[i, 0], sorrounding[i, 1]].GetComponent<playerScript>() ) || (combatHandler.mapData.objects[sorrounding[i, 0], sorrounding[i, 1]] != null && combatHandler.mapData.objects[sorrounding[i, 0], sorrounding[i, 1]].GetComponent<genericCharacterScript>() && !combatHandler.mapData.objects[sorrounding[i, 0], sorrounding[i, 1]].GetComponent<genericEnemyScript>())))
                    {
                        character.damageOther(combatHandler.mapData.objects[sorrounding[i, 0], sorrounding[i, 1]]);
                        return true;
                    }
                    else if (!hostile && (combatHandler.mapData.objects[sorrounding[i, 0], sorrounding[i, 1]] != null && combatHandler.mapData.objects[sorrounding[i, 0], sorrounding[i, 1]].GetComponent<genericEnemyScript>()))
                    {
                        character.damageOther(combatHandler.mapData.objects[sorrounding[i, 0], sorrounding[i, 1]]);
                        return true;
                    }
                }

                float min = 0;
                int minPos = -1;
                for(int i = 0; i < sorrounding.Length/2; i++)
                {
                    float dist = Vector3.Distance(combatHandler.mapData.calculatePos(sorrounding[i, 0], sorrounding[i, 1]), combatHandler.mapData.player.transform.position);
                    if (dist < min || min==0 )
                    {
                        min = dist;
                        minPos = i;
                    }
                }
                if (minPos != -1)
                {
                    if (character.move(sorrounding[minPos, 0], sorrounding[minPos, 1])) {
                        return true; 
                    }
                }
                int random = Random.Range(0, 4);
                switch (random){
                    case 1:
                        int cap = 0;
                        while(cap < 100)
                        {
                            Debug.Log(cap);
                            cap++;
                            int randPos = Random.Range(0, sorrounding.Length / 2);
                            if (character.move(sorrounding[randPos, 0], sorrounding[randPos, 1]) && combatHandler.mapData.objects[sorrounding[randPos,0], sorrounding[randPos,1]] == null)
                            {
                                return true;
                            }
                        }
                        character.wait();
                        return true;
                    default:
                        character.wait();
                        return true;
                }
            }
            if (combatMode == false && combatHandler.mapData.selectedMoving == false)
            {
                int random = Random.Range(0, 2);
                switch (random)
                {
                    case 0:
                        character.wait();
                        return true;
                    case 1:
                        int cap = 0;
                        while (cap < 100)
                        {
                            Debug.Log(cap);
                            cap++;
                            int randPos = Random.Range(0, sorrounding.Length / 2);
                            if (character.move(sorrounding[randPos, 0], sorrounding[randPos, 1]))
                            {
                                return true;
                            }
                        }
                        character.wait();
                        return true;
                    case 2:
                        Debug.Log("what");
                        break;
                }
            }
            return true;
        }
        return false;
    }

    public int[,] sorroundingTiles()
    {
        int[,] result = new int[8,2];
        result[0, 0] = character.MapPosI - 1; result[0, 1] = character.MapPosJ - 1;
        result[1, 0] = character.MapPosI - 1; result[1, 1] = character.MapPosJ;
        result[2, 0] = character.MapPosI - 1; result[2, 1] = character.MapPosJ + 1;
        result[3, 0] = character.MapPosI; result[3, 1] = character.MapPosJ - 1;
        result[4, 0] = character.MapPosI; result[4, 1] = character.MapPosJ + 1;
        result[5, 0] = character.MapPosI + 1; result[5, 1] = character.MapPosJ - 1;
        result[6, 0] = character.MapPosI + 1; result[6, 1] = character.MapPosJ;
        result[7, 0] = character.MapPosI + 1; result[7, 1] = character.MapPosJ + 1;
        int[,] actualResult;
        int counter = 0;
        for(int i = 0; i < 8; i++)
        {
            if (result[i, 0] < combatHandler.mapData.gridLength && result[i,1] < combatHandler.mapData.gridHeight)
            {
                counter++;
            }
        }
        actualResult = new int[counter, 2];
        counter = 0;
        for (int i = 0; i < 8; i++)
        {
            if (result[i, 0] < combatHandler.mapData.gridLength && result[i, 1] < combatHandler.mapData.gridHeight)
            {
                actualResult[counter, 0] = result[i, 0];
                actualResult[counter++, 1] = result[i, 1];
            }
        }
        return actualResult;
    }

    // Combat Decisions

    public void wait()
    {
        character.wait();
    }

}
