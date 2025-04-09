using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class uiScript : MonoBehaviour
{
    //GameObject arisaSrci;
    arisaScript aS = new arisaScript();

    
    public Image defaultImage;
    public Sprite redIcon;
    public Sprite greenIcon;

    void Awake()
    {
        aS  = GetComponent<arisaScript>();
    }


    public void OnClick()
    {
        arisaScript.isSpeaking = true;
    
    }
           
}
