using UnityEngine;

[CreateAssetMenu(fileName = "ChaseBehaviour", menuName = "Enemy/Behaviours/ChaseBehaviour")]
public class ChaseBehaviour : EnemyBehaviour
{
    [SerializeField] private float priority = 1f;

    public override bool CanExecute(EnemyContext context) => !context.isActionLocked;
    public override float GetPriority(EnemyContext context) => priority;

    public override void Execute(EnemyContext context)
    {
        context.desiredDirection = context.directionToPlayer;
    }
}
