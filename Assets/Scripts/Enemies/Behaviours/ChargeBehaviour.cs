using UnityEngine;

[CreateAssetMenu(fileName = "ChargeBehaviour", menuName = "Enemy/Behaviours/ChargeBehaviour")]
public class ChargeBehaviour : EnemyBehaviour
{
    [SerializeField] private float cooldown = 5f;

    private float cooldownTimer = 0f;

    public override bool CanExecute(EnemyContext context) => !context.isActionLocked;
    public override float GetPriority(EnemyContext context) => cooldownTimer <= 0f ? float.MaxValue : 0f;

    public override void Execute(EnemyContext context)
    {
        cooldownTimer -= Time.fixedDeltaTime;

        if (cooldownTimer <= 0f)
        {
            context.actionTrigger = "Charge";

            cooldownTimer = cooldown;
        }
    }
}
