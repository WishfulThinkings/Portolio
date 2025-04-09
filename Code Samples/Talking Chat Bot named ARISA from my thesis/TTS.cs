using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TextToSpeechApi;
public class TTS : MonoBehaviour
{
    TextToSpeech textToSpeech = new();

    void Start()
    {
        textToSpeech.Init();
        textToSpeech.SpeechText("Yohohoho").OnSuccess((audioData) =>
        {
            // Create a new game object
            GameObject audioGameObject = new();
            audioGameObject.name = "My text to speech";
            // Create an Audio Source Component attached to the game object
            AudioSource audioSource = audioGameObject.AddComponent(typeof(AudioSource)) as AudioSource;
            // Create an audio clip with the audio data
            AudioClip audioClip = AudioClip.Create(audioGameObject.name, audioData.value.Length, 1, textToSpeech.samplerate, false);
            audioClip.SetData(audioData.value, 0);
            // Play The Audio!
            audioSource.clip = audioClip;
            audioSource.Play();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void nDestroy()
    {
        textToSpeech.Stop();
    }
}
