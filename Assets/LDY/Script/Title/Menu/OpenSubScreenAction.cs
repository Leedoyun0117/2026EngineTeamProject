using UnityEngine;

namespace LDY.Script
{
    public class OpenSubScreenAction : MenuActionBehaviour
    {
        [SerializeField] private SubScreenController controller;
        [SerializeField] private SubScreenPanel panel;

        public override void Execute()
        {
            controller.Open(panel);
        }
    }
}
