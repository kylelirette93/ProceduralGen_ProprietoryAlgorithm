using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    // Well height and width.
    int wellWidth = 20;
    int wellHeight = 100;
    int[,] map;
    const int walkLength = 20;
    List<Coord> walkPoints = new List<Coord>();
    

    private void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        // Determine random seed.
        int seedX = Random.Range(10, 20);
        map = new int[wellHeight, wellWidth];
        FillMap();
        RandomWalk();
        GeneratePlatforms();
        MapRenderer mapRenderer = GetComponent<MapRenderer>();
        mapRenderer.RenderMap(map);
    }

    void FillMap()
    {
        for (int y = 0; y < wellHeight; y++)
        {
            for (int x = 0; x < wellWidth; x++)
            {
                if (y == 0 && x == 0)
                {
                    // Top left corner.
                    map[y, x] = 1;
                }
                else if (y == 0 && x == wellWidth - 1)
                {
                    // Create top right corner.
                    map[y, x] = 2;
                }
                else if (x == 0 && y == wellHeight - 1)
                {
                    // Bottom left corner.
                    map[y, x] = 3;
                }
                else if (x == wellWidth - 1 && y == wellHeight - 1)
                {
                    // Create bottom right corner.
                    map[y, x] = 4;
                }
                else if (x == 0)
                {
                    // Create left wall.
                    map[y, x] = 5;
                }
                else if (x == wellWidth - 1)
                {
                    // Create right wall.
                    map[y, x] = 6;
                }
                else
                {
                    // Create empty space.
                    map[y, x] = 0;
                }
            }
        }
    }

    void RandomWalk()
    {
        int currentX = wellWidth / 2;
        int currentY = 0;

        while (currentY < walkLength)
        {
            int direction = Random.Range(0, 3); 
            if (direction == 0 && currentX > 1)
            {
                currentX--;
            }
            else if (direction == 1 && currentX < wellWidth - 2)
            {
                currentX++;
            }
            else if (direction == 2)
            {
                currentY++;
            }
            walkPoints.Add(new Coord(currentX, currentY));
        }
    }

    public void GeneratePlatforms()
    {
        // Get center of well for comparisons.
        int center = wellWidth / 2;
        foreach (var Coord in walkPoints)
        {
            if (Coord.tileX > center)
            {
                DrawPlatformToRightWall(Coord.tileX, center);
            }
        }
    }

    public void DrawPlatformToRightWall(int coordX, int center) 
    {
        while (coordX < wellWidth - 1)
        {
            // Draw platform tile.
        }
    }
    struct Coord 
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }
}
