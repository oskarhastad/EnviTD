using System.Collections.Generic;
using UnityEngine;

public class TowerController : MonoBehaviour
{

    enum State
    {
        SEARCHING_TARGET,
        HAS_TARGET,
    }

    State state;
    CreepController activeTarget;
    List<CreepController> creeps;
    [SerializeField] ProjectileController projectilePrefab;
    [SerializeField] Transform projectileSpawn;
    [SerializeField] float attackRange;
    [SerializeField] float attackDelay;

    AudioSource attackAudioSource;

    float attackCounter = 0;

    void Start()
    {
        state = State.SEARCHING_TARGET;
        attackAudioSource = GetComponent<AudioSource>();

    }

    public void SetCreeps(List<CreepController> creepList)
    {
        creeps = creepList;
    }


    void Update()
    {
        if (attackCounter > 0)
        {
            attackCounter -= 0.016f; //Time.deltaTime;
        }

        switch (state)
        {
            case State.SEARCHING_TARGET:
                if (creeps == null) return;

                foreach (CreepController creep in creeps)
                {
                    float distance = Vector3.Distance(transform.position, creep.transform.position);
                    if (distance <= attackRange)
                    {
                        activeTarget = creep;
                        state = State.HAS_TARGET;
                        return;
                    }
                }
                break;

            case State.HAS_TARGET:
                if (activeTarget == null || !activeTarget.isAlive)
                {
                    state = State.SEARCHING_TARGET;
                    return;
                }

                {
                    float distance = Vector3.Distance(transform.position, activeTarget.transform.position);
                    if (distance > attackRange)
                    {
                        activeTarget = null;
                        state = State.SEARCHING_TARGET;
                        return;
                    }
                }

                if (attackCounter <= 0)
                {
                    Shoot(activeTarget);
                    attackCounter = attackDelay;
                }
                break;
        }
    }

    void Shoot(CreepController target)
    {
        attackAudioSource.Play();
        ProjectileController projectile = Instantiate(projectilePrefab, projectileSpawn.position, projectilePrefab.transform.rotation);
        projectile.SetTarget(target);
    }


}
