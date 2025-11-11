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
    public int WellHeight => wellHeight;

    // General map data.
    private MapData mapData;
    private MapData roomData;
    BoxCollider2D[] collisionData;
    private MapRenderer mapRenderer;
    List<int> rightWallPositions = new List<int>();
    List<int> leftWallPositions = new List<int>();
    List<int> platformPositions = new List<int>();
    List<int> mapEdges = new List<int>();
    List<Coord> doorways = new List<Coord>();
    List<(MapData, Coord)> rooms = new List<(MapData, Coord)>();

    [Header("Walker Settings")]
    [SerializeField] private WalkerType walkerPersonality = WalkerType.Random;
    [SerializeField] bool isRandomPersonality = true;
    private int walkerX = 0;

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

        // Generate map and room.
        mapData = new MapData(wellWidth, wellHeight);
        roomData = new MapData(wellWidth, wellHeight);
        // Send that guy down the well.
        RandomWalker walker = new RandomWalker(wellHeight, wellWidth, walkerPersonality, perlinXScale, perlinYScale, isRandomPersonality);
        walker.GenerateWalk(mapData);
        // Fill the well around walker's path.
        FillMap(mapData, walker);
        mapRenderer = GetComponent<MapRenderer>();
        // Render map.
        mapRenderer.RenderMap(mapData.Map);
        // Render rooms where doorways are, offset rendering by map width.
        foreach (var (roomData, doorway) in rooms)
        {
            Vector2 offset;
            if (doorway.tileX == 0)
            {
                offset = new Vector2(-roomData.Width, doorway.tileY);
            }
            else
            {
                offset = new Vector2(mapData.Width, doorway.tileY);
            }
            mapRenderer.RenderMap(roomData.Map, offset);
        }
        // Generate collision data.
        GenerateCollisionData(mapData);
    }

    /// <summary>
    /// Fill the map based on walkers path.
    /// </summary>
    /// <param name="mapData">The map.</param>
    /// <param name="walker">The dude.</param>
    void FillMap(MapData mapData, RandomWalker walker)
    {
        for (int y = 0; y < mapData.Height; y++)
        {
            // Get walkers x pos at this y level.
            walkerX = GetWalkerXPos(y, walker);
            
            // Calculate wall positions based on walker.
            CalculateWallPositions();
            int leftWallPos = leftWallPositions[y];
            int rightWallPos = rightWallPositions[y];

            // Calculate platforms for the walker to poop out.
            platformPositions = CalculatePlatformPositions(y, walkerX);

            for (int x = 0; x < mapData.Width; x++)
            {
                //Debug.Log(leftWallPos);
                // Set wall and empty tiles.
                if (x == leftWallPos) mapData.SetTile(x, y, TileType.Solid);
                else if (x == rightWallPos) mapData.SetTile(x, y, TileType.Solid);
                else if (x > leftWallPos && x < rightWallPos) mapData.SetTile(x, y, TileType.Empty);
                else mapData.SetTile(x, y, TileType.Solid);
            }
            // Poop out those platforms!
            PlacePlatform(leftWallPos, rightWallPos, y);
            // Create doorways at intervals.
            if (y % 25 == 0)
            {
                CalculateDoorway(y);
            }
        }
    }

    /// <summary>
    /// Calculate wall positions based on walkers x position.
    /// </summary>
    void CalculateWallPositions()
    {
        // Calculate wall based on walkers x position.
        int wellWidth = Random.Range(10, 14);
        int leftWallPos = walkerX - wellWidth / 2;
        int rightWallPos = walkerX + wellWidth / 2;
        // Clamp to map bounds.
        leftWallPos = Mathf.Clamp(leftWallPos, 0, mapData.Width - 1);
        rightWallPos = Mathf.Clamp(rightWallPos, 0, mapData.Width - 1);
        if (leftWallPos == 0 || rightWallPos == mapData.Width - 1)
        {
            // If were at the edge, store it. Gonna need this later.
            CalculateEdges(leftWallPos, rightWallPos);
        }
        // Store the wall positions.
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

    /// <summary>
    /// Calculate platform positions based on walkers x position.
    /// </summary>
    /// <param name="y">The walker's current y level.</param>
    /// <param name="walkerX">The walker's x position.</param>
    /// <returns></returns>
    private List<int> CalculatePlatformPositions(int y, int walkerX)
    {
        int platformX = walkerX;
        if (mapData.InBounds(platformX, y))
        {
            platformPositions.Add(platformX);
        }

        return platformPositions;
    }
    /// <summary>
    /// Place platforms between walls at intervals.
    /// </summary>
    /// <param name="leftWallPos">The left wall position.</param>
    /// <param name="rightWallPos">The right wall position.</param>
    /// <param name="y">This y level.</param>
    private void PlacePlatform(int leftWallPos, int rightWallPos, int y)
    {
        int[] platformInterval = { 5, 10 };
        int randomIndex = Random.Range(0, platformInterval.Length);
        if (platformPositions.Contains(walkerX) && y % platformInterval[randomIndex] == 0 
            && walkerX > leftWallPos + 1 && walkerX < rightWallPos - 1)
        {
            // It's ok dude, you can poop now.
            mapData.SetTile(walkerX, y, TileType.Platform);
        }
    }

    /// <summary>
    /// Calculate edges of map for placing doors.
    /// </summary>
    /// <param name="leftWallPos">The left wall position.</param>
    /// <param name="rightWallPos">The right wall position.</param>
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

    /// <summary>
    /// Generate collision data for solid and platform tiles.
    /// </summary>
    /// <param name="mapData"></param>
    private void GenerateCollisionData(MapData mapData)
    {
        // Loop through map renderer dictionary and create colliders for solid and platform tiles.      
        List<BoxCollider2D> colliders = new List<BoxCollider2D>();
        foreach (var mapping in mapRenderer.tileMappings)
        {
            TileType tileType = mapping.tileType;
            if (tileType == TileType.Solid)
            {
                if (mapRenderer.SpawnedTiles.TryGetValue(tileType, out List<GameObject> tiles))
                {
                    foreach (GameObject tileObj in tiles)
                    {
                        tileObj.layer = LayerMask.NameToLayer("Ground");
                        tileObj.AddComponent<BoxCollider2D>();
                    }
                }
            }
            else if (tileType == TileType.Platform)
            {
                if (mapRenderer.SpawnedTiles.TryGetValue(tileType, out List<GameObject> tiles))
                {
                    foreach (GameObject tileObj in tiles)
                    {
                        tileObj.layer = LayerMask.NameToLayer("Ground");
                        BoxCollider2D platformCollider = tileObj.AddComponent<BoxCollider2D>();
                        // Offset the height a little bit for the player.
                        platformCollider.size = new Vector2(platformCollider.size.x, platformCollider.size.y * 0.5f);
                        platformCollider.offset = new Vector2(platformCollider.offset.x, platformCollider.offset.y - platformCollider.size.y * 0.50f);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Calculate doorway at given y level.
    /// </summary>
    /// <param name="y">The y level to place a door.</param>
    private void CalculateDoorway(int y)
    {
        int doorHeight = Random.Range(3, 6);
        foreach (int edge in mapEdges)
        {
            int nx = edge + (edge == 0 ? 1 : -1);

            // Check if theres space for daorway.
            if (mapData.GetTile(nx, y + 1) == TileType.Empty &&
                mapData.GetTile(nx, y) == TileType.Empty &&
                mapData.GetTile(nx, y - 1) == TileType.Empty)
            {
                for (int i = 0; i < doorHeight; i++)
                {
                    // Generate doorway based on height.
                    mapData.SetTile(edge, y - i, TileType.Empty);
                    doorways.Add(new Coord(edge, y - i));
                }
                // Generate room based on doorway.
                int roomWidth = mapData.Width;
                MapData room = new MapData(roomWidth, doorHeight + 2);
                bool isLeftEdge = (edge == 0);
                CalculateRoom(room, doorHeight, isLeftEdge);

                // Store room with coordinate to offset in rendering.
                rooms.Add((room, new Coord(edge, y - doorHeight)));
            }          
        }
    }

    /// <summary>
    /// Calculate room layout based on door.
    /// </summary>
    /// <param name="roomData">The room data to </param>
    /// <param name="doorHeight">The height of doorway.</param>
    /// <param name="isLeftEdge">If it's a left edge, than it's not not left.</param>
    private void CalculateRoom(MapData roomData, int doorHeight, bool isLeftEdge)
    {
        // Center of the door.
        int doorStart = (roomData.Height - doorHeight) / 2;
        // Bottom of door.
        int doorEnd = doorStart + doorHeight;

        for (int y = 0; y < roomData.Height; y++)
        {
            for (int x = 0; x < roomData.Width; x++)
            {
                // Top of room.
                bool topWall = (y == 0);               
                // Bottom of room.
                bool bottomWall = (y == roomData.Height - 1);             
                // Back wall of room.
                bool backWall = isLeftEdge ? (x == 0) : (x == roomData.Width - 1);
                // Doorway side of room, depending on which edge of map its on.
                bool doorwaySide = isLeftEdge ? (x == roomData.Width - 1) : (x == 0);
                // Doorway area which should be empty.
                bool isDoorway = doorwaySide && (y >= doorStart && y < doorEnd);

                // Walls around room except for doorway, obviously. U.U
                // If someone's reading this, this project was a pain in my bottomWall.
                bool isWall = (topWall || bottomWall || backWall || doorwaySide) && !isDoorway;
                {
                    // Set tiles based on walls.
                    roomData.SetTile(x, y, isWall ? TileType.Solid : TileType.Empty);
                }
            }
        }
    }
    /// <summary>
    /// Responsible for clearing the map data and tiles in scene.
    /// </summary>
    private void ClearMap()
    {
        // Map go poof now.
        mapData.Clear();
        roomData.Clear();
        rightWallPositions.Clear();
        leftWallPositions.Clear();
        platformPositions.Clear();
        mapEdges.Clear();
        mapRenderer.SpawnedTiles.Clear();
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

        /// <summary>
        /// Now that's a big dirty constructor right there. Guess this guy needs some info.
        /// </summary>
        /// <param name="length">Length of walk.</param>
        /// <param name="width">Width of map, use for center calculation.</param>
        /// <param name="personality">Gotta have some personality here.</param>
        /// <param name="perlinX">X value for perlin noise.</param>
        /// <param name="perlinY">Y value for perlin noise.</param>
        /// <param name="isRandom">Whether or not the personality is spur of the moment.</param>
        public RandomWalker(int length, int width, WalkerType personality, float perlinX, float perlinY, bool isRandom)
        {
            // Length of the walk.
            walkLength = length;
            // Center of map for walker to start at.
            center = width / 2;
            // Personality of the walker, because walks are boring.
            walkerPersonality = personality;
            perlinXScale = perlinX;
            perlinYScale = perlinY;
            isRandomPersonality = isRandom;
        }

        /// <summary>
        /// Generate's the walk down well. Not the game downwell though, I swear that's different.
        /// </summary>
        /// <param name="mapData">Map data, just cause I like parameters.</param>
        public void GenerateWalk(MapData mapData)
        {
            for (int i = 0; i < walkLength; i++)
            { 
                int x = center;
                int randomDirection = Random.Range(1, 3);

                // Give walker a memory to avoid consistent move cycles.
                if (randomDirection == lastDirection)
                {
                    consecutiveMoves++;
                }
                else
                {
                    consecutiveMoves = 0;
                }
                if (consecutiveMoves > 2)
                {
                    randomDirection = (randomDirection == 1) ? 2 : 1;
                    consecutiveMoves = 0;
                }
                lastDirection = randomDirection;

                // If random personality is toggled, pick a personality at each step.
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
                    // Reset center for next iteration.
                    center = x;
                }
            }
        }
    }

    public enum WalkerType { Random, Drunkard, ZigZag, PerlinMerlin }

    /// <summary>
    /// Coord struct for storing tile coordinates. Not reinventing the wheel here.
    /// </summary>
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

    /// <summary>
    /// Class for storing map data, width height and tile types. Pretty boring stuff in here.
    /// </summary>
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

        /// <summary>
        /// Get's a tile at given coordinates.
        /// </summary>
        /// <param name="x">I'm an X!</param>
        /// <param name="y">I'm a Y!</param>
        /// <returns></returns>
        public TileType GetTile(int x, int y)
        {
            // Return tile type at coordinates.
            if (InBounds(x, y))
            {
                return map[y, x];
            }
            // Fallback for out of bounds.
            return TileType.Empty; 
        }

        /// <summary>
        /// Sets tile at given coordinates.
        /// </summary>
        /// <param name="x">I'm also x!</param>
        /// <param name="y">I'm also y!</param>
        /// <param name="tileType"></param>
        public void SetTile(int x, int y, TileType tileType)
        {
            if (InBounds(x, y))
            {
                map[y, x] = tileType;
            }
        }
        /// <summary>
        /// Boundary check.
        /// </summary>
        /// <param name="x">Can you guess what I am?</param>
        /// <param name="y">;O</param>
        /// <returns></returns>
        public bool InBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        /// <summary>
        /// Clears the map tiles.
        /// </summary>
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
/// <summary>
/// Tile types to use in my dictionary. Yawn.
/// </summary>
public enum TileType
{
    Empty,
    Solid,
    Platform
}
