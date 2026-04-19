using UnityEngine;

public class PlayerReference : MonoBehaviour
{
    public static Transform Instance { get; private set; }

    private void Awake() => Instance = transform;
    private void OnDestroy() => Instance = null;
}
