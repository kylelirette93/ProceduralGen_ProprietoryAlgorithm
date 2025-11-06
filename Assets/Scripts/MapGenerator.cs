using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class MapGenerator : MonoBehaviour
{
    int wellWidth = 20;
    int wellHeight = 100;

    private MapData mapData;
    private MapData roomData;
    private MapRenderer mapRenderer;
    List<int> rightWallPositions = new List<int>();
    List<int> leftWallPositions = new List<int>();
    
    private void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        mapData = new MapData(wellWidth, wellHeight);
        RandomWalker walker = new RandomWalker(wellHeight, wellWidth);
        walker.GenerateWalk(mapData);
        FillMap(mapData, walker);
        mapRenderer = GetComponent<MapRenderer>();
        mapRenderer.RenderMap(mapData.Map);
    }

    void FillMap(MapData mapData, RandomWalker walker)
    {
        for (int y = 0; y < mapData.Height; y++)
        {
            // Get walker's x position so I can build walls around it.
            int walkerX = 0;
            foreach (Coord coord in walker.walkPoints)
            {
                // Find walkers x pos at this y level.
                if (coord.tileY == y)
                {
                    walkerX = coord.tileX;
                }
            }
            // Vary the well width and set wall position.
            int wellWidth = Random.Range(10, 14);
            int leftWallPos = walkerX - wellWidth / 2;
            int rightWallPos = walkerX + wellWidth / 2;
            leftWallPos = Mathf.Clamp(leftWallPos, 1, mapData.Width - 2);
            rightWallPos = Mathf.Clamp(rightWallPos, 1, mapData.Width - 2);
            leftWallPositions.Add(leftWallPos);
            rightWallPositions.Add(rightWallPos);

            for (int x = 0; x < mapData.Width; x++)
            {
                Debug.Log(leftWallPos);
                if (x == leftWallPos) mapData.SetTile(x, y, TileType.Solid);
                else if (x == rightWallPos) mapData.SetTile(x, y, TileType.Solid);
                else if (x > leftWallPos && x < rightWallPos) mapData.SetTile(x, y, TileType.Empty);
                else mapData.SetTile(x, y, TileType.Solid);
                AddDoorways(x, y);
            }
        }
    }

    void AddDoorways(int gridX, int gridY)
    {
        List<Coord> doorPositions = new List<Coord>();

        for (int y = 0; y < mapData.Height; y++)
        {
            if (y % 10 == 0 && gridX == 0 || gridX == mapData.Width)
            {
                int leftWallX = leftWallPositions[y];
                int rightWallX = rightWallPositions[y];
                // Add door on left side.
                
                int leftDoorX = leftWallX;
                mapData.SetTile(leftDoorX, y, TileType.Empty);
                // Add door on right side.
                int rightDoorX = rightWallX;
                mapData.SetTile(rightDoorX, y, TileType.Empty);
            }
        }
    }

    /// <summary>
    /// Random Walker class is responsible for generating a path down the well.
    /// </summary>
    public class RandomWalker
    {
        public List<Coord> walkPoints = new List<Coord>();
        private int walkLength;
        private int center;
        public RandomWalker(int length, int width)
        {
            // Length of the walk.
            walkLength = length;
            // Center of map for walker to start at.
            center = width / 2;
        }

        public void GenerateWalk(MapData mapData)
        {
            for (int i = 0; i < walkLength; i++)
            { 
                int x = center;
                int randomDirection = Random.Range(1, 3);
                if (mapData.InBounds(x, i))
                {
                    if (randomDirection == 1) x--;
                    else if (randomDirection == 2)
                    {
                        x++;
                    }
                    x = Mathf.Clamp(x, 1, mapData.Width - 2);
                    Debug.Log("Walker at: " + x + ", " + i);
                    walkPoints.Add(new Coord(x, i));
                    center = x;
                }
            }
        }

        public List<Coord> GetWalkPoints()
        {
            // Get the path of the walker.
            return walkPoints;
        }
    }

    public struct Coord
    {
        public int tileX;
        public int tileY;
        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }

    public class MapData
    {
        public TileType[,] Map => map;
        private TileType[,] map;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public MapData(int width, int height)
        {
            Width = width;
            Height = height;
            map = new TileType[height, width];
        }

        public TileType GetTile(int x, int y)
        {
            // Return tile type at coordinates.
            if (InBounds(x, y))
            {
                return map[y, x];
            }
            return TileType.Empty; // Fallback for out of bounds.
        }

        public void SetTile(int x, int y, TileType tileType)
        {
            if (InBounds(x, y))
            {
                map[y, x] = tileType;
            }
        }
        public bool InBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public void Clear()
        {
            // Clears the map when I need to.
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    map[y, x] = TileType.Empty;
                }
            }
        }
    }
}
public enum TileType
{
    Empty,
    Solid
}
