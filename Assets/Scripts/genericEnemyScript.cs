using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class genericEnemyScript : genericCharacterScript
{
    // Start is called before the first frame update
    void Start()
    {
        init();
    }

    // Update is called once per frame
    void Update()
    {
        if (health < 0f)
        {
            Die();
        }
    }
}
