using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuButtons : MonoBehaviour
{
    public GameObject optionPanel; 
    
    public Button defaultSelectedButton; //메뉴 진입 시 기본으로 선택될 버튼
    public AudioSource bgmAudioSource; 
    public IntroManager introManager;
    void Awake()
    {
        // 모든 왼쪽 페이지 패널들을 초기에는 비활성화
        if (optionPanel != null) optionPanel.SetActive(false);
        
    }
    void OnEnable()
    {
        if (bgmAudioSource != null && !bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Play();
        }
        
        IntroManager.OnMenuFadeInComplete += SelectDefaultButtonAfterFadeIn;
    }
    void OnDisable()
    {
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
        }
        IntroManager.OnMenuFadeInComplete -= SelectDefaultButtonAfterFadeIn;
    }
    void SelectDefaultButtonAfterFadeIn()
    {
        if (defaultSelectedButton != null)
        {
            defaultSelectedButton.Select();
        }
    }
    // 모든 왼쪽 페이지 패널을 비활성화
    void HideAllLeftPanels()
    {
        if (optionPanel != null) optionPanel.SetActive(false);
    }
    
    public void OnStartGameButton()
    {
        //LoadingSceneController.LoadScene("TutorialScene");
        introManager.StartStorySequence();
    }

    public void OnOptionButton()
    {
        HideAllLeftPanels(); 
        if (optionPanel != null) optionPanel.SetActive(true);
    }
    
    public void OnExitGameButton()
    {
        #if UNITY_EDITOR 
        UnityEditor.EditorApplication.isPlaying = false;
        #else 
        Application.Quit();
        #endif
    }
}
