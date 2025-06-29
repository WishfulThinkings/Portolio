using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueScriptable : ScriptableObject
{
    public DialogueType dialogueType;
    public List<Dialogue>dialogues;
    public enum DialogueType
    {
        NPC,
        Story
    }
}


[System.Serializable]
public class Dialogue
{
    public Sprite image;
    public string name;
    public string dialogue;
    public AudioClip voice;
}
