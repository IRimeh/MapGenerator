using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ProceduralMapGenerator : MapGenerator
{
    static GameObject StaticPlayer;
    [SerializeField]
    private GameObject Player;
    [SerializeField]
    private Transform ChunkParent;

    [Header("Height Variation")]
    [SerializeField]
    private List<Texture2D> HeightVariationTextures = new List<Texture2D>();
    [SerializeField]
    private float MaxHeight = 2.0f;

    private Vector2Int chunkSpawnPlayerPos = new Vector2Int(0, 0);
    private Vector2Int chunkDisablePlayerPos = new Vector2Int(0, 0);
    private Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();
    private List<Chunk> newlySpawnedChunks = new List<Chunk>();

    // Start is called before the first frame update
    void Start()
    {
        StaticPlayer = Player;
        SpawnInitialChunks();
    }

    // Update is called once per frame
    private void Update()
    {
        SpawnNewChunks();
        DisableOldChunks();
        GenerateNewlySpawnedChunks();
    }

    private void SpawnNewChunks()
    {
        int xPlayerChunkPos = Mathf.RoundToInt(Player.transform.position.x / Settings.ChunkSize);
        int yPlayerChunkPos = Mathf.RoundToInt(Player.transform.position.z / Settings.ChunkSize);

        Vector2Int diff = new Vector2Int(Mathf.Abs(xPlayerChunkPos - chunkSpawnPlayerPos.x), Mathf.Abs(yPlayerChunkPos - chunkSpawnPlayerPos.y));

        if (xPlayerChunkPos > chunkSpawnPlayerPos.x)
        {
            SpawnXChunks(diff.x, chunkSpawnPlayerPos.x + Settings.HalfViewDistanceCeil, yPlayerChunkPos);
            chunkSpawnPlayerPos.x = xPlayerChunkPos;
        }
        else if (xPlayerChunkPos < chunkSpawnPlayerPos.x)
        {
            SpawnXChunks(diff.x, chunkSpawnPlayerPos.x - Settings.HalfViewDistanceCeil - diff.x + 1, yPlayerChunkPos);
            chunkSpawnPlayerPos.x = xPlayerChunkPos;
        }

        if (yPlayerChunkPos > chunkSpawnPlayerPos.y)
        {
            SpawnYChunks(diff.y, chunkSpawnPlayerPos.y + Settings.HalfViewDistanceCeil, xPlayerChunkPos);
            chunkSpawnPlayerPos.y = yPlayerChunkPos;
        }
        else if (yPlayerChunkPos < chunkSpawnPlayerPos.y)
        {
            SpawnYChunks(diff.y, chunkSpawnPlayerPos.y - Settings.HalfViewDistanceCeil - diff.y + 1, xPlayerChunkPos);
            chunkSpawnPlayerPos.y = yPlayerChunkPos;
        }
    }

    private void DisableOldChunks()
    {
        int xPlayerChunkPos = Mathf.RoundToInt(Player.transform.position.x / Settings.ChunkSize);
        int yPlayerChunkPos = Mathf.RoundToInt(Player.transform.position.z / Settings.ChunkSize);

        Vector2Int diff = new Vector2Int(Mathf.Abs(xPlayerChunkPos - chunkDisablePlayerPos.x), Mathf.Abs(yPlayerChunkPos - chunkDisablePlayerPos.y));

        if (xPlayerChunkPos > chunkDisablePlayerPos.x)
        {
            for (int i = 0; i < diff.x; i++)
            {
                DisableChunksInColumn(chunkDisablePlayerPos.x - Settings.HalfViewDistanceFloor + i, yPlayerChunkPos, diff.y);
            }

            chunkDisablePlayerPos.x = xPlayerChunkPos;
        }
        else if (xPlayerChunkPos < chunkDisablePlayerPos.x)
        {
            for (int i = 0; i < diff.x; i++)
            {
                DisableChunksInColumn(chunkDisablePlayerPos.x + Settings.HalfViewDistanceFloor - i, yPlayerChunkPos, diff.y);
            }
            chunkDisablePlayerPos.x = xPlayerChunkPos;
        }

        if (yPlayerChunkPos > chunkDisablePlayerPos.y)
        {
            for (int i = 0; i < diff.y; i++)
            {
                DisableChunksInRow(chunkDisablePlayerPos.y - Settings.HalfViewDistanceFloor + i, xPlayerChunkPos, diff.x);
            }
            chunkDisablePlayerPos.y = yPlayerChunkPos;
        }
        else if (yPlayerChunkPos < chunkDisablePlayerPos.y)
        {
            for (int i = 0; i < diff.y; i++)
            {
                DisableChunksInRow(chunkDisablePlayerPos.y + Settings.HalfViewDistanceFloor - i, xPlayerChunkPos, diff.x);
            }
            chunkDisablePlayerPos.y = yPlayerChunkPos;
        }
    }

    private void GenerateNewlySpawnedChunks()
    {
        //newlySpawnedChunks.Sort(SortChunksByDistanceToPlayer);
        newlySpawnedChunks.Sort(delegate (Chunk a, Chunk b)
        {
            return Vector2.Distance(Player.transform.position, a.transform.position)
            .CompareTo(
              Vector2.Distance(Player.transform.position, b.transform.position));
        });

        StartCoroutine(GenerateChunks(new List<Chunk>(newlySpawnedChunks)));

        newlySpawnedChunks.Clear();
    }

    private IEnumerator GenerateChunks(List<Chunk> chunksToGenerate)
    {
        foreach (Chunk chunk in chunksToGenerate)
        {
            AssignAdjecentChunks(chunk);
            chunk.Generate();
            yield return new WaitForFixedUpdate();
        }
    }

    private void SpawnInitialChunks()
    {
        int floor   = Mathf.FloorToInt(Settings.ViewDistance / 2.0f);
        int ceiling = Mathf.CeilToInt(Settings.ViewDistance / 2.0f);

        for (int i = -floor; i < ceiling; i++)
        {
            for (int j = -floor; j < ceiling; j++)
            {
                Chunk chunk = SpawnChunk(new Vector2Int(j, i));
                chunks.Add(new Vector2Int(j, i), chunk);
            }
        }
    }

    private void SpawnXChunks(int columns, int startChunkXPos, int middleChunkYPos)
    {
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < Settings.ViewDistance; y++)
            {
                Vector2Int chunkPos = new Vector2Int(startChunkXPos + x, middleChunkYPos - Settings.HalfViewDistanceFloor + y);
                EnableOrSpawnChunk(chunkPos);
            }
        }
    }

    private void SpawnYChunks(int rows, int startChunkYPos, int middleChunkXPos)
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < Settings.ViewDistance; x++)
            {
                Vector2Int chunkPos = new Vector2Int(middleChunkXPos - Settings.HalfViewDistanceFloor + x, startChunkYPos + y);
                EnableOrSpawnChunk(chunkPos);
            }
        }
    }

    private void DisableChunksInColumn(int x, int middleYPos, int additionalDist = 0)
    {
        for (int i = 0; i < Settings.ViewDistance + (additionalDist * 2); i++)
        {
            int y = middleYPos + i - Settings.HalfViewDistanceFloor - additionalDist;
            Vector2Int pos = new Vector2Int(x, y);
            TryDisableChunk(pos);
        }
    }

    private void DisableChunksInRow(int y, int middleXPos, int additionalDist = 0)
    {
        for (int i = 0; i < Settings.ViewDistance + (additionalDist * 2); i++)
        {
            int x = middleXPos + i - Settings.HalfViewDistanceFloor - additionalDist;
            Vector2Int pos = new Vector2Int(x, y);
            TryDisableChunk(pos);
        }
    }

    private bool TryDisableChunk(Vector2Int position)
    {
        if (chunks.ContainsKey(position))
        {
            chunks[position].gameObject.SetActive(false);
            return true;
        }
        return false;
    }

    private void EnableOrSpawnChunk(Vector2Int position)
    {
        if (chunks.ContainsKey(position))
        {
            //Enable the pre-existing chunk
            chunks[position].gameObject.SetActive(true);
        }
        else
        {
            //Spawn a new chunk and add it to the dictionary
            Chunk chunk = SpawnChunk(position);
            chunks.Add(position, chunk);
            
        }
    }

    private Chunk SpawnChunk(Vector2Int position)
    {
        GameObject chunkObj = new GameObject("Chunk (" + position.x + ", " + position.y + ")");
        chunkObj.transform.SetParent(ChunkParent);
        chunkObj.transform.position = new Vector3(position.x * Settings.ChunkSize, 0, position.y * Settings.ChunkSize);
        Chunk chunk = chunkObj.AddComponent<Chunk>();
        chunk.position = position;
        chunk.SetMapGenerator(this);
        newlySpawnedChunks.Add(chunk);
        return chunk;
    }

    private void AssignAdjecentChunks(Chunk chunk)
    {
        if (chunks.TryGetValue(chunk.position + new Vector2Int(0, 1), out Chunk chunkUp))
            chunk.AssignChunk(Chunk.Direction.Up, chunkUp);
        if (chunks.TryGetValue(chunk.position + new Vector2Int(0, -1), out Chunk chunkDown))
            chunk.AssignChunk(Chunk.Direction.Down, chunkDown);
        if (chunks.TryGetValue(chunk.position + new Vector2Int(-1, 0), out Chunk chunkLeft))
            chunk.AssignChunk(Chunk.Direction.Left, chunkLeft);
        if (chunks.TryGetValue(chunk.position + new Vector2Int(1, 0), out Chunk chunkRight))
            chunk.AssignChunk(Chunk.Direction.Right, chunkRight);
    }

    static int SortChunksByDistanceToPlayer(Chunk chunk1, Chunk chunk2)
    {
        float chunk1Dist = Vector3.Distance(chunk1.transform.position, StaticPlayer.transform.position);
        float chunk2Dist = Vector3.Distance(chunk2.transform.position, StaticPlayer.transform.position);

        return chunk1Dist < chunk2Dist ? 0 : 1;
    }

    protected override Tile PlaceTile(int tileIndex, TileMapData mapData, Vector3 middlePos, Vector3 position, TileRotation rotation, GameObject parent = null)
    {
        float centeringValue = mapData.GetSize() / 2.0f - 0.5f;
        Vector3 pos = middlePos + new Vector3(position.x - centeringValue, 0, position.y - centeringValue);

        float textureValue = 1.0f;
        for (int i = 0; i < HeightVariationTextures.Count; i++)
        {
            int x = (int)(Mathf.Abs(pos.x) / Settings.TileWidth % HeightVariationTextures[i].width - 1);
            int y = (int)(Mathf.Abs(pos.z) / Settings.TileWidth % HeightVariationTextures[i].height - 1);
            textureValue *= HeightVariationTextures[i].GetPixel(x, y).r;
        }

        return base.PlaceTile(tileIndex, mapData, middlePos + new Vector3(0, textureValue * MaxHeight, 0), position, rotation, parent);
    }
}

[CustomEditor(typeof(ProceduralMapGenerator))]
public class ProceduralMapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Get Tiles"))
        {
            ((ProceduralMapGenerator)serializedObject.targetObject).GetTiles();
        }

        base.OnInspectorGUI();
    }
}

