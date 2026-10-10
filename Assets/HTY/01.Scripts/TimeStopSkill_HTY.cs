using System.Collections;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.InputSystem;

public class TimeStopSkill_HTY : MonoBehaviour
{
    [SerializeField] private GameObject _selectRangeObj;
    private Rigidbody2D _rb;
    private bool _isStop = false;

    public void SelectObj()
    {

    }

    public void TimeStop()
    {
        _rb = gameObject.GetComponent<Rigidbody2D>();
        _isStop = true;
        StartCoroutine(StopObj());
    }

    private void Update()
    {
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            _isStop=false;
        }
    }
    private IEnumerator StopObj()
    {
        while (_isStop)
        {
            yield return null;
            _rb.linearVelocity = Vector3.zero;
        }
    }


}
