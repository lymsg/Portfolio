using System;
using UnityEngine;
[CreateAssetMenu(fileName = "NewTutorialStep", menuName = "Tutorial/Tutorial Step")]
public class TutorialStep : ScriptableObject
{
    [TextArea]
    public string QuestDescription;

    public event Action OnStepCompleted; // 스텝 완료 시 호출될 이벤트

    public virtual void Activate() { }
    public virtual void Deactivate() { }

    internal void CompleteStep()
    {
        OnStepCompleted?.Invoke();
        OnStepCompleted = null;
    }
}
