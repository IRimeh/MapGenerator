using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField]
    private Tile[] TilePrefabs;
    [SerializeField]
    private List<TileData> Tiles = new List<TileData>();

    [SerializeField]
    private bool Test = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void LoadTiles()
    {
        Tiles.Clear();
        TilePrefabs = Resources.LoadAll<Tile>("Tiles/");
        for (int i = 0; i < TilePrefabs.Length; i++)
        {
            Tiles.Add(TilePrefabs[i].data);
        }
        Debug.Log("Test Loaded " + Tiles.Count + " Tiles.");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnValidate()
    {
        if(Test)
        {
            LoadTiles();
            Test = false;
        }
    }
}
