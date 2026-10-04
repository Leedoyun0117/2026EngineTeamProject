using UnityEngine;
using UnityEngine.InputSystem;

public static class Utils_HTY
{
    public static Vector3 GetMousePos()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = 0;
        return mousePos;
    }
}
