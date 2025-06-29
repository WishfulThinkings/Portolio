using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeoDialogue : MonoBehaviour
{
    public DialogueScriptable takeoChangeName;
    public UserInformationSO userInfo;
    void Start()
    {
        takeoChangeName.dialogues[0].dialogue = "Hello " + userInfo.playerName.ToString() + "! You can go anywhere you like. Just enjoy the Convention Hall";
    }


}
