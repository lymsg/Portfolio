using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상점 매니저: 플레이어와 상점 간의 거래를 처리하고, 결과에 따른 UI 피드백을 제공.
/// </summary>
public class ShopManager:MonoBehaviour
{
    private IInventory playerInventory;
    
    private void Start()
    {
        StartCoroutine(InitializeShopManager());
    }
    private IEnumerator InitializeShopManager()
    {
        yield return new WaitUntil(() => GameManager.Instance != null && GameManager.Instance.Player != null);
        SetupInventory();
    }
    
    /// <summary>
    /// 플레이어 오브젝트에서 직접 인벤토리 참조
    /// </summary>
    private void SetupInventory()
    {
        var player = GameManager.Instance?.Player;

        if (player == null)
        {
            Debug.LogError("GameManager.Instance.Player가 null입니다.");
            return;
        }

        playerInventory = player.GetComponent<Inventory>();

        if (playerInventory == null)
        {
            Debug.LogError("Player 오브젝트에서 Inventory 컴포넌트를 찾을 수 없습니다.");
        }
    }
    
    /// <summary>
    /// 선택된 아이템으로 거래를 시도.
    /// 골드와 인벤토리 공간을 확인하고, 거래 성공/실패 시 팝업 UI로 결과를 알림.
    /// </summary>
    /// <param name="sellItems">플레이어가 판매할 아이템 슬롯 목록</param>
    /// <param name="buyItems">플레이어가 구매할 아이템 슬롯 목록</param>
    /// <param name="result">거래 결과 메시지</param>
    /// <returns>거래 성공 시 true, 실패 시 false</returns>
    public bool TryExecuteTransaction(List<InventoryItemSlot> sellItems, List<InventoryItemSlot> buyItems, out string result)
    {
        
        sellItems ??= new List<InventoryItemSlot>();
        buyItems ??= new List<InventoryItemSlot>();

        //sellItems와 buyItems가 모두 비어있을 경우 처리
        if (sellItems.Count == 0 && buyItems.Count == 0)
        {
            result = "선택한 아이템이 없습니다!";
            UIManager.Instance.ShowConfirmPopup(result, onConfirm: () => { }, confirmText: "확인(Enter)");
            return false;
        }
        
        PlayerController playerController = GameManager.Instance.Player.GetComponent<PlayerController>();
        BaseCondition playerCondition = playerController?.Condition;
        
        if (playerCondition == null)
        {
            result = "거래 실패: 플레이어 상태 정보가 초기화되지 않았습니다.";
            Debug.LogError(result);
            return false;
        }
        
        int sellTotal = CalculateTotalPrice(sellItems, true, out string sellError);
        if (sellTotal < 0)
        {
            result = sellError;
            return false;
        }

        int buyTotal = CalculateTotalPrice(buyItems, false, out string buyError);
        if (buyTotal < 0)
        {
            result = buyError;
            return false;
        }
        if (buyItems.Count > 0) 
        {
            if (playerInventory == null)
            {
                result = "거래 실패: 플레이어 인벤토리를 찾을 수 없습니다.";
                Debug.LogError(result);
                return false;
            }

            // 구매하려는 아이템의 개수만큼 인벤토리에 빈 슬롯이 있는지 확인
            int requiredSlots = buyItems.Count;
            int availableEmptySlots = 0;

            foreach (var slot in playerInventory.GetInventorySlots()) // 모든 인벤토리 슬롯을 가져와 확인
            {
                if (slot.IsEmpty)
                {
                    availableEmptySlots++;
                }
            }

            if (availableEmptySlots < requiredSlots)
            {
                result = $"인벤토리에 {requiredSlots}개의 아이템을 구매할 공간이 부족합니다. (현재 빈 슬롯: {availableEmptySlots}개)";
                Debug.LogWarning(result);
                UIManager.Instance.ShowConfirmPopup(result, onConfirm: () => { }, confirmText: "확인(Enter)");
                return false;
            }
        }
        if(!playerCondition.CurrentConditions.TryGetValue(ConditionType.Gold, out float currentGold))
        {
            result = "골드 정보 없음";
            return false;
        }

        float newGold = currentGold + sellTotal - buyTotal;
        if(newGold < 0)
        {
            result = "골드 부족";
            UIManager.Instance.ShowConfirmPopup(result, onConfirm: () => { }, confirmText: "확인(Enter)");

            return false;
        }

        // 골드 반영
        playerCondition.ChangeGold(newGold - currentGold);

        // 아이템 제거/추가 (기존 메서드 사용)
        foreach(var slot in sellItems)
            playerInventory.RemoveFromInventory(slot.InventoryItem);

        foreach(var slot in buyItems)
            playerInventory.AddToInventory(slot.InventoryItem);

        result = $"거래 완료! 현재 골드: {newGold}";
        UIManager.Instance.ShowConfirmPopup(result, onConfirm: () => { }, confirmText: "확인(Enter)");
        return true;
    }
    
    /// <summary>
    /// 아이템 슬롯 목록의 총 가격을 계산
    /// </summary>
    /// <param name="slots">계산할 아이템 슬롯 목록</param>
    /// <param name="isSell">판매 가격 계산 시 true, 구매 가격 계산 시 false</param>
    /// <returns>총 가격</returns>
    private int CalculateTotalPrice(List<InventoryItemSlot> slots, bool isSell, out string error)
    {
        int total = 0;
        error = string.Empty;

        foreach (var slot in slots)
        {
            total += isSell ? slot.InventoryItem.sellPrice : slot.InventoryItem.buyPrice;
        }

        return total;
    }
}
