using UnityEngine;

public class Utility_HTY : MonoBehaviour 
{
    [SerializeField] private GameObject _pointerPrefab;
    public GameObject Pointer_HTY { get; private set; }

    public void OnPointer()
    {
        Pointer_HTY = Instantiate(_pointerPrefab,GameManager_HTY.Instance._player.transform.position,Quaternion.identity);
    }

    public void OffPointer()
    {
        Destroy(Pointer_HTY);
    }
}
