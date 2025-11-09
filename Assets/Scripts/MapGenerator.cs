using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class MapGenerator : MonoBehaviour
{
    [Header("Well Settings")]
    [Range(10, 20)]
    [SerializeField] int wellWidth = 20;
    [Range(50, 2000)]
    [SerializeField] int wellHeight = 50;

    int walkerX = 0;

    private MapData mapData;
    BoxCollider2D[] collisionData;
    private MapData roomData;
    private MapRenderer mapRenderer;
    List<int> rightWallPositions = new List<int>();
    List<int> leftWallPositions = new List<int>();
    List<int> platformPositions = new List<int>();
    List<int> mapEdges = new List<int>();

    [Header("Walker Settings")]
    [SerializeField] private WalkerType walkerPersonality = WalkerType.Random;
    [SerializeField] bool isRandomPersonality = true;

    [Header("Noise Settings")]
    [Range(0.1f, 1f)]
    [SerializeField] private float perlinXScale = 0.1f;
    [Range(0.1f, 1f)]
    [SerializeField] private float perlinYScale = 0.2f;



    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        if (mapData != null) ClearMap();

        mapData = new MapData(wellWidth, wellHeight);
        roomData = new MapData(wellWidth + wellWidth / 2, wellHeight + wellHeight / 4);
        RandomWalker walker = new RandomWalker(wellHeight, wellWidth, walkerPersonality, perlinXScale, perlinYScale, isRandomPersonality);
        walker.GenerateWalk(mapData);
        FillMap(mapData, walker);
        mapRenderer = GetComponent<MapRenderer>();
        mapRenderer.RenderMap(mapData.Map);
        GenerateCollisionData(mapData);
    }

    void FillMap(MapData mapData, RandomWalker walker)
    {
        for (int y = 0; y < mapData.Height; y++)
        {
            walkerX = GetWalkerXPos(y, walker);
           
            CalculateWallPositions();
            int leftWallPos = leftWallPositions[y];
            int rightWallPos = rightWallPositions[y];
            platformPositions = CalculatePlatformPositions(y, walkerX);

            for (int x = 0; x < mapData.Width; x++)
            {
                Debug.Log(leftWallPos);
                if (x == leftWallPos) mapData.SetTile(x, y, TileType.Solid);
                else if (x == rightWallPos) mapData.SetTile(x, y, TileType.Solid);
                else if (x > leftWallPos && x < rightWallPos) mapData.SetTile(x, y, TileType.Empty);
                else mapData.SetTile(x, y, TileType.Solid);
            }
            PlacePlatform(leftWallPos, rightWallPos, y);
            //CalculateDoorway(y);
        }
    }

    void CalculateWallPositions()
    {
        int wellWidth = Random.Range(10, 14);
        int leftWallPos = walkerX - wellWidth / 2;
        int rightWallPos = walkerX + wellWidth / 2;
        leftWallPos = Mathf.Clamp(leftWallPos, 0, mapData.Width - 1);
        rightWallPos = Mathf.Clamp(rightWallPos, 0, mapData.Width - 1);
        if (leftWallPos == 0 || rightWallPos == mapData.Width - 1)
        {
            // If were at the edge, store it.
            CalculateEdges(leftWallPos, rightWallPos);
        }
        leftWallPositions.Add(leftWallPos);
        rightWallPositions.Add(rightWallPos);
    }

    /// <summary>
    /// Get's the walkers x position at a given y level.
    /// </summary>
    /// <param name="y">The walker's y level, while descending.</param>
    /// <param name="walker">The walker is passed in, to loop through it's walk points.</param>
    /// <returns></returns>
    private int GetWalkerXPos(int y, RandomWalker walker)
    {
        // Find walker's x pos at this y level.
        foreach (Coord coord in walker.walkPoints)
        {
            if (coord.tileY == y)
            {
                return coord.tileX;
            }
        }
        return 0; 
    }

    private List<int> CalculatePlatformPositions(int y, int walkerX)
    {
        int platformX = walkerX;
        if (mapData.InBounds(platformX, y))
        {
            platformPositions.Add(platformX);
        }

        return platformPositions;
    }

    private void PlacePlatform(int leftWallPos, int rightWallPos, int y)
    {
        int[] choices = new int[] { 5, 10 };
        int randomIndex = Random.Range(0, choices.Length);
        if (platformPositions.Contains(walkerX) && y % choices[randomIndex] == 0 && walkerX > leftWallPos + 1 && walkerX < rightWallPos - 1)
        {
            mapData.SetTile(walkerX, y, TileType.Platform);
        }
    }

    private void CalculateEdges(int leftWallPos, int rightWallPos)
    {
        if (leftWallPos == 0)
        {
            mapEdges.Add(leftWallPos);
        }

        if (rightWallPos == mapData.Width - 1)
        {
            mapEdges.Add(rightWallPos);
        }
    }

    private void GenerateCollisionData(MapData mapData)
    {
        for (int y = 0; y < mapData.Height; y++)
        {
            for (int x = 0; x < mapData.Width; x++)
            {
                TileType tile = mapData.GetTile(x, y);
                if (tile == TileType.Solid)
                {
                    GameObject colliderObj = new GameObject("Collider_" + x + "_" + y);
                    colliderObj.transform.gameObject.layer = LayerMask.NameToLayer("Ground");
                    colliderObj.transform.position = new Vector3(-mapData.Width / 2 + x, -y);
                    colliderObj.transform.parent = this.transform;
                    BoxCollider2D boxCollider = colliderObj.AddComponent<BoxCollider2D>();
                    boxCollider.size = new Vector2(1, 1);
                }
                else if (tile == TileType.Platform)
                {
                    GameObject colliderObj = new GameObject("Collider_" + x + "_" + y);
                    // Set layer mask.
                    colliderObj.transform.gameObject.layer = LayerMask.NameToLayer("Ground");
                    // Offset for platform tile collider, idk it just works.
                    colliderObj.transform.position = new Vector3(-mapData.Width / 2 + x, -y + -0.25f);
                    colliderObj.transform.parent = this.transform;
                    BoxCollider2D boxCollider = colliderObj.AddComponent<BoxCollider2D>();
                    boxCollider.size = new Vector2(1, 0.5f);
                }
            }
        }
    }

    private void ClearMap()
    {
        mapData.Clear();
        rightWallPositions.Clear();
        leftWallPositions.Clear();
        platformPositions.Clear();
        mapEdges.Clear();
        // Destroy existing tiles in the scene.
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Random Walker class is responsible for generating a path down the well.
    /// </summary>
    public class RandomWalker
    {
        public List<Coord> walkPoints = new List<Coord>();
        private int center;
        private int lastDirection = 0;
        private int consecutiveMoves = 0;
        int walkLength;
        private WalkerType walkerPersonality;

        float perlinXScale = 0.1f;
        float perlinYScale = 0.2f;
        bool isRandomPersonality = true;

        public RandomWalker(int length, int width, WalkerType personality, float perlinX, float perlinY, bool isRandom)
        {
            // Length of the walk.
            walkLength = length;
            // Center of map for walker to start at.
            center = width / 2;
            // Personality of the walker.
            walkerPersonality = personality;
            perlinXScale = perlinX;
            perlinYScale = perlinY;
            isRandomPersonality = isRandom;
        }

        public void GenerateWalk(MapData mapData)
        {
            for (int i = 0; i < walkLength; i++)
            { 
                int x = center;
                int randomDirection = Random.Range(1, 3);

                // Give walker a memory.
                if (randomDirection == lastDirection) consecutiveMoves++;
                else consecutiveMoves = 0;
                if (consecutiveMoves > 2)
                {
                    randomDirection = (randomDirection == 1) ? 2 : 1;
                    consecutiveMoves = 0;
                }
                lastDirection = randomDirection;

                if (isRandomPersonality)
                {
                    walkerPersonality = (WalkerType)Random.Range(0, 4);
                }
                if (mapData.InBounds(x, i))
                {
                    switch (walkerPersonality)
                    {
                        case WalkerType.Random:
                            if (randomDirection == 1) x--;
                            else if (randomDirection == 2) x++;
                            break;
                        case WalkerType.Drunkard:
                            if (randomDirection == 1) x -= Random.Range(1, 3);
                            else if (randomDirection == 2) x += Random.Range(1, 3);
                            break;
                        case WalkerType.ZigZag:
                            if (i % 4 < 2) x--;
                            else x++;
                            break;
                        case WalkerType.PerlinMerlin:
                            float perlinNoise = Mathf.PerlinNoise(i * perlinXScale, perlinYScale);
                            if (perlinNoise < 0.4f) x--;
                            else if (perlinNoise > 0.6f) x++;
                            break;
                    }
                   
                    // Clamp walker within bounds.
                    x = Mathf.Clamp(x, 1, mapData.Width - 2);
                    //Debug.Log("Walker at: " + x + ", " + i);
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

    public enum WalkerType { Random, Drunkard, ZigZag, PerlinMerlin }

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
    Solid,
    Platform
}
