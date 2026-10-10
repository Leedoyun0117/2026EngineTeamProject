using UnityEngine;
using UnityEngine.EventSystems;

public class TpPlayer_HTY : MonoBehaviour,IPointerDownHandler
{
    [SerializeField] private GameObject _player;
    [SerializeField] private GameObject _panel;

    public void Tp()
    {
        _panel.SetActive(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _player.transform.position = Utils_HTY.GetMousePos();
        _panel.SetActive(false);
    }
}
