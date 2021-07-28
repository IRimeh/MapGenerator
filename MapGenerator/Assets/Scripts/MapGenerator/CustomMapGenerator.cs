using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CustomMapGenerator : MapGenerator
{
    [SerializeField]
    private TileMap tileMap;

    public void GenerateCustomMap()
    {
        if(!Application.isPlaying)
        {
            Debug.Log("Can only generate while playing");
            return;
        }

        if (tileMap != null)
        {
            Generate(tileMap.tileMapData, transform.position, gameObject);
            Debug.Log("Generated.");
        }
    }
}


[CustomEditor(typeof(CustomMapGenerator))]
public class CustomMapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Get Tiles"))
        {
            ((CustomMapGenerator)serializedObject.targetObject).GetTiles();
        }

        base.OnInspectorGUI();

        if (GUILayout.Button("Generate"))
        {
            ((CustomMapGenerator)serializedObject.targetObject).GenerateCustomMap();
        }
    }
}
