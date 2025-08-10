
/// <summary>
/// 하나의 인벤토리 슬롯에 해당하는 데이터 구조.
/// 아이템 정보 포함
/// </summary>
/// 
[System.Serializable]
public class InventoryItemSlot
{
    /// <summary> 슬롯에 담긴 아이템 데이터 </summary>
    public ItemData InventoryItem { get; private set; }
    
    /// <summary> 슬롯이 비어있는지 여부 </summary>
    public bool IsEmpty => InventoryItem == null;

    /// <summary>
    /// 슬롯에 아이템과 수량을 설정
    /// </summary>
    public void Set(ItemData item)
    {
        InventoryItem = item;
    }
    
    /// <summary>
    /// 슬롯을 비움
    /// </summary>
    public void Clear()
    {
        InventoryItem = null;
    }
    
    // /// <summary>
    // /// 슬롯의 아이템이 특정 아이템과 동일한지 비교
    // /// </summary>
    // /// <param name="target">비교할 아이템</param>
    // /// <returns>동일 여부</returns>
    // public bool IsSameItem(ItemData target)
    // {
    //     return !IsEmpty && Item.ID == target.ID;
    // }

    /// <summary>
    /// 현재 슬롯에 있는 아이템의 설명 반환 (툴팁 등에서 사용)
    /// </summary>
    public string GetDescription(SlotType type)
    {
        if (InventoryItem == null) return "";
        return type == SlotType.Equip ? $"<color=#ff0000>[장착중]</color>\n{InventoryItem.description}" : InventoryItem.description;
    }
}
