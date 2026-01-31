using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImage : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float duration;
    
    private void OnEnable()
    {
        // spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
        StartCoroutine(StartFade());
    }

    private IEnumerator StartFade()
    {
        Color startColor = spriteRenderer.color;
        float elapsedTime = 0f;
        while(elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsedTime / duration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;

        }

        spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        ObjectPoolManager.ReturnObjectToPool(gameObject, ObjectPoolManager.PoolType.Sprites);
        
    }

    private void OnDisable()
    {
        Color startColor = spriteRenderer.color;
        spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 1f);
    }
}
