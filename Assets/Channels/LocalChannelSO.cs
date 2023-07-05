using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Local Channel")]
public class LocalChannelSO : ScriptableObject
{
    public UnityAction<int> OnTowerBuildButton;
    public void TowerBuildButton(int idx) => OnTowerBuildButton?.Invoke(idx);

    public UnityAction<CreepController> OnCreepSpawned;
    public void CreepSpawned(CreepController creep) => OnCreepSpawned?.Invoke(creep);


    public UnityAction<int> OnDecreaseLife;
    public void DecreaseLife(int lives) => OnDecreaseLife?.Invoke(lives);


    public UnityAction<int> OnSetGold;
    public void SetGold(int gold) => OnSetGold?.Invoke(gold);

    public UnityAction<int> OnBounty;
    public void GiveBounty(int gold) => OnBounty?.Invoke(gold);

    public UnityAction<int> OnGenerateGrid;
    public void TriggerGenerateGridEvent(int seed) => OnGenerateGrid?.Invoke(seed);

    public UnityAction<bool> WaveCompleteEvent;
    public void TriggerWaveComplete(bool isLocal) => WaveCompleteEvent?.Invoke(isLocal);

}