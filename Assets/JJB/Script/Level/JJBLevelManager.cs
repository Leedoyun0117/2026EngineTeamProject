
using System;
using UnityEngine;

namespace JJB.Script.Level
{
    public class JJBLevelManager : MonoSingleton<JJBLevelManager>
    {
        [Header("Player")]
        [SerializeField] private Rigidbody2D playerRb;

        [Header("Level Groups")]
        [SerializeField] private Transform[] levelGroups;

        [Header("Start Settings")]
        [SerializeField, Min(0)] private int startGroupIndex;
        [SerializeField, Min(0)] private int startStageIndex;

        private LevelInstance _currentInstance;

        private int _currentGroupIndex = -1;
        private int _currentStageIndex = -1;

        private bool _isChanging;
        private bool _isCompleted;

        public int CurrentGroupIndex => _currentGroupIndex;
        public int CurrentStageIndex => _currentStageIndex;

        public LevelInstance CurrentStage => _currentInstance;

        public event Action<LevelInstance> OnStageLoaded;
        public event Action OnAllStagesCompleted;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            LoadStage(startGroupIndex, startStageIndex);
        }

        // 특정 그룹의 스테이지 로드
        public void LoadStage(int groupIndex, int stageIndex)
        {
            if (_isChanging)
                return;

            if (playerRb == null || levelGroups == null)
            {
                Debug.LogError("Player 또는 LevelGroups가 연결되지 않았습니다.");
                return;
            }

            if (groupIndex < 0 || groupIndex >= levelGroups.Length)
            {
                Debug.LogError("잘못된 LevelGroup 인덱스입니다.");
                return;
            }

            Transform group = levelGroups[groupIndex];

            if (group == null || stageIndex < 0 ||
                stageIndex >= group.childCount)
            {
                Debug.LogError("해당 Stage를 찾을 수 없습니다.");
                return;
            }

            Transform stage = group.GetChild(stageIndex);

            LevelInstance instance =
                stage.GetComponent<LevelInstance>();

            if (instance == null)
            {
                Debug.LogError(
                    $"{stage.name}에 LevelInstance가 없습니다.",
                    stage
                );
                return;
            }

            _isChanging = true;

            // 모든 Stage 비활성화
            SetAllStagesInactive();

            // 선택한 Stage 활성화
            stage.gameObject.SetActive(true);

            _currentInstance = instance;
            _currentGroupIndex = groupIndex;
            _currentStageIndex = stageIndex;

            _isCompleted = false;

            // 플레이어 위치 이동
            PlacePlayer(instance.SpawnPosition);

            _isChanging = false;

            OnStageLoaded?.Invoke(instance);

            Debug.Log(
                $"Stage Loaded: {group.name} / {stage.name}"
            );
        }

        // 다음 Stage
        public void LoadNextStage()
        {
            if (_isChanging || _isCompleted)
                return;

            if (_currentGroupIndex < 0)
                return;

            Transform currentGroup =
                levelGroups[_currentGroupIndex];

            int nextStageIndex = _currentStageIndex + 1;
            
            if (nextStageIndex < currentGroup.childCount)
            {
                LoadStage(_currentGroupIndex, nextStageIndex);
                return;
            }
            
            int nextGroupIndex = _currentGroupIndex + 1;

            if (nextGroupIndex < levelGroups.Length &&
                levelGroups[nextGroupIndex] != null &&
                levelGroups[nextGroupIndex].childCount > 0)
            {
                LoadStage(nextGroupIndex, 0);
                return;
            }

            _isCompleted = true;
            OnAllStagesCompleted?.Invoke();
        }
        
        public void RestartStage()
        {
            if (_currentGroupIndex < 0)
                return;

            LoadStage(_currentGroupIndex, _currentStageIndex);
        }
        
        private void SetAllStagesInactive()
        {
            foreach (Transform group in levelGroups)
            {
                if (group == null)
                    continue;

                for (int i = 0; i < group.childCount; i++)
                {
                    group.GetChild(i).gameObject.SetActive(false);
                }

                group.gameObject.SetActive(true);
            }
        }

        private void PlacePlayer(Vector3 spawnPosition)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;

            playerRb.position = spawnPosition;

            Physics2D.SyncTransforms();
        }
    }
}
