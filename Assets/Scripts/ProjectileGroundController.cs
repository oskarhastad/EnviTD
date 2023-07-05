using UnityEngine;
public class ProjectileGroundController : ProjectileController
{
    [SerializeField] float aoe;

    public override void SetTarget(CreepController newTarget)
    {
        base.SetTarget(newTarget);
        lastPos = newTarget.transform.position;
        lastPos.y += 0.5f;
    }
    public override void Update()
    {
        if (hasHit) return;
        if (lastPos == null) return;

        var step = speed * 0.016f; // * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, lastPos, step);
        transform.LookAt(lastPos, Vector3.up);
        if (Vector3.Distance(transform.position, lastPos) < 0.001f)
        {

            hasHit = true;
            soundHit.Play();

            // TODO: play hit animation
            foreach (Renderer r in renderers) r.enabled = false;

            // Iterate backwards since creeps might be removed as we itarate.
            for (int i = creeps.Count - 1; i >= 0; i--)
            {
                CreepController creep = creeps[i];
                if (Vector3.Distance(lastPos, creep.transform.position) < aoe)
                {
                    creep.TakeDamage(damage);
                }
            }
            Destroy(this.gameObject, 1f);
        }
    }
}
