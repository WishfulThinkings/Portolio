using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SavableEntity : MonoBehaviour
{

    [SerializeField] private string id;

    
    public string Id => id;

    [ContextMenu("Generate ID")]
    private void GenerateID()
    {
        id = Guid.NewGuid().ToString();
    }

    //find all ISavable components

   
    public object SaveState()
    {
        var state = new Dictionary<string, object>();
        foreach (var savable in GetComponents<ISavable>())
        {
            state[savable.GetType().ToString()] = savable.SaveState();
        }

        return state;
    }

    public void LoadState(object state)
    {
        var stateDictionary = (Dictionary<string, object>)state;

        foreach (var savable in GetComponents<ISavable>())
        {
            string typeName = savable.GetType().ToString();
            if(stateDictionary.TryGetValue(typeName, out object savedState))
            {
                savable.LoadState(savedState);
            }
        }
    }
}
