using UnityEngine;

public class OpacityObj_HTY : MonoBehaviour
{
    private Collider2D _coll;
    private SpriteRenderer _sp;

    private void Awake()
    {
        _sp = GetComponent<SpriteRenderer>();
        _coll = GetComponent<Collider2D>();
    }

    private void Update()
    {
        _coll.isTrigger = _sp.color.a < 0.9f ? true : false;
    }
    /*private void OnCollisionEnter2D(Collision2D collision)
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
    }*/
}
