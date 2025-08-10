using UnityEngine;
public enum TutorialInputType
{
    Move,     
    Jump,     
}
[CreateAssetMenu(fileName = "NewTutorialInputStep", menuName = "Tutorial/Input Step")]
public class TutorialInputStep : TutorialStep
{
    [Header("입력 스텝 설정")]
    [TextArea]
    public string inputHintMessage;
    public TutorialInputType inputType = TutorialInputType.Move;

    private bool _isMonitoringInput = false;

    public override void Activate()
    {
        base.Activate();
       
        TutorialManager.Instance.NotifyStepActivated(inputHintMessage, QuestDescription);
        
        _isMonitoringInput = true;
      }

    public override void Deactivate()
    {
        base.Deactivate();
        _isMonitoringInput = false; // 입력 감지 중지
    }

    // TutorialManager에서 호출하여 입력 감지 및 스텝 완료 여부를 확인
    public void CheckInput()
    {
        if (!_isMonitoringInput) return;

        switch(inputType)
        {
            case TutorialInputType.Move:
                if(Input.GetAxis("Horizontal") != 0)
                    Complete();
                break;
            case TutorialInputType.Jump:
                if(Input.GetKeyDown(KeyCode.Space))
                    Complete();
                break;
        }
    }
    private void Complete()
    {
        _isMonitoringInput = false;
        CompleteStep();
    }
}
