using UnityEngine;

public class TileMapCell
{
    public Vector2 position;
    public CellState state = CellState.Unavailable;
    private Tile correspondingTile;
    private TileMapData tileMap;

    public TileMapCell(TileMapData _tileMap, Vector2 _position, CellState _state = CellState.Unavailable)
    {
        tileMap = _tileMap;
        position = _position;
        state = _state;
    }

    public void SetCorrespondingTile(Tile tile)
    {
        correspondingTile = tile;
    }

    public Tile GetCorrespondingTile()
    {
        return correspondingTile;
    }
}

public enum CellState
{
    Unavailable,
    Available,
    Occupied
}
