using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxFloatingEffect : MonoBehaviour
{
    public float rotateSpeed = 45f; // 초당 회전 속도 (Y축)
    public float floatAmplitude = 0.1f; // 위아래 부유 범위
    public float floatFrequency = 1f; // 위아래 진동 속도

    private Vector3 _startPos;

    void Start()
    {
        _startPos = transform.localPosition;
    }

    void Update()
    {
        // Y축 회전
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);

        // 위아래 부유
        float newY = _startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.localPosition = new Vector3(_startPos.x, newY, _startPos.z);
    }
}
