using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoSingleton<TutorialManager>
{
    [SerializeField] private List<TutorialStep> tutorialSteps;
    [SerializeField] private string nextSceneName = "LobbyScene";
    
    private int currentStepIndex = -1;
    private TutorialStep currentStep;
    
    [SerializeField] private StaminaTipUIController staminaTipUI;
    [SerializeField] private GameObject skillTipUI;
    
    private bool _isViewChangeInputAllowed = false; // 기본적으로 V키 입력은 막음
    public bool IsViewChangeInputAllowed()
    {
        return _isViewChangeInputAllowed;
    }
    public void SetViewChangeInputAllowed(bool allowed)
    {
        _isViewChangeInputAllowed = allowed;
    }
    protected override bool Persistent => false;

    protected override void Awake()
    {
        base.Awake();
        SetViewChangeInputAllowed(false);
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => UIManager.Instance != null && UIManager.Instance.IsUILoaded()); 
        yield return new WaitUntil(() => OtherStageManager.Instance != null);
        
        StartTutorial();
    }

    void Update()
    {
        if (currentStep is TutorialInputStep inputStep)
        {
            inputStep.CheckInput();
        }
    }
    private void StartTutorial()
    {
        currentStepIndex = -1;
        NextStep();
    }

    public void NextStep()
    {
        if (currentStep != null)
        {
            currentStep.OnStepCompleted -= NextStep;
            currentStep.Deactivate();
        }

        currentStepIndex++;
        
        // 스태미너 UI 표시 조건
        if (currentStepIndex == 4 || currentStepIndex == 16)
        {
            staminaTipUI.ShowWithBounce();
        }
        else
        {
            staminaTipUI.Hide();
        }
        
        if (UIManager.Instance.TryGetUI<ContextualUIHint>(out var hintUI))
        {
            
            if(currentStepIndex == 13)// 위로
            {
                hintUI.SetPositionY(hintUI.UpY);
                skillTipUI.SetActive(true);
            } 
            else
            {
                hintUI.SetPositionY(hintUI.DefaultY); // 원위치
                skillTipUI.SetActive(false);
            }
        }
        
        if (currentStepIndex >= tutorialSteps.Count)
        {
            EndTutorial();
            return;
        }

        currentStep = tutorialSteps[currentStepIndex];
        currentStep.OnStepCompleted += NextStep;
        currentStep.Activate();
        SoundManager.Instance.PlaySFX(GameManager.Instance.Player.transform.position,"TutorialHint");
    }
    public void NotifyStepActivated(string hintMessage, string questDescription)
    {
        if (!string.IsNullOrEmpty(hintMessage))
        {
            UIManager.Instance.ShowContextualHint(hintMessage);
        }
    }
    private void EndTutorial()
    {
        if (currentStep != null)
        {
            currentStep.OnStepCompleted -= NextStep;
            currentStep.Deactivate();
        }

        UIManager.Instance.HideContextualHint();

        PlayerPrefs.SetInt("TutorialCompleted", 1); // 튜토리얼 완료로 설정
        SceneManager.LoadScene(nextSceneName);
    }

    public void TutorialSkipBtn()
    {
        UIManager.Instance.ShowConfirmPopup(
            "튜토리얼을 스킵하시겠습니까?",
            onConfirm: () =>
            {
                PlayerPrefs.SetInt("TutorialCompleted", 1); // 튜토리얼 완료로 설정
                SceneManager.LoadScene(nextSceneName);
            },
            onCancel: () =>
            { });
    }
}
