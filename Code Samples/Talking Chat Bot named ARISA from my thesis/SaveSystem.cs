using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem 
{
   public static string fileName = "ArisaDataBase.txt";
    public static string directory = "/SaveData/";

    
    public static void Save(ArisaData arisa)
   {
      string dir = Application.persistentDataPath + directory;

    if (Directory.Exists(dir))
         Directory.CreateDirectory(dir);
        

     string json = JsonUtility.ToJson(arisa);
      File.WriteAllText(dir + fileName, json);
   }

    public static ArisaData Load()
    {
        string fullPath = Application.persistentDataPath + directory + fileName;
        ArisaData arisaD = new ArisaData();
        if (File.Exists(fullPath))
        {
            string json = File.ReadAllText(fullPath);
            arisaD = JsonUtility.FromJson<ArisaData>(json);
        }

        else
        {
            Debug.Log("No save file");
        }

        return arisaD;
    }

    //public static void SavePlayer(MessagingSystem arisaChatBoxDatabase)
    //{
    //    BinaryFormatter formatter = new BinaryFormatter();
    //    string path = Application.persistentDataPath + "/Arisa.tite";
    //    FileStream stream = new FileStream(path, FileMode.Create);

    //    ArisaData arisaData = new ArisaData(arisaChatBoxDatabase);

    //    formatter.Serialize(stream, arisaData);
        
    //    stream.Close();
    //}

    //public static void ArisaVoiceDataBaseSaveState(arisaScript arisaVoiceDatabase)
    //{
    //    BinaryFormatter formatter = new BinaryFormatter();
    //    string path = Application.persistentDataPath + "/Arisa.tite";
    //    FileStream stream = new FileStream(path, FileMode.Create);

    //    ArisaVoiceData arisaVoiceData = new ArisaVoiceData(arisaVoiceDatabase);

    //    formatter.Serialize(stream, arisaVoiceData);
    //    stream.Close();
    //}

    //public static ArisaData LoadArisa ()
    //{
    //    string path = Application.persistentDataPath + "/Arisa.tite";
    //    if (File.Exists(path))
    //    {
    //        BinaryFormatter formatter = new BinaryFormatter();
    //        FileStream stream = new FileStream(path, FileMode.Open);

    //        ArisaData arisaData = formatter.Deserialize(stream) as ArisaData;
    //        ArisaVoiceData arisaVoiceData = formatter.Deserialize(stream) as ArisaVoiceData;
    //        Debug.Log("Chatbox Database loaded");
            

    //        stream.Close();
            
    //        return arisaData;

    //    }
    //    else {
    //        Debug.LogError("Database not found in " + path);
    //        return null;
    //    }
    //}

    //public static ArisaVoiceData LoadArisaVoiceDatabase()
    //{
    //    string path = Application.persistentDataPath + "/Arisa.tite";
    //    if (File.Exists(path))
    //    {
    //        BinaryFormatter formatter = new BinaryFormatter();
    //        FileStream stream = new FileStream(path, FileMode.Open);
    //        Debug.Log("Database Loaded");

            
    //        ArisaVoiceData arisaVoiceData = formatter.Deserialize(stream) as ArisaVoiceData;
    //        stream.Close();

    //        return arisaVoiceData;

    //    }
    //    else
    //    {
    //        Debug.LogError("Database not found in " + path);
    //        return null;
    //    }
    //}


}
