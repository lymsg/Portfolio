using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BookAnimator : MonoBehaviour
{
    public Sprite[] sprites;           // 책 넘김 프레임들
    public float frameRate;    // 프레임 간 시간 간격
    private Image imageRenderer;
    private int currentFrame;
    private float timer;

    void Start()
    {
        imageRenderer = GetComponent<Image>();
    }

    void Update()
    {
        if (sprites.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= frameRate)
        {
            timer -= frameRate;
            currentFrame = (currentFrame + 1) % sprites.Length;
            imageRenderer.sprite = sprites[currentFrame];
        }
    }
}
