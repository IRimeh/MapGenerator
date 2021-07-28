using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileMapData
{
    [SerializeField][HideInInspector]
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

    public TileMapData(TileMapData dataToCopy)
    {
        tileMapSize = dataToCopy.tileMapSize;
        cells = new TileMapCell[dataToCopy.cells.Length];
        System.Array.Copy(dataToCopy.cells, cells, dataToCopy.cells.Length);
        availableCells = new List<TileMapCell>(dataToCopy.availableCells);
    }

    public void SetInitialTileMapState(bool[] tiles)
    {
        availableCells.Clear();
        if(tiles.Length != cells.Length)
        {
            Debug.LogError("Length of input array does not equal the tilemaps size.");
            return;
        }

        for (int i = 0; i < tiles.Length; i++)
        {
            Vector2 position;
            TryIndexToPosition(i, out position);
            CellState cellState = tiles[i] ? CellState.Available : CellState.Unavailable;
            cells[i] = new TileMapCell(this, position, cellState);
            if (tiles[i])
                availableCells.Add(cells[i]);
        }
    }

    public int GetSize()
    {
        return tileMapSize;
    }

    public bool TryPositionToIndex(Vector2 position, out int index)
    {
        index = (int)(position.y * tileMapSize + position.x);

        if (position.x < 0 || position.x >= tileMapSize ||
            position.y < 0 || position.y >= tileMapSize)
            return false;

        return true;
    }

    public bool TryIndexToPosition(int index, out Vector2 position)
    {
        if (index == 0)
        {
            position = new Vector2(0, 0);
            return true;
        }

        int x = index % tileMapSize;
        int y = Mathf.FloorToInt(index / tileMapSize);
        position = new Vector2(x, y);

        if (index < 0 || index >= cells.Length)
            return false;

        return true;
    }
}
