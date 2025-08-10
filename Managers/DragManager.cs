using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragManager : MonoBehaviour
{
    [Header("드래그용 프리팹")]
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private Canvas targetCanvas;
    
    private List<GameObject> ghostPool = new();
    private GameObject ghostInstance; 
    private Image ghostImage;
    
    /// <summary>
    /// 드래그 고스트 생성 또는 재사용
    /// </summary>
    public void CreateGhost(Sprite sprite)
    {
        ghostInstance = GetGhostFromPool();
        ghostImage = ghostInstance.GetComponent<Image>();
        ghostImage.sprite = sprite;

        ghostInstance.SetActive(true);
        ghostImage.raycastTarget = false;
    }
    
    /// <summary>
    /// 고스트를 풀에서 가져오기 (없으면 생성)
    /// </summary>
    private GameObject GetGhostFromPool()
    {
        foreach (var ghost in ghostPool)
        {
            if (!ghost.activeInHierarchy)
                return ghost;
        }

        // 풀에 사용 가능한 오브젝트가 없으면 새로 생성
        var newGhost = Instantiate(ghostPrefab, targetCanvas.transform);
        ghostPool.Add(newGhost);
        return newGhost;
    }
    
    /// <summary>
    /// 드래그 중 마우스 위치에 따라 고스트 이동
    /// </summary>
    public void UpdateGhostPosition(Vector2 screenPosition)
    {
        if (ghostInstance != null)
            ghostInstance.transform.position = screenPosition;
    }

    /// <summary>
    /// 드래그 종료 시 고스트 제거
    /// </summary>
    public void ClearGhost()
    {
        if (ghostInstance != null)
        {
            ghostInstance.SetActive(false);
            ghostInstance = null;
            ghostImage = null;
        }
    }

    // protected override bool Persistent => true; // 씬 전환에도 유지됨
    // protected override bool ShouldRename => true;
}
