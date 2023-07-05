using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{

    public Transform spawn, end;
    public Transform origin;

    public GridCellObject[] gridCells;

    public GridCellObject[] sceneryObject;

    public List<Transform> path;

    public TileType[,] tileMap;

    private List<Vector2Int> pathCells = new List<Vector2Int>();

    // public static int randomSeed;

    private int NCROSSROADS = 3;
    private int MAX_PATH_LENGTH = 500;

    [SerializeField] LocalChannelSO localChannel;


    void Start()
    {
        localChannel.OnGenerateGrid += GenerateGrid;
        // Seed can be shared between players to generate the same map.
        // if (randomSeed == 0) randomSeed = Random.Range(int.MinValue, int.MaxValue);
        // GenerateGrid(randomSeed);
    }

    void GenerateGrid(int seed)
    {
        Random.InitState(seed);

        tileMap = new TileType[15, 15];  // TODO: not dynamic

        // Fill with grass
        for (int y = 0; y < tileMap.GetLength(0); y++)
            for (int x = 0; x < tileMap.GetLength(1); x++)
                tileMap[x, y] = TileType.GRASS;

        pathCells = GeneratePath();

        for (int i = 0; i < NCROSSROADS; i++)
            GenerateCrossroads();

        GenerateWaypoints();
        LayPathCells();
        LaySceneryCells();

    }

    private void GenerateWaypoints()
    {
        path.Add(end);
        foreach (Vector2Int pathCell in pathCells)
        {
            GameObject temp = new GameObject("Waypoint");
            Vector2Int world = GridToWorld(pathCell);
            temp.transform.position = new Vector3(world.x, 0.3f, world.y);
            temp.transform.SetParent(this.transform, false);
            path.Add(temp.transform);
        }
        path.Add(spawn);
        path.Reverse();
    }

    public Vector2Int GridToWorld(Vector2Int grid)
    {
        Vector2Int world = new Vector2Int(
            grid.x - (((int)origin.localScale.x - 1) / 2),
            grid.y - (((int)origin.localScale.z - 1) / 2)
        );
        return world;
    }

    public Vector2Int WorldToGrid(Vector2Int world)
    {
        Vector2Int grid = new Vector2Int(
            world.x + (((int)origin.localScale.x - 1) / 2),
            world.y + (((int)origin.localScale.z - 1) / 2)
        );
        return grid;
    }


    bool CrossroadCheck(int x, int y, int loopId)
    {

        int xmul = loopId % 2 == 0 ? 1 : -1;
        int ymul = loopId < 2 ? 1 : -1;
        bool isOk = (
                cellIsFree(x, y + 3 * ymul) &&
                cellIsFree(x - 1 * xmul, y + 3 * ymul) &&
                cellIsFree(x - 2 * xmul, y + 3 * ymul) &&

                cellIsFree(x + 1 * xmul, y + 2 * ymul) &&
                cellIsFree(x, y + 2 * ymul) &&
                cellIsFree(x - 1 * xmul, y + 2 * ymul) &&
                cellIsFree(x - 2 * xmul, y + 2 * ymul) &&
                cellIsFree(x - 3 * xmul, y + 2 * ymul) &&

                cellIsFree(x + 1 * xmul, y + 1 * ymul) &&
                cellIsFree(x, y + 1 * ymul) &&
                cellIsFree(x - 1 * xmul, y + 1 * ymul) &&
                cellIsFree(x - 2 * xmul, y + 1 * ymul) &&
                cellIsFree(x - 3 * xmul, y + 1 * ymul) &&

                cellIsFree(x - 1 * xmul, y) &&
                cellIsFree(x - 2 * xmul, y) &&
                cellIsFree(x - 3 * xmul, y) &&

                cellIsFree(x - 1 * xmul, y - 1 * ymul) &&
                cellIsFree(x - 2 * xmul, y - 1 * ymul)
            );

        return isOk;
    }

    List<Vector2Int> Crossroad(Vector2Int start, int loopId)
    {
        int xmul = loopId % 2 == 0 ? 1 : -1;
        int ymul = loopId < 2 ? 1 : -1;
        return new List<Vector2Int> {
            new Vector2Int(start.x, start.y + 1*ymul),
            new Vector2Int(start.x ,start.y + 2*ymul),

            new Vector2Int(start.x - 1*xmul, start.y + 2*ymul),
            new Vector2Int(start.x - 2*xmul, start.y + 2*ymul),

            new Vector2Int(start.x - 2*xmul, start.y + 1*ymul),
            new Vector2Int(start.x - 2*xmul, start.y),

            new Vector2Int(start.x - 1*xmul, start.y),

            new Vector2Int(start.x, start.y)
        };
    }

    public bool GenerateCrossroads()
    {
        // Array to store indexes
        int[] indexs = new int[pathCells.Count];

        // Fill array with 0..n
        for (int i = 0; i < indexs.Length; i++) indexs[i] = i;

        // Shuffle array
        for (int i = 0; i < indexs.Length - 1; i++)
        {
            int rnd = Random.Range(i, indexs.Length);
            int temp = indexs[rnd];
            indexs[rnd] = indexs[i];
            indexs[i] = temp;
        }

        foreach (int i in indexs)
        {
            Vector2Int pathCell = pathCells[i];
            for (int loopId = 0; loopId < 4; loopId++)
            {
                if (CrossroadCheck(pathCell.x, pathCell.y, loopId))
                {
                    List<Vector2Int> tiles = Crossroad(pathCell, loopId);
                    foreach (Vector2Int tile in tiles)
                    {
                        tileMap[tile.x, tile.y] = TileType.PATH;
                    }

                    pathCells.InsertRange(i + 1, tiles);
                    return true;
                }
            }

        }
        return false;
    }


    public void LayPathCells()
    {

        foreach (Vector2Int pathCell in pathCells)
        {
            int neighbourValue = getNeighbourValues(pathCell.x, pathCell.y);
            Vector2Int world = GridToWorld(pathCell);
            GameObject cellPrefab = gridCells[neighbourValue].cellPrefab;
            GameObject test = Instantiate(cellPrefab, new Vector3(world.x, 0.051f, world.y), Quaternion.identity);
            test.transform.Rotate(0f, gridCells[neighbourValue].yRotation, 0f);
            test.transform.SetParent(this.transform, false);

        }
    }

    public void LaySceneryCells()
    {
        for (int y = 0; y < tileMap.GetLength(0); y++)
        {
            for (int x = 0; x < tileMap.GetLength(1); x++)
            {
                if (cellIsFree(x, y))
                {
                    int random = Random.Range(0, 100);
                    if (random < 33)
                    {
                        Vector2Int world = GridToWorld(new Vector2Int(x, y));
                        GameObject temp = Instantiate(
                            sceneryObject[Random.Range(1, sceneryObject.Length)].cellPrefab,
                            new Vector3(world.x, 0, world.y),
                            Quaternion.identity
                        );
                        tileMap[x, y] = TileType.OBJECT;
                        temp.transform.SetParent(this.transform, false);
                    }
                }
            }
        }

    }

    public List<Vector2Int> GeneratePath()
    {
        Vector2Int endPos = WorldToGrid(new Vector2Int((int)end.transform.localPosition.x, (int)end.transform.localPosition.z));
        tileMap[endPos.x, endPos.y] = TileType.END;

        // Path start
        int x = endPos.x;
        int y = endPos.y + 1;
        pathCells.Add(new Vector2Int(x, y));
        tileMap[x, y] = TileType.PATH;

        for (int i = 0; i < MAX_PATH_LENGTH; i++)
        {
            int move = Random.Range(0, 4);

            // up
            if (move == 0 && cellIsFree(x, y + 1) && cellIsFree(x, y + 2) && cellIsFree(x - 1, y + 1) && cellIsFree(x + 1, y + 1))
            {
                y++;
            }
            // right
            else if (move == 1 && cellIsFree(x + 1, y) && cellIsFree(x + 2, y) && cellIsFree(x + 1, y - 1) && cellIsFree(x + 1, y + 1))
            {
                x++;
            }
            // left
            else if (move == 2 && cellIsFree(x - 1, y) && cellIsFree(x - 2, y) && cellIsFree(x - 1, y + 1) && cellIsFree(x - 1, y - 1))
            {
                x--;
            }
            // down
            else if (move == 3 && cellIsFree(x, y - 1) && cellIsFree(x, y - 2) && cellIsFree(x + 1, y - 1) && cellIsFree(x - 1, y - 1))
            {
                y--;
            }
            else
            {
                continue;
            }

            pathCells.Add(new Vector2Int(x, y));
            tileMap[x, y] = TileType.PATH;
        }

        // Replace last tile with spawn-tile.
        pathCells.RemoveAt(pathCells.Count - 1);

        Vector2Int prevCell = pathCells[^1];
        int dx = x - prevCell.x;
        int dy = y - prevCell.y;
        placeAndRotateSpawn(x, y, dx, dy);


        return pathCells;

    }

    private bool cellIsFree(int x, int y)
    {
        if (x < 0 | x >= 15) return false;
        if (y < 0 | y >= 15) return false;
        return tileMap[x, y] == TileType.GRASS;
    }


    private void placeAndRotateSpawn(int x, int y, int dx, int dy)
    {
        Vector2Int worldPos = GridToWorld(new Vector2Int(x, y));
        spawn.transform.localPosition = new Vector3(worldPos.x, 0.2f, worldPos.y);
        tileMap[x, y] = TileType.SPAWN;

        int rot;
        if (dx == -1)
            rot = -90;
        else if (dx == 1)
            rot = 90;
        else if (dy == -1)
            rot = 180;
        else
            rot = 0;

        spawn.transform.Rotate(0, rot, 0);

    }


    private int getNeighbourValues(int x, int y)
    {
        int returnValue = 0;
        if (!cellIsFree(x, y + 1)) returnValue += 1;
        if (!cellIsFree(x, y - 1)) returnValue += 8;
        if (!cellIsFree(x + 1, y)) returnValue += 4;
        if (!cellIsFree(x - 1, y)) returnValue += 2;
        return returnValue;
    }

}

public enum TileType
{
    TOWER,
    PATH,
    END,
    SPAWN,
    GRASS,
    OBJECT,
}

