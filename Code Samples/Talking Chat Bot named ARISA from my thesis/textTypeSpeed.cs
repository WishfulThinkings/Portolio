using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class textTypeSpeed : MonoBehaviour
{
    public TextMeshProUGUI subtitleText;
    public float displayTextDuration;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(SentenceTyper(subtitleText.ToString()));
    }

    IEnumerator idleSentence(string idleSentence)
    {
        subtitleText.text = " ";
        foreach (char letter in idleSentence.ToCharArray())
        {
            //subtitleText.text = "...";
            subtitleText.text += letter;
            yield return new WaitForSeconds(3f);



        }
    }
    IEnumerator SentenceTyper(string arisaTextReply)
    {
        subtitleText.text = "";
        StopCoroutine("idleSentence");
        foreach (char letter in arisaTextReply.ToCharArray())
        {
            subtitleText.text += letter;
            yield return new WaitForSeconds(displayTextDuration);
        }
    }

    public void TypeSentences(string sentence)
    {
        subtitleText.text = "";
        StartCoroutine(SentenceTyper(sentence));
    }
}
