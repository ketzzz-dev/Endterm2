using UnityEngine;

[CreateAssetMenu(fileName = "ShootBehaviour", menuName = "Enemy/Behaviours/ShootBehaviour")]
public class ShootBehaviour : EnemyBehaviour
{
    [SerializeField] private float cooldown = 5f;

    public override bool CanExecute(EnemyContext context) => !context.isActionLocked;
    public override float GetPriority(EnemyContext context) => context.timers["ShootCooldown"] <= 0f ? float.MaxValue : 0f;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);

        enemy.context.timers["ShootCooldown"] = 0f;
    }

    public override void Execute(EnemyContext context)
    {
        context.timers["ShootCooldown"] -= Time.fixedDeltaTime;

        if (context.timers["ShootCooldown"] <= 0f)
        {
            context.actionTrigger = "Shoot";

            context.timers["ShootCooldown"] = cooldown;
        }
    }
}
