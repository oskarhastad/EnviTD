using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreepController : MonoBehaviour, IEffectable
{
    List<Transform> path;
    int waypointIndex = 0;
    Transform target;

    float initialSpeed;
    float maxHp;
    int goldBounty;
    [SerializeField] Slider hpBar;
    [SerializeField] LocalChannelSO localChannel;
    [SerializeField] ParticleSystem hitEffect;
    [SerializeField] Animator animator;
    [SerializeField] AudioSource deathSound;

    public float currentHp;
    public List<CreepController> creeps;
    bool isLocal;
    public bool isAlive;

    private Dictionary<string, ModifierData> modifiers;
    float speed;
    float dmgOverTime = 0;

    void Start()
    {
        isAlive = true;
        modifiers = new Dictionary<string, ModifierData>();
    }

    public void SetLocal(bool local)
    {
        isLocal = local;
    }

    public void SetCreepData(CreepWaveData creepData)
    {
        this.maxHp = creepData.maxHp;
        this.initialSpeed = creepData.speed;
        this.goldBounty = creepData.bounty;
    }

    public void SetPath(List<Transform> newPath)
    {
        currentHp = maxHp;
        hpBar.maxValue = maxHp;
        hpBar.value = maxHp;
        path = newPath;
        ChangeTarget();
    }

    public void SetCreepList(List<CreepController> creepsList)
    {
        creeps = creepsList;
    }

    void ChangeTarget()
    {
        if (waypointIndex == path.Count)
        {
            if (isLocal)
                localChannel.DecreaseLife(1);
            Die();
            return;
        }

        target = path[waypointIndex++];
    }

    void Update()
    {
        if (!isAlive) return;
        if (target == null) return;

        if (modifiers.Count > 0) HandleEffects();
        HandleMove();
        HandleTurn();


    }

    private void HandleMove()
    {
        var step = speed * 0.016f; // * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);

        if (Vector3.Distance(transform.position, target.position) < 0.001f)
        {
            ChangeTarget();
        }

        speed = initialSpeed;
    }

    private void HandleTurn()
    {
        Vector3 directionToTurn = target.position - transform.position;

        if (directionToTurn.x != 0 || directionToTurn.z != 0)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToTurn), 0.13f);
        }

    }

    public void TakeDamage(float dmg)
    {
        hitEffect.Play();
        // hitAudioSource.time = 0.45f;
        // hitAudioSource.Play();
        currentHp -= dmg;
        hpBar.value = currentHp;
        if (currentHp <= 0 && isAlive)
        {
            Die();
        }
    }

    void Die()
    {

        if (animator)
            animator.SetBool("Death", true);
        deathSound.Play();
        isAlive = false;
        creeps.Remove(this);
        hpBar.gameObject.SetActive(false);
        if (isLocal)
            localChannel.GiveBounty(goldBounty);
        Destroy(this.gameObject, 1.5f);
        if (creeps.Count == 0)
        {
            localChannel.TriggerWaveComplete(isLocal);
        }
    }

    public void ApplyEffect(ModifierData modifier)
    {
        // TODO : Om vi vill att vissa spells ska kunna stackas (alltså en spell appliad flera gånger) fixa en stackable flagga
        modifiers[modifier.Name] = modifier;
        HandleEffects();
    }

    public void HandleEffects()
    {
        float speedAfterModifiers = 1;

        foreach (var modifier in modifiers)
        {
            if (modifier.Value.MovementModifier != 0)
            {
                speedAfterModifiers += (modifier.Value.MovementModifier / 10f);
            }
            if (modifier.Value.LifeTime > 0f) modifier.Value.LifeTime -= 0.016f;
            else modifiers.Remove(modifier.Value.Name);
        }
        speed = speedAfterModifiers;
    }

    public void RemoveEffect()
    {
        throw new NotImplementedException();
    }
}
