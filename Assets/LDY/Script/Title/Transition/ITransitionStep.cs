using System;
using System.Collections;
using UnityEngine;

namespace LDY.Script
{
    public interface ITransitionStep
    {
        IEnumerator Run(TransitionContext context);
    }

    public class TransitionContext
    {
        public bool Failed { get; private set; }
        public string Reason { get; private set; }

        public void Fail(string reason)
        {
            if (Failed)
                return;

            Failed = true;
            Reason = reason;
            Debug.LogWarning($"[Transition] 실패: {reason}");
        }

        public void Fail(Exception exception)
        {
            Fail(exception.Message);
            Debug.LogException(exception);
        }
    }
}
