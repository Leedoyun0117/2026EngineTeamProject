using System;
using System.Collections;
using System.Collections.Generic;

namespace LDY.Script
{
    // 단계가 예외를 내거나 실패를 알리면 거기서 멈춤
    public class TransitionSequence
    {
        private readonly IReadOnlyList<ITransitionStep> _steps;

        public TransitionSequence(IReadOnlyList<ITransitionStep> steps)
        {
            _steps = steps;
        }

        public IEnumerator Run(TransitionContext context)
        {
            foreach (ITransitionStep step in _steps)
            {
                yield return SafeEnumerator.Guard(step.Run(context), context.Fail);
                if (context.Failed)
                    yield break;
            }
        }
    }

    // yield 안에서는 try/catch를 못 써서, 중첩 코루틴 예외까지 잡으려고 스택으로 직접 돌림
    public static class SafeEnumerator
    {
        public static IEnumerator Guard(IEnumerator root, Action<Exception> onError)
        {
            var stack = new Stack<IEnumerator>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                IEnumerator top = stack.Peek();
                bool moved = false;
                Exception error = null;
                try
                {
                    moved = top.MoveNext();
                }
                catch (Exception e)
                {
                    error = e;
                }

                if (error != null)
                {
                    onError(error);
                    yield break;
                }

                if (!moved)
                {
                    stack.Pop();
                    continue;
                }

                if (top.Current is IEnumerator nested)
                {
                    stack.Push(nested);
                    continue;
                }

                yield return top.Current;
            }
        }
    }
}
