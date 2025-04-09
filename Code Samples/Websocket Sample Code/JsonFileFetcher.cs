using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameDataFetcher : MonoBehaviour
{
    // URL of the API endpoint
    public string apiURL = "https://192.168.68.69:8081/api/games/list"; 


    public string authToken = "your-authentication-token-here"; 

    public List<GameResponse> responsesLists;

    void Start()
    {
        StartCoroutine(PostData());
    }

    IEnumerator PostData()
    {

        string jsonPayload = JsonUtility.ToJson(new { });


        using (UnityWebRequest www = UnityWebRequest.Post(apiURL, jsonPayload, "api/games/fetch/data"))
        {

            www.SetRequestHeader("Content-Type", "application/json");


            www.SetRequestHeader("Authorization", "Bearer " + authToken);


            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                Debug.Log("POST request successful!");


                string jsonResponse = www.downloadHandler.text;
                Debug.Log("Response: " + jsonResponse);


                GameResponse responseList = JsonUtility.FromJson<GameResponse>(jsonResponse);
                
                responsesLists.Add(responseList);

                if (responseList != null)
                {
                    foreach (var game in responseList.data)
                    {
                        Debug.Log("Type "+ game.id);

                    }
                }
            }
        }
    }
}

[Serializable]
public class GameResponse
{
    public string type;
    public string message;
    public string className;
    public string methodName;
    public List<Datas> data;
}

// This class is required to handle a list of GameResponse objects
[Serializable]
public class GameResponseList
{
    public List<GameResponse> items;
}

[Serializable]
public class Datas
{
    public int id;
    public string uuid;
    public string gameName;
    public string createdAt;
    public string updatedAt;
    public string updatedBy;
    public string createdBy;
}
