using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using TextToSpeechApi;
using System;

public class MessagingSystemRobot : MonoBehaviour, ISavable
{
    public GameObject chatPanel, textObject;
    public InputField chatBox;




    //script references
    TextToSpeech textToSpeech = new TextToSpeech();
    public arisaScriptRobot arisaScriptRobot;

    public Color yourMessage;
    public Color info;
    public Color database;
 
    //Audio Clip
    [SerializeField]
    private AudioClip audioClip;
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private bool addingAnswer = false;

    public int maxMessages = 25;

    private string tempChatBox;
    private string previousQuestion;

    //Text
    public TextMeshProUGUI subtitleText;

    //Animation
    public Animator anim;

    public float displayTextDuration;

    public GameObject searchOnlineBtn, addToDatabaseBtn;

    public List<string> chatboxKeywords;
    public List<string> chatboxAnswers;

    [SerializeField]
    List<Message> messageList = new List<Message>();

    // Update is called once per frame
    void Update()
    {
        if (chatBox.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SendMessageToChat(chatBox.text.Trim(), MessageRobot.MessageType.playerMessage);
                tempChatBox = chatBox.text.Trim();
                chatBox.text = "";
                if (tempChatBox.Contains("stop"))
                {
                    subtitleText.text = "";
                    StopAllCoroutines();
                    audioSource.Stop();
                }

                if (chatboxKeywords.Contains(tempChatBox))
                {

                    var answerAlgorithm = from keywords in chatboxKeywords
                                          where keywords.Contains(tempChatBox)
                                          select keywords;


                    foreach (var possibleAnswer in answerAlgorithm)
                    {
                        subtitleText.text = "";
                        var keywordIndex = chatboxKeywords.IndexOf(possibleAnswer);
                        Debug.Log(keywordIndex);
                        StartCoroutine(AnswerDelay(keywordIndex, 1f));
                        //SendMessageToChat(chatboxAnswers[keywordIndex], Message.MessageType.info);


                        //arisaScript.arisaAnswer(keywordIndex, arisaScript.answers[keywordIndex]);
                    }
                }

                else
                {
                    if (addingAnswer == false)
                    {
                        SendMessageToChat("I'm sorry, I do not recognize '" + tempChatBox + "' in my database do you want to search online or give me the answer for future reference?", MessageRobot.MessageType.info);
                        StartCoroutine(SimpleUITransition(2f));
                    }

                }

                if (addingAnswer == true)
                {
                    tempChatBox.Trim();
                    chatboxAnswers.Add(tempChatBox);
                    arisaScriptRobot.answers.Add(tempChatBox);

                    //Adds the new answer to the system
                    //SaveSystem.SavePlayer(this);
                    SendMessageToChat(previousQuestion + " has now been added to my database. Try asking me the question again", MessageRobot.MessageType.database);
                    addingAnswer = !addingAnswer;
                    searchOnlineBtn.SetActive(false);
                    addToDatabaseBtn.SetActive(false);

                }
                //Impletement the algorithm that makes arisa answer the question its asked
            }
        }
        else
        {
            if (!chatBox.isFocused && Input.GetKeyDown(KeyCode.Return))
            {
                chatBox.ActivateInputField();
            }
        }

        if (!chatBox.isFocused)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SendMessageToChat("Arisa: " + "You pressed space!", MessageRobot.MessageType.info);
            }
        }


    }

    IEnumerator SimpleUITransition(float waitTime)
    {
        yield return new
        WaitForSeconds(waitTime);
        searchOnlineBtn.SetActive(true);
        addToDatabaseBtn.SetActive(true);
        StopCoroutine(SimpleUITransition(waitTime));
    }

    public void ArisaDynamicAnswerAlgorithm()
    {
        addToDatabaseBtn.SetActive(false);
        searchOnlineBtn.SetActive(false);

        addingAnswer = true;
        previousQuestion = tempChatBox;
        tempChatBox.Trim();
        chatboxKeywords.Add(tempChatBox);
        arisaScriptRobot.keyword.Add(tempChatBox);
        //Saves the new keyword
        //SaveSystem.SavePlayer(this);
        //SaveSystem.ArisaVoiceDataBaseSaveState(arisaScript);
        SendMessageToChat("Adding " + tempChatBox + " to my database, can you kindly provide me the answer?", MessageRobot.MessageType.info);
    }

    public void ArisaSearchOnline()
    {
        Application.OpenURL("https://www.google.com/search?q=" + tempChatBox);
        searchOnlineBtn.SetActive(false);
        addToDatabaseBtn.SetActive(false);

    }

    public void SendMessageToChat(string text, MessageRobot.MessageType messageType)
    {
        if (messageList.Count >= maxMessages)
        {
            Destroy(messageList[0].textObject.gameObject);

            messageList.Remove(messageList[0]);
        }
        Message newMessage = new Message();

        newMessage.text = text;

        GameObject newText = Instantiate(textObject, chatPanel.transform);

        newMessage.textObject = newText.GetComponent<TextMeshProUGUI>();
        //newMessage.textObject.fontStyle = (FontStyles)FontStyle.Bold;
        newMessage.textObject.text = newMessage.text;
        newMessage.textObject.color = MessageTypeColor(messageType);

        messageList.Add(newMessage);
    }



    Color MessageTypeColor(MessageRobot.MessageType messageType)
    {
        Color color = info;

        switch (messageType)
        {
            case MessageRobot.MessageType.playerMessage:
                color = yourMessage;
                break;
            case MessageRobot.MessageType.database:
                color = database;
                break;       
        }


        return color;
    }

    public void playChatboxText(int answerNumber)
    {
        textToSpeech.SpeechText(chatboxAnswers[answerNumber]).OnSuccess((audioData) =>
        {
            // Create a new game object
            //GameObject audioGameObject = new();
            //audioGameObject.name = "My text to speech";
            // Create an Audio Source Component attached to the game object
            //AudioSource audioSource = audioGameObject.AddComponent(typeof(AudioSource)) as AudioSource;
            // Create an audio clip with the audio data
            AudioClip audioClip = AudioClip.Create(this.name, audioData.value.Length, 1, textToSpeech.samplerate, false);
            audioClip.SetData(audioData.value, 0);
            // Play The Audio!
            audioSource.clip = audioClip;
            audioSource.Play();

        });
    }

    public void arisaChatboxVoicePlayer(int voiceNumber, string answerKey)
    {
        StopAllCoroutines();
        playChatboxText(voiceNumber);
        StartCoroutine(TypeSentence(answerKey));
        anim.enabled = true;
        anim.Play("arisaRobotAnswer");
    }

    IEnumerator TypeSentence(string arisaTextReply)
    {
        subtitleText.text = "";
        StopCoroutine("idleSentence");
        foreach (char letter in arisaTextReply.ToCharArray())
        {
            subtitleText.text += letter;
            yield return new WaitForSeconds(displayTextDuration);
        }
        anim.Play("arisaRobotIdle");
        //anim.enabled = false;
    }

    IEnumerator AnswerDelay(int keywordIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        SendMessageToChat("Arisa: " + chatboxAnswers[keywordIndex], MessageRobot.MessageType.info);
        arisaChatboxVoicePlayer(keywordIndex, chatboxAnswers[keywordIndex]);


    }

    public object SaveState()
    {
        return new SaveData()
        {
            addedKeywords = this.chatboxKeywords,
            addedAnswers = this.chatboxAnswers
        };
    }

    public void LoadState(object state)
    {
        var saveData = (SaveData)state;
        chatboxKeywords = saveData.addedKeywords;
        chatboxAnswers = saveData.addedAnswers;

    }

    [Serializable]
    private struct SaveData
    {
        public List<string> addedKeywords;
        public List<string> addedAnswers;
    }

}

[System.Serializable]
public class MessageRobot
{
    public string textRobot;
    public TextMeshProUGUI textObjectRobot;
    public MessageType messageType;

    public enum MessageType
    {
        playerMessage,
        info,
        database,
        
    }
}
