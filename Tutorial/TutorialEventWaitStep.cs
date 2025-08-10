using UnityEngine;
[CreateAssetMenu(fileName = "NewTutorialEventWaitStep", menuName = "Tutorial/Event Wait Step")]
public class TutorialEventWaitStep : TutorialStep
{
     [Header("Event Wait Settings")]
    [TextArea] public string waitMessage;
     
    public enum GameEventTypeToWait
    {
        None,
        OnMonsterKilled,
        OnPassiveItemUIOpened,
        OnPassiveItemTake,
        OnActiveItemUIOpened,
        OnActiveItemTake,
        OnActiveSkillUse,
        OnInventoryOpened,
        OnItemEquipped
    }

    public GameEventTypeToWait eventToWait;

    public override void Activate()
    {
        base.Activate();
       
        TutorialManager.Instance.NotifyStepActivated(waitMessage, QuestDescription);

        
        switch (eventToWait)
        {
            case GameEventTypeToWait.OnMonsterKilled:
                GameEvents.OnMonsterKilled += OnEventTriggered;
                break;
            case GameEventTypeToWait.OnPassiveItemUIOpened:
                GameEvents.OnPassiveItemUIOpened += OnEventTriggered;
                break;
            case GameEventTypeToWait.OnPassiveItemTake:
                GameEvents.OnPassiveItemTake += OnEventTriggered;
                break;
            case GameEventTypeToWait.OnActiveItemUIOpened:
                GameEvents.OnActiveItemUIOpened += OnEventTriggered;
                break;
            case GameEventTypeToWait.OnActiveItemTake:
                GameEvents.OnActiveItemTake += OnEventTriggered;
                break;
            case GameEventTypeToWait.OnActiveSkillUse:
                GameEvents.OnActiveSkillUse += OnEventTriggered;
                break;
            case GameEventTypeToWait.OnInventoryOpened:
                GameEvents.OnInventoryOpened += OnEventTriggered;
                break;
            case GameEventTypeToWait.OnItemEquipped:
                GameEvents.OnItemEquipped += OnEventTriggered;
                break;
            default:
                Debug.LogWarning($"[TutorialEventWaitStep] '{QuestDescription}' 스텝: 기다릴 이벤트가 설정되지 않았습니다. 즉시 완료됩니다.");
                CompleteStep(); 
                break;
        }
    }

    public override void Deactivate()
    {
        base.Deactivate();
        
        switch (eventToWait)
        {
            case GameEventTypeToWait.OnMonsterKilled:
                GameEvents.OnMonsterKilled -= OnEventTriggered;
                break;
            case GameEventTypeToWait.OnPassiveItemUIOpened:
                GameEvents.OnPassiveItemUIOpened -= OnEventTriggered;
                break;
            case GameEventTypeToWait.OnPassiveItemTake:
                GameEvents.OnPassiveItemTake -= OnEventTriggered;
                break;
            case GameEventTypeToWait.OnActiveItemUIOpened:
                GameEvents.OnActiveItemUIOpened += OnEventTriggered;
                break;
            case GameEventTypeToWait.OnActiveItemTake:
                GameEvents.OnActiveItemTake -= OnEventTriggered;
                break;
            case GameEventTypeToWait.OnActiveSkillUse:
                GameEvents.OnActiveSkillUse -= OnEventTriggered;
                break;
            case GameEventTypeToWait.OnInventoryOpened:
                GameEvents.OnInventoryOpened -= OnEventTriggered;
                break;
            case GameEventTypeToWait.OnItemEquipped:
                GameEvents.OnItemEquipped -= OnEventTriggered;
                break;
        }
        UIManager.Instance.HideContextualHint();
    }

    private void OnEventTriggered()
    {
        CompleteStep();
    }
}
