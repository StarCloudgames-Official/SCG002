using System;
using UnityEngine;

public class ObjectSlider : CachedMonoBehaviour
{
    [SerializeField] private bool isHorizontal;
    [SerializeField] private SpriteRenderer sliderSpriteRenderer;
    [SerializeField] private SpriteRenderer fillSpriteRenderer;

    [Range(0,1)]
    [SerializeField] private float amount;

    private Transform fillTransform;
    private float sliderOriginalWidth;
    private float sliderOriginalHeight;
    private float fillOriginalWidth;
    private float fillOriginalHeight;

    private void Awake()
    {
        fillTransform = fillSpriteRenderer.transform;
        sliderOriginalWidth = sliderSpriteRenderer.sprite.bounds.size.x;
        sliderOriginalHeight = sliderSpriteRenderer.sprite.bounds.size.y;
        fillOriginalWidth = fillSpriteRenderer.sprite.bounds.size.x;
        fillOriginalHeight = fillSpriteRenderer.sprite.bounds.size.y;

        SetLayer();
        UpdateSlider();
    }

    private void SetLayer()
    {
        fillSpriteRenderer.sortingLayerID = sliderSpriteRenderer.sortingLayerID;
        fillSpriteRenderer.sortingOrder = sliderSpriteRenderer.sortingOrder + 1;
    }

    public void SetValue(float targetAmount)
    {
        amount = Mathf.Clamp01(targetAmount);
        UpdateSlider();
    }

    private void UpdateSlider()
    {
        amount = Mathf.Clamp01(amount);

        if (isHorizontal)
        {
            fillTransform.localScale = new Vector3(amount, 1f, 1f);

            float offset = -sliderOriginalWidth / 2f + (fillOriginalWidth * amount) / 2f;
            fillTransform.localPosition = new Vector3(offset, 0f, 0f);
        }
        else
        {
            fillTransform.localScale = new Vector3(1f, amount, 1f);

            float offset = -sliderOriginalHeight / 2f + (fillOriginalHeight * amount) / 2f;
            fillTransform.localPosition = new Vector3(0f, offset, 0f);
        }
    }

    public async Awaitable SetValueAsync(float targetAmount, float duration = 0.3f)
    {
        targetAmount = Mathf.Clamp01(targetAmount);
        float startAmount = amount;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!gameObject.activeInHierarchy)
            {
                amount = targetAmount;
                UpdateSlider();
                return;
            }

            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            amount = Mathf.Lerp(startAmount, targetAmount, t);
            UpdateSlider();
            await Awaitable.NextFrameAsync();
        }

        amount = targetAmount;
        UpdateSlider();
    }
}
