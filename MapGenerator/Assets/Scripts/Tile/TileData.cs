using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileData
{
    private Vector2[] occupiedSpaces;
    private List<Vector2[]> rotatedOccupiedSpaces = new List<Vector2[]>();

    public Vector2[] GetOccupiedSpaces(TileRotation rotation)
    {
        switch (rotation)
        {
            case TileRotation._0:
                return rotatedOccupiedSpaces[0];
            case TileRotation._90:
                return rotatedOccupiedSpaces[1];
            case TileRotation._180:
                return rotatedOccupiedSpaces[2];
            case TileRotation._270:
                return rotatedOccupiedSpaces[3];
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
            rotatedOccupiedSpaces.Add((Vector2[])spaces.Clone());

            for (int j = 0; j < spaces.Length; j++)
            {
                spaces[j] = new Vector2(spaces[j].y, -spaces[j].x);
            }
        }
    }
}
