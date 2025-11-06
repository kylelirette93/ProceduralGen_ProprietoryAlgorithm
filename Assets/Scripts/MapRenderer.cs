using System;
using System.Collections.Generic;
using UnityEngine;

public class MapRenderer : MonoBehaviour
{
    /// <summary>
    /// Tile Mapping struct for associating type of tile with prefab.
    /// </summary>
    [Serializable]
    public struct TileMapping
    {
        public TileType tileType;
        public GameObject prefab;
    }
    public List<TileMapping> tileMappings;
    public Dictionary<TileType, GameObject> tilePrefabs;

    private void Awake()
    {
        tilePrefabs = new Dictionary<TileType, GameObject>();
        foreach (var mapping in tileMappings)
        {
            tilePrefabs[mapping.tileType] = mapping.prefab;
        }
    }

    public void RenderMap(TileType[,] map)
    {
        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                Vector3 pos = new Vector3(-map.GetLength(1) / 2 + x, -y);
                TileType tile = map[y, x];
                if (tilePrefabs.ContainsKey(tile))
                {
                    Instantiate(tilePrefabs[tile], pos, Quaternion.identity, transform);
                }
            }
        }
    }
}
