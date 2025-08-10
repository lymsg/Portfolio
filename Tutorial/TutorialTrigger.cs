using System;
using UnityEngine;
using UnityEngine.Events;

public class TutorialTrigger : MonoBehaviour
{
    [Header("고유 ID")]
    public string triggerID;

    public string GetTriggerID() => triggerID;
    
    [Header("트리거 조건")]
    public bool activateOnce = true; 
    private bool hasActivated = false; 
    private TutorialStep ownerStep;
    [Header("조건만족할때 true되는 오브젝트들")]
    public GameObject monster_1;
    public GameObject monster_2;
    public GameObject activeItemBox;
    public event Action<TutorialTrigger> OnTriggerCompletedByPlayer; 
    
    [Header("이벤트")]
    public UnityEvent onTriggerEnterEvent; 
    public UnityEvent onTriggerExitEvent;  
    
    void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"'{gameObject.name}'에 Collider가 없습니다. TutorialTrigger는 Collider가 필요합니다.", this);
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"'{gameObject.name}'의 Collider가 Is Trigger로 설정되어 있지 않습니다. 설정해주세요.", this);
        }
    }
    // 플레이어가 이 트리거 영역에 진입했을 때 호출
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(TagName.Player) && (!activateOnce || !hasActivated))
        {
            onTriggerEnterEvent?.Invoke();
            hasActivated = true;
            if (triggerID == "4" && monster_1 != null)
            {
                monster_1.SetActive(true);
            }
            if (triggerID == "5" && activeItemBox != null)
            {
                activeItemBox.SetActive(true);
            }
            CompleteTrigger();
        }
    }

    // 플레이어가 이 트리거 영역을 벗어났을 때 호출
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(TagName.Player))
        {
            onTriggerExitEvent?.Invoke();
        }
    }
    public void SetOwnerStep(TutorialStep step)
    {
        ownerStep = step;
    }
    
    // 외부에서 이 트리거의 튜토리얼 조건을 강제로 완료할 때 호출
    public void CompleteTrigger()
    {
        OnTriggerCompletedByPlayer?.Invoke(this);
        gameObject.SetActive(false);
        ownerStep?.CompleteStep(); // 여기서 직접 스텝 완료 호출
    }
}
