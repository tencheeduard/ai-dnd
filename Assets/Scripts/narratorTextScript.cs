using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class narratorTextScript : MonoBehaviour
{

    public string text;
    public GameObject textObject;

    private string currentText;
    private int showChars;
    private TextMeshPro textMesh;

    // Start is called before the first frame update
    void Start()
    {
        showChars = 0;
        textMesh = textObject.GetComponent<TextMeshPro>();
        changeText(text);
        InvokeRepeating("incChars", 0f, 0.03f);
    }

    // Update is called once per frame
    void Update()
    {
        if (showChars <= text.Length)
        {
            currentText = "";
            for (int i = 0; i < showChars; i++)
                currentText += text[i];
        }
        textMesh.text = currentText;
    }


    public void changeText(string text)
    {
        showChars = 0;
        currentText = "";
        this.text = text;
    }

    private void incChars()
    {
        if(showChars < text.Length)
            showChars++;
    }

    
}
