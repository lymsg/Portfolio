using UnityEngine;
using UnityEngine.EventSystems;


public interface ISlotTooltipTarget
{
    InventoryItemSlot GetItemSlot();
    bool IsEquipSlot();
}
/// <summary>
/// 슬롯에 마우스를 올렸을 때 툴팁을 표시,
/// 벗어났을 때 툴팁을 숨기는 역할을 담당
/// </summary>
public class SlotTooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private ISlotTooltipTarget tooltipTarget;

    private void Awake()
    {
        tooltipTarget = GetComponent<ISlotTooltipTarget>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipTarget == null) return;
        SoundManager.Instance.PlaySFX(GameManager.Instance.Player.transform.position,"SlotPointer");  

        var slot = tooltipTarget.GetItemSlot();
        if (slot == null || slot.IsEmpty) return;

        var tooltip = UIManager.Instance.GetUI<TooltipUI>();
        if (tooltip != null)
        {
            tooltip.Show(slot.InventoryItem, eventData.position, tooltipTarget.IsEquipSlot());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        var tooltip = UIManager.Instance.GetUI<TooltipUI>();
        if (tooltip != null) tooltip.Hide();
    }
}
