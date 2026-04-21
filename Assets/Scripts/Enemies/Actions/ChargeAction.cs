using UnityEngine;

public class ChargeAction : EnemyAction
{
    [SerializeField] private float chargeSpeed = 15f;

    private void Charge()
    {
        enemy.rigidbody.linearVelocity = enemy.context.directionToPlayer * chargeSpeed;
    }
}