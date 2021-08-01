using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    private Tile BackupTilePrefab;
    [SerializeField]
    private Tile[] TilePrefabs;
    [SerializeField][HideInInspector]
    private List<TileData> Tiles = new List<TileData>();
    [SerializeField][HideInInspector]
    private List<int> TilePrefabIndices = new List<int>();

    public void GetTiles()
    {
        Tiles.Clear();
        TilePrefabIndices.Clear();
        TilePrefabs = Resources.LoadAll<Tile>("Tiles/");
        for (int i = 0; i < TilePrefabs.Length; i++)
        {
            Tiles.Add(TilePrefabs[i].data);
            TilePrefabIndices.Add(i);
        }
        Debug.Log("Loaded " + Tiles.Count + " Tiles.");
    }

    public void Generate(TileMapData mapData, Vector3 middlePos, GameObject parent = null)
    {
        TileMapData data = new TileMapData(mapData);
        GenerateInitialTiles(data, middlePos, parent);
        GenerateRestOfTiles(data, middlePos, parent);
    }

    private void GenerateInitialTiles(TileMapData mapData, Vector3 middlePos, GameObject parent = null)
    {
        List<int> indices = GetRandomIndicesFromAvailableCells(mapData, mapData.availableCells.Count / 20);
        List<int> cellIndicesNowOccupied = new List<int>();

        foreach (int index in indices)
        {
            if (mapData.availableCells[index].state != CellState.Available)
                continue;

            List<int> cellIndices = null;
            TileRotation tileRotation = TileRotation._0;
            int tileIndex = 0;

            GetPlaceableTile(mapData, mapData.availableCells[index].position, out cellIndices, out tileRotation, out tileIndex);
            Tile tile = PlaceTile(tileIndex, mapData, middlePos, mapData.availableCells[index].position, tileRotation, parent);

            foreach (int cellIndex in cellIndices)
            {
                mapData.cells[cellIndex].SetCorrespondingTile(tile);
                mapData.cells[cellIndex].state = CellState.Occupied;
                cellIndicesNowOccupied.Add(cellIndex);
            }
        }

        RemoveIndicesFromAvailableCells(mapData, cellIndicesNowOccupied);
    }

    private void GenerateRestOfTiles(TileMapData mapData, Vector3 middlePos, GameObject parent = null)
    {
        List<int> randomIndexOrder = new List<int>();
        for (int i = 0; i < mapData.availableCells.Count; i++)
        {
            randomIndexOrder.Add(i);
        }
        randomIndexOrder = GetShuffledIndexList(randomIndexOrder);
        List<int> cellIndicesNowOccupied = new List<int>();

        for (int i = 0; i < mapData.availableCells.Count; i++)
        {
            if (mapData.availableCells[randomIndexOrder[i]].state != CellState.Available)
                continue;

            List<int> cellIndices = null;
            TileRotation tileRotation = TileRotation._0;
            int tileIndex = 0;

            GetPlaceableTile(mapData, mapData.availableCells[randomIndexOrder[i]].position, out cellIndices, out tileRotation, out tileIndex);
            Tile tile = PlaceTile(tileIndex, mapData, middlePos, mapData.availableCells[randomIndexOrder[i]].position, tileRotation, parent);

            foreach (int cellIndex in cellIndices)
            {
                mapData.cells[cellIndex].SetCorrespondingTile(tile);
                mapData.cells[cellIndex].state = CellState.Occupied;
                cellIndicesNowOccupied.Add(cellIndex);
            }
        }

        RemoveIndicesFromAvailableCells(mapData, cellIndicesNowOccupied);
    }

    private void GetPlaceableTile(TileMapData mapData, Vector2 position, out List<int> cellIndices, out TileRotation tileRotation, out int tileIndex)
    {
        List<int> tilePrefabOrder = GetShuffledIndexList(TilePrefabIndices);

        //Test all tile prefabs
        foreach (int tilePrefabIndex in tilePrefabOrder)
        {
            int randomStartRot = Random.Range(0, 4);
            //Test all rotations for tile prefab
            for (int i = 0; i < 4; i++)
            {
                tileRotation = (TileRotation)(i);
                if(CanTileBePlaced(mapData, position, Tiles[tilePrefabIndex].GetOccupiedSpaces(tileRotation), out cellIndices))
                {
                    tileIndex = tilePrefabIndex;
                    return;
                }
            }
        }

        //Default to backup tile
        tileIndex = -1;
        int index = 0;
        mapData.TryPositionToIndex(position, out index);
        cellIndices = new List<int>();
        cellIndices.Add(index);
        tileRotation = (TileRotation)((int)Random.Range(0, 3));
    }

    protected virtual Tile PlaceTile(int tileIndex, TileMapData mapData, Vector3 middlePos, Vector3 position, TileRotation rotation, GameObject parent = null)
    {
        float centeringValue = mapData.GetSize() / 2.0f - 0.5f;
        Vector3 pos = middlePos + new Vector3(position.x - centeringValue, 0, position.y - centeringValue) * Settings.TileWidth;
        Tile tile;

        if (tileIndex < 0)
        {
            tile = Instantiate(BackupTilePrefab, pos, Quaternion.Euler(0, TileRotationToDegrees(rotation), 0), parent.transform);
            return tile;
        }

        tile = Instantiate(TilePrefabs[tileIndex], pos, Quaternion.Euler(0, TileRotationToDegrees(rotation), 0), parent.transform);
        return tile;
    }

    private void RemoveIndicesFromAvailableCells(TileMapData mapData, List<int> indicesToRemove)
    {
        foreach (int index in indicesToRemove)
        {
            if (mapData.availableCells.Contains(mapData.cells[index]))
            {
                mapData.availableCells.Remove(mapData.cells[index]);
            }
        }
    }

    private List<int> GetRandomIndicesFromAvailableCells(TileMapData mapData, int indicesCount)
    {
        if(indicesCount > mapData.availableCells.Count)
        {
            Debug.LogError("Random indices count requested is higher than the amount of cells in mapdata.");
            return null;
        }

        List<int> randomIndices = new List<int>();
        for (int i = 0; i < indicesCount; i++)
        {
            int index = Random.Range(0, mapData.availableCells.Count);
            while (randomIndices.Contains(index))
            {
                index++;
                if (index >= mapData.availableCells.Count)
                    index = 0;
            }
            randomIndices.Add(index);
        }
        return randomIndices;
    }

    private bool CanTileBePlaced(TileMapData mapData, Vector2 position, Vector2[] tileOccupyingSpaces, out List<int> cellIndexes)
    {
        cellIndexes = new List<int>();

        for (int i = 0; i < tileOccupyingSpaces.Length; i++)
        {
            int cellIndex = 0;
            if (!mapData.TryPositionToIndex(position + tileOccupyingSpaces[i], out cellIndex))
                return false;

            cellIndexes.Add(cellIndex);

            if (mapData.cells[cellIndex].state != CellState.Available)
            {
                return false;
            }
        }

        return true;
    }

    private List<int> GetShuffledIndexList(List<int> originalList)
    {
        List<int> newList = new List<int>(originalList);
        for (int i = 0; i < newList.Count; i++)
        {
            int temp = newList[i];
            int randomIndex = Random.Range(i, newList.Count);
            newList[i] = newList[randomIndex];
            newList[randomIndex] = temp;
        }
        return newList;
    }

    private float TileRotationToDegrees(TileRotation tileRotation)
    {
        switch (tileRotation)
        {
            case TileRotation._0:
                return 0.0f;
            case TileRotation._90:
                return 90.0f;
            case TileRotation._180:
                return 180.0f;
            case TileRotation._270:
                return 270.0f;
            default:
                return 0.0f;
        }
    }
}

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if(GUILayout.Button("Get Tiles"))
        {
            ((MapGenerator)serializedObject.targetObject).GetTiles();
        }

        base.OnInspectorGUI();
    }
}
