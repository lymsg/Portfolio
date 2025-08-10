using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestMainMenuButtons : MonoBehaviour
{
    public GameObject optionPanel;   
    
    public Button defaultSelectedButton; //메뉴 진입 시 기본으로 선택될 버튼
    public AudioSource bgmAudioSource; 
    
    void Awake()
    {
        // 모든 왼쪽 페이지 패널들을 초기에는 비활성화
        if (optionPanel != null) optionPanel.SetActive(false);
        
    }
    void OnEnable()
    {
        if (bgmAudioSource != null && !bgmAudioSource.isPlaying)
            bgmAudioSource.Play();

        defaultSelectedButton.Select();
    }
    void HideAllLeftPanels()
    {
        if (optionPanel != null) optionPanel.SetActive(false);
    }

    public void OnStartGameButton()
    {
        LoadingSceneController.LoadScene("LobbyScene");
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
