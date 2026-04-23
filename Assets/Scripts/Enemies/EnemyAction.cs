using UnityEngine;

[RequireComponent(typeof(Enemy))]
public abstract class EnemyAction : MonoBehaviour
{
    protected Enemy enemy;

    protected virtual void Awake()
    {
        enemy = GetComponent<Enemy>();
    }
}
