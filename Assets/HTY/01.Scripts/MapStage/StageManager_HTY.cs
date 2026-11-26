using System;
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
            UnLock();
            Debug.Log($"챕터 {_unLockChapter},스테이지 {_unLockStage-1}");
            _chapterList._list[_unLockChapter - 1]._stageList[_unLockStage - 1]._unLock = true;
        }

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            ResetStage();
        }
        
    }

    public void Init()//저장 시스템 도입 시급
    {
        _unLockChapter = 1;
        _unLockStage = 1;

        _chapterList._list[_unLockChapter - 1]._stageList[_unLockStage - 1]._unLock = true;
        Debug.Log($"챕터 {_unLockChapter},스테이지 {_unLockStage - 1}");
    }

    public void UnLock()
    {
        if(_unLockChapter== _chapterList._list.Count && _unLockStage == _chapterList._list[_unLockChapter - 1]._stageList.Count)
        {
            Ending();
            return;
        }
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
        Debug.Log($"현재 스테이지 {_unLockChapter} - {_unLockStage-1} 클리어 end");
    }


    public void ResetStage()
    {
        for (int i = 0; i < 3; i++)
        {
            foreach (var stageSo in _chapterList._list[i]._stageList)
            {
                stageSo._unLock = false;
            }
        }

        Init();

    }
}
