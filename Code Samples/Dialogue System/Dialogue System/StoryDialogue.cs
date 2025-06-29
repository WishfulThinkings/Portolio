using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Story Dialogue", menuName = "Dialogue System/Story Dialogue")]
public class StoryDialogue : DialogueScriptable
{
    public StoryDialogue()
    {
        dialogueType = DialogueType.Story;
    }
}
