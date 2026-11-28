using System.Collections.Generic;
using LDY.StageImport.Core;

namespace LDY.StageImportEditor {
    internal enum SeedCertainty {
        Sure,
        Unsure,
        None
    }

    internal sealed class CustomSeed {
        public CustomSeed(string name, CustomBinding binding, CustomSizing sizing, SeedCertainty certainty,
            string[] candidates, string reason, params string[] aliases) {
            Name = name;
            Binding = binding;
            Sizing = sizing;
            Certainty = certainty;
            Candidates = candidates;
            Reason = reason;
            Aliases = aliases;
        }

        public string Name { get; }
        public IReadOnlyList<string> Aliases { get; }
        public CustomBinding Binding { get; }
        public CustomSizing Sizing { get; }
        public SeedCertainty Certainty { get; }

        // 확실(Sure)일 때만 앞에서부터 있는 프리팹을 대응표에 넣는다. 애매(Unsure)면 메모에만 적는다.
        public IReadOnlyList<string> Candidates { get; }
        public string Reason { get; }

        // 새 항목을 만들 때만 넣는 값(있는 항목은 사람이 바꿨을 수 있어 건드리지 않는다).
        public CustomTileReplace Tile { get; set; }

        // 프리팹 대신 놓을 충돌 없는 장식 그림 경로(.aseprite면 안의 스프라이트). 없으면 SkipIfEmpty대로.
        public string Decoration { get; set; }
        public bool SkipIfEmpty { get; set; }

        // 씬의 다른 오브젝트를 가리켜야 동작하는 필드('스크립트.필드'). 있으면 대응표의 "연결 필요"를 켠다.
        public string[] LinkFields { get; set; } = new string[0];

        public string CertaintyLabel =>
            Certainty == SeedCertainty.Sure ? "확실" : Certainty == SeedCertainty.Unsure ? "애매" : "없음";
    }

    // stages.json(17맵)에 쓰인 custom 이름과 0단계 조사 결과. UnityEditor를 쓰지 않아 코어 테스트에서도 읽을 수 있다.
    internal static class CustomBlockSeed {
        private const string MainObj = "Assets/Main/02.Prefabs/Obj/";
        private const string Hes = "Assets/HES/prefab/";
        private const string Jjb = "Assets/JJB/Prefab/";
        private const string Ldy1 = "Assets/LDY/Prefabs/Stage1/";
        private const string Ldy2 = "Assets/LDY/Prefabs/Stage2/";
        public const string RopePrefab = "Assets/LDY/Prefab/Rope/Rope.prefab";
        public const string ChainSprite = "Assets/LDY/Sprites/stage1_music_objects/axe_guillotine_chain.aseprite";

        // L/R 특수 규칙에 쓰는 항목 이름
        public const string LrDoorName = "벽 L이랑 R을 둘다 먹어야지 열리는 벽";
        public const string LName = "L";
        public const string RName = "R";

        private const SeedCertainty Sure = SeedCertainty.Sure;
        private const SeedCertainty Unsure = SeedCertainty.Unsure;
        private const SeedCertainty None = SeedCertainty.None;

        private const CustomBinding PerCell = CustomBinding.PerCell;
        private const CustomBinding Group4 = CustomBinding.Group4;
        private const CustomBinding Run = CustomBinding.HorizontalRun;
        private const CustomBinding Area = CustomBinding.Area;

        private const CustomSizing Stretch = CustomSizing.Stretch;
        private const CustomSizing Repeat = CustomSizing.RepeatPerCell;
        private const CustomSizing One = CustomSizing.OnePerGroup;

        private static readonly string[] NoCandidates = new string[0];

        public static readonly IReadOnlyList<CustomSeed> All = new[] {
            new CustomSeed("거미?", Group4, One, Sure, P(Ldy2 + "Spider.prefab"),
                "지정값: LDY Spider(SpiderController), 묶음당 하나·묶음 가운데. 2-3에 42칸(7x11) 한 덩어리"),
            new CustomSeed("벽", Group4, Stretch, Sure, NoCandidates,
                "지정값: Ground 타일로 대체. 1월드(1-3, 1-4)·2월드(2-2, 2-4)의 id 2개 모두") { Tile = CustomTileReplace.Ground },
            new CustomSeed("어두울때만 보이는 블록", PerCell, Stretch, None, NoCandidates,
                "dark_only_block 그림이 프로젝트에 없어 분홍 유지. 그림이 생기면 DarkOnlyBlock 프리팹(BoxCollider2D, 스크립트 없음)을 만들어 연결"),
            new CustomSeed("유리", PerCell, Stretch, Sure, P(MainObj + "Glass.prefab", Hes + "Glass.prefab"),
                "이름·태그 Glass 일치 (Main Glass = HES Glass 사본). 여러 칸 덩어리는 칸마다 하나씩"),
            new CustomSeed("대포", Group4, CustomSizing.OneAtLeft, Sure, P(MainObj + "Cannon.prefab", Hes + "cannon.prefab"),
                "Cannon 스크립트. 지정값: 묶음당 하나, 묶음의 가장 왼쪽 칸(1-4의 24칸 가로줄도 하나)"),
            new CustomSeed("신호가 가면 사라지는 블록", Group4, Repeat, Sure, P(Hes + "SwitchToInvisibleDoor.prefab"),
                "태그 Invisible: HES Receiver.response가 켜지면 꺼진다. 동작하려면 씬 Receiver의 목록에 손으로 넣어야 함") { LinkFields = L("Receiver.receivers") },
            new CustomSeed("깨지는 블록", PerCell, Stretch, Sure, P(Ldy1 + "Stage1_Glass.prefab"),
                "LDY Stage1_Glass = HES Glass 변형에 breakable_block 아트를 입힌 것. 1-4에서만 쓰임"),
            new CustomSeed("엘레베이터", Group4, One, Sure, P(Hes + "Elevator.prefab"),
                "Elevator + SwitchElevator. 3x4 덩어리마다 하나(원래 크기, 덩어리 가운데)"),
            new CustomSeed("올라가면 보이는 영역", Area, Stretch, Unsure, P(Hes + "Blind.prefab"),
                "영역 표시로 둠(트리거 사각형). HES Blind(닿으면 가림막이 걷힘)가 비슷한 동작"),
            new CustomSeed("버튼문 테스트 열림", Area, Stretch, None, NoCandidates,
                "col none, 문이 열린 상태를 표시한 칸으로 보여 영역으로 둠"),
            new CustomSeed("버튼 문", Group4, Stretch, Sure, P(Hes + "StandardDoor.prefab"),
                "HES NormalButton.door에 연결하는 문(스크립트 없음, 1x1). 9x1 덩어리를 하나로 늘림. 버튼 연결은 손으로"),
            new CustomSeed("신호가 가면 나타남", Area, Stretch, Unsure, P(Hes + "SwitchToVisibleDoor.prefab"),
                "요청대로 영역 표시로 둠. 실제 블록이면 HES SwitchToVisibleDoor(태그 Visible)로 바꾸고 묶는 방식을 4방향 묶음으로") { LinkFields = L("Receiver.receivers") },
            new CustomSeed("밧줄", Group4, CustomSizing.HangFromTop, Sure, P(RopePrefab),
                "지정값: LDY Rope 프리팹(RopeTest 씬 구성). 맨 위 칸 윗변(천장 면) 가운데가 고정점, 그 열의 칸 수가 줄 길이(RopeSettings 사본)"),
            new CustomSeed("기요틴", Group4, One, Sure,
                P(MainObj + "Guillotine.prefab", Hes + "Guillotine.prefab", Ldy1 + "Stage1_Guillotine.prefab"),
                "Guillotine + KillHitbox. 1-2의 5x3 덩어리 하나에 하나"),
            new CustomSeed("360도 회전 블록", Run, One, Sure, P(MainObj + "RotateBlock.prefab", Jjb + "RotateBlock.prefab"),
                "RotateBlock 스크립트. 콜라이더가 이미 가로 3칸이라 늘리지 않고 가로 런마다 하나", "360도 회전하는 블록"),
            new CustomSeed("톱 어두울때만 보이는 톱니바퀴", PerCell, Stretch, Sure, P(Ldy1 + "Stage1_Gear.prefab"),
                "지정값: JJB에 Gear 스크립트가 붙은 프리팹이 없어 LDY Stage1_Gear(톱니 아트). '어두울 때만 보임' 동작 없음"),
            new CustomSeed("벽 L이랑 R을 둘다 먹어야지 열리는 벽", Group4, One, Sure,
                P(MainObj + "Stage1_LRDoor.prefab", Hes + "LRDoor.prefab"),
                "LRDoor + LButton + RButton. 같은 맵에 L·R 칸이 하나씩 있으면 인스턴스의 L/R 버튼을 그 칸으로 옮김(원본 그대로)"),
            new CustomSeed("문", Group4, Stretch, Sure, P(Hes + "StandardDoor.prefab"),
                "버튼·스위치의 door 대상으로 쓰는 문(스크립트 없음). 연결은 손으로"),
            new CustomSeed("버튼", Group4, One, Sure, P(MainObj + "NormalButton.prefab", Hes + "NormalButton.prefab"),
                "NormalButton 스크립트. col이 none/solid 두 종류 → 같은 프리팹. 2월드용 NormalButtonStage2도 있음. door 연결은 손으로") { LinkFields = L("NormalButton.door") },
            new CustomSeed("상자로만 누를 수 있는 버튼", PerCell, Stretch, None, NoCandidates,
                "HES ButtonBox.cs(Box 태그에만 반응)는 있지만 프리팹 없음"),
            new CustomSeed("길게 누르기 버튼", PerCell, Stretch, Sure,
                P(MainObj + "LongPressButton.prefab", Hes + "LongPressButton.prefab"),
                "LongPressButton 스크립트", "길게 누르는 버튼") { LinkFields = L("LongPressButton.door") },
            new CustomSeed("레버", PerCell, Stretch, Sure, P(Hes + "Switch.prefab"),
                "지정값: HES Switch(닿을 때마다 door 토글). door 연결은 손으로") { LinkFields = L("Switch.door") },
            new CustomSeed("사슬", Group4, Stretch, Sure, NoCandidates,
                    "지정값: 충돌 없는 장식, axe_guillotine_chain 그림을 묶음 크기로 늘림. 그림이 없으면 건너뜀")
                { Decoration = ChainSprite, SkipIfEmpty = true },
            new CustomSeed("곰덫", PerCell, Stretch, Sure, P(Ldy2 + "MouseTrap.prefab"),
                "지정값: LDY 2스테이지 MouseTrap(MouseTrap + DragObj), 칸마다"),
            new CustomSeed("떨어지는 오브젝트 (피해야함)", Group4, One, Sure,
                P(MainObj + "FallingBlock.prefab", Hes + "FallingBlock.prefab"),
                "FallingBlock 스크립트. 2-4의 1x4 세로 덩어리에 하나만 놓음(낙하 경로를 칠한 것이면 위치 조정)"),
            new CustomSeed("가짜 블록", PerCell, Stretch, Sure, P(Hes + "FakeBlock.prefab"), "Block(isReal=false)"),
            new CustomSeed("진짜 블록", PerCell, Stretch, Sure, P(Hes + "RealBlock.prefab"), "Block(isReal=true)"),
            new CustomSeed(RName, PerCell, Stretch, Unsure, P(MainObj + "Stage1_LRDoor.prefab"),
                "LRDoor 프리팹 안의 RButton 자식. LRDoor와 짝이 맞으면 버튼을 이 칸으로 옮기고 분홍 블록을 놓지 않음"),
            new CustomSeed(LName, PerCell, Stretch, Unsure, P(MainObj + "Stage1_LRDoor.prefab"),
                "LRDoor 프리팹 안의 LButton 자식. LRDoor와 짝이 맞으면 버튼을 이 칸으로 옮기고 분홍 블록을 놓지 않음"),
            new CustomSeed("공격하는 무언가?", Group4, One, None, NoCandidates,
                "지정값: 비워 둠. 이름에 '?'. 2-3(거미 맵)에 1x2"),
            new CustomSeed("좌우 적", PerCell, Stretch, None, NoCandidates,
                "좌우로 움직이는 적 프리팹 없음 (Main MovingBlock은 발판)"),
            new CustomSeed("스위치", PerCell, Stretch, Sure, P(Hes + "Switch.prefab"), "HES Switch(닿을 때마다 door 토글)") { LinkFields = L("Switch.door") }
        };

        private static string[] P(params string[] paths) => paths;

        private static string[] L(params string[] fields) => fields;
    }
}
