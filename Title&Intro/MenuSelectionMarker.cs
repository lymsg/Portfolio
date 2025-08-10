using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuSelectionMarker : MonoBehaviour
{
    public List<GameObject> menuButtons;
    private GameObject currentSelected;
    private GameObject lastSelected;
    private GameObject lastConfirmedSelectedButton;
    
    public AudioSource menuAudioSource; 
    public AudioClip selectionSound;

    void Awake()
    {
        foreach (GameObject buttonObject in menuButtons)
        {
            AddEventTriggersToButton(buttonObject);
        }
    }
    void Update()
    {
        currentSelected = EventSystem.current.currentSelectedGameObject;

        //현재 아무것도 선택되지 않았을 때 (UI 외부 클릭 등)
        if (currentSelected == null)
        {
            // 이전에 확실히 선택된 버튼이 있었다면 그 버튼을 다시 선택하도록 강제
            if (lastConfirmedSelectedButton != null)
            {
                Button btnToReselect = lastConfirmedSelectedButton.GetComponent<Button>();
                if (btnToReselect != null)
                {
                    btnToReselect.Select(); 
                }
            }
            
        }
        //선택된 UI가 변경된 경우 (방향키 이동, 마우스로 다른 버튼 클릭 등)
        else if (currentSelected != lastSelected)
        {
            // 이전 선택 해제
            if (lastSelected != null)
            {
                ToggleIndicator(lastSelected, false);
            }

            // 현재 선택 표시
            ToggleIndicator(currentSelected, true);
            //효과음 재생
            if (menuAudioSource != null && selectionSound != null)
            {
                menuAudioSource.PlayOneShot(selectionSound);
            }
            
            lastSelected = currentSelected;
            
            if (currentSelected.GetComponent<Button>() != null)
            {
                lastConfirmedSelectedButton = currentSelected;
            }
        }
    }
    void AddEventTriggersToButton(GameObject buttonObject)
    {
        if (buttonObject == null) return;

        EventTrigger trigger = buttonObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = buttonObject.AddComponent<EventTrigger>();
        }
        
        EventTrigger.Entry entryPointerEnter = new EventTrigger.Entry();
        entryPointerEnter.eventID = EventTriggerType.PointerEnter;
        entryPointerEnter.callback.AddListener((data) => { OnPointerEnter(buttonObject); });
        trigger.triggers.Add(entryPointerEnter);
        
        EventTrigger.Entry entryPointerExit = new EventTrigger.Entry();
        entryPointerExit.eventID = EventTriggerType.PointerExit;
        entryPointerExit.callback.AddListener((data) => { OnPointerExit(buttonObject); });
        trigger.triggers.Add(entryPointerExit);
    }

    // 마우스가 버튼에 진입했을 때 호출될 함수
    public void OnPointerEnter(GameObject buttonObject)
    {
        ToggleIndicator(buttonObject, true);
        
        if (menuAudioSource != null && selectionSound != null)
        {
            menuAudioSource.PlayOneShot(selectionSound);
        }
        EventSystem.current.SetSelectedGameObject(buttonObject);
    }

    // 마우스가 버튼에서 벗어났을 때 호출될 함수
    public void OnPointerExit(GameObject buttonObject)
    {
        if (EventSystem.current.currentSelectedGameObject != buttonObject)
        {
            ToggleIndicator(buttonObject, false);
        }
    }
    void ToggleIndicator(GameObject buttonObject, bool enable)
    {
        if (buttonObject == null) return;
        
        foreach (Transform child in buttonObject.transform)
        {
            if (child.CompareTag("SelectionMarker"))
            {
                child.gameObject.SetActive(enable);
                return;
            }
        }
    }
}
