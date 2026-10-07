using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput_HTY : MonoBehaviour
{
    public Vector2 _moveDir;

    public void OnMove(InputValue value)
    {
        _moveDir = value.Get<Vector2>();
    }

    public Vector2 GetVector()
    {
        return _moveDir;
    }
}
