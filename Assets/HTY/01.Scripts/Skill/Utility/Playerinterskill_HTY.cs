using UnityEngine;
using UnityEngine.InputSystem;

public class Playerinterskill_HTY : MonoBehaviour
{
    [SerializeField] private Vector2 _sizeBox;
    private SkillManager_HTY _skillManager;

    private void Awake()
    {
        _skillManager = GetComponent<SkillManager_HTY>();
    }

    private void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            Collider2D[] coll = Physics2D.OverlapBoxAll(transform.position, _sizeBox, 0);

            foreach (var v in coll)
            {
                if (v.TryGetComponent(out SkillBox_HTY skillbox))
                {
                    _skillManager.ChangeSkillState(skillbox._myskill);
                    Destroy(skillbox.gameObject);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, _sizeBox);
    }
}
