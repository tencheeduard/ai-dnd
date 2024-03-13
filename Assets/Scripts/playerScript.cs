using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerScript : genericCharacterScript
{
    private int xp;
    private int xpNeeded;
    // Start is called before the first frame update
    void Start()
    {
        init();
        charName = "Player";
        level = 1;
        xp = 0;
        xpNeeded = 100;
        health = 100;
        ad = 25;
        armor = 0.05f;
        initiative = 5;
        speed = 2;
    }

    // Update is called once per frame
    void Update()
    {
        if (health < 0f)
        {
            Die();
        }
        if (xp == xpNeeded)
            levelUp();
    }
}
