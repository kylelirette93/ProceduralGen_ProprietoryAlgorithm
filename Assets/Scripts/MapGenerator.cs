using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    int mapWidth = 10;
    int mapHeight = 10;
    int[,] map;

    private void Start()
    {
        InitializeMap();
    }

    void InitializeMap()
    {
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                
            }
        }
    }
}
