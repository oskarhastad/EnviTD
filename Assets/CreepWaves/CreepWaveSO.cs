using System;
using UnityEngine;


public class CreepWaveData
{
    public int maxHp;
    public float speed;
    public int bounty;

    public CreepWaveData(int maxHp, float speed, int bounty)
    {
        this.maxHp = maxHp;
        this.speed = speed;
        this.bounty = bounty;
    }
}

[Serializable]
public class CreepWave
{
    public CreepController creepPrefab;
    public int nSpawns;
    public CreepWaveData creepWaveData;


    public CreepWave(String prefab, int nSpawns, int maxHp, float speed, int bounty)
    {
        this.creepPrefab = Resources.Load<CreepController>(prefab);
        this.nSpawns = nSpawns;
        this.creepWaveData = new CreepWaveData(maxHp, speed, bounty);
    }
}

public static class CreepWaves
{
    public static CreepWave[] allCreepWaves = new CreepWave[]{
        new CreepWave("CarRed", 5, 10, 1f, 1),
        new CreepWave("CarRed", 5, 20, 1f, 1),
        new CreepWave("CarRed", 5, 40, 1f, 1),
    };

}

// public class CreepWaveSO : ScriptableObject
// {
//     public CreepWaveSegment[] segments;
// }