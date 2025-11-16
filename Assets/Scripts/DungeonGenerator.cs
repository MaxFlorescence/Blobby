using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    private int scale = 10;

    private int[,] tileMap = {
        {5, 0}, // 0000
        {4, 3}, // 0001
        {4, 2}, // 0010
        {2, 3}, // 0011
        {4, 1}, // 0100
        {3, 0}, // 0101
        {2, 2}, // 0110
        {1, 2}, // 0111
        {4, 0}, // 1000
        {2, 0}, // 1001
        {3, 1}, // 1010
        {1, 3}, // 1011
        {2, 1}, // 1100
        {1, 0}, // 1101
        {1, 1}  // 1110
        // 1111 corresponds with no tile
    };
    private GameObject[] tilePrefabs = new GameObject[6];

    private const int HEIGHT = 4;
    private const int WIDTH = 4;
    private byte[,] layout = {
        {0b_1011_1000, 0b_1110_1111},
        {0b_1001_0000, 0b_1000_1100},
        {0b_0101_0111, 0b_0101_0101},
        {0b_0011_1010, 0b_0110_0111}
    };

    private void LoadTilePrefabs()
    {
        tilePrefabs[0] = null;
        tilePrefabs[1] = Resources.Load("DungeonPrefabs/Corridors/DungeonDeadEnd",  typeof(GameObject)) as GameObject;
        tilePrefabs[2] = Resources.Load("DungeonPrefabs/Corridors/DungeonCorner",   typeof(GameObject)) as GameObject;
        tilePrefabs[3] = Resources.Load("DungeonPrefabs/Corridors/DungeonHallway",  typeof(GameObject)) as GameObject;
        tilePrefabs[4] = Resources.Load("DungeonPrefabs/Corridors/DungeonJunction", typeof(GameObject)) as GameObject;
        tilePrefabs[5] = Resources.Load("DungeonPrefabs/Corridors/DungeonCrossing", typeof(GameObject)) as GameObject;
    }

    void Start()
    {
        LoadTilePrefabs();

        for (int i = 0; i < HEIGHT; i++)
        {
            for (int j = 0; j < WIDTH/2; j++)
            {
                byte tiles = layout[i, j];

                for (int k = 0; k < 2; k++)
                {
                    int t = (k==0) ? (tiles >> 4) : (tiles & 0b1111);
                    if (t == 0b1111) continue;
                    int l = 2 * j + k;
                    
                    int tileIndex = tileMap[t, 0];
                    int tileRotation = tileMap[t, 1]*90;
                    Vector3 tilePosition = new Vector3(-scale * l, 0, scale * i);

                    GameObject Tile = Instantiate(tilePrefabs[tileIndex]);
                    Tile.name = "Dungeon Tile " + i.ToString() + "," + l.ToString();
                    Tile.transform.SetPositionAndRotation(
                        tilePosition, Quaternion.Euler(0, tileRotation, 0)
                    );
                    Tile.transform.parent = transform;
                }
            }
        }
    }
}
