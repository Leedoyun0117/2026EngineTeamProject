using UnityEngine;

public enum CanIf
{
    CanCut,
    CanStop,
    CanResize,
    CanDrag,
    CanOpacity
}

public class Object_HTY : MonoBehaviour
{
    public CanIf _myCan;
}
