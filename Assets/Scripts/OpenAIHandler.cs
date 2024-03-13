using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;

public class OpenAIHandler : MonoBehaviour
{

    public bool generatingText;

    public bool nameSet;

    public bool initiated = false;

    public GameObject narratorTextBox;

    private narratorTextScript narratorText;

    public GameObject mapObj;
    public MapScript mapData;

    public int inputs = 0;

    public string dialogueName;
    public string dialogueDesc;
    public string dialogueBack;

    bool storyEvent0 = false;
    bool storyEvent1 = false;

    List<Message> systemMessages = new List<Message> {
        new Message(Role.System, "You are the narrator for a tabletop roleplaying game set in the medieval fantasy world of Carania. Describe the events happening to the player, and the results of their actions, in about 2 or three sentences. Do not make the player commit actions that they have not themselves described. All characters outside of the player are non-player characters, and as such the player may choose to act however they desire upon them. Now welcome the player, and ask him to create a character!"),
    };

    List<Message> localMemory = new List<Message> { };

    List<Message> shortMemory = new List<Message> { };
    List<Message> combatMemory = new List<Message> { };

    // Start is called before the first frame update
    void Start()
    {
        mapData = mapObj.GetComponent<MapScript>();
        narratorText = narratorTextBox.GetComponent<narratorTextScript>();
        setFirstText();
    }

    // Update is called once per frame
    void Update()
    {
        if(nameSet && !initiated)
        {
            systemMessages.Add(new Message(Role.User, $"My character's name is {mapData.playerStats.charName}"));
            localMemory.Add(new Message(Role.Assistant, $"You are enjoying a cozy night at a tavern. There are 2 other people, both very obviously drunk, sitting at the other tables. Inform the player of all these facts. Do not describe the other characters."));
            systemMessages.Add(new Message(Role.System, "Do not create new characters unless instructed by the system."));
            getResponse();
            initiated = true;
        }
        if(inputs==2 && !storyEvent0)
        {
            storyEvent0 = true;
            localMemory.Add(new Message(Role.System, $"You hear horses, rustling armor and aggressive footsteps outside."));
        }
        if(inputs==4 && !storyEvent1)
        {
            storyEvent1 = true;
            localMemory.Add(new Message(Role.System, $"You see 3 armored men walking into the tavern. They seem up to no good. Immediately they start attacking the drunken patrons."));
            mapData.PlaceObject("GenericEnemy", 2, 6);
            mapData.PlaceObject("GenericEnemy", 3, 6);
            mapData.PlaceObject("GenericEnemy", 4, 6);
            mapData.combatHandler.checkEnemiesOnMap();
        }
    }

    public async void getResponse(string text = "")
    {
        generatingText = true;
        var api = new OpenAIClient();
        List<Message> currentInput = new List<Message> { };
        if (text.Length != 0)
            shortMemory.Add(new Message(Role.User, text));
        foreach (Message message in systemMessages)
        {
            currentInput.Add(message);
            Debug.Log(message.Content);
        }
        foreach (Message message in shortMemory)
        {
            currentInput.Add(message);
            Debug.Log(message.Content);
        }
        foreach (Message message in localMemory)
        {
            currentInput.Add(message);
            Debug.Log(message.Content);
        }

        var chatRequest = new ChatRequest(currentInput, Model.GPT4, null, null, null, null, 256);
        var result = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
        shortMemory.Add(new Message(Role.Assistant, result.FirstChoice.Message.Content));
        Debug.Log($"{result.FirstChoice.Message.Role}: {result.FirstChoice.Message.Content}");
        narratorText.changeText(result.FirstChoice.Message.Content);
        inputs++;
        Invoke("setGeneratingText", 4f);

    }

    public async void getDialogueResponse(string name = "", string desc = "", string background = "", string text = "")
    {
        string actualName = name;
        string actualDesc = desc;
        string actualBack = background;
        if (name == "")
            actualName = dialogueName;
        if (desc == "")
            actualDesc = dialogueDesc;
        if (background == "")
            actualBack = dialogueBack;
        generatingText = true;
        var api = new OpenAIClient();
        List<Message> currentInput = new List<Message> { };
        foreach (Message message in systemMessages)
        {
            currentInput.Add(message);
            Debug.Log(message.Content);
        }
        foreach (Message message in shortMemory)
        {
            currentInput.Add(message);
            Debug.Log(message.Content);
        }
        currentInput.Add(new Message(Role.System, $"The player greets {actualName}. He does not know anything about them, not even their name."));
        currentInput.Add(new Message(Role.System, $"{actualName}'s physical appearance: {actualDesc}. This information is immediately visible to the player."));
        currentInput.Add(new Message(Role.System, $"{actualName}'s background: {actualBack}. This information can't be revealed to the player unless {actualName} does."));
        if (text.Length != 0)
            shortMemory.Add(new Message(Role.User, text));

        currentInput.Add(new Message(Role.System, $"Do not tell the player background information about the character without them revealing it. The player only knows their appearance and what they have learned from previous dialogue"));

        var chatRequest = new ChatRequest(currentInput, Model.GPT4, null, null, null, null, 256);
        var result = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
        shortMemory.Add(new Message(Role.Assistant, result.FirstChoice.Message.Content));
        Debug.Log($"{result.FirstChoice.Message.Role}: {result.FirstChoice.Message.Content}");
        narratorText.changeText(result.FirstChoice.Message.Content);
        inputs++;
        Invoke("setGeneratingText", 4f);

    }

    public async void sendCombatActions(List<Message> list)
    {
        generatingText = true;
        var api = new OpenAIClient();
        List<Message> currentInput = new List<Message> { };


        foreach (Message message in systemMessages)
        {
            currentInput.Add(message);
            Debug.Log(message.Content);
        }

        if (shortMemory.Count < 10)
        {
            for (int i = shortMemory.Count - shortMemory.Count / 4; i < shortMemory.Count; i++)
            {
                currentInput.Add(shortMemory[i]);
                Debug.Log(shortMemory[i].Content);
            }
        }
        else
            foreach(Message message in shortMemory)
            {
                currentInput.Add(message);
                Debug.Log(message.Content);
            }

        foreach (Message message in combatMemory)
        {
            currentInput.Add(message);
            Debug.Log(message.Content);
        }

        foreach(Message message in list)
        {
            currentInput.Add(message);
            Debug.Log(message.Content);
        }

        var chatRequest = new ChatRequest(currentInput, Model.GPT4, null, null, null, null, 64);
        var result = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
        combatMemory.Add(new Message(Role.Assistant, result.FirstChoice.Message.Content));
        Debug.Log($"{result.FirstChoice.Message.Role}: {result.FirstChoice.Message.Content}");
        narratorText.changeText(result.FirstChoice.Message.Content);
        Invoke("setGeneratingText", 4f);
        inputs++;
    }

    void setGeneratingText()
    {
        generatingText = false;
    }

    void setFirstText()
    {
        narratorText.changeText("What is your name?");
        generatingText = true;
    }
}
