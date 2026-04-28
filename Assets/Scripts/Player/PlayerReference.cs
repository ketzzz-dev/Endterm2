using UnityEngine;

public class PlayerReference : MonoBehaviour
{
    public static GameObject Instance { get; private set; }

    private void Awake() => Instance = gameObject;
    private void OnDestroy() => Instance = null;
}
