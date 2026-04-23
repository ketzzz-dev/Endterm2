using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChargeBehaviour", menuName = "Enemy/Behaviours/ChargeBehaviour")]
public class ChargeBehaviour : EnemyBehaviour
{
    [SerializeField] private float cooldown = 5f;

    public override bool CanExecute(EnemyContext context) => !context.isActionLocked;
    public override float GetPriority(EnemyContext context) => context.timers["ChargeCooldown"] <= 0f ? float.MaxValue : 0f;
    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);

        enemy.context.timers["ChargeCooldown"] = 0f;
    }
    public override void Execute(EnemyContext context)
    {
        context.timers["ChargeCooldown"] -= Time.fixedDeltaTime;

        if (context.timers["ChargeCooldown"] <= 0f)
        {
            context.actionTrigger = "Charge";

            context.timers["ChargeCooldown"] = cooldown;
        }
    }
}
