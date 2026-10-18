using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CursorLighting_HTY : MonoBehaviour,ISkill_HTY
{
    private Light2D _light;
    private bool _isLight = false;

    private void Awake()
    {
        _light = GetComponentInChildren<Light2D>();
    }

    public bool CanUse()
    {
        return true;
    }

    public void UseSkill()
    {
        if (CanUse())
        {
            _light.enabled = _isLight;
            _isLight = !_isLight;
        }
    }
}
