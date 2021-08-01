using UnityEngine;

public static class Settings
{
    public static float TileWidth = 1.0f;
    public static int ChunkSize = 32;
    public static int ViewDistance = 11;
    public static int MinimumStartingPositionsInChunk = 2;
    public static int PathWidth = 3;

    public static int HalfViewDistanceCeil
    {
        get { return Mathf.CeilToInt(ViewDistance / 2.0f); }
    }

    public static int HalfViewDistanceFloor
    {
        get { return Mathf.FloorToInt(ViewDistance / 2.0f); }
    }
}
