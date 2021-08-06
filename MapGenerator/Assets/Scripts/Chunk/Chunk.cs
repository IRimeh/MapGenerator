using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public Vector2Int position;
    private MapGenerator mapGenerator;
    private TileMapData mapData;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;

    [SerializeField] private Chunk chunkUp;
    [SerializeField] private Chunk chunkDown;
    [SerializeField] private Chunk chunkLeft;
    [SerializeField] private Chunk chunkRight;

    private Dictionary<Direction, Chunk> adjecentChunks = new Dictionary<Direction, Chunk>();
    private Dictionary<Direction, Vector2Int> pathStartingPoints = new Dictionary<Direction, Vector2Int>();
    private List<Direction> availableDirectionsForPoints = new List<Direction>() { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
    private List<Path> paths = new List<Path>();

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public void SetMapGenerator(MapGenerator _mapGenerator)
    {
        mapGenerator = _mapGenerator;
    }

    public void Generate()
    {
        mapData = new TileMapData(Settings.ChunkSize);
        bool[] tileStates = new bool[Settings.ChunkSize * Settings.ChunkSize];

        ShuffleDirectionList();
        CreateStartingPoints();
        CreatePaths();
        PopulatePaths(tileStates);
        WidenPaths(tileStates, Settings.PathWidth);

        mapData.SetInitialTileMapState(tileStates);
        mapGenerator.Generate(mapData, transform.position, gameObject);
        CombineTilesIntoOneMesh();
    }


    private void CombineTilesIntoOneMesh()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            Matrix4x4 matrix = meshFilters[i].transform.localToWorldMatrix;
            matrix.SetColumn(3, new Vector4(matrix[0,3] - transform.position.x, matrix[1,3], matrix[2,3] - transform.position.z));
            combine[i].transform = matrix;
            meshFilters[i].gameObject.SetActive(false);

            i++;
        }

        Mesh mesh = new Mesh();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = meshFilters[0].GetComponent<MeshRenderer>().sharedMaterial;

        mesh.CombineMeshes(combine, true);
    }

    private void ShuffleDirectionList()
    {
        List<Direction> shuffledDirections = new List<Direction>();
        for (int i = 0; i < 4; i++)
        {
            int index = Random.Range(0, availableDirectionsForPoints.Count);
            Direction direction = availableDirectionsForPoints[index];
            shuffledDirections.Add(direction);
            availableDirectionsForPoints.Remove(direction);
        }
        availableDirectionsForPoints = shuffledDirections;
    }

    private void CreateStartingPoints()
    {
        int numberOfStartingPoints = Random.Range(Settings.MinimumStartingPositionsInChunk, 5);

        //Get starting points necessary from adjecent chunks
        foreach (KeyValuePair<Direction, Chunk> adjecentChunk in adjecentChunks)
        {
            Direction reverseDirection = GetReverseDirection(adjecentChunk.Key);
            Vector2Int point;
            if(adjecentChunk.Value.pathStartingPoints.TryGetValue(reverseDirection, out point))
            {
                Vector2Int newStartingPoint = GetEquivalentPointInAdjecentChunk(point, reverseDirection);
                pathStartingPoints.Add(adjecentChunk.Key, newStartingPoint);
                availableDirectionsForPoints.Remove(adjecentChunk.Key);
            }
        }

        //Create additional starting points if necessary
        while(pathStartingPoints.Count < numberOfStartingPoints)
        {
            Vector2Int newPoint = CreateStartingPointForDirection(availableDirectionsForPoints[0]);
            pathStartingPoints.Add(availableDirectionsForPoints[0], newPoint);
            availableDirectionsForPoints.RemoveAt(0);
        }
    }

    private void CreatePaths()
    {
        //If 0 or 1 starting points, don't create any paths
        if (pathStartingPoints.Count <= 1)
        {
            foreach (KeyValuePair<Direction, Vector2Int> startingPoint in pathStartingPoints)
            {
                Path path = new Path(startingPoint.Value, startingPoint.Value, true);
                paths.Add(path);
                return;
            }
            return;
        }

        //Create start and end points of the paths
        List<Vector2Int> startingPoints = Enumerable.ToList(pathStartingPoints.Values);
        switch (pathStartingPoints.Count)
        {
            case 2:
                paths.Add(new Path(startingPoints[0], startingPoints[1]));
                break;
            case 3:
                int index = Random.Range(1, 3);
                int remainingIndex = index == 1 ? 2 : 1;
                paths.Add(new Path(startingPoints[0], startingPoints[index]));
                paths.Add(new Path(startingPoints[remainingIndex], startingPoints[index]));
                break;
            case 4:
                paths.Add(new Path(startingPoints[0], startingPoints[1]));
                paths.Add(new Path(startingPoints[2], startingPoints[3]));
                break;
        }
    }

    private void PopulatePaths(bool[] tileStates)
    {
        foreach (Path path in paths)
        {
            Vector2Int currentPos = path.StartPosition;
            while(currentPos != path.EndPosition)
            {
                path.Positions.Add(currentPos);
                if(mapData.TryPositionToIndex(currentPos, out int j))
                {
                    tileStates[j] = true;
                }

                int xDiff = (int)Mathf.Pow(path.EndPosition.x - currentPos.x, 3);
                int yDiff = (int)Mathf.Pow(path.EndPosition.y - currentPos.y, 3);
                int rand = Random.Range(0, Mathf.Abs(xDiff) + Mathf.Abs(yDiff) + 1) - Mathf.Abs(xDiff);

                if(rand >= 0)
                {
                    //Y
                    currentPos.y += (int)Mathf.Sign(yDiff);
                }
                else
                {
                    //X
                    currentPos.x += (int)Mathf.Sign(xDiff);
                }
            }

            path.Positions.Add(currentPos);
            if (mapData.TryPositionToIndex(currentPos, out int i))
            {
                tileStates[i] = true;
            }
        }
    }

    private void WidenPaths(bool[] tileStates, int widenAmount)
    {
        foreach (Path path in paths)
        {
            for (int i = 0; i < widenAmount; i++)
            {
                List<Vector2Int> positionsToWiden = i == 0 ? path.Positions : path.WidenedPositions[i - 1];
                path.WidenedPositions.Add(new List<Vector2Int>());


                Vector2Int pos;
                foreach (Vector2Int position in positionsToWiden)
                {
                    pos = position + new Vector2Int(0, 1);
                    if (!positionsToWiden.Contains(pos) && !path.WidenedPositions[i].Contains(pos))
                    {
                        path.WidenedPositions[i].Add(pos);
                        if(mapData.TryPositionToIndex(pos, out int index))
                        {
                            tileStates[index] = true;
                        }
                    }

                    pos = position + new Vector2Int(0, -1);
                    if (!positionsToWiden.Contains(pos) && !path.WidenedPositions[i].Contains(pos))
                    {
                        path.WidenedPositions[i].Add(pos);
                        if (mapData.TryPositionToIndex(pos, out int index))
                        {
                            tileStates[index] = true;
                        }
                    }

                    pos = position + new Vector2Int(1, 0);
                    if (!positionsToWiden.Contains(pos) && !path.WidenedPositions[i].Contains(pos))
                    {
                        path.WidenedPositions[i].Add(pos);
                        if (mapData.TryPositionToIndex(pos, out int index))
                        {
                            tileStates[index] = true;
                        }
                    }

                    pos = position + new Vector2Int(-1, 0);
                    if (!positionsToWiden.Contains(pos) && !path.WidenedPositions[i].Contains(pos))
                    {
                        path.WidenedPositions[i].Add(pos);
                        if (mapData.TryPositionToIndex(pos, out int index))
                        {
                            tileStates[index] = true;
                        }
                    }
                }
            }
        }
    }

    public void AssignChunk(Direction direction, Chunk chunk)
    {
        if(adjecentChunks.ContainsKey(direction))
        {
            adjecentChunks[direction] = chunk;
        }
        else
        {
            adjecentChunks.Add(direction, chunk);
        }

        switch (direction)
        {
            case Direction.Up:
                chunkUp = chunk;
                break;
            case Direction.Down:
                chunkDown = chunk;
                break;
            case Direction.Left:
                chunkLeft = chunk;
                break;
            case Direction.Right:
                chunkRight = chunk;
                break;
            default:
                break;
        }
    }

    private Direction GetReverseDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Direction.Down;
            case Direction.Down:
                return Direction.Up;
            case Direction.Left:
                return Direction.Right;
            case Direction.Right:
                return Direction.Left;
            default:
                return Direction.Up;
        }
    }

    private Vector2Int GetEquivalentPointInAdjecentChunk(Vector2Int point, Direction directionOfAdjecentChunk)
    {
        switch (directionOfAdjecentChunk)
        {
            case Direction.Up:
                return new Vector2Int(point.x, 0);
            case Direction.Down:
                return new Vector2Int(point.x, mapData.GetSize() - 1);
            case Direction.Left:
                return new Vector2Int(mapData.GetSize() - 1, point.y);
            case Direction.Right:
                return new Vector2Int(0, point.y);
            default:
                return point;
        }
    }

    private Vector2Int CreateStartingPointForDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return new Vector2Int(Random.Range(2, mapData.GetSize() - 2), mapData.GetSize() - 1);
            case Direction.Down:
                return new Vector2Int(Random.Range(2, mapData.GetSize() - 2), 0);
            case Direction.Left:
                return new Vector2Int(0, Random.Range(2, mapData.GetSize() - 2));
            case Direction.Right:
                return new Vector2Int(mapData.GetSize() - 1, Random.Range(1, mapData.GetSize() - 2));
            default:
                return Vector2Int.zero;
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 diff = (Camera.current.transform.position - transform.position);
        if (Vector2.SqrMagnitude(new Vector2(diff.x, diff.z)) < 1000.0f)
        {
            Gizmos.color = new Color(0.7f, 0.7f, 1.0f);
            Gizmos.DrawWireCube(transform.position, new Vector3(Settings.ChunkSize * Settings.TileWidth, 0.0f, Settings.ChunkSize * Settings.TileWidth));
        }
    }
}

public class Path
{
    public Vector2Int StartPosition;
    public Vector2Int EndPosition;
    public List<Vector2Int> Positions = new List<Vector2Int>();
    public List<List<Vector2Int>> WidenedPositions = new List<List<Vector2Int>>();

    public Path(Vector2Int startPos, Vector2Int endPos, bool addStartAsOnlyPosition = false)
    {
        StartPosition = startPos;
        EndPosition = endPos;

        if (addStartAsOnlyPosition)
            Positions.Add(startPos);
    }
}
