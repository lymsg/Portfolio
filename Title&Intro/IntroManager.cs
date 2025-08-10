using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.Video;

public class IntroManager : MonoBehaviour
{
    public VideoPlayer videoPlayer; 
    public GameObject menuCanvas;
    public CanvasGroup uiFadeGroup;
    public CanvasGroup blackScreenGroup;
    public Animator blackScreenAnimator;
    public PlayableDirector timelineDirector;
    public GameObject skipTextUI;
    
    
    [Header("Story")]
    public GameObject storyGroup;            // 전체 스토리 UI 그룹
    public CanvasGroup storyCanvasGroup;     // 페이드용
    public Image storyImage;                 // 이미지
    public TextMeshProUGUI storyText;        // 타이핑 텍스트
    public List<Sprite> storySprites;        // 이미지 배열
    public List<string> storyLines;          // 텍스트 배열
    public float typingSpeed = 0.05f;
    public List<AudioClip> storyVoices;
    public AudioSource voicePlayer;
    
    private int currentIndex = 0;
    private bool isTyping = false;
    public static event Action OnMenuFadeInComplete; 

    private const string isTutorialCompletedKey = "TutorialCompleted";

    void Start()
    {
        if (timelineDirector != null)
        {
            timelineDirector.played += OnCutsceneStarted;
            timelineDirector.stopped += OnCutsceneEnded;
        }
    }
    void OnEnable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
        }
    }

    void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }
    void OnDestroy()
    {
        if (timelineDirector != null)
        {
            timelineDirector.played -= OnCutsceneStarted;
            timelineDirector.stopped -= OnCutsceneEnded;
        }
    }
    
    void OnCutsceneStarted(PlayableDirector director)
    {
        if (skipTextUI != null)
            skipTextUI.SetActive(true);
    }

    void OnCutsceneEnded(PlayableDirector director)
    {
        if (skipTextUI != null)
            skipTextUI.SetActive(false);
    }
    public void PlayVideo()
    {
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = Application.streamingAssetsPath + "/Intro_book4.mp4";

        if (videoPlayer != null)
        {
            if (!videoPlayer.isPrepared) 
            {
                videoPlayer.Prepare();
                videoPlayer.prepareCompleted += (source) => {
                    videoPlayer.Play();
                };
            }
            else
            {
                videoPlayer.Play();
            }
        }
        else
        {
            Debug.LogError("비디오플레이어가 없습니다."); 
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        if (menuCanvas != null)
        {
            menuCanvas.SetActive(true);
        }
        
        if (uiFadeGroup != null)
        {
            uiFadeGroup.alpha = 0f;
            uiFadeGroup.interactable = false;
            uiFadeGroup.blocksRaycasts = false;

            StartCoroutine(FadeInCanvasGroup(uiFadeGroup, 2f, 2f)); 
        }
        blackScreenAnimator.enabled = false;

        // 이제 직접 제어 가능
        blackScreenGroup.alpha = 1f;
        
        
        vp.gameObject.SetActive(false); 
    }
    public void StartStorySequence()
    {
        StartCoroutine(StorySequence());
    }
    IEnumerator StorySequence()
    {
        // 메뉴 꺼짐 + 책도 꺼짐
        yield return StartCoroutine(FadeOutCanvasGroup(uiFadeGroup, 1f));
        menuCanvas.SetActive(false);

        // 스토리 그룹 켜짐 + 페이드 인
        storyGroup.SetActive(true);
        storyCanvasGroup.alpha = 0;
        yield return StartCoroutine(FadeInCanvasGroup(storyCanvasGroup, 1f, 1f));

        // 첫 세트 보여주기
        ShowCurrentStory();
    }
    void Update()
    {
        if (storyGroup.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                storyText.text = storyLines[currentIndex];
                isTyping = false;
            }
            else
            {
                currentIndex++;
                if (currentIndex >= storyLines.Count)
                {
                    if(PlayerPrefs.HasKey(isTutorialCompletedKey) && PlayerPrefs.GetInt(isTutorialCompletedKey) == 1)
                    {
                        // 튜토리얼 완료된 경우
                        LoadingSceneController.LoadScene("LobbyScene");
                    }
                    else
                    {
                        PlayerPrefs.SetInt(isTutorialCompletedKey, 0); // 튜토리얼 미완료로 설정
                        LoadingSceneController.LoadScene("TutorialScene");
                    }
                }
                else
                {
                    ShowCurrentStory();
                }
            }
        }
        
        // 컷신 스킵 처리
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SkipCutscene();
        }
    }

    void ShowCurrentStory()
    {
        storyImage.sprite = storySprites[currentIndex];
        storyText.text = "";
        
        if (voicePlayer.isPlaying)
            voicePlayer.Stop();
        
        if (storyVoices != null && currentIndex < storyVoices.Count && storyVoices[currentIndex] != null)
        {
            voicePlayer.clip = storyVoices[currentIndex];
            voicePlayer.Play();
        }
        
        StartCoroutine(TypeText(storyLines[currentIndex]));
    }

    IEnumerator TypeText(string line)
    {
        isTyping = true;
        foreach (char c in line)
        {
            storyText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }
    IEnumerator FadeInCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            yield return null; 
        }
        
        canvasGroup.alpha = targetAlpha; 
        canvasGroup.interactable = true; 
        canvasGroup.blocksRaycasts = true; 
        
        OnMenuFadeInComplete?.Invoke();
    }
    
    IEnumerator FadeOutCanvasGroup(CanvasGroup canvasGroup, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, timer / duration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void SkipStory()
    {
        LoadingSceneController.LoadScene("TutorialScene");
    }
    
    public void SkipCutscene()
    {
        // 1. 타임라인 멈춤
        if (timelineDirector != null && timelineDirector.state == PlayState.Playing)
        {
            timelineDirector.Stop();
        }

        // 2. VideoPlayer 중지
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
            videoPlayer.gameObject.SetActive(false);
        }

        // 3. 메뉴 등장 처리 수동 호출
        if (menuCanvas != null)
        {
            menuCanvas.SetActive(true);
        }

        if (uiFadeGroup != null)
        {
            uiFadeGroup.alpha = 1f;
            uiFadeGroup.interactable = true;
            uiFadeGroup.blocksRaycasts = true;
        }

        if (blackScreenAnimator != null)
            blackScreenAnimator.enabled = false;

        if (blackScreenGroup != null)
            blackScreenGroup.alpha = 1f;
    }
}
