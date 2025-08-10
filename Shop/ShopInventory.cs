using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// 상점 전용 인벤토리. 초기 아이템 셋업 및 IInventory 인터
/// </summary>
public class ShopInventory : MonoBehaviour,IInventory
{
    public int npcID;
    
    [Header("레어도별 등장 확률 (%)")]
    private Dictionary<Rarity, float> rarityChances = new()
    {
        { Rarity.Common, 84f },
        { Rarity.Uncommon, 10f },
        { Rarity.Rare, 5f },
        { Rarity.Epic, 1f }
    };
    
    [Header("아이템 개수 확률 (%)")]
    private readonly Dictionary<int, float> itemCountChances = new()
    {
        { 1, 30f },
        { 2, 20f },
        { 3, 15f },
        { 4, 12f },
        { 5, 10f },
        { 6, 8f },
        { 7, 4f },
        { 8, 1f }
    };
    private ItemDataTable itemDataTable;
    public List<InventoryItemSlot> inventorySlots { get; private set; } = new();

    public bool Initialized { get; private set; } = false;
    public event Action OnInitialized;
    
    private void Awake()
    {
        if (TryGetComponent<NPCController>(out var npcController))
        {
            npcID = npcController.ID;
        }
        else
        {
            Debug.LogWarning("[ShopInventory] NPCController를 찾을 수 없습니다.");
        }
    }
    /// <summary>
    /// 테이블 로드가 완료될 때까지 대기 후 상점 아이템 초기화
    /// </summary>
    private void Start()
    {
        itemDataTable = TableManager.Instance.GetTable<ItemDataTable>();

        Init();
        GenerateRandomItems();

        Initialized = true;
        OnInitialized?.Invoke();
    }

    /// <summary>
    /// 상점 인벤토리 초기화 (슬롯 클리어)
    /// </summary>
    public void Init()
    {
        inventorySlots.Clear();
    }
    
    /// <summary>
    /// 레어도 확률 기반으로 랜덤 아이템 생성
    /// </summary>
    private void GenerateRandomItems()
    {
        int itemCount = GetRandomItemCount(); // 1~8개 확률적 선택
        var allItems = itemDataTable.dataList;
        int attempts = 0;

        while (inventorySlots.Count < itemCount && attempts < 100)
        {
            attempts++;

            Rarity selectedRarity = GetRandomRarityByChance();
            var candidates = allItems.Where(i => i.Rarity == selectedRarity).ToList();
            if(candidates.Count == 0)
            {
                candidates = allItems.Where(i => i.Rarity == Rarity.Uncommon).ToList();
                if (candidates.Count == 0) continue;
            }

            var selectedItem = candidates[UnityEngine.Random.Range(0, candidates.Count)];

            if (!inventorySlots.Exists(s => s.InventoryItem == selectedItem))
            {
                inventorySlots.Add(CreateSlot(selectedItem));
            }
        }

        if(inventorySlots.Count == 0 && allItems.Count == 0)
        {
            var fallbackItem = allItems[UnityEngine.Random.Range(0, allItems.Count)];
            inventorySlots.Add(CreateSlot(fallbackItem));
        }
    }
    private int GetRandomItemCount()
    {
        float total = itemCountChances.Values.Sum();
        float roll = UnityEngine.Random.Range(0f, total);
        float current = 0f;

        foreach (var kvp in itemCountChances)
        {
            current += kvp.Value;
            if (roll <= current)
                return kvp.Key;
        }

        return 1;
    }
    /// <summary>
    /// 가중치 확률 기반으로 랜덤 레어도 반환
    /// </summary>
    private Rarity GetRandomRarityByChance()
    {
        float total = rarityChances.Values.Sum();
        float roll = UnityEngine.Random.Range(0f, total);
        float current = 0f;

        foreach (var kvp in rarityChances)
        {
            current += kvp.Value;
            if (roll <= current)
                return kvp.Key;
        }

        // 예외적으로 Common 반환
        return Rarity.Common;
    }
    
    /// <summary>
    /// 지정된 아이템과 수량으로 새로운 ItemSlot 생성
    /// </summary>
    private InventoryItemSlot CreateSlot(ItemData item)
    {
        var slot = new InventoryItemSlot();
        slot.Set(item);
        return slot;
    }
    
    
    /// <summary>
    /// 현재 보유한 모든 아이템 슬롯 반환 (IInventory 구현)
    /// </summary>
    public List<InventoryItemSlot> GetInventorySlots(bool includeEquip = false) => inventorySlots;

    /// <summary>
    /// 아이템을 상점 인벤토리에 추가 (빈 슬롯에만 추가)
    /// </summary>
    public bool AddToInventory(ItemData item)
    {
        foreach (var slot in inventorySlots)
        {
            if (slot.IsEmpty)
            {
                slot.Set(item);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 아이템을 상점 인벤토리에서 제거
    /// </summary>
    public bool RemoveFromInventory(ItemData item)
    {
        foreach (var slot in inventorySlots)
        {
            if (!slot.IsEmpty && slot.InventoryItem == item)
            {
                slot.Clear();
                return true;
            }
        }
        return false;
    }

}
