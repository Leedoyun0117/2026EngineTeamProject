using UnityEngine;

public class GameManager_HTY : MonoBehaviour
{
    [Header("PlayerInfo")]
    public GameObject _player;
    public GameObject _playerPrefab;
    public PlayerInput_HTY _inputValue;

    public static GameManager_HTY Instance;
    public Utility_HTY _utiltyCompo;
    public Material _originMat;
    public Material _outLineMat;
    public SkillListSo_HTY _skillList;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Vector2 GetPointerPos() => _utiltyCompo.Pointer_HTY.transform.position;
}
