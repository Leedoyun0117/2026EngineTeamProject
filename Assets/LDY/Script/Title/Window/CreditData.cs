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

    // 크레딧 창에 나오는 역할과 이름. 코드를 고치지 않고 이 에셋만 편집한다.
    [CreateAssetMenu(menuName = "LDY/Credit Data", fileName = "CreditData")]
    public class CreditData : ScriptableObject
    {
        public CreditEntry[] entries = Array.Empty<CreditEntry>();
    }
}
