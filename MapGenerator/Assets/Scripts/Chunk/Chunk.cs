using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    private MapGenerator mapGenerator;
    private TileMapData mapData;

    public void SetMapGenerator(MapGenerator _mapGenerator)
    {
        mapGenerator = _mapGenerator;
    }

    public void Generate()
    {
        mapData = new TileMapData(Settings.ChunkSize);

        bool[] tileStates = new bool[Settings.ChunkSize * Settings.ChunkSize];
        for (int i = 0; i < tileStates.Length; i++)
        {
            tileStates[i] = true;
        }
        mapData.SetInitialTileMapState(tileStates);
        mapGenerator.Generate(mapData, transform.position, gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 1.0f);
    }
}
