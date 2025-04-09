using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class LocalizationFormatter 
{
    public static void FormatText(TextMeshProUGUI component, string english, string korean)
    {
        if (PlayerPrefs.GetInt("Language") == 0)
        {
            component.text = korean;
        }
        else
        {
            component.text = english;
        }
    }
}
