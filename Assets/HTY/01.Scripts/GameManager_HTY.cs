using UnityEngine;

public class GameManager_HTY : MonoBehaviour {
    public static GameManager_HTY instance;

    [Header("PlayerInfo")] public GameObject _player;

    public PlayerInput_HTY _inputValue;


    private void Awake() {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
}