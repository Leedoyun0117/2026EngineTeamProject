using UnityEngine;

public class OpacityObj_HTY : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr)) if(sr.color.a == 0.5f) gameObject.GetComponent<Collider2D>().isTrigger=true;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr)) if (sr.color.a != 0.5f) gameObject.GetComponent<Collider2D>().isTrigger = false;
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr)) if (sr.color.a == 0.5f) gameObject.GetComponent<Collider2D>().isTrigger = true;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr)) if (sr.color.a != 0.5f) gameObject.GetComponent<Collider2D>().isTrigger = false;
    }
}
