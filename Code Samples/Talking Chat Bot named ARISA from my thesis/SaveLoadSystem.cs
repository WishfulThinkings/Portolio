using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveLoadSystem : MonoBehaviour
{
    public string SavePath => $"{Application.persistentDataPath}/database.txt";
   

    private void Start()
    {
        Load();
        
    }

    [ContextMenu("Save")]
    public void Save()
    {
        var state = LoadFile();
        SaveState(state);
        SaveFile(state);
        Debug.Log("Saved");
    }
    [ContextMenu("Load")]
    public void Load()
    {
        var state = LoadFile();
        LoadState(state);
        Debug.Log("Loaded");
    }
    
    public void SaveFile(object state)
    {
        using (var stream = File.Open(SavePath, FileMode.Create))
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, state);
        }
    }

    Dictionary<string, object> LoadFile()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("No Database Found in Filepath");
            return new Dictionary<string, object>();
        }

        using (FileStream stream = File.Open(SavePath, FileMode.Open))
        {
            var formatter = new BinaryFormatter();
            return (Dictionary<string, object>)formatter.Deserialize(stream);
        }
    }


    void SaveState(Dictionary<string, object> state)
    {
        foreach (var savable in FindObjectsOfType<SavableEntity>())
        {
            state[savable.Id] = savable.SaveState();
        }
    }

    void LoadState(Dictionary<string, object> state)
    {
        foreach (var savable in FindObjectsOfType<SavableEntity>())
        {
           if(state.TryGetValue(savable.Id, out object savedState))
            {
                savable.LoadState(savedState);
            }
        }
    }

   
}
