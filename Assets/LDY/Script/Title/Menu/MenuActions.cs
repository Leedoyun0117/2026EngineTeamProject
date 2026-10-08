using UnityEngine;

namespace LDY.Script
{
    public class StartGameAction : MenuActionBehaviour
    {
        [SerializeField] private GameStartTransition transition;

        public override void Execute()
        {
            transition.Begin();
        }
    }

    public class OpenSubScreenAction : MenuActionBehaviour
    {
        [SerializeField] private SubScreenController controller;
        [SerializeField] private SubScreenPanel panel;

        public override void Execute()
        {
            controller.Open(panel);
        }
    }

    public class CloseSubScreenAction : MenuActionBehaviour
    {
        [SerializeField] private SubScreenController controller;

        public override void Execute()
        {
            controller.CloseCurrent();
        }
    }

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
