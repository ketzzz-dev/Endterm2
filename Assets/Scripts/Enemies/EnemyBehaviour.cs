using UnityEngine;

public abstract class EnemyBehaviour : ScriptableObject
{
    public virtual void Initialize(Enemy enemy) {}
    public virtual bool CanExecute(EnemyContext context) => true;

    public abstract float GetPriority(EnemyContext context);
    public abstract void Execute(EnemyContext context);
}
