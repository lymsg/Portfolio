using System.Collections.Generic;
using UnityEngine;

public class PassiveItemBox : MonoBehaviour,IInteractable
{
    [SerializeField] private string interactionPrompt = "[F] 열기";
    [SerializeField] private Transform promptPivot;
    public ItemDataTable itemDataTable;
    
    private List<int> possibleItemIds = new List<int>(); // 랜덤 대상 ID 목록

    private int? fixedPicekedId; //한번 뽑히면 저장되는 아이템id값
    
    public string InteractionPrompt => interactionPrompt;
    public Transform PromptPivot => promptPivot;
    
    private bool hasPlayedAnimation = false;
    public bool HasPlayedAnimation => hasPlayedAnimation;
    
    public void MarkAnimationPlayed()
    {
        hasPlayedAnimation = true;
    }
    private void Awake()
    {
        LoadPossibleItemIds();
    }
    private void LoadPossibleItemIds()
    {
        possibleItemIds.Clear();

        if (itemDataTable == null)
        {
            Debug.LogError("[PassiveItemBox] ItemDataTable이 할당되지 않았습니다.");
            return;
        }

        foreach (var item in itemDataTable.DataDic.Values)
        {
            possibleItemIds.Add(item.ID);
        }

        if (possibleItemIds.Count == 0)
        {
            Debug.LogWarning("[PassiveItemBox] 조건에 맞는 아이템이 없습니다.");
        }
    }

    public bool CanInteract(GameObject interactor)
    {
        return interactor.GetComponent<Inventory>() != null;
    }

    public void Interact(GameObject interactor)
    {
        var inventory = interactor.GetComponent<Inventory>();
        if (inventory == null) return;

        if (possibleItemIds == null || possibleItemIds.Count == 0)
        {
            Debug.LogWarning("[PassiveItemBox] 아이템 ID 목록이 비어 있습니다.");
            return;
        }
        
        if (UIManager.Instance.IsUIOpen<TakePassiveItem>()) return;
        
        if(fixedPicekedId == null)
        {
            fixedPicekedId = PickItemByRarity();
        }
        
        int pickedID = fixedPicekedId.Value;
        var item = itemDataTable.GetDataByID(pickedID);
        if (item == null)
        {
            Debug.LogError($"[PassiveItemBox] ID {fixedPicekedId}에 해당하는 아이템을 찾을 수 없습니다.");
            return;
        }

        var takePassiveItemUI = UIManager.Instance.GetUI<TakePassiveItem>();
        takePassiveItemUI.Open(item, inventory, this);
        
        
    }
    
    private int? PickItemByRarity()
    {
        var itemTable = itemDataTable;
        if (itemTable == null)
        {
            Debug.LogError("[PassiveItemBox] ItemDataTable을 불러올 수 없습니다.");
            return null;
        }

        // 1. possibleItemIds로부터 아이템 데이터 가져오기
        List<ItemData> possibleItems = new List<ItemData>();
        foreach (int id in possibleItemIds)
        {
            var item = itemTable.GetDataByID(id);
            if (item != null)
            {
                possibleItems.Add(item);
            }
        }

        if (possibleItems.Count == 0)
        {
            Debug.LogWarning("[PassiveItemBox] 유효한 아이템이 없습니다.");
            return null;
        }

        // 2. Rarity 확률 분포 설정 (누적 확률로 사용)
        var rarityChances = new Dictionary<Rarity, float>
        {
            { Rarity.Common, 70f },
            { Rarity.Uncommon, 20f },
            { Rarity.Rare, 7f },
            { Rarity.Epic, 3f }
        };

        // 3. 누적 확률로 Rarity 뽑기
        float totalChance = 0f;
        foreach (var chance in rarityChances.Values)
            totalChance += chance;

        float roll = Random.Range(0f, totalChance);
        float cumulative = 0f;
        Rarity selectedRarity = Rarity.Common; // 기본값

        foreach (var kvp in rarityChances)
        {
            cumulative += kvp.Value;
            if (roll <= cumulative)
            {
                selectedRarity = kvp.Key;
                break;
            }
        }

        // 4. 뽑힌 Rarity에 해당하는 아이템들 필터링
        var rarityItems = possibleItems.FindAll(item => item.Rarity == selectedRarity);

        // 5. 해당 Rarity에 아이템이 없으면 낮은 Rarity로 내려감
        while (rarityItems.Count == 0 && (int)selectedRarity > (int)Rarity.Common)
        {
            selectedRarity = (Rarity)((int)selectedRarity - 1);
            rarityItems = possibleItems.FindAll(item => item.Rarity == selectedRarity);
        }

        if (rarityItems.Count == 0)
        {
            Debug.LogWarning("[PassiveItemBox] 조건에 맞는 아이템이 없습니다.");
            return null;
        }

        // 6. 최종 선택된 Rarity에서 랜덤으로 하나 선택
        int randomIndex = Random.Range(0, rarityItems.Count);
        return rarityItems[randomIndex].ID;
    }
    
    public List<int> GetPossibleItemIds()
    {
        return possibleItemIds;
    }
}
