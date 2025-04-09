using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

public class Settings : MonoBehaviour
{
    public Scrollbar musicSlider;
    public Scrollbar audioSlider;

    public TMP_Dropdown languageDropdown;


    private void OnEnable()
    {
        if (PlayerPrefs.HasKey("Music"))
        {
            musicSlider.value = PlayerPrefs.GetFloat("Music");
            AudioManager.Instance.musicSource.volume = musicSlider.value;
            AudioManager.Instance.musicSourceB.volume = musicSlider.value;
        }
        if(PlayerPrefs.HasKey("Sound Effect"))
        {
            audioSlider.value = PlayerPrefs.GetFloat("Sound Effects");
            AudioManager.Instance.sfxSource.volume = audioSlider.value;
        }
        if (PlayerPrefs.HasKey("Language"))
        {
          languageDropdown.value = PlayerPrefs.GetInt("Language"); 
          LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[PlayerPrefs.GetInt("Language")];
        }

    }
    void Update()
    {
        PlayerPrefs.SetFloat("Music", musicSlider.value);
        PlayerPrefs.SetFloat("Sound Effects", audioSlider.value);
        AudioManager.Instance.musicSource.volume = musicSlider.value;
        AudioManager.Instance.musicSourceB.volume = musicSlider.value;
        AudioManager.Instance.sfxSource.volume = audioSlider.value;
        
        PlayerPrefs.SetInt("Language", languageDropdown.value);
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[languageDropdown.value];
    }
}
