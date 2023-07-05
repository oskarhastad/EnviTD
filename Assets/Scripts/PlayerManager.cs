using UnityEngine;
using System.Collections;
using Mirror;
public class PlayerManager : NetworkBehaviour
{
    [SerializeField] GameChannelSO gameChannel;
    [SerializeField] LocalChannelSO localChannel;
    static int randomSeed;

    void Start()
    {
        if (isLocalPlayer)
        {
            gameChannel.OnCmdTowerBuild += TowerBuild;
            gameChannel.OnCmdSetLife += SetLife;
            if (isServer)
            {
                Debug.Log("Is Server");
                gameChannel.RequestCreepSpawnEvent += SpawnWave;
                randomSeed = Random.Range(int.MinValue, int.MaxValue);
                localChannel.TriggerGenerateGridEvent(randomSeed);
                StartCoroutine(GameStartTimer());
            }
            else if (isClient)
            {
                Debug.Log("Is Client");
                // Ask server for seed
                GetGridSeed();
            }

            gameChannel.TriggerGameStartEvent();
        }
    }

    IEnumerator GameStartTimer()
    {
        yield return new WaitForSeconds(5f);
        SpawnWave();
    }

    [Command]
    public void GetGridSeed()
    {
        BroadcastGridSeed(randomSeed);
    }

    [ClientRpc]
    public void BroadcastGridSeed(int seed)
    {
        if (!isLocalPlayer) return;
        Debug.Log($"Recv seed {seed}");
        localChannel.TriggerGenerateGridEvent(seed);
    }


    public void SpawnWave()
    {
        CmdSpawnWave();
    }

    [Command]
    public void CmdSpawnWave()
    {
        RpcSpawnWave();
    }

    [ClientRpc]
    public void RpcSpawnWave()
    {
        gameChannel.TriggerCreepSpawnEvent();
    }


    void TowerBuild(int idx, int x, int z)
    {
        CmdTowerBuild(idx, x, z);
    }

    [Command]
    void CmdTowerBuild(int idx, int x, int z)
    {
        RpcTowerBuild(idx, x, z);
    }

    [ClientRpc]
    public void RpcTowerBuild(int idx, int x, int z)
    {
        if (isLocalPlayer)
        {
            gameChannel.RpcTowerBuild(idx, x, z, true);
        }
        else
        {
            gameChannel.RpcTowerBuild(idx, -x + 17, z, false);
        }
    }

    void SetLife(int life)
    {
        CmdSetLife(life);
    }

    [Command]
    void CmdSetLife(int life)
    {
        RpcSetLife(life);
        if (life == 0)
        {
            RpcEndGame();
        }
    }

    [ClientRpc]
    public void RpcSetLife(int life)
    {
        if (isLocalPlayer)
        {
            gameChannel.RpcSetLife(life, true);
        }
        else
        {
            gameChannel.RpcSetLife(life, false);
        }
    }

    [ClientRpc]
    public void RpcEndGame()
    {
        //local player will be loser
        if (isLocalPlayer)
        {
            gameChannel.TriggerGameEndEvent(false);
        }
        else
        {
            gameChannel.TriggerGameEndEvent(true);
        }

    }


}
