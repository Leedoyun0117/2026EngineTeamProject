using UnityEngine;

namespace LDY.Script
{
    public class CloseSubScreenAction : MenuActionBehaviour
    {
        [SerializeField] private SubScreenController controller;

        public override void Execute()
        {
            controller.CloseCurrent();
        }
    }
}
