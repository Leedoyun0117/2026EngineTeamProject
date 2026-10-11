using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditor.Experimental.GraphView.GraphView;

public class TpObj_HTY : MonoBehaviour,IPointerDownHandler
{
    [SerializeField] private TpPlayer_HTY _tpCompo;
    [SerializeField] private GameObject _player;
    

    public void OnPointerDown(PointerEventData eventData)
    {
        _player.transform.position = Utils_HTY.GetMousePos();
        _tpCompo._panel.SetActive(false);
    }
}
