using UnityEngine;

public abstract class EnemyBehaviourState {}
public class EmptyState : EnemyBehaviourState {}

public class EnemyBehaviourContext
{
    
}

public abstract class EnemyBehaviour : ScriptableObject
{
    public virtual EnemyBehaviourState CreateState() => new EmptyState();
    public abstract Vector2 GetDesiredVelocity(EnemyBehaviourContext context, EnemyBehaviourState state);
}
