using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum SlotType
{
    Inventory,
    Equip,
    ActiveItem
}
/// <summary>
/// 하나의 인벤토리 또는 장비 슬롯을 표현하는 UI 컴포넌트
/// </summary>
public class SlotUI : MonoBehaviour, ISlotTooltipTarget
{
    public SlotType slotType;
    
    public Image iconImage;
    public Image backgroundImage;
    public Image equipFrame;
    public InventoryUI inventoryUI { get; private set; }
    public InventoryItemSlot InventorySlot { get; private set; }
    public ActiveItemSlot ActiveSlot { get; private set; }
    
    public InventoryItemSlot GetItemSlot() => InventorySlot;
    public RectTransform GetTooltipPosition() => (RectTransform)transform;
    public bool IsEquipSlot() => slotType == SlotType.Equip;
    
    public void Init(InventoryUI ui)
    {
        inventoryUI = ui;
        
        // 핸들러 자동 등록
        if (GetComponent<SlotDragHandler>() == null)
            gameObject.AddComponent<SlotDragHandler>();
        if (GetComponent<SlotTooltipHandler>() == null)
            gameObject.AddComponent<SlotTooltipHandler>();
        if (GetComponent<SlotClickHandler>() == null)
            gameObject.AddComponent<SlotClickHandler>();
    }
    /// <summary>
    /// 슬롯 데이터에 따라 UI를 갱신
    /// </summary>
    public void Set(InventoryItemSlot slot)
    {
        InventorySlot = slot;
        ActiveSlot = null;
        RefreshVisual();
    }
    public void Set(ActiveItemSlot slot)
    {
        ActiveSlot = slot;
        InventorySlot = null;
        RefreshVisual();
    }
    
    /// <summary>
    /// 현재 슬롯 데이터에 따라 아이콘과 수량 등의 시각적 요소를 갱신
    /// </summary>
    public void RefreshVisual()
    {
        //액티브 아이템
        if (slotType == SlotType.ActiveItem)
        {
            if (ActiveSlot == null || ActiveSlot.IsEmpty)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
                return;
            }

            iconImage.sprite = Resources.Load<Sprite>(ActiveSlot.ActiveItem.IconPath);
            iconImage.enabled = true;
            return;
        }

        // 일반 아이템
        if (InventorySlot == null || InventorySlot.IsEmpty)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;

            if (slotType == SlotType.Equip && equipFrame != null)
            {
                equipFrame.gameObject.SetActive(false);
            }

            return;
        }

        iconImage.sprite = Resources.Load<Sprite>(InventorySlot.InventoryItem.IconPath);
        iconImage.enabled = true;
        //장비 프레임 활성화
        if (slotType == SlotType.Equip && equipFrame != null)
        {
            equipFrame.gameObject.SetActive(true);
        }
    }
    
}
