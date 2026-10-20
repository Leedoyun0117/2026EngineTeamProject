using UnityEngine;

public class StopObjSkill_HTY : MonoBehaviour, ISkill_HTY
{
    [SerializeField] private TargetSelector targetSelector;
    [SerializeField] private Camera mainCamera;

    private Rigidbody2D _otherRb;
    private bool _isStop = false;
    private GameObject _stopObj;

    private RigidbodyType2D _saveBType;
    private Vector2 _saveVel;
    private float _saveAngVel;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (_isStop && _stopObj != null)
        {
            if (!IsVisible(_stopObj))
            {
                ReSetObject();
            }
        }
    }

    public bool CanUse()
    {
        if (targetSelector == null)
            return false;
        Transform currentTarget = targetSelector._currentTarget;

        if (currentTarget == null)
            return false;
        if (_stopObj == null)//다거치고 멈춰진 개체가 없다면
            return true;

        return _stopObj == currentTarget.gameObject;//멈췄던 개체라면
    }

    public void UseSkill()
    {
        if (!CanUse())
            return;

        GameObject target =targetSelector._currentTarget.gameObject;

        if (_isStop)
        {
            ReSetObject();
            return;
        }

        StopObject(target);
    }

    

    private void StopObject(GameObject target)
    {
        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();

        if (rb == null) return;

        _otherRb = rb;
        _stopObj = target;
        _saveVel = rb.linearVelocity;
        _saveAngVel = rb.angularVelocity;
        _saveBType = rb.bodyType;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        _isStop = true;
    }

    private void ReSetObject()
    {
        if (_otherRb != null)
        {
            _otherRb.bodyType = _saveBType;
            _otherRb.linearVelocity = _saveVel;
            _otherRb.angularVelocity = _saveAngVel;
        }

        _isStop = false;
        _stopObj = null;
        _otherRb = null;
    }

    private bool IsVisible(GameObject target)
    {
        if (target == null) return false;

        Vector3 screenPos = mainCamera.WorldToViewportPoint(target.transform.position);

        return screenPos.z > 0 && screenPos.x >= 0 && screenPos.x <= 1 && screenPos.y >= 0 && screenPos.y <= 1;
    }

    private void OnDisable()
    {
        ReSetObject();
    }
}