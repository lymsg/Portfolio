using UnityEngine;
using UnityEngine.UI;

public class ButtonClickSound : MonoBehaviour
{
    public AudioSource audioSource; 
    public AudioClip clickSound;    

    void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(PlayClickSound);
        }
        else
        {
            Debug.LogWarning("ButtonClickSound 스크립트는 Button 컴포넌트가 없는 GameObject에 붙어있습니다: " + gameObject.name);
        }
    }

    public void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
