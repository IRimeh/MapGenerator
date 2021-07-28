using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileData
{
    [SerializeField]
    private Vector2[] occupiedSpaces;
    [SerializeField]
    private List<RotatedSpacesWrapper> rotatedOccupiedSpaces = new List<RotatedSpacesWrapper>();

    [SerializeField]
    private TileMapData correspondingTileMap;
    [SerializeField]
    private List<TileMapCell> correspondingTileMapCells;

    public Vector2[] GetOccupiedSpaces(TileRotation rotation)
    {
        switch (rotation)
        {
            case TileRotation._0:
                return rotatedOccupiedSpaces[0].RotatedSpaces;
            case TileRotation._90:
                return rotatedOccupiedSpaces[1].RotatedSpaces;
            case TileRotation._180:
                return rotatedOccupiedSpaces[2].RotatedSpaces;
            case TileRotation._270:
                return rotatedOccupiedSpaces[3].RotatedSpaces;
            default:
                return occupiedSpaces;
        }
    }
    
    public void SetOccupiedSpaces(Vector2[] _occupiedSpaces)
    {
        occupiedSpaces = _occupiedSpaces;
        CalculateRotatedOccupiedSpaces();
    }

    private void CalculateRotatedOccupiedSpaces()
    {
        Vector2[] spaces = (Vector2[])occupiedSpaces.Clone();
        rotatedOccupiedSpaces.Clear();

        for (int i = 0; i < 4; i++)
        {
            RotatedSpacesWrapper rotatedSpaces = new RotatedSpacesWrapper { RotatedSpaces = (Vector2[])spaces.Clone() };
            rotatedOccupiedSpaces.Add(rotatedSpaces);

            for (int j = 0; j < spaces.Length; j++)
            {
                spaces[j] = new Vector2(spaces[j].y, -spaces[j].x);
            }
        }
    }

    private void SetTileMap(TileMapData tileMap)
    {
        correspondingTileMap = tileMap;
    }

    private void SetTileMapCells(List<TileMapCell> tileMapCells)
    {
        correspondingTileMapCells = tileMapCells;
    }
}

[Serializable]
public struct RotatedSpacesWrapper
{
    public Vector2[] RotatedSpaces;
}
