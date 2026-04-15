using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDefinition", menuName = "Enemies/EnemyDefinition")]
public class EnemyDefinition : ScriptableObject
{
    [Header("Identity")]
    public string enemyId;
    public GameObject prefab;

    [Header("Stats")]
    public float maxHealth = 30f;
    public float moveSpeed = 2f;
    public float contactDamage = 10f;

    // How often contact damage is applied — prevents one-frame kill spikes
    public float damageCooldown = 0.5f;

    [Header("Behaviour")]
    public EnemyBehaviour behaviour;
}