using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileMapData
{
    private int tileMapSize;
    public TileMapCell[] cells;
    public List<TileMapCell> availableCells = new List<TileMapCell>();

    public TileMapData(int _tileMapSize)
    {
        tileMapSize = _tileMapSize;
        cells = new TileMapCell[tileMapSize * tileMapSize];
    }

    public TileMapData(int _tileMapSize, bool[] _tiles)
    {
        tileMapSize = _tileMapSize;
        cells = new TileMapCell[tileMapSize * tileMapSize];
        SetInitialTileMapState(_tiles);
    }

    public void SetInitialTileMapState(bool[] tiles)
    {
        if(tiles.Length != cells.Length)
        {
            Debug.LogError("Length of input array does not equal the tilemaps size.");
            return;
        }

        for (int i = 0; i < tiles.Length; i++)
        {
            Vector2 position = IndexToPosition(i);
            CellState cellState = tiles[i] ? CellState.Available : CellState.Unavailable;
            cells[i] = new TileMapCell(this, position, cellState);
        }
    }

    private int PositionToIndex(Vector2 position)
    {
        return (int)(position.y * tileMapSize + position.x);
    }

    private Vector2 IndexToPosition(int index)
    {
        if (index == 0)
            return new Vector2(0, 0);

        int x = index % tileMapSize;
        int y = Mathf.FloorToInt(index / tileMapSize);
        return new Vector2(x, y);
    }
}
