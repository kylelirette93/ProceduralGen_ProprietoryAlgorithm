using UnityEngine;

public class MapRenderer : MonoBehaviour
{
    public GameObject topLeftCornerTile;
    public GameObject topRightCornerTile;
    public GameObject bottomLeftCornerTile;
    public GameObject bottomRightCornerTile;
    public GameObject rightWallTile;
    public GameObject leftWallTile;
    public GameObject emptyTile;

    public void RenderMap(int[,] map)
    {
        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                // Flip when rendering to match coordinates.
                Vector3 position = new Vector3(-map.GetLength(1) / 2 + x, -y);

                if (map[y, x] == 1)
                {
                    GameObject newWallTile = Instantiate(topLeftCornerTile, position, Quaternion.identity);
                }
                else if (map[y, x] == 2)
                {
                    GameObject newWallTile = Instantiate(topRightCornerTile, position, Quaternion.identity);
                }
                else if (map[y, x] == 3)
                {
                    GameObject newWallTile = Instantiate(bottomLeftCornerTile, position, Quaternion.identity);
                }
                else if (map[y, x] == 4)
                {
                    GameObject newWallTile = Instantiate(bottomRightCornerTile, position, Quaternion.identity);
                }
                else if (map[y, x] == 5)
                {
                    GameObject newWallTile = Instantiate(leftWallTile, position, Quaternion.identity);
                }
                else if (map[y, x] == 6)
                {
                    GameObject newWallTile = Instantiate(rightWallTile, position, Quaternion.identity);
                }
                else
                {
                    GameObject newEmptyTile = Instantiate(emptyTile, position, Quaternion.identity);
                }
            }
        }
    }
}
