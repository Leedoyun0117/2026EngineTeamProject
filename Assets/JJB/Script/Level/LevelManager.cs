using System;
using UnityEngine;

namespace JJB.Script.Level
{
    public class LevelManager : MonoBehaviour
    {
        /*[SerializeField] private LevelListSO levelList;
        [SerializeField] private Transform levelRoot;
        [SerializeField, Min(0)] private int startLevelIndex;
        
        private Rigidbody2D _rb;
        private LevelInstance _currentInstance;
        
        private int _currentLevelIndex = -1;
        private bool _isChanging;
        private bool _isCompleted;

        public int CurrentLevelIndex => _currentLevelIndex;
        public bool IsChanging => _isChanging;
        public LevelSO CurrentLevel => levelList == null ? null : levelList.GetLevel(_currentLevelIndex);

        public event Action<LevelSO> OnLevelLoaded;
        public event Action OnAllLevelsCompleted;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            LoadLevel(startLevelIndex);
        }

        public void LoadLevel(int index)
        {
            if (_isChanging)
                return;

            if (levelList == null || levelRoot == null || _rb == null)
            {
                Debug.LogError("LevelManager의 Inspector 연결을 확인하세요.", this);
                return;
            }

            LevelSO level = levelList.GetLevel(index);

            if (level == null || level.LevelPrefab == null)
            {
                Debug.LogError($"레벨 데이터 또는 프리팹을 확인하세요. Index: {index}", this);
                return;
            }

            _isChanging = true;

            RemoveCurrentLevel();
            CreateLevel(level);
            PlacePlayer();

            _currentLevelIndex = index;
            _isCompleted = false;
            _isChanging = false;

            OnLevelLoaded?.Invoke(level);
        }

        public void RestartLevel()
        {
            if (_currentLevelIndex < 0)
                return;

            LoadLevel(_currentLevelIndex);
        }

        public void LoadNextLevel()
        {
            if (_isChanging || _isCompleted || CurrentLevel == null)
                return;

            int nextIndex = _currentLevelIndex + 1;

            if (nextIndex >= levelList.Count)
            {
                _isCompleted = true;
                OnAllLevelsCompleted?.Invoke();
                return;
            }

            LoadLevel(nextIndex);
        }

        private void RemoveCurrentLevel()
        {
            if (_currentInstance == null)
                return;

            _currentInstance.gameObject.SetActive(false);
            Destroy(_currentInstance.gameObject);
            _currentInstance = null;
        }

        private void CreateLevel(LevelSO level)
        {
            _currentInstance = Instantiate(level.LevelPrefab, levelRoot);
            _currentInstance.transform.localPosition = Vector3.zero;
            _currentInstance.transform.localRotation = Quaternion.identity;
        }

        private void PlacePlayer()
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _rb.position = _currentInstance.SpawnPosition;

            Physics2D.SyncTransforms();
        }*/
    }
}