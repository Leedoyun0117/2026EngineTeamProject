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
}
