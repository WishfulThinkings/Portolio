using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class uiScriptRobot : MonoBehaviour
{
    //GameObject arisaSrci;
    arisaScriptRobot aSR = new arisaScriptRobot();

    
    public Image defaultImage;
    public Sprite redIcon;
    public Sprite greenIcon;

    void Awake()
    {
        aSR  = GetComponent<arisaScriptRobot>();
    }


    public void OnClick()
    {
        arisaScriptRobot.isSpeaking = true;
    
    }
           
}
