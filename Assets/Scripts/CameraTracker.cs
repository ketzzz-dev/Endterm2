using UnityEngine;

public class CameraTracker : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.25f;

    private Vector3 offset;
    private Vector3 velocity;

    private void Start()
    {
        if (!target)
            return;
        
        offset = transform.position - target.position;
        
        transform.position = target.position + offset;
    }

    private void LateUpdate()
    {
        if (!target)
            return;
        
        transform.position = Vector3.SmoothDamp(transform.position, target.position + offset, ref velocity, smoothTime);
    }
}
