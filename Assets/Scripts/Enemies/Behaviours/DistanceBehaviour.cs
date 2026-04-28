using UnityEngine;

[CreateAssetMenu(fileName = "DistanceBehaviour", menuName = "Enemy/Behaviours/DistanceBehaviour")]
public class DistanceBehaviour : EnemyBehaviour
{
    [SerializeField] private float priority = 1f;
    [SerializeField] private float distance = 1f;

    public override bool CanExecute(EnemyContext context) => !context.isActionLocked;
    public override float GetPriority(EnemyContext context) => context.distanceToPlayer < distance ? priority : 0f;

    public override void Execute(EnemyContext context)
    {
        if (context.distanceToPlayer > distance)
            return;

        context.desiredDirection -= 2f * context.directionToPlayer;
    }
}
