using UnityEngine;

public class TileMapCell
{
    public Vector2 position;
    public CellState state = CellState.Unavailable;
    private Tile correspondingTile;

    public TileMapCell(Vector2 _position, CellState _state = CellState.Unavailable, Tile _correspondingTile = null)
    {
        position = _position;
        state = _state;
        correspondingTile = _correspondingTile;
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
