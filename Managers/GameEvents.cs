using System;
using UnityEngine;

public static class GameEvents
{
    // 몬스터 처치 시 호출
    public static event Action OnMonsterKilled;
    // TakePassiveItem UI가 열렸을 때 호출
    public static event Action OnPassiveItemUIOpened;
    // 패시브아이템 획득 시 호출
    public static event Action OnPassiveItemTake;
    // TakeActiveItem UI가 열렸을 때 호출
    public static event Action OnActiveItemUIOpened;
    // 액티브아이템 획득 시 호출
    public static event Action OnActiveItemTake;
    //액티브스킬쓸때 호출
    public static event Action OnActiveSkillUse;
    //인베토리 열때 호출
    public static event Action OnInventoryOpened;
    // 아이템 착용 시 호출
    public static event Action OnItemEquipped;

    public static void TriggerMonsterKilled()
    {
        OnMonsterKilled?.Invoke();
    }
    public static void TriggerPassiveItemUIOpened()
    {
        OnPassiveItemUIOpened?.Invoke();
    }
    public static void TriggerPassiveItemTake()
    {
        OnPassiveItemTake?.Invoke();
    }
    public static void TriggerActiveItemUIOpened()
    {
        OnActiveItemUIOpened?.Invoke();
    }
    public static void TriggerActiveItemTake()
    {
        OnActiveItemTake?.Invoke();
    }

    public static void TriggerActiveSkillUse()
    {
        OnActiveSkillUse?.Invoke();
    }
    public static void TriggerInventoryOpened()
    {
        OnInventoryOpened?.Invoke();
    }
    public static void TriggerItemEquipped()
    {
        OnItemEquipped?.Invoke();
    }
}
