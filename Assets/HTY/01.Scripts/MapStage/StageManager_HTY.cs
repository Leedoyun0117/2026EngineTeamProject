using System;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

public class StageManager_HTY : MonoBehaviour
{
    public static StageManager_HTY Instance { get; private set; }
    [SerializeField] private MapLoader_HTY _mapLoader;
    [SerializeField] private ChapterListSO_HTY _chapterList;
    public int _unLockChapter;
    public int _unLockStage;
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
            OpenLockStage();
            Debug.Log($"챕터 {_unLockChapter},스테이지 {_unLockStage}");
        }
        
    }

    public void Init()
    {
        _unLockChapter = 1;
        _unLockStage = 1;
    }

    public void OpenLockStage()
    {
        _unLockStage++;
        StageSO_HTY stage;

        try
        {
             stage = _chapterList._list[_unLockChapter-1]._stageList[_unLockStage-1];
        }
        catch(ArgumentOutOfRangeException)
        {
            if(_chapterList._list.Count <= _unLockChapter)
            {
                Ending();
                return;
            }

            _unLockChapter++;
            _unLockStage = 1;
            stage = _chapterList._list[_unLockChapter-1]._stageList[_unLockStage-1];
        }
        _mapLoader.ChangeMap(stage);
    }

    public void LoadMap(StageSO_HTY stageSO)
    {
        _mapLoader.ChangeMap(stageSO);
    } 

    private void Ending()
    {
        //엔딩 컷씬이나 다른 쪽으로 활용
    }
}
