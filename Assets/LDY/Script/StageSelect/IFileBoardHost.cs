using System;
using System.Collections.Generic;
using UnityEngine;

namespace LDY.StageSelect {
    // 선택 화면 안을 걷는 플레이어(Assembly-CSharp 쪽 브리지)가 화면에 요구하는 것만 모은다.
    public interface IFileBoardHost {
        bool IsReady { get; }

        // 대화상자·설정창이 열려 있거나 씬 전환(실행 중...)이 진행 중. 이동 입력을 잠그고 실행을 막는다.
        bool Blocking { get; }

        string SelectedId { get; }
        IReadOnlyList<string> VisibleIds { get; }

        // 파일 목록 패널. 발판은 이 사각형 안에만 만든다.
        RectTransform FileArea { get; }

        // 보이는 파일 목록이 바뀜(필터 탭, 월드 전환). 발판을 다시 만든다.
        event Action LayoutChanged;

        void Select(string id);
        void Execute(string id);
    }
}
