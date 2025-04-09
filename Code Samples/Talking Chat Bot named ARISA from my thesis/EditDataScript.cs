using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
public class EditDataScript : MonoBehaviour
{
    public GameObject editDataUI;
    [SerializeField]
    private bool editDataBool;
    [SerializeField]
    private string tempEditText;
   
    //Messaging System Reference
    public MessagingSystem messSystem;

    public int tempIndexNumber;
    
    //Edit Data System
    public TMP_InputField editTextPanel;
    public TextMeshProUGUI editText;

    [SerializeField]
    private bool addingAnswer;
    private bool addingNewAnswer;
    
    void Update()
    {
        
        if (editDataBool == true)
        {
            editDataUI.SetActive(true);
        }
        else {editDataUI.SetActive(false);}

        tempEditText = editText.text;

        if (Input.GetKeyDown(KeyCode.Return))
            {
                tempEditText = editTextPanel.text;
                if (messSystem.chatboxKeywords.Contains(tempEditText))
                {
                //editText.text = "The data being edited is " + editTextPanel.text;
                var answerAlgorithm = from keywords in messSystem.chatboxKeywords
                                          where keywords.Contains(tempEditText)
                                          select keywords;


                    foreach (var possibleAnswer in answerAlgorithm)
                    {

                        var keywordIndex = messSystem.chatboxKeywords.IndexOf(possibleAnswer);
                        editText.text = "What will you change " + messSystem.chatboxKeywords[keywordIndex] + " to?";    
                        Invoke("InvokeTempTrue", 1f);
                        tempIndexNumber = keywordIndex;
                        Debug.Log(keywordIndex);
                        // StartCoroutine(AnswerDelay(keywordIndex, 1f));
                        //SendMessageToChat(chatboxAnswers[keywordIndex], Message.MessageType.info);


                        //arisaScript.arisaAnswer(keywordIndex, arisaScript.answers[keywordIndex]);
                    }
                }

            else
            {
                editText.text = tempEditText + " is not present in my database";
            }

             
        }

        if (Input.GetKeyDown(KeyCode.Return) && addingAnswer == true)
        {
         
            messSystem.chatboxKeywords[tempIndexNumber] = tempEditText;
            Invoke("InvokeTemp", 1f);
            //addingNewAnswer = true;
           

        }

        if(Input.GetKeyDown(KeyCode.Return) && addingNewAnswer == true)
        {
            messSystem.chatboxAnswers[tempIndexNumber] = tempEditText;
            Invoke("AllFalse", 1f);
            Invoke("newAnswerAndKeyword", 1f);
            Invoke("InvokeEditData", 5f);
        }
       
    
    }


    public void EditDataToggle()
    {
        editDataBool = !editDataBool;
    }

    void InvokeTemp()
    {
        editText.text = "What will be the new answer?";
        addingAnswer = false;
        addingNewAnswer = true;
    }

    void InvokeTempTrue()
    {
        addingAnswer = true;
    }
    
    void InvokeEditData()
    {
        editText.text = "Edit Data";
    }

    void AllFalse()
    {
        addingNewAnswer = false;
        addingAnswer = false;
    }

    void newAnswerAndKeyword()
    {
        editText.text = "Your new keyword will be " + messSystem.chatboxKeywords[tempIndexNumber] + " and the answer will be " + messSystem.chatboxAnswers[tempIndexNumber];
    }

}

