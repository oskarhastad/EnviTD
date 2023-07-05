using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Game Channel")]
public class GameChannelSO : ScriptableObject
{

    public UnityAction<int, int, int> OnCmdTowerBuild;
    public UnityAction<int, int, int, bool> OnRpcTowerBuild;

    public UnityAction<int> OnCmdSetLife;
    public UnityAction<int, bool> OnRpcSetLife;


    public UnityAction RequestCreepSpawnEvent;
    public void TriggerRequestCreepSpawnEvent() => RequestCreepSpawnEvent?.Invoke();

    public UnityAction CreepSpawnEvent;
    public void TriggerCreepSpawnEvent() => CreepSpawnEvent?.Invoke();

    public void CmdTowerBuild(int idx, int x, int z)
    {
        OnCmdTowerBuild?.Invoke(idx, x, z);
    }

    public void RpcTowerBuild(int idx, int x, int z, bool isLocalPlayer)
    {
        OnRpcTowerBuild?.Invoke(idx, x, z, isLocalPlayer);
    }

    public void CmdSetLife(int lives)
    {
        OnCmdSetLife?.Invoke(lives);
    }

    public void RpcSetLife(int lives, bool isLocalPlayer)
    {
        OnRpcSetLife?.Invoke(lives, isLocalPlayer);
    }

    public UnityAction<bool> GameEndEvent;
    public void TriggerGameEndEvent(bool isWinner) => GameEndEvent?.Invoke(isWinner);
    public UnityAction GameStartEvent;
    public void TriggerGameStartEvent() => GameStartEvent?.Invoke();

    public UnityAction RequestGridSeedEvent;
    public void TriggerRequestGridSeedEvent() => RequestGridSeedEvent?.Invoke();


}