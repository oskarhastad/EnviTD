# RemakeTD

## NetworkManager
- No changes
- Spawns [PlayerManager](#playermanager) for each Client.

## PlayerManager
- Only NetworkBehaviour in Scene.
- Responsible of sending and receving all network events.
- Recevies events on [GameChannel](#gamechannel).
- When some local object in the scene invokes an event on [GameChannel](#gamechannel) the PlayerManager calls the corresponding function on the server. The server will then broadcast back the event to all player via an ClientRpc.

```c#
void Start()
{
    // Only our own local PlayerManager will listen on the GameCannel.
    if (isLocalPlayer)
    {
        // Listen to CmdCreepSpawn Event.
        gameChannel.OnCmdCreepSpawn += SpawnWave;
    }
}

// Local code to run before transmitting event to server.
public void SpawnWave()
{
    CmdSpawnWave();
}

// Transmit event to server.
[Command]
public void CmdSpawnWave()
{
    // Server only brodcasts event.
    RpcSpawnWave();
}

// When server broadcasts event, this will be called on all clients,
// on the PlayerManager belonging to the client who orginally transmitted the event.
[ClientRpc]
public void RpcSpawnWave()
{
    // Invoke the CreepSpawn Event.
    // The CreepManager listen for this events and will spawn the wave when it is invoked.
    // Here it is also possible to check if I initiated the spawn with isLocalPlayer,
    // if I should only spawn creeps in my lane for example.
    gameChannel.CreepSpawn();
}
```
## CreepManager
- Holds reference to both [PathManager](#pathmanager)s.
- Holds a static reference to a CreepList for each player. This is for towers to iterate over and find their targets. 
- Listen on RpcCreepSpawn event.
- When RpcCreepSpawn event is invoked the CreepManager is responsible for spawning the next wave.
- Local Player is playing on first path, enemy on second.
- Each wave is constructed of a CreepWave-ScriptableObject, which in turn consists of CreepWaveSegments. Each segment references some creep prefab and how many of it the segment should spawn.
```c#
public class CreepWaveSegment
{
    public CreepController creepPrefab;
    public int nSpawns;
}
```
- The CreepManager iterates through each segment and spawns the given number of instances, for each path.
- For each created creep, the CreepManager passes the path which the creep should take, and also 
```c#
IEnumerator SpawnWaveRoutine()
{
    CreepWaveSO wave = creepWaves[waveIndex];

    foreach (CreepWaveSegment waveSegment in wave.segments)
    {
        for (int n = 0; n < waveSegment.nSpawns; n++)
        {
            // Spawn each creep for both paths.
            for (int pathIndex = 0; pathIndex < pathManagers.Length; pathIndex++)
            {
                CreepController creep = Instantiate(waveSegment.creepPrefab, pathManagers[pathIndex].spawn.position, waveSegment.creepPrefab.transform.rotation);
                creep.SetPath(pathManagers[pathIndex].path);
                creep.SetCreepList(CreepManager.creeps[pathIndex]);
                // Local player always is on the first (left) path.
                creep.SetLocal(pathIndex == 0);
                creeps[pathIndex].Add(creep);
            }

            // Wait .5 seconds between each creep
            yield return new WaitForSeconds(.5f);
        }
    }
}
```

## BuildingManager
- Handles local player build interactions.
- Holds reference to a collider for the allowed build area.
- Communicates with UI over LocalChannel.
- When players places a tower invoke the CmdTowerBuild event on the GameChannel. This event is invoked with buildingID, and grid coordinates.
- The server will recevie the CmdTowerBuild event and broadcast RpcTowerBuild.
- RpcTowerBuild will pick out the buildingPrefab using the buildingID and place it according to the given X,Z coords. It also checks if we are building this tower or not. If it is the enemy, translate the coords to their side of the map.
- The TowerManager has an array with TowerScribtableObjects. The TowerSO holds a prefab for the "prebuilt" version and the actual built Tower.

```c#
void OnRpcTowerBuild(int idx, int x, int z, bool isLocalTower)
{
    TowerController towerPrefab = towerSOs[idx].realTower;
    TowerController tower = Instantiate(towerPrefab, new Vector3(x, .2f, z), towerPrefab.transform.rotation);
    int creepsIdx = isLocalTower ? 0 : 1;
    tower.SetCreeps(CreepManager.creeps[creepsIdx]);
}
```
## Area
- There is one Area for each player.
- Each area has one path with a [PathManager](#pathmanager) attached to it.
- Each area has some collider for the allowed build area.

## PathManager
- Refernces a spawn point.
- References an array of transform for each creep waypoint in order.

## UIManager
- (Debug) Button to spawn creep wave.
- Button to select tower to build. Invokes event on LocalChannel.
- Life of player and enemy.
- Life is deducted from the player by having creeps emit events on the LocalChannel. The UIManager then retransmit these on the GameChannel via the CmdSetLife event.
- Listens on the GameChannel RpcSetLife event. Same as in [BuildingManager](#buildingmanager)s logic, RpcSetLife checks if it the local player or the enemy's life which is being updated.

## CreepController
- When created is should be given:
    - a Path
    - if it is local players creep
    - CreepList from where to remove itself once it reaches the end or dies.
    - The creep will move between each waypoint in the path until it reaches the last waypoint.
    - If it is a local player creep and reaches the last waypoint it will invoke the DecreaseLife event on the LocalChannel.
    - The [ProjectileController](#projectilecontroller) will call the `TakeDamage` method on each projectile hit. This will also play some sound and damage animation.
    - Checks if its currentHp has reached 0, if so remove self.

## TowerController
- Has a reference to the correct creepList. There is one list for each player's creeps.
- Reference to some projectilePrefab to shoot.
- When no target has been selected, it loops through tge creep list to find one within range.
- When a target has been found, shoot at it (with some delay between each shot).
- When shooting it spawns the projectilePrefab and passes it's [ProjectileController](#projectilecontroller) the target. Also play some attack sound.

## ProjectileController
- Is given a target at spawn by the [TowerController](#towercontroller).
- Each frame save the last position of the target and move towards it.
- If the target has been killed or removed, keep going towards the last saved position.
- When reaching the saved position, check if the creep still exists and damage it.
