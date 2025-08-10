using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaTipUIController : MonoBehaviour
{
    public RectTransform tipTransform;
    public float moveAmount = 50f;
    public float duration = 0.5f;

    private Vector2 originalPos;
    private Coroutine bounceCoroutine;

    void Awake()
    {
        originalPos = tipTransform.anchoredPosition;
        gameObject.SetActive(false);
    }

    public void ShowWithBounce()
    {
        gameObject.SetActive(true);
        if (bounceCoroutine != null)
            StopCoroutine(bounceCoroutine);

        bounceCoroutine = StartCoroutine(BounceLoop());
    }

    public void Hide()
    {
        if (bounceCoroutine != null)
        {
            StopCoroutine(bounceCoroutine);
            bounceCoroutine = null;
        }
        tipTransform.anchoredPosition = originalPos;
        gameObject.SetActive(false);
    }

    private IEnumerator BounceLoop()
    {
        while (true)
        {
            // 올라가기
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float y = Mathf.Lerp(0, moveAmount, t);
                tipTransform.anchoredPosition = originalPos + Vector2.up * y;
                elapsed += Time.deltaTime;
                yield return null;
            }

            // 내려가기
            elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float y = Mathf.Lerp(moveAmount, 0, t);
                tipTransform.anchoredPosition = originalPos + Vector2.up * y;
                elapsed += Time.deltaTime;
                yield return null;
            }

            tipTransform.anchoredPosition = originalPos;
        }
    }
}
