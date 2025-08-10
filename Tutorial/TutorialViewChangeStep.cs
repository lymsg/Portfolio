using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTutorialViewChangeStep", menuName = "Tutorial/View Change Step")]
public class TutorialViewChangeStep : TutorialStep
{
    [Header("시점 변환 스텝 설정")]
    [TextArea]
    public string viewChangeHintMessage;
    private Action<ViewModeType> _cachedViewModeChangedCallback; 
    
    public override void Activate()
    {
        base.Activate();
        
        TutorialManager.Instance.NotifyStepActivated(viewChangeHintMessage, QuestDescription);
        //v키 입력허용
        TutorialManager.Instance.SetViewChangeInputAllowed(true); 
        
        _cachedViewModeChangedCallback = HandleViewModeChanged;
        
        ViewManager.Instance.OnViewChanged += _cachedViewModeChangedCallback;
    }

    public override void Deactivate()
    {
        base.Deactivate();
        
        if (_cachedViewModeChangedCallback != null)
        {
            if (ViewManager.HasInstance)
                ViewManager.Instance.OnViewChanged -= _cachedViewModeChangedCallback;
        }
    }
    private void HandleViewModeChanged(ViewModeType newViewMode)
    {
        CompleteStep(); 
    }
}
