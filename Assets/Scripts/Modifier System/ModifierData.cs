using UnityEngine;

[CreateAssetMenu(menuName = "Modifier")]
public class ModifierData : ScriptableObject
{
    public string Name;
    public Sprite Icon;
    public float DmgOverTime;
    public float TickSpeed;
    public float LifeTime;
    public float MovementModifier;

    public ParticleSystem ParticleSystem;
}
