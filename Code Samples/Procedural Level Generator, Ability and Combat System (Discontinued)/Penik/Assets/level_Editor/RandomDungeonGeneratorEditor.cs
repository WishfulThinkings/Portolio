using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AbstractDungenGenerator), true)]

public class RandomDungeonGeneratorEditor : Editor
{
    AbstractDungenGenerator generator;

    private void Awake()
    {
        generator = (AbstractDungenGenerator)target;

    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Create Dungeon"))
        {
            generator.DungenGenerator();
        }
    }
}
