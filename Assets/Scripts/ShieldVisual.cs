using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ShieldVisual : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void ActivateShield()
    {
        gameObject.SetActive(true);
        animator.SetTrigger("Activate");
    }

    public void BreakShield()
    {
        animator.SetTrigger("Break");
    }

    private void OnBreakAnimationFinished()
    {
        gameObject.SetActive(false);
    }
}
