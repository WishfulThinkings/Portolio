using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class OptionsToggle : MonoBehaviour
{
    public GameObject options;
    public ArisaData arisaD;

    public AudioClip[] songs;
    public AudioSource player;
    public TextMeshProUGUI audioTitle;
    [SerializeField]
    private bool isOpen = false;
    
    
    public void OptionsOpen()
    {
        isOpen = !isOpen;
        options.SetActive(isOpen);       
    }

    public void AppQiot()
    {
        Application.Quit();
    }

    public void SaveProgress()
    {
        SaveSystem.Save(arisaD);

    }

    public void PlayMusic()
    {
       player.Stop();
       int songPicker = Random.Range(0, songs.Length);
       player.PlayOneShot(songs[songPicker]);
        string tempStore = songs[songPicker].name.ToString();
       
       
       Debug.Log("Song: " + tempStore);
       audioTitle.text = tempStore;

    }

    public void StopMusic()
    {
        player.Stop();
        audioTitle.text = "";
    }
}
