using UnityEngine;
using System.Collections.Generic;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] protected float speed;
    [SerializeField] protected float damage;
    [SerializeField] protected Renderer[] renderers;
    [SerializeField] protected ModifierData modifier;


    protected AudioSource soundHit;
    protected CreepController target;
    protected List<CreepController> creeps;
    protected bool hasHit;
    public Vector3 lastPos;

    public virtual void Start()
    {
        soundHit = GetComponent<AudioSource>();
    }

    public virtual void SetTarget(CreepController newTarget)
    {
        target = newTarget;
        creeps = newTarget.creeps;
    }

    public virtual void Update()
    {
        if (hasHit) return;
        if (target != null)
        {
            lastPos = target.transform.position;
            lastPos.y += 0.5f;
        }
        if (lastPos == null) return;

        var step = speed * 0.016f; // * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, lastPos, step);
        transform.LookAt(lastPos, Vector3.up);
        if (Vector3.Distance(transform.position, lastPos) < 0.001f)
        {
            hasHit = true;
            if (target != null)
            {
                soundHit.Play();

                target.TakeDamage(damage);
                if (modifier)
                    target.ApplyEffect(modifier.Clone());
            }

            // TODO: play hit animation
            foreach (Renderer r in renderers) r.enabled = false;


            Destroy(this.gameObject, 1f);
        }
    }

}
