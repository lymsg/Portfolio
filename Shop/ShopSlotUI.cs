using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

/// <summary>
/// 상점 슬롯 UI 컴포넌트. 아이템 아이콘/가격 출력, 클릭 선택/장착 해제 처리 및 선택 상태 유지 기능을 담당.
/// </summary>
public class ShopSlotUI : MonoBehaviour,IPointerClickHandler, ISlotTooltipTarget
{
    [Header("UI")]
    public Image iconImage;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI blockText;
    public TextMeshProUGUI equipText;
    public Image BackgroundImage;
    
    private ShopUI shopUI;

    private InventoryItemSlot inventoryItemSlot;
    public RectTransform TooltipAnchor;
    
    public bool isPlayerSlot;
    private bool isSelected;
    private bool isInteractable = true;

    public Color defaultColor = Color.white;
    public Color selectedColor = Color.red;
    public Color disabledColor = Color.gray;
    public bool IsSelected => isSelected;
    public InventoryItemSlot InventoryItemSlot => inventoryItemSlot;
    
    public InventoryItemSlot GetItemSlot() => InventoryItemSlot;
    public bool IsEquipSlot() => isPlayerSlot && equipText != null && equipText.gameObject.activeSelf;
    public event Action<ShopSlotUI> OnClicked;
    
    /// <summary>
    /// 아이템 슬롯 UI 초기화: 아이템 정보, 소유자 타입, 참조 설정
    /// </summary>
    public void Set(InventoryItemSlot slot, bool isPlayerSlot,ShopUI shopUI)
    {
        this.inventoryItemSlot = slot;
        this.isPlayerSlot = isPlayerSlot;
        this.shopUI = shopUI;
        isSelected = false;
        isInteractable = true;
        // 장착중인 슬롯이면 equipText 활성화
        if (isPlayerSlot && shopUI.IsEquippedSlot(inventoryItemSlot))
        {
            equipText.gameObject.SetActive(true);
        }
        else
        {
            equipText.gameObject.SetActive(false);
        }
        Refresh();
    }
    
    /// <summary>
    /// 슬롯 UI를 새로 그리며 아이콘, 가격, 수량 표시 업데이트
    /// </summary>
    private void Refresh()
    {
        if (inventoryItemSlot == null || inventoryItemSlot.IsEmpty)
        {
            iconImage.enabled = false;
            priceText.text = "";
            return;
        }

        iconImage.enabled = true;
        iconImage.sprite = Resources.Load<Sprite>(inventoryItemSlot.InventoryItem.IconPath);
        
        priceText.text = (isPlayerSlot ? inventoryItemSlot.InventoryItem.sellPrice : inventoryItemSlot.InventoryItem.buyPrice) + " G";
    }
    
    /// <summary>
    /// 슬롯 클릭 시 장착 해제 여부를 체크하고, 선택 또는 해제 팝업을 실행
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if(!isInteractable)
            return;
        SoundManager.Instance.PlaySFX(GameManager.Instance.Player.transform.position,"ShopSlotClick");
        // 장착중인 아이템인지 확인 (플레이어 슬롯일 때만)
        OnClicked?.Invoke(this); 
    }
    
    /// <summary>
    /// 슬롯을 선택 불가능한 상태로 변경하고 배경/텍스트를 회색 처리
    /// </summary>
    /// <param name="interactable">선택 가능 여부</param>
    public void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
        isSelected = false;
        BackgroundImage.color = interactable ? defaultColor : disabledColor;
        blockText.gameObject.SetActive(!interactable);
    }
    // /// <summary>
    // /// 선택 상태 토글 및 UI 반영, 선택된 슬롯을 ShopUI에 등록/해제
    // /// </summary>
    // private void SelectSlot()
    // {
    //     isSelected = !isSelected;
    //     BackgroundImage.color = isSelected ? selectedColor : defaultColor;
    //     if (isPlayerSlot)
    //     {
    //         if (isSelected)
    //             shopUI.RememberSelectedItem(inventoryItemSlot); // 선택 기억
    //         else
    //             shopUI.ForgetSelectedItem(inventoryItemSlot);   // 선택 해제
    //     }
    //     shopUI.UpdateTotalPrices();
    // }
    public void ToggleSelect()
    {
        isSelected = !isSelected;

        // UI 색상 변경
        BackgroundImage.color = isSelected ? selectedColor : defaultColor;

        // 플레이어 슬롯이면 기억/해제 처리
        if (isPlayerSlot)
        {
            if (isSelected)
                shopUI.RememberSelectedItem(inventoryItemSlot);
            else
                shopUI.ForgetSelectedItem(inventoryItemSlot);
        }
    }
    /// <summary>
    /// 강제로 선택된 상태로 설정 (색상 변경만 적용됨)
    /// </summary>
    public void ForceSelect()
    {
        isSelected = true;
        BackgroundImage.color = selectedColor;
    }
}
