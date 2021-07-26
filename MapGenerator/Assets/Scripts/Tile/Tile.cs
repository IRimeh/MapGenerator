using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class Tile : MonoBehaviour
{
    private int oldTileSize = 3;
    [Min(1)]
    public int TileSize = 3;
    public bool[] Tiles = new bool[9];

    [SerializeField]
    public TileData data = new TileData();

    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            RecalculateTileData();
        }
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            AdjustTileSize();
            RecalculateTileData();
        }
    }

    private void AdjustTileSize()
    {
        if (TileSize != oldTileSize)
        {
            if (TileSize % 2 == 0)
            {
                TileSize -= (int)Mathf.Sign(oldTileSize - TileSize);
            }

            Tiles = new bool[TileSize * TileSize];
            oldTileSize = TileSize;

            Debug.Log("Adjusted Tile Size.");
        }
    }

    private void RecalculateTileData()
    {
        Vector2[] occupiedTiles = GetOccupiedTiles();
        data.SetOccupiedSpaces(occupiedTiles);
        Debug.Log("Recalculated Tile Data.");
    }

    private Vector2[] GetOccupiedTiles()
    {
        List<Vector2> occupiedTiles = new List<Vector2>();
        for (int i = 0; i < Tiles.Length; i++)
        {
            if(Tiles[i])
                occupiedTiles.Add(IndexToTilePosition(i));
        }
        return occupiedTiles.ToArray();
    }

    private Vector2 IndexToTilePosition(int index)
    {
        int x = index % TileSize;
        int y = Mathf.FloorToInt(index / TileSize);

        x -= TileSize / 2;
        y -= TileSize / 2;
        y = -y;

        return new Vector2(x, y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.5f, 1.0f, 0.5f);
        Vector2[] occupiedTiles = data.GetOccupiedSpaces(TileRotation._0);
        for (int i = 0; i < occupiedTiles.Length; i++)
        {
            Vector3 offset = new Vector3(occupiedTiles[i].x * TileSettings.TileWidth, 0, occupiedTiles[i].y * TileSettings.TileWidth);
            Gizmos.DrawCube(transform.position + offset, Vector3.one * TileSettings.TileWidth);
        }
    }
}

[CustomEditor(typeof(Tile))]
public class TileEditor : Editor
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
        tileSize = serializedObject.FindProperty("TileSize");

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
