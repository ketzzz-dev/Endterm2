using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthMana : MonoBehaviour
{
    [SerializeField] private RectTransform healthRect;
    [SerializeField] private RectTransform manaRect;
    [SerializeField] private RectMask2D healthMask;
    [SerializeField] private RectMask2D manaMask;

    private float healthMaskRelativeLeft, healthMaskRight;
    private float manaMaskRelativeLeft, manaMaskRight;

    private PlayerStats playerStats;

    private void Start()
    {
        healthMaskRelativeLeft = healthRect.rect.width - healthMask.padding.x;
        healthMaskRight = healthMask.padding.z;

        manaMaskRelativeLeft = manaRect.rect.width - manaMask.padding.x;
        manaMaskRight = manaMask.padding.z;

        StartCoroutine(WaitForPLayer());
    }

    private void OnEnable()
    {
        PlayerStats.OnHealthChanged += UpdateHealthBar;
        PlayerStats.OnManaChanged += UpdateManaBar;
    }

    private void OnDisable()
    {
        PlayerStats.OnHealthChanged -= UpdateHealthBar;
        PlayerStats.OnManaChanged -= UpdateManaBar;
    }

    private IEnumerator WaitForPLayer()
    {
        while (PlayerReference.Instance == null)
            yield return null;

        playerStats = PlayerReference.Instance.GetComponent<PlayerStats>(); 
    }

    private void UpdateHealthBar(float currentHealth)
    {
        if (playerStats == null)
            return;

        var healthPercent = currentHealth / playerStats.maxHealth;
        var padding = healthMask.padding;

        padding.z = Mathf.Lerp(healthMaskRight, healthMaskRelativeLeft, 1f - healthPercent);
        healthMask.padding = padding;
    }

    private void UpdateManaBar(float currentMana)
    {
        if (playerStats == null)
            return;
        
        var manaPercent = currentMana / playerStats.maxMana;
        var padding = manaMask.padding;

        padding.z = Mathf.Lerp(manaMaskRight, manaMaskRelativeLeft, 1f - manaPercent);
        manaMask.padding = padding;
    }
}
