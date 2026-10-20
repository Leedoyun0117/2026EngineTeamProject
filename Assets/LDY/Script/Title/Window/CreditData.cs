using System;
using UnityEngine;

namespace LDY.Script
{
    [Serializable]
    public class CreditEntry
    {
        [Tooltip("역할 (예: 프로그래밍)")] public string role;
        [Tooltip("이 역할을 맡은 사람들")] public string[] names = Array.Empty<string>();
    }

    [CreateAssetMenu(menuName = "LDY/Credit Data", fileName = "CreditData")]
    public class CreditData : ScriptableObject
    {
        public CreditEntry[] entries = Array.Empty<CreditEntry>();
    }
}
