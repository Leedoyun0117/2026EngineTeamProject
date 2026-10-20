using UnityEngine;

public class OpacityObj_HTY : MonoBehaviour
{
    private Collider2D _coll;

    private void Awake()
    {
        _coll = GetComponent<Collider2D>();
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out OpacityPlayer_HTY sr)) 
        {
            if (sr._isOpacity) 
                _coll.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out OpacityPlayer_HTY sr))
        {
            if (!sr._isOpacity)
                _coll.isTrigger = false;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out OpacityPlayer_HTY sr)) 
        {
            if (sr._isOpacity) 
                _coll.isTrigger = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out OpacityPlayer_HTY sr)) 
        {
            if (!sr._isOpacity) 
                _coll.isTrigger = false;
        }
    }
}
