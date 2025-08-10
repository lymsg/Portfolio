using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 슬롯 클릭 시 발생하는 로직을 처리하는 컴포넌트
/// 현재 액티브 아이템 슬롯 클릭 시 정보 텍스트를 출력
/// </summary>
[RequireComponent(typeof(SlotUI))]
public class SlotClickHandler : MonoBehaviour, IPointerClickHandler
{
    private SlotUI slotUI;

    private void Awake()
    {
        slotUI = GetComponent<SlotUI>();
    }
    /// <summary>
    /// 슬롯 클릭 시 정보 텍스트 출력 처리
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (slotUI.slotType != SlotType.ActiveItem)
            return;
        
        SoundManager.Instance.PlaySFX(GameManager.Instance.Player.transform.position, "ButtonClick");
        
        if(slotUI.ActiveSlot == null || slotUI.ActiveSlot.ActiveItem == null)
        {
            string description = String.Empty;
            slotUI.inventoryUI?.SetInfoText(description);
        }
        else
        {
            string description = slotUI.ActiveSlot.GetDescription();
            slotUI.inventoryUI?.SetInfoText(description);
        }
    }
}
