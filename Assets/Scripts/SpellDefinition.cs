using UnityEngine;

[CreateAssetMenu(fileName = "SpellDefinition", menuName = "Spells/SpellDefinition")]
public class SpellDefinition : ScriptableObject
{
    public string symbolId;

    public float manaCost;
    public float cooldown;

    // TODO: add functionality
}
