[System.Serializable]

public class ActiveItemSlot
{
    public ActiveItemData ActiveItem { get; private set; }
    public bool IsEmpty => ActiveItem == null;

    /// <summary>
    /// 슬롯에 아이템과 수량을 설정
    /// </summary>
    public void Set(ActiveItemData  item)
    {
        ActiveItem = item;
    }
    
    /// <summary>
    /// 슬롯을 비움
    /// </summary>
    public void Clear()
    {
        ActiveItem = null;
    }

    public string GetDescription() => ActiveItem?.description ?? "";
}
