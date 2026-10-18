using UnityEngine;

public class PositionSelectSkill_HTY : MonoBehaviour, ISkill_HTY
{
    [SerializeField]
    private PointerController_HTY _pointer;

    public Vector2 SelectedPosition { get; private set; }

    public bool CanUse()
    {
        return _pointer != null && _pointer.isActiveAndEnabled;
    }

    public void UseSkill()
    {
        if (!CanUse())
            return;

        SelectedPosition = _pointer.CurrentPosition;

        Debug.Log($"선택된 위치: {SelectedPosition}");
    }
}