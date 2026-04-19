using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteBlinker : MonoBehaviour
{
    [SerializeField] private float blinkDuration = 0.25f;

    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;

    private bool isBlinking = false;
    private float blinkTimer = 0f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        propertyBlock = new();
    }

    private void Update()
    {
        if (!isBlinking)
            return;
        
        blinkTimer -= Time.deltaTime;

        var blinkFactor = Mathf.Clamp01(blinkTimer / blinkDuration);

        spriteRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat("_BlinkFactor", blinkFactor);
        spriteRenderer.SetPropertyBlock(propertyBlock);

        if (blinkTimer <= 0f)
            isBlinking = false;
    }

    public void Blink()
    {
        if (isBlinking)
            return;
        
        isBlinking = true;
        blinkTimer = blinkDuration;
    }
}
