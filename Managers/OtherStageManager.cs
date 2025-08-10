using UnityEngine;

public class OtherStageManager : StageManager
{
    [SerializeField] private string mapTitle;
    [SerializeField] private Room otherRoomPrefab;
    public bool IsStageReady { get; private set; } = false;
    //[SerializeField] protected Material _skyboxMaterial; 
    protected override bool Persistent => false;

    private bool _titleShown = false;
    public override void LoadStage()
    {
        ClearStage();
        ApplyStageEnvironment();
        // 튜토리얼 방 하나만 생성
        if (otherRoomPrefab != null)
        {
            GameObject otherRoomGO = Instantiate(otherRoomPrefab.gameObject);
            Room otherRoom = otherRoomGO.GetComponent<Room>();
            otherRoom.Initialize(otherRoom.GetHashCode(), new Vector2Int(0, 0), RoomType.Start);

            if (otherRoom != null)
            {
                currentStage = new StageData(-1);
                currentStage.RegisterRoom(otherRoom, 1, 1);
                GameManager.Instance.Player.transform.position = currentStage.playerSpawnPoint;
                
                if (viewCameraController != null && ViewManager.HasInstance)
                {
                    Debug.Log($"[StageManager] 씬 로드 시 현재 뷰 모드 유지: {ViewManager.Instance.CurrentViewMode}");
                    // ViewCameraController에게 ViewManager가 현재 기억하는 뷰 모드를 전달하여 초기화
                    viewCameraController.InitCameraForStage(ViewManager.Instance.CurrentViewMode); 
                }
                else if (viewCameraController == null)
                {
                    Debug.LogWarning("[StageManager] ViewCameraController가 할당되지 않았습니다!");
                }
                else // ViewManager.HasInstance가 false인 경우 (매우 드물지만)
                {
                    Debug.LogWarning("[StageManager] ViewManager가 아직 초기화되지 않았습니다. 기본 뷰 모드로 설정.");
                    viewCameraController.InitCameraForStage(ViewModeType.View2D); // 안전을 위한 기본값
                }
                
            }
            else
            {
                Debug.LogError("할당된 튜토리얼 방 프리팹에 Room 컴포넌트가 없습니다.");
            }
            IsStageReady = true; 
        }
        else
        {
            Debug.LogError("튜토리얼 방 프리팹이 할당되지 않았습니다!");
        }
        
        TryShowMapTitle();
    }
    private void TryShowMapTitle()
    {
        if (_titleShown) return;
        
        void Show()
        {
            if (UIManager.Instance.TryGetUI<MapTitleUI>(out var mapTitleUI))
            {
                UIManager.Instance.ShowUI<MapTitleUI>();
                mapTitleUI.ShowTitle(mapTitle);
                _titleShown = true;
            }
            else
            {
                Debug.LogWarning("[OtherStageManager] MapTitleUI를 찾을 수 없습니다.");
            }
        }

        if (UIManager.Instance.IsUILoaded())
        {
            Show();
        }
        else
        {
            UIManager.Instance.OnAllUIReady(() => Show());
        }
    }
    protected override void Awake()
    {
        base.Awake(); 
    }

    void Start()
    {
        LoadStage();
    }
    protected void ApplyStageEnvironment()
    {
        // BGM 재생
        if(SoundManager.HasInstance && !string.IsNullOrEmpty(bgmKey))
            SoundManager.Instance.PlayBGM(null, bgmKey);
        
        // 스카이박스 변경
        if(skyboxMaterial != null)
        {
            RenderSettings.skybox = skyboxMaterial;
        }
    }

}
