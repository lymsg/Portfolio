
using UnityEngine;

/// <summary>
/// 장비 아이템 교체 및 능력치 적용을 담당하는 매니저 클래스.
/// 플레이어 상태(Stat)에 아이템 효과를 적용하거나 제거한다.
/// </summary>
public class EquipmentManager : MonoSingleton<EquipmentManager>
{
    private GameObject cachedPlayer;
    private Inventory playerInventory;
    
    private BaseCondition playerCondition;
    private BaseCondition PlayerCondition
    {
        get
        {
            var currentPlayer = GameManager.Instance.Player;
            if(cachedPlayer != currentPlayer || playerCondition == null)
            {
                cachedPlayer = currentPlayer;
                playerCondition = null;

                if(cachedPlayer != null && cachedPlayer.TryGetComponent<PlayerController>(out var controller))
                {
                    playerCondition = controller.Condition;
                }
            }
            return playerCondition;
        }
    }
    protected override void Awake()
    {
        base.Awake(); // MonoSingleton<T> 내부 싱글톤 초기화 로직 실행
    }
    
    /// <summary>
    /// 아이템 슬롯 간 교체(Swap)를 수행하고, 기존 능력치를 제거한 뒤 교체된 아이템의 능력치를 다시 적용한다.
    /// </summary>
    /// <param name="slotA">첫 번째 슬롯</param>
    /// <param name="typeA">첫 번째 슬롯 타입</param>
    /// <param name="slotB">두 번째 슬롯</param>
    /// <param name="typeB">두 번째 슬롯 타입</param>
    public void SwapItemEffects(InventoryItemSlot slotA, SlotType typeA, InventoryItemSlot slotB, SlotType typeB)
    {
        if (PlayerCondition == null) return;
        if(typeA == SlotType.ActiveItem || typeB == SlotType.ActiveItem) return;
        // 기존 스탯 제거
        RemoveItemStat(slotA.InventoryItem, typeA);
        RemoveItemStat(slotB.InventoryItem, typeB);

        // 스왑
        var tempItem = slotA.InventoryItem;
        slotA.Set(slotB.InventoryItem);
        slotB.Set(tempItem);

        // 스탯 재적용
        ApplyItemStat(slotA.InventoryItem, typeA);
        ApplyItemStat(slotB.InventoryItem, typeB);
    }
    
    /// <summary>
    /// 아이템 효과를 플레이어 능력치에 적용한다. (장비 슬롯일 경우에만 적용)
    /// </summary>
    /// <param name="item">적용할 아이템</param>
    /// <param name="type">해당 슬롯 타입</param>
    private void ApplyItemStat(ItemData item, SlotType type)
    {
        if (item == null || type != SlotType.Equip) return;
        PlayerCondition.ChangeModifierValue(item.ConditionType, ModifierType.ItemEnhance, item.value);
        GameEvents.TriggerItemEquipped();
    }
    
    /// <summary>o
    /// 아이템 효과를 플레이어 능력치에서 제거한다. (장비 슬롯일 경우에만 제거)
    /// </summary>
    /// <param name="item">제거할 아이템</param>
    /// <param name="type">해당 슬롯 타입</param>
    private void RemoveItemStat(ItemData item, SlotType type)
    {
        if (item == null || type != SlotType.Equip) return;
        PlayerCondition.ChangeModifierValue(item.ConditionType, ModifierType.ItemEnhance, -item.value);
    }
    /// <summary>
    /// 아이템 장착 해제처리
    /// </summary>
    /// <param name="equipSlot"></param>
    public void UnEquip(InventoryItemSlot equipSlot)
    {
        if(PlayerCondition == null || equipSlot == null || equipSlot.IsEmpty)
            return;

        var item = equipSlot.InventoryItem;
        // 능력치 제거
        RemoveItemStat(item, SlotType.Equip);

        // 슬롯 비우기
        equipSlot.Clear();
        
        if (playerInventory == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.Player != null)
            {
                playerInventory = GameManager.Instance.Player.GetComponent<Inventory>();
            }

            if (playerInventory == null)
            {
                Debug.LogWarning("[EquipmentManager] 플레이어의 Inventory 컴포넌트를 찾을 수 없습니다.");
                return;
            }
        }
        // 인벤토리에 아이템 넣기
        if(playerInventory != null)
        {
            bool success = playerInventory.AddToInventory(item);
            if(!success)
            {
                Debug.LogWarning("[EquipmentManager] 인벤토리가 가득 차서 장착 해제 아이템을 넣을 수 없습니다.");
                // 필요 시 아이템 복구 또는 메시지 처리
            }
        }
    }


    public void RefreshPlayer()
    {
        cachedPlayer = null;
        playerCondition = null;
        playerInventory = null;
    }
}
