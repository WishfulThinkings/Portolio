using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using TextToSpeechApi;
using TMPro;
using System.Linq;
using System;

static class ArrayExtensionsRobot
{
    public static int IndexOf<T>(this T[] array, T value)
    {
        return Array.IndexOf(array, value);
    }
}

public class arisaScriptRobot : MonoBehaviour, ISavable
{
    //note ayusin mo yung pag destroy ng audioclip pagkatapos magamit after ng consultation ty
    public List<string> keyword;
    public List<string> answers;
    private KeywordRecognizer recognizer;
    private Coroutine attentionSpan;
    public bool isOn;
    [SerializeField]
    private float attentionDuration;
    
    //Script References
    TextToSpeech textToSpeech = new TextToSpeech();

    //Audio Source
    [SerializeField]
    private AudioSource audioSource;
   
    
    
    [SerializeField] 
    private int speechSpeed;
    public TextMeshProUGUI speechText;
    
    public static bool isSpeaking;
    public GameObject greenCircle;


    //Dialogue Script dont destroy please i am in pain help me oh my god this is so hard

    public TextMeshProUGUI subtitleText;
    public float displayTextDuration;

    //Audio Source tang ina 
    public AudioSource musicPlaylist;
    public AudioClip[] musicClip;

    //animator ng putang ina para gumalaw ang hayop tang ina 
    public Animator anim;


    void Start()
    {
        textToSpeech.Init();
        recognizer = new KeywordRecognizer(keyword.ToArray());
        recognizer.OnPhraseRecognized += OnPhraseRecognized;
    }


    void Update()
    {
        textToSpeech.SetNewSpeechSpeed(speechSpeed);

        if(isSpeaking == true)
        {
            ButtonClick();
        }
    }



   public void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        if(args.text == "Arisa")
        {
            attentionSpan = StartCoroutine(isListening());
        }

        if (isOn)
        {
            if(keyword.Contains(args.text))
            {
                ArisaAnswerAlgorithm(args.text);
            }
        }
    }

    public void ArisaAnswerAlgorithm(string answer)
    {
        var answerAlgorithm = from keywords in keyword
                              where keywords.Contains(answer)
                              select keywords;


        foreach (var possibleAnswer in answerAlgorithm)
        {
            var keywordIndex = keyword.IndexOf(possibleAnswer);
            Debug.Log(keywordIndex);
            arisaAnswer(keywordIndex, answers[keywordIndex]);
        }
    }

    private IEnumerator isListening()
    {
        isOn = true;
        greenCircle.SetActive(true);
        yield return new WaitForSeconds(attentionDuration);
        greenCircle.SetActive(false);
        isOn = false;

        stopClikedFunction();
    }

    public void playVoice(int answerNumber)
    {
        textToSpeech.SpeechText(answers[answerNumber]).OnSuccess((audioData) =>
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
    IEnumerator TypeSentence (string arisaTextReply)
    {
        subtitleText.text = "";
        StopCoroutine("idleSentence");
        foreach(char letter in arisaTextReply.ToCharArray())
        {
            subtitleText.text += letter;
            yield return new WaitForSeconds(displayTextDuration);

           
        }
        anim.Play("arisaRobotIdle");


    }

    //waiting text that is scraped for now but be sure to go back to it again 
    IEnumerator idleSentence (string idleSentence)
    {
        subtitleText.text = " ";
        foreach(char letter in idleSentence.ToCharArray())
        {
            //subtitleText.text = "...";
            subtitleText.text += letter;
            yield return new WaitForSeconds(3f);
           
            

        }    
    }
    public void ButtonClick()
    {
        recognizer.Start();
        speechText.text = "Listening";
        isSpeaking = !isSpeaking;
       // StartCoroutine(idleSentence("..."));
        subtitleText.text = "";
        //anim.Play("arisaQuestion");
    }

    void stopClikedFunction()
    {
        recognizer.Stop();
        speechText.text = "Start";
        isSpeaking = false;
       
        //anim.Play("arisaBackToIdle");
    }

    public void arisaAnswer (int voiceNumber, string answerKey)
    {
        
        
        playVoice(voiceNumber);
        StopCoroutine("TypeSentence");
        StartCoroutine(TypeSentence(answerKey));
        anim.Play("arisaRobotAnswer");
    }

    public object SaveState()
    {
        return new SaveData()
        {
            addedKeywords = this.keyword,
            addedAnswers = this.answers
        };
    }

    public void LoadState(object state)
    {
        var saveData = (SaveData)state;
        keyword = saveData.addedKeywords;
        answers = saveData.addedAnswers;

    }

    [Serializable]
    private struct SaveData
    {
        public List<string> addedKeywords;
        public List<string> addedAnswers;
    }

}


