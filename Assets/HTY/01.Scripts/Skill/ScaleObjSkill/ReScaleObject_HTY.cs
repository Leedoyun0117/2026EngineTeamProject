using UnityEngine;

public class ReScaleObject_HTY : MonoBehaviour
{
    public enum ScaleState
    {
        Small,
        Normal,
        Large
    }

    [SerializeField] private float _smallScale = 0.5f;
    [SerializeField] private float _largeScale = 1.5f;

    private Vector3 _originScale;
    public ScaleState CurrentState { get; private set; } = ScaleState.Normal;

    private void Awake()
    {
        _originScale = transform.localScale;
    }

    public void ChangeScale()
    {
        switch (CurrentState)
        {
            case ScaleState.Small:
                SetScale(ScaleState.Normal);
                break;

            case ScaleState.Normal:
                SetScale(ScaleState.Large);
                break;

            case ScaleState.Large:
                SetScale(ScaleState.Small);
                break;
        }
    }

    private void SetScale(ScaleState state)
    {
        CurrentState = state;
        float multi =1;

        switch (state)
        {
            case ScaleState.Small:
                multi = _smallScale;
                break;
            case ScaleState.Normal:
                multi = 1f;
                break;
            case ScaleState.Large:
                multi = _largeScale;
                break;
        }


        transform.localScale = _originScale * multi;
    }
}