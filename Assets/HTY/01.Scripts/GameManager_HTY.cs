using UnityEngine;

public class GameManager_HTY : MonoBehaviour
{
    [Header("PlayerInfo")]
    public GameObject _player;
    public PlayerInput_HTY _inputValue;

    public static GameManager_HTY instance;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
