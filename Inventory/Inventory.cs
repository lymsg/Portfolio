using System;
using System.Collections.Generic;
using UnityEngine;

public interface IInventory
{
    bool Initialized { get; }
    event Action OnInitialized;
    List<InventoryItemSlot> GetInventorySlots(bool includeEquip = false); // 장비슬롯도 같이 가져올수있게

    bool AddToInventory(ItemData item);
    bool RemoveFromInventory(ItemData item); // 필요 시
}
/// <summary>
/// 인벤토리 데이터를 관리하는 클래스.
/// 슬롯 초기화 및 아이템 추가 기능을 포함.
/// </summary>
public class Inventory:MonoBehaviour, IInventory
{
    private ItemDataTable itemDataTable;
    private ActiveItemDataTable activeItemDataTable;
    private PlayerActiveItemController PlayerActiveItemController;
    /// <summary> 실제 인벤토리 슬롯 리스트 (16칸) </summary>

    public List<InventoryItemSlot> inventorySlots = new List<InventoryItemSlot>();
    /// <summary> 실제 장비 슬롯 리스트 (4칸) </summary>

    public List<InventoryItemSlot> equipSlots = new List<InventoryItemSlot>();

    public List<ActiveItemSlot> activeItemSlots = new List<ActiveItemSlot>();

    /// <summary> 인벤토리가 초기화 완료되었는지 여부 </summary>
    public bool Initialized { get; private set; } = false;
    public event Action OnInitialized;
    public void InitializeInventory()
    {
        WaitAndInitialize();
    }

    /// <summary>
    /// 테이블 매니저가 로드 완료될 때까지 대기 후, 슬롯 초기화
    /// </summary>
    private void WaitAndInitialize()
    {
        itemDataTable = TableManager.Instance.GetTable<ItemDataTable>();
        activeItemDataTable = TableManager.Instance.GetTable<ActiveItemDataTable>();
        PlayerActiveItemController = transform.GetComponent<PlayerActiveItemController>();
        Init();
        Initialized = true;
        OnInitialized?.Invoke();
    }

    /// <summary>
    /// 인벤토리 슬롯을 초기화하고 비워진 상태로 설정
    /// </summary>
    public void Init()
    {
        inventorySlots.Clear();
        for(int i = 0; i < 16; i++)
            inventorySlots.Add(new InventoryItemSlot());

        equipSlots.Clear();
        for(int i = 0; i < 4; i++)
            equipSlots.Add(new InventoryItemSlot());

        activeItemSlots.Clear();
        for(int i = 0; i < 2; i++)
            activeItemSlots.Add(new ActiveItemSlot());
    }

    public void ApplyItemStat()
    {
        if(!GameManager.Instance.Player.TryGetComponent<PlayerController>(out var controller))
        {
            return;
        }

        foreach(var item in equipSlots)
        {
            if(!item.IsEmpty) controller.Condition.ChangeModifierValue(item.InventoryItem.ConditionType, ModifierType.ItemEnhance, item.InventoryItem.value);

        }
    }
    /// <summary>
    /// 인벤토리에 아이템을 추가할 수 있는지 확인
    /// 빈 슬롯이 있는지 여부만 확인
    /// </summary>
    /// <param name="item">추가하려는 아이템 데이터</param>
    /// <returns>아이템을 추가할 수 있으면 true, 그렇지 않으면 false</returns>
    public bool CanAddItem(ItemData item)
    {
        if(item == null)
        {
            return false;
        }

        // 빈슬롯 있는지 확인
        foreach(var slot in inventorySlots)
        {
            if(slot.IsEmpty)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 인벤토리의 빈 슬롯에 아이템을 추가
    /// 추가 불가 시 팝업 메시지 표시
    /// </summary>
    /// <param name="item">추가할 아이템 데이터</param>
    /// <returns>성공 시 true, 실패 시 false</returns>
    public bool AddToInventory(ItemData item)
    {
        // 먼저 추가 가능한지 확인
        if(!CanAddItem(item))
        {
            UIManager.Instance.ShowConfirmPopup(
                "인벤토리가 가득 차서 아이템을 추가할 수 없습니다.",
                onConfirm: () => { },
                onCancel: null,
                confirmText: "확인(Enter)"
            );
            return false;
        }
        foreach(var slot in inventorySlots)
        {
            if(slot.IsEmpty)
            {
                slot.Set(item);
                GameEvents.TriggerPassiveItemTake();
                return true;
            }
        }
        //혹시몰라 방어 코드임다
        UIManager.Instance.ShowConfirmPopup(
            "인벤토리가 가득 차서 아이템을 추가할 수 없습니다.",
            onConfirm: () => { },
            onCancel: null,
            confirmText: "확인(Enter)"
        );
        return false;
    }
    /// <summary>
    /// 선택한 인덱스의 액티브아이템 슬롯에(비어있을 때만) 아이템 추가
    /// </summary>
    /// <param name="slotIndex">장착할 슬롯 인덱스</param>
    /// <param name="activeItem">추가할 아이템</param>
    /// <param name="skillinput">스킬 입력</param>
    /// <returns>성공 시 true, 실패 시 false</returns>
    public bool AddtoActiveSlot(int slotIndex, ActiveItemData activeItem, Skillinput skillinput)
    {
        // 범위 체크
        if(slotIndex < 0 || slotIndex >= activeItemSlots.Count)
            return false;

        var slot = activeItemSlots[slotIndex];
        if(slot.IsEmpty)
        {
            slot.Set(activeItem);
            PlayerActiveItemController.TakeItem(skillinput, activeItem);
            GameEvents.TriggerActiveItemTake();
            return true;
        }
        return false; // 이미 아이템이 들어있으면 추가 불가
    }

    public void ReplaceActiveSlot(int slotIndex, ActiveItemData newItem, Skillinput skillinput)
    {
        if(slotIndex < 0 || slotIndex >= activeItemSlots.Count)
            return; // 범위 오류 방지

        // 기존 아이템 정보
        var oldItem = activeItemSlots[slotIndex].ActiveItem;

        // 새 아이템 세팅(덮어쓰기)
        activeItemSlots[slotIndex].Set(newItem);

        // 컨트롤러에도 반영
        if(skillinput != Skillinput.None)
            PlayerActiveItemController.TakeItem(skillinput, newItem);
        else
            PlayerActiveItemController.TakeItem(newItem);

        // (옵션) oldItem을 인벤토리로 돌려주거나, 파기 등 처리 가능
    }

    /// <summary>
    /// 인벤토리 슬롯 반환
    /// </summary>
    /// <returns></returns>
    public List<InventoryItemSlot> GetInventorySlots(bool includeEquip = false)
    {
        var result = new List<InventoryItemSlot>(inventorySlots);

        if(includeEquip)
            result.AddRange(equipSlots);

        return result;
    }
    /// <summary>
    /// 인벤토리에서 특정 아이템을 제거
    /// (해당 아이템이 있는 첫 번째 슬롯을 비움)
    /// </summary>
    /// <param name="item">제거할 아이템 데이터</param>
    /// <returns>성공적으로 제거되었는지 여부</returns>
    public bool RemoveFromInventory(ItemData item)
    {
        if(item == null) return false;
        foreach(var slot in inventorySlots)
        {
            if(!slot.IsEmpty && slot.InventoryItem == item)
            {
                slot.Clear();
                return true;
            }
        }
        return false;
    }

}
