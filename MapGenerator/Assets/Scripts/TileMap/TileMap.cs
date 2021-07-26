using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "TileMap", menuName = "ScriptableObjects/Create TileMap", order = 1)]
public class TileMap : ScriptableObject
{
    private int oldMapSize = 3;
    [Min(1)]
    public int MapSize = 8;
    public bool[] Tiles = new bool[64];

    [SerializeField]
    public TileMapData tileMapData = new TileMapData(8);

    private void OnValidate()
    {
        AdjustMapSize();
        RecalculateMapData();
    }

    private void RecalculateMapData()
    {
        tileMapData = new TileMapData(MapSize, Tiles);
        Debug.Log("Recalculated Map Data.");
    }

    private void AdjustMapSize()
    {
        if (MapSize != oldMapSize)
        {
            Tiles = new bool[MapSize * MapSize];
            oldMapSize = MapSize;

            Debug.Log("Adjusted Map Size.");
        }
    }
}


[CustomEditor(typeof(TileMap))]
public class TileMapEditor : Editor
{
    SerializedProperty occupiedTiles;
    SerializedProperty tileSize;

    private GUIContent _toggleButtonDepressedLabel;
    private GUIContent _toggleButtonDefaultLabel;

    // Current toggle state
    bool[] _toggleButtonStates;


    void OnEnable()
    {
        _toggleButtonDepressedLabel = new GUIContent();
        _toggleButtonDefaultLabel = new GUIContent();

        occupiedTiles = serializedObject.FindProperty("Tiles");
        tileSize = serializedObject.FindProperty("MapSize");

        RefreshToggleButtons();
    }

    private void RefreshToggleButtons()
    {
        _toggleButtonStates = new bool[occupiedTiles.arraySize];
        for (int i = 0; i < _toggleButtonStates.Length; i++)
        {
            _toggleButtonStates[i] = occupiedTiles.GetArrayElementAtIndex(i).boolValue;
        }
    }

    private void DrawToggleButtons()
    {
        int size = occupiedTiles.arraySize;
        int rowSize = (int)Mathf.Sqrt((float)size);

        for (int i = 0; i < tileSize.intValue; i++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < tileSize.intValue; j++)
            {
                int index = i * tileSize.intValue + j;
                if (index < _toggleButtonStates.Length)
                    DisplayToggle(index);
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void UpdateButtonValues()
    {
        for (int i = 0; i < occupiedTiles.arraySize; i++)
        {
            if (_toggleButtonStates.Length - 1 >= i)
            {
                occupiedTiles.GetArrayElementAtIndex(i).boolValue = _toggleButtonStates[i];
                serializedObject.ApplyModifiedProperties();
            }
        }
    }

    public override void OnInspectorGUI()
    {
        DrawPropertiesExcluding(serializedObject, "Tiles");

        RefreshToggleButtons();
        DrawToggleButtons();
        UpdateButtonValues();

        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();
    }

    void DisplayToggle(int index)
    {
        var image = _toggleButtonStates[index]
                    ? _toggleButtonDepressedLabel
                    : _toggleButtonDefaultLabel;
        var newState = EditorGUILayout.Toggle(image, _toggleButtonStates[index], GUILayout.MaxWidth(20.0f));
        if (newState != _toggleButtonStates[index])
        {
            _toggleButtonStates[index] = newState;
        }
    }
}
