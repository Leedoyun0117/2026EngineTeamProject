using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class StageManager_HTY : MonoBehaviour
{
    public static StageManager_HTY Instance { get; private set; }
    [SerializeField] private MapLoader_HTY _mapLoader;
    [SerializeField] private ChapterListSO_HTY _chapterList;
    public int _currentChapter;
    public int _currentStage;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _mapLoader = GetComponent<MapLoader_HTY>();
            DontDestroyOnLoad(gameObject);
        }
            
        else Destroy(gameObject);
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            NextStage();
            Debug.Log($"챕터 {_currentChapter},스테이지 {_currentStage}");
        }
        
    }

    public void Init()
    {
        _currentChapter = 1;
        _currentStage = 1;
    }

    public void NextStage()
    {
        _currentStage++;
        StageSO_HTY stage;

        try
        {
             stage = _chapterList._list[_currentChapter-1]._stageList[_currentStage-1];
        }
        catch(ArgumentOutOfRangeException)
        {
            if(_chapterList._list.Count <= _currentChapter)
            {
                Ending();
                return;
            }

            _currentChapter++;
            _currentStage = 1;
            stage = _chapterList._list[_currentChapter-1]._stageList[_currentStage-1];
        }
        _mapLoader.ChangeMap(stage);
    }

    private void Ending()
    {
        //엔딩 컷씬이나 다른 쪽으로 활용
    }
}
