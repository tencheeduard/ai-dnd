using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class textScript : MonoBehaviour
{
    public GameObject text;
    public bool inputEnabled;
    public GameObject aiHandlerObject;

    private OpenAIHandler aiHandler;
    private TextMesh textMesh;
    private string input;

    public GameObject mapObj;
    public MapScript mapData;

    // Start is called before the first frame update
    void Start()
    {
        mapData = mapObj.GetComponent<MapScript>();
        input = "";
        textMesh = text.GetComponent<TextMesh>();
        aiHandler = aiHandlerObject.GetComponent<OpenAIHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        if (inputEnabled == true)
        {
            foreach (char c in Input.inputString)
            {
                if (c == '\b')
                {
                    if (input.Length != 0)
                        input = input.Substring(0, input.Length - 1);
                }
                else if (c == '\n' || c=='\r')
                {
                    inputEnabled = false;
                    if (aiHandler.nameSet == true)
                        aiHandler.getResponse(input);
                    else if (mapData.signalDialogue == true)
                    {
                        aiHandler.getDialogueResponse("", "", "", input);
                    }
                    else
                    {
                        mapData.playerStats.charName = input;
                        aiHandler.nameSet = true;
                        aiHandler.generatingText = false;
                    }
                    input = "";
                }
                else
                    if(input.Length <= 100)
                        input += (Input.inputString);
            }
        }
        textMesh.text = input;
    }
}
