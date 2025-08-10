using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 상점 UI. 판매/구매 슬롯 생성, 거래 처리, 금액 계산 및 UI 갱신을 담당
/// </summary>
public class ShopUI : UIBase
{
    public override string UIName => this.GetType().Name;

    private PlayerController player;
    private IInventory playerInventory => GameManager.Instance.Player?.GetComponent<Inventory>();
    private BaseCondition playerCondition => player?.Condition;
    
    [SerializeField] private ShopInventory shopInventoryRaw;
    private IInventory shopInventory => shopInventoryRaw;

    public Transform sellParent;
    public Transform buyParent;
    public GameObject shopSlotPrefab;

    public TextMeshProUGUI sellTotalText;
    public TextMeshProUGUI buyTotalText;
    public TextMeshProUGUI calculateText;
    public TextMeshProUGUI curGoldText;
    public TextMeshProUGUI resultGoldText;
    public Image InfoImage;
    
    public Button dealBtn;
    public Button exitBtn;
    
    private List<ShopSlotUI> sellSlots = new();
    private List<ShopSlotUI> buySlots = new ();
    
    private HashSet<InventoryItemSlot> selectedSellItems = new(); //선택된 슬롯 기억리스트 (슬롯과 그 안에 아이템 함께기억)
    private HashSet<InventoryItemSlot> purchaseSlots = new(); //거래된 슬롯 기억리스트

    private ShopManager shopManager;

    private void Start()
    {
        shopManager = FindObjectOfType<ShopManager>();
    }

    public void OpenWithInventory(ShopInventory inventory)
    {
        if (gameObject.activeSelf) return;
        
        shopInventoryRaw = inventory;
        base.Open();
        UIManager.Instance.uiStack.Push(this);
        
        SoundManager.Instance.PlaySFX(GameManager.Instance.Player.transform.position,"ShopOpen");
        
        dealBtn.onClick.RemoveAllListeners();
        dealBtn.onClick.AddListener(ExecuteTransaction);
        exitBtn.onClick.RemoveAllListeners();
        exitBtn.onClick.AddListener(Close);
        
        StartCoroutine(WaitForInitAndBind());
    }
    /// <summary>
    /// 상점 UI 닫기 시 선택된 아이템 상태 초기화
    /// </summary>
    public override void Close()
    {
        SoundManager.Instance.PlaySFX(GameManager.Instance.Player.transform.position,"ShopClose");
        base.Close();
        selectedSellItems.Clear();
        UnsubscribeGoldUpdate();
        
        // 툴팁숨김
        var tooltip = UIManager.Instance.GetUI<TooltipUI>();
        if (tooltip != null)
            tooltip.Hide();
    }
    private IEnumerator WaitForInitAndBind()
    {
        yield return new WaitUntil(() =>
            GameManager.Instance != null && GameManager.Instance.Player != null && // 플레이어 존재 확인
            GameManager.Instance.Player.GetComponent<PlayerController>() != null && // PlayerController 존재 확인
            shopInventoryRaw != null && shopInventoryRaw.Initialized &&
            playerInventory != null && playerInventory.Initialized);

        player = GameManager.Instance.Player.GetComponent<PlayerController>();
        
        yield return new WaitUntil(() => player.Condition != null);
        
        InitAndGenerate();

        if (playerCondition != null)
        {
            if(playerCondition.statModifiers.ContainsKey(ConditionType.Gold))
            {
                playerCondition.statModifiers[ConditionType.Gold] -= UpdateGoldUI; // 기존 구독 해제
                playerCondition.statModifiers[ConditionType.Gold] += UpdateGoldUI; // 새롭게 구독
            }

            UpdateGoldUI(); // 초기 UI 세팅
        }
        else
        {
            Debug.LogError("ShopUI: playerCondition을 찾을 수 없습니다.");
        }
    }
    private void InitAndGenerate()
    {
        GenerateSlots();
        UpdateTotalPrices();
    }
    /// <summary>
    /// 기존 슬롯 UI 제거 후, 판매/구매 슬롯을 다시 생성
    /// </summary>
    /// 
    public void GenerateSlots()
    {
        ClearChildren(sellParent);
        ClearChildren(buyParent);
        sellSlots.Clear();
        buySlots.Clear();

        CreateSlots(playerInventory.GetInventorySlots(true), sellParent, true, sellSlots);
        CreateSlots(shopInventory.GetInventorySlots(), buyParent, false, buySlots);
    }
    
    /// <summary>
    /// 특정 슬롯 리스트로 상점 슬롯 UI 생성 및 리스트에 저장, 이전 선택 상태 반영
    /// </summary>
    /// <param name="slots">아이템 슬롯들</param>
    /// <param name="parent">UI 부모 오브젝트</param>
    /// <param name="isPlayer">플레이어 인벤토리인지 여부</param>
    /// <param name="targetList">슬롯 저장 리스트</param>
    private void CreateSlots(List<InventoryItemSlot> slots, Transform parent, bool isPlayer, List<ShopSlotUI> targetList)
    {
        foreach (var itemSlot in slots)
        {
            if (!itemSlot.IsEmpty)
            {
                var go = Instantiate(shopSlotPrefab, parent);
                var slotUI = go.GetComponent<ShopSlotUI>();
                
                slotUI.Set(itemSlot, isPlayer,this);
                targetList.Add(slotUI);
                slotUI.OnClicked += HandleSlotClick;
                
                if(!isPlayer && purchaseSlots.Contains(itemSlot))
                {
                    slotUI.SetInteractable(false);
                }
                if (isPlayer && selectedSellItems.Contains(itemSlot))
                {
                    slotUI.ForceSelect();
                }
            }
        }
    }
    private void HandleSlotClick(ShopSlotUI slotUI)
    {
        var slot = slotUI.InventoryItemSlot;

        // 플레이어 아이템이고 장착 상태라면
        if (slotUI.isPlayerSlot && IsEquippedSlot(slot))
        {
            UIManager.Instance.ShowConfirmPopup(
                "장착한 아이템입니다. 장착 해제하시겠습니까?",
                onConfirm: () =>
                {
                    RememberSelectedItem(slot); // 거래 대상으로 등록
                    EquipmentManager.Instance.UnEquip(slot); // 장착 해제
                    RefreshAllUI(); // 전체 UI 다시 그림
                },
                onCancel: () =>
                { });
        }
        else
        {
            // 선택 상태만 변경
            slotUI.ToggleSelect();
            UpdateTotalPrices();
        }
    }
    /// <summary>
    /// 선택된 슬롯들의 판매/구매 금액 계산 후 텍스트 갱신
    /// </summary>
    public void UpdateTotalPrices()
    {
        int sellTotal = 0;
        int buyTotal = 0;

        foreach (var s in sellSlots)
            if(s.IsSelected && s.InventoryItemSlot != null && !s.InventoryItemSlot.IsEmpty)
            {
                sellTotal += s.InventoryItemSlot.InventoryItem.sellPrice;
            }

        foreach (var b in buySlots)
            if(b.IsSelected  && b.InventoryItemSlot != null && !b.InventoryItemSlot.IsEmpty)
            {
                buyTotal += b.InventoryItemSlot.InventoryItem.buyPrice;
            }

        sellTotalText.text = $"판매 금액: {sellTotal} G";
        buyTotalText.text = $"구매 금액: {buyTotal} G";
        
        float calculatePrice = sellTotal - buyTotal;
        float currentGold = playerCondition?.GetTotalCurrentValue(ConditionType.Gold) ?? 0;
        float resultGold = currentGold + calculatePrice;
        
        string sign = calculatePrice >= 0 ? "+" : "-";
        string calcPriceText = $"{sign}{Mathf.Abs(calculatePrice)}";
        
        string currentColor = "#FFD700"; 
        string resultColor = resultGold >= 0 ? "#00FF00" : "#FF4444"; // 초록 또는 빨강
        
        calculateText.text = 
            $"<color={currentColor}>{currentGold}G</color>{calcPriceText}G=<color={resultColor}>{resultGold}G</color>";
        
        // 거래 후 금액 텍스트 색상 동기화
        if (resultGoldText != null)
        {
            Color parsedColor;
            if (ColorUtility.TryParseHtmlString(resultColor, out parsedColor))
            {
                resultGoldText.color = parsedColor;
            }
        }
        
        // 선택되었을 때만 텍스트 보이도록
        bool anySelected = sellTotal > 0 || buyTotal > 0;
        calculateText.gameObject.SetActive(anySelected);
        InfoImage.gameObject.SetActive(anySelected);
    }
    
    /// <summary>
    /// 선택된 슬롯을 기반으로 거래 시도 → 성공 시 거래 아이템 반영 및 UI 갱신
    /// </summary>
    public void ExecuteTransaction()
    {
        var sellSlotSelected = sellSlots.FindAll(s => s.IsSelected);
        var buySlotSelected = buySlots.FindAll(s => s.IsSelected);
        
        var sellItems = sellSlotSelected.ConvertAll(s => s.InventoryItemSlot);
        var buyItems = buySlotSelected.ConvertAll(s => s.InventoryItemSlot);
        
        if (shopManager.TryExecuteTransaction(sellItems, buyItems, out var result))
        {
            SoundManager.Instance.PlaySFX(GameManager.Instance.Player.transform.position,"Deal");
            foreach(var slot in buySlotSelected)
            {
                purchaseSlots.Add(slot.InventoryItemSlot);
            }
            selectedSellItems.Clear();
            RefreshAllUI();
        }
        else
        {
            Debug.LogWarning(result);
        }

        UpdateTotalPrices();
    }
    
    /// <summary>
    /// 부모 Transform의 모든 자식 오브젝트 제거 (슬롯 재생성 전 사용)
    /// </summary>
    private void ClearChildren(Transform t)
    {
        foreach (Transform child in t)
            Destroy(child.gameObject);
    }
    
    /// <summary>
    /// 현재 플레이어 골드를 UI에 표시
    /// </summary>
    private void UpdateGoldUI()
    {
        if (playerCondition != null)
        {
            curGoldText.text = $"보유골드: {playerCondition.GetTotalCurrentValue(ConditionType.Gold)}G";
        }
        else
        {
            curGoldText.text = "보유골드: ???";
        }
    }
    /// <summary>
    /// UI갱신메소드 모음(슬롯, 가격, 골드 UI 전부 다시 갱신)
    /// </summary>
    public void RefreshAllUI()
    {
        GenerateSlots();
        UpdateTotalPrices();
        UpdateGoldUI();
    }
    /// <summary>
    /// 주어진 슬롯이 장착 슬롯에 포함되어 있는지 여부 확인
    /// </summary>
    /// <param name="slot">선택한 슬롯</param>
    /// <returns>그 슬롯이 equipslots인지 불값 반환</returns>
    public bool IsEquippedSlot(InventoryItemSlot slot)
    {
        var inventory = GameManager.Instance.Player?.GetComponent<Inventory>();
        return inventory != null && inventory.equipSlots.Contains(slot);
    }
    /// <summary>
    /// 특정 슬롯+아이템 쌍을 선택된 리스트에 등록
    /// </summary>
    public void RememberSelectedItem(InventoryItemSlot slot)
    {
        if (slot != null)
            selectedSellItems.Add(slot);
    }
    /// <summary>
    /// 특정 슬롯+아이템 쌍을 선택된 리스트에서 제거
    /// </summary>
    public void ForgetSelectedItem(InventoryItemSlot slot)
    {
        if (slot != null)
            selectedSellItems.Remove(slot);
    }
    
    private void UnsubscribeGoldUpdate()
    {
        if (playerCondition != null && playerCondition.statModifiers.ContainsKey(ConditionType.Gold))
        {
            playerCondition.statModifiers[ConditionType.Gold] -= UpdateGoldUI;
        }
    }
}
