using LDY.StageSelect;
using LDY.StageSelect.Core;
using UnityEngine;

namespace LDY.Script {
    // 파일 썸네일 윗변 발판 하나. 타이틀 아이콘(MenuIcon)과 같은 IStandTarget이라 MenuSelector/StandingMenuFinder를 그대로 쓴다.
    // 강조 = 선택 화면의 기존 선택(파일명 남색 바 + 점선 포커스). 발판을 떠날 때는 선택을 그대로 둔다.
    public sealed class FilePlatformTarget : IStandTarget {
        private readonly IFileBoardHost _host;

        public FilePlatformTarget(string id, Collider2D platform, Vector2 standPoint, IFileBoardHost host) {
            Id = id;
            Platform = platform;
            StandPoint = standPoint;
            _host = host;
        }

        public string Id { get; }
        public Collider2D Platform { get; }

        // 발판 윗면 가운데(월드)
        public Vector2 StandPoint { get; }

        public bool IsUsable(IMenuGate gate) {
            return Platform != null && Platform.isActiveAndEnabled && gate.CanExecute;
        }

        public bool IsStoodOn(Collider2D body) {
            return StandCheck.IsStandingOn(body, Platform);
        }

        public void SetHighlighted(bool highlighted) {
            _host.Select(StandSelection.Next(_host.SelectedId, highlighted ? Id : null));
        }

        public void Execute() {
            _host.Execute(Id);
        }
    }
}
