using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BuildingManager : MonoBehaviour
{
    enum State
    {
        DEFAULT,
        BUILDING,
    }

    public TowerSO[] towerSOs;
    [SerializeField] GridManager gridManager;
    [SerializeField] LocalChannelSO localChannel;
    [SerializeField] GameChannelSO gameChannel;
    State state;

    GameObject buildingTower;
    int buildingIdx;
    int targetX;
    int targetZ;

    [SerializeField] int gold;


    // Start is called before the first frame update
    void Start()
    {
        localChannel.OnTowerBuildButton += OnTowerBuildButton;
        gameChannel.OnRpcTowerBuild += OnRpcTowerBuild;
        localChannel.OnBounty += OnGoldBounty;
        state = State.DEFAULT;

        localChannel.SetGold(gold);
    }

    // Update is called once per frame
    void Update()
    {

        if (state == State.BUILDING)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                targetX = Mathf.RoundToInt(hit.point.x);
                targetZ = Mathf.RoundToInt(hit.point.z);
                Vector3 snapPos = new Vector3(targetX, hit.point.y, targetZ);
                buildingTower.transform.position = snapPos;

                //checking if position is in bounds of area and the tile is grass
                Vector2Int buildingCoordinates = gridManager.WorldToGrid(new Vector2Int(targetX, targetZ));
                int buildingX = buildingCoordinates.x;
                int buildingZ = buildingCoordinates.y;
                bool inArea = false;
                if(buildingX >= 0 && buildingX < gridManager.tileMap.GetLength(0) &&
                 buildingZ >= 0 && buildingZ < gridManager.tileMap.GetLength(1)) inArea = true;

                if (Input.GetMouseButtonDown(0) && inArea && gridManager.tileMap[buildingX,buildingZ] == TileType.GRASS)
                {
                    Debug.Log("Building tower at x: " + targetX + "and z: " + targetZ);
                    gridManager.tileMap[buildingX, buildingZ] = TileType.TOWER;
                    // Instantiate(towerPrefab, snapPos, towerPrefab.transform.rotation);
                    gold -= towerSOs[buildingIdx].price;
                    localChannel.SetGold(gold);
                    gameChannel.CmdTowerBuild(buildingIdx, targetX, targetZ);
                    ResetState();
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                ResetState();
            }

        }

    }

    void ResetState()
    {
        if (buildingTower) Destroy(buildingTower);
        buildingTower = null;
        state = State.DEFAULT;
    }

    void OnTowerBuildButton(int idx)
    {
        TowerSO tower = towerSOs[idx];
        if (tower.price > gold) return;

        buildingIdx = idx;
        buildingTower = Instantiate(tower.hoverTower);
        state = State.BUILDING;
    }

    void OnRpcTowerBuild(int idx, int x, int z, bool isLocalTower)
    {
        TowerController towerPrefab = towerSOs[idx].realTower;
        TowerController tower = Instantiate(towerPrefab, new Vector3(x, .2f, z), towerPrefab.transform.rotation);
        int creepsIdx = isLocalTower ? 0 : 1;
        tower.SetCreeps(CreepManager.creeps[creepsIdx]);
    }

    void OnGoldBounty(int bounty)
    {
        gold += bounty;
        localChannel.SetGold(gold);
    }
}
