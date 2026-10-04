using UnityEngine;
using UnityEngine.EventSystems;

public class CopyPlayer_HTY : MonoBehaviour,IPointerDownHandler
{
    [SerializeField] private GameObject _copyObj;
    private bool _canCopy = false;
    private GameObject _currentCopyObj;


    private void Start()
    {
        gameObject.SetActive(false);
    }

    public bool CanCopy()
    {
        return _canCopy;
    }

    public void Copy()
    {
        _canCopy = !_canCopy;
        if (_canCopy) gameObject.SetActive(true);
        else gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (CanCopy())
        {
            Destroy(_currentCopyObj);
            _currentCopyObj = Instantiate(_copyObj,Utils_HTY.GetMousePos(),Quaternion.identity);
            _currentCopyObj.GetComponent<TestPlayerMove_HTY>().SetPlay(false);
            FindAnyObjectByType<PastePlayer_HTY>()._otherObj = _currentCopyObj;
            _canCopy = false;
            gameObject.SetActive(false);
        }
    }
}
