using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New NPC Dialogue", menuName = "Dialogue System/NPC Dialogue")]
public class NPCDialogue : DialogueScriptable
{
    public NPCDialogue()
    {
        dialogueType = DialogueType.NPC;
    }
}
