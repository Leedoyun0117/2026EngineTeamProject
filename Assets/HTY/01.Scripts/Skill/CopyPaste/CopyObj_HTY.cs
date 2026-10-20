using UnityEngine;
using UnityEngine.EventSystems;

public class CopyObj_HTY : MonoBehaviour,IPointerDownHandler
{
    [SerializeField] private CopyPlayer_HTY _copySkill;
    private GameObject _currentCopyObj;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_currentCopyObj != null) return;
        //알려주기?
        if (_copySkill.CanUse())
        {
            _currentCopyObj = Instantiate(_copySkill._copyObj, Utils_HTY.GetMousePos(), Quaternion.identity);
            _currentCopyObj.GetComponent<TestPlayerMove_HTY>().SetPlay(false);
            FindAnyObjectByType<PastePlayer_HTY>()._otherObj = _currentCopyObj;
            _copySkill._canCopy = false;
            gameObject.SetActive(false);
        }
    }
}
