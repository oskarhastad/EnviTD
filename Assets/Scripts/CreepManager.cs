using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CreepManager : MonoBehaviour
{
    public GridManager[] gridManagers;
    // [SerializeField] CreepWaveSO[] creepWaves;
    [SerializeField] GameChannelSO gameChannel;
    [SerializeField] LocalChannelSO localChannel;
    // [SerializeField] LocalChannelSO localChannel;
    public static List<CreepController>[] creeps;
    // public static List<CreepWave> creepWaves;

    int currentWaveIdx = 0;
    bool[] waveActive;

    void Start()
    {
        waveActive = new bool[gridManagers.Length];
        // GenerateWaves();
        gameChannel.CreepSpawnEvent += SpawnWave;
        localChannel.WaveCompleteEvent += OnWaveComplete;
        creeps = new List<CreepController>[gridManagers.Length];
        for (int i = 0; i < creeps.Length; i++)
        {
            creeps[i] = new List<CreepController>();
        }
    }


    void GenerateWaves()
    {

        // creepWaves = new CreepWaveSO[20];

        // for (int i = 0; i < 20; i++)
        // {
        //     CreepWaveSO wave = ScriptableObject.CreateInstance<CreepWaveSO>();
        //     CreepWaveSegment segment = new CreepWaveSegment();
        //     segment.nSpawns = i + 1;
        //     segment.creepPrefab = Resources.Load("CarRed", typeof(CreepController)) as CreepController;
        //     wave.segments = new CreepWaveSegment[1];
        //     wave.segments[0] = segment;
        //     creepWaves[i] = wave;
        // }
    }


    void SpawnWave()
    {
        StartCoroutine(SpawnWaveRoutine(currentWaveIdx));

        currentWaveIdx++;
        if (currentWaveIdx == CreepWaves.allCreepWaves.Length)
            currentWaveIdx = 0;

    }

    void OnWaveComplete(bool isLocal)
    {
        //   waveActive[isLocal ? 0 : 1] = false;
        //   for (int pathIndex = 0; pathIndex < waveActive.Length; pathIndex++)
        //   {
        //       if (waveActive[pathIndex] == true) return;
        //   }
        gameChannel.TriggerRequestCreepSpawnEvent();
    }

    IEnumerator SpawnWaveRoutine(int waveIdx)
    {
        // CreepWaveSO wave = creepWaves[waveIdx];
        CreepWave wave = CreepWaves.allCreepWaves[waveIdx];

        for (int pathIndex = 0; pathIndex < waveActive.Length; pathIndex++)
        {
            waveActive[pathIndex] = true;
        }

        // foreach (CreepWaveSegment waveSegment in wave.segments)
        // {
        for (int n = 0; n < wave.nSpawns; n++)
        {
            // Spawn each creep for both paths.
            for (int pathIndex = 0; pathIndex < gridManagers.Length; pathIndex++)
            {
                CreepController creep = Instantiate(wave.creepPrefab, gridManagers[pathIndex].spawn.position, wave.creepPrefab.transform.rotation);
                creep.SetCreepData(wave.creepWaveData);
                creep.SetPath(gridManagers[pathIndex].path);
                creep.SetCreepList(CreepManager.creeps[pathIndex]);
                creep.SetLocal(pathIndex == 0);
                creeps[pathIndex].Add(creep);

                // if (pathIndex == 0)
                // {
                //     localChannel.CreepSpawned(creep);
                // }
            }
            yield return new WaitForSeconds(.5f);
        }
        // }
    }

}
