using UnityEngine;

namespace LDY.Script
{
    public class QuitAction : MenuActionBehaviour
    {
        public override void Execute()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
