using UnityEngine;

public class Items : MonoBehaviour , IInteractable
{
    public int ID;

    [SerializeField]private ItemData data;
    
    [SerializeField] private string interactionPrompt = "[F] 줍기";
    [SerializeField] private Transform promptPivot;
    
    public string InteractionPrompt => interactionPrompt;
    public Transform PromptPivot => promptPivot;

    private void Start()
    {
        data = TableManager.Instance.GetTable<ItemDataTable>().GetDataByID(ID);
    }
    public bool CanInteract(GameObject interactor) => true;

    public void Interact(GameObject interactor)
    {
        var inventory = interactor.GetComponent<IInventory>();
        if (inventory != null && data != null)
        {
            bool success = inventory.AddToInventory(data);
            if (success)
            {
                InteractionController controller = interactor.GetComponent<InteractionController>();
                if (controller != null)
                {
                    // 직접 부모를 끊고 비활성화
                    Transform textTransform = controller.GetInteractTextTransform();
                    if (textTransform != null)
                    {
                        textTransform.SetParent(null); 
                        textTransform.gameObject.SetActive(false); 
                    }
                }
                Destroy(this.gameObject); // 아이템 오브젝트 삭제
            }
            else
            {
                Debug.LogWarning("[Items] 인벤토리에 공간이 없습니다.");
            }
        }
    }
}
