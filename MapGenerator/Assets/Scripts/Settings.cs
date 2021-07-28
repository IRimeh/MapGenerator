using UnityEngine;

public static class Settings
{
    public static float TileWidth = 1.0f;
    public static int ChunkSize = 12;
    public static int ViewDistance = 7;

    public static int HalfViewDistanceCeil
    {
        get { return Mathf.CeilToInt(ViewDistance / 2.0f); }
    }

    public static int HalfViewDistanceFloor
    {
        get { return Mathf.FloorToInt(ViewDistance / 2.0f); }
    }
}
