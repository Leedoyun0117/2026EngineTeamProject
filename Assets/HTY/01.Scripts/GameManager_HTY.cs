using UnityEngine;

public class GameManager_HTY : MonoBehaviour
{
    public static GameManager_HTY Instance;

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
}
