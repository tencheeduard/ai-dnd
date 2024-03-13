using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickDetector_Script : MonoBehaviour
{

    //Detects Mouse clicks in world and handles them

    public Camera mainCamera;
    private Ray ray;
    private RaycastHit hit;
    public GameObject textBox;


    private textScript textBoxScript;

    // Start is called before the first frame update
    void Start()
    {
        textBoxScript = textBox.GetComponent<textScript>();
    }

    // Update is called once per frame
    void Update()
    {
        //Detect mouseclick
        if (Input.GetButtonDown("Fire1"))
        {
            ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100))
            {
                Debug.DrawRay(mainCamera.transform.position, ray.direction * 100, Color.white, 1);
                //Debug.Log(hit.point);
                if (hit.transform.name == "Map")
                {
                    hit.collider.GetComponentInParent<MapScript>().handleClick(hit.point);
                }
                if (hit.transform.name == "InputText")
                {
                    if (textBoxScript.inputEnabled == false)
                        textBoxScript.inputEnabled = true;
                }
                else
                    textBoxScript.inputEnabled = false;
            }
            // Map clicked
        }
    }
}
