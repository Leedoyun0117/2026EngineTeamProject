using UnityEngine;

public class DragObj : MonoBehaviour
{
    public Rigidbody2D _rb;

    private void Awake()
    {
        _rb=GetComponent<Rigidbody2D>();
    }
}
