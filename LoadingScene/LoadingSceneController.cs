using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneController : MonoBehaviour
{
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    private GameObject player;
    private static string nextScene;

    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }

    private void Start()
    {
        player = GameObject.FindWithTag("Player"); // 플레이어에 Tag 설정해야 함
        if (player != null)
        {
            var inputHandler = player.GetComponent<PlayerInputHandler>();
            if (inputHandler != null)
                inputHandler.SetInputEnabled(false);
        }

        StartCoroutine(LoadNextScene());
    }

    IEnumerator LoadNextScene()
    {
        // 허수 로딩 시간
        float fakeLoadTime = 2f;
        float elapsed = 0f;

        while (elapsed < fakeLoadTime)
        {
            elapsed += Time.deltaTime;
            float fakeProgress = Mathf.Clamp01(elapsed / fakeLoadTime);
            progressBar.value = fakeProgress;
            progressText.text = $"로딩중...{fakeProgress * 100f:F0}%";
            yield return null;
        }

        // 진짜 씬 로딩 (비동기)
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            yield return null;
        }
        
        if (player != null)
        {
            var inputHandler = player.GetComponent<PlayerInputHandler>();
            if (inputHandler != null)
                inputHandler.SetInputEnabled(true);
        }
        
        // 허수 로딩 끝 + 진짜 로딩 끝 최종 전환
        op.allowSceneActivation = true;
    }
}
