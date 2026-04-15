using UnityEngine;

[CreateAssetMenu(fileName = "Chaser", menuName = "Enemies/Behaviours/Chaser")]
public class ChaserBehaviour : EnemyBehaviour
{
    public override Vector2 GetDesiredVelocity(EnemyBehaviourContext context, EnemyBehaviourState state)
    {
        return Vector2.zero;
    }
}
