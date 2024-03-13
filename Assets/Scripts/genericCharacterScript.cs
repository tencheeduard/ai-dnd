using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class genericCharacterScript : mapObjectScript
{
    public string charName;
    public string description;
    public string background;

    public float health;
    public float ad;
    public float armor;
    public int level;

    public bool inDialogue;


    public int initiative;

    public int speed;

    public GameObject combatHandlerObj;
    [HideInInspector] public combatHandlerScript combatHandler;

    public GameObject openAIHandlerObj;
    public OpenAIHandler openAIHandler;

    public bool signalUsedTurn;

    // Start is called before the first frame update
    void Start()
    {
        init();
        signalUsedTurn = false;
        inDialogue = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(health<0f)
        {
            Die();
        }
    }

    public void init()
    {
        openAIHandlerObj = GameObject.Find("OpenAIHandler");
        openAIHandler = openAIHandlerObj.GetComponent<OpenAIHandler>();

        combatHandlerObj = GameObject.Find("combatHandler");
        combatHandler = combatHandlerObj.GetComponent<combatHandlerScript>();
    }

    

    public void addHealth(float added)
    {
        health += added;
    }
    public void addAD(float added)
    {
        ad += added;
    }
    public void addArmor(float added)
    {
        armor += added;
    }
    public void levelUp()
    {
        addHealth(0.05f*health);
        addAD(0.05f * ad);
    }

    public void Die()
    {
        Destroy(gameObject);
        combatHandler.mapData.objects[MapPosI, MapPosJ] = null;
        combatHandler.checkEnemiesOnMap();
        Debug.Log($"{charName} has died.");

        string finalAction = $"{charName} has died.";
        combatHandler.addToActions(finalAction);
    }


    // turns

    public void damageOther(GameObject other)
    {
        genericCharacterScript stats = other.GetComponent<genericCharacterScript>();
        damageOther(stats);
    }

    public void damageOther(genericCharacterScript other)
    {
        if (Mathf.Abs(MapPosI - other.MapPosI) > 1 && Mathf.Abs(MapPosJ - other.MapPosJ) > 1)
            Debug.Log("Couldn't attack, enemy too far away");
        else
        {
            float originalHealth = other.health;
            other.health -= ad - other.armor * ad;
            Debug.Log($"{charName} hit {other.charName} for {ad - other.armor * ad} Damage!, their health is now {other.health}");
            signalUsedTurn = true;

            float percentChange = 1 - other.health / originalHealth;

            string damageAmount;
            if (percentChange < 0.1f)
                damageAmount = "very low";
            else if (percentChange < 0.2f)
                damageAmount = "low";
            else if (percentChange < 0.4f)
                damageAmount = "significant";
            else if (percentChange < 0.80f)
                damageAmount = "critical";
            else if (percentChange < 0.99f)
                damageAmount = "incredible";
            else
                damageAmount = "lethal";

            string finalAction = $"{charName} hits {other.charName}, dealing {damageAmount} damage!";
            combatHandler.addToActions(finalAction);
        }
    }

    public bool move(int i, int j)
    {
        if (combatHandler.mapData.isValidMove(MapPosI, MapPosJ, i, j))
        {
            combatHandler.mapData.MoveObjectLerp(this.gameObject, i, j);
            signalUsedTurn = true;
            return true;
        }
        return false;
    }

    public void wait()
    {
        signalUsedTurn = true;
    }

    public void startDialogue(int i, int j)
    {
        if(combatHandler.mapData.objects[i, j] != null && combatHandler.mapData.objects[i,j].GetComponent<genericCharacterScript>() && !combatHandler.mapData.objects[i, j].GetComponent<genericEnemyScript>() && !combatHandler.mapData.objects[i, j].GetComponent<playerScript>())
        {
            combatHandler.mapData.signalDialogue = true;
            genericCharacterScript other = combatHandler.mapData.objects[i, j].GetComponent<genericCharacterScript>();
            genericAIScript otherAI = other.gameObject.GetComponent<genericAIScript>();
            otherAI.freeze = true;
            openAIHandler.getDialogueResponse(other.charName, other.description, other.background);
        }
    }


    // Generic Interaction, takes whatever is valid

    public void interactWith(int i, int j)
    {
        if (combatHandler.mapData.objects[i,j] != null && combatHandler.mapData.objects[i, j].GetComponent<genericEnemyScript>())
            damageOther(combatHandler.mapData.objects[i, j]);
        else if (combatHandler.mapData.objects[i, j] != null && combatHandler.mapData.objects[i, j].GetComponent<genericCharacterScript>() && !combatHandler.mapData.objects[i, j].GetComponent<playerScript>())
        {
            startDialogue(i, j);
        }
        else if (move(i, j)) { combatHandler.mapData.signalDialogue = false; }
        else
            Debug.Log("Couldn't find Interaction with tile");
    }
}
