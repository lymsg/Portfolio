using TMPro;
using UnityEngine;

public class ContextualUIHint : UIBase
{
    [SerializeField] private TextMeshProUGUI hintText; 
    [SerializeField] private RectTransform rectTransform;
    
    public override string UIName => this.GetType().Name;
    public float DefaultY = -463f; // 기본 위치
    public float UpY = -173f;
    
    
    public void SetHintText(string text)
    {
        if (hintText != null)
        {
            hintText.text = text;
        }
        else
        {
            Debug.LogWarning("ContextualUIHint: hintText가 할당되지 않았습니다. 인스펙터에서 할당해주세요.", this);
        }
    }
    
    public void SetPositionY(float y)
    {
        if (rectTransform != null)
        {
            var pos = rectTransform.anchoredPosition;
            pos.y = y;
            rectTransform.anchoredPosition = pos;
        }
    }
}
