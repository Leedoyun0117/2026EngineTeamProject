# 스테이지 씬 템플릿 사용법

## 새 스테이지 만들기
1. 메뉴 `LDY/Merge/Create Stage Scene`
2. Stage id(예: `2-3`), 씬 이름, 폴더(기본 `Assets/Scenes/Stages`)를 고르고 **만들기**
3. 씬이 열리고 다음이 자동으로 들어간다. 이름은 바꾸지 말 것.

| 오브젝트 | 내용 |
|---|---|
| StageRoot | 스테이지 id. 씬에 1개 |
| PlayerStart / Player | `Assets/prefab/Player.prefab` 인스턴스 1개 |
| Grid | Tilemap `Ground` / `Platform`(위로 통과) / `Hazard`(닿으면 재시작) |
| StageGoal | 플레이어가 닿으면 클리어 → 기록 저장 → 선택 화면 |
| Main Camera + StageCamera | ortho 5, 플레이어 따라감, `Bounds` 안에서만 움직임 |
| Bounds | 카메라 이동 범위. 맵 크기에 맞게 Polygon 점을 늘린다 |
| DeathZone | 떨어지면 재시작. 맵 아래쪽에 맞게 옮긴다 |
| Global Light 2D | 기본 조명 |

같은 경로로 다시 만들기를 누르면 **빠진 것만** 채운다(기존 오브젝트는 그대로).

## 규칙
- Player는 씬에 **1개만**. 복사 플레이어는 런타임에만 만든다.
- 골은 `StageGoal` 컴포넌트(트리거 콜라이더)를 붙이면 몇 개든 된다.
- 플레이어가 `Destroy`되면(KillHitbox 등) 자동으로 스테이지를 다시 시작한다.
- Esc를 누르면 선택 화면으로 돌아간다.

## 한 씬에 스테이지 여러 개 (JJBLevel 방식)
- 스테이지마다 루트 오브젝트 하나(예: `Stage1-2`). 같은 부모 아래 형제로 둔다.
- 실행하면 고른 루트만 켜지고 나머지 등록 루트는 꺼진다(씬 파일은 안 바뀜).
- 각 루트 안에 **시작 지점**을 하나 둔다: 빈 오브젝트에 `StageSpawnPoint` 컴포넌트, 또는 이름 `Start`.
  없으면 플레이어가 씬에 놓인 자리에서 시작한다.
- 골은 `Stage1_GoalMusicFile` 프리팹 인스턴스를 두면 자동으로 연결된다.
- 카메라 범위를 직접 정하려면 루트 안에 `CameraBounds`(Polygon/Box Collider2D)를 둔다. 없으면 타일맵 범위로 잡는다.

## 끝나면 검사
메뉴 `LDY/Merge/Validate Open Stage` → Console에 체크리스트가 나온다. 씬을 고치지 않는다.

- StageRoot가 있고 id가 카탈로그에 있음
- Player 정확히 1개, Player 프리팹 인스턴스
- StageGoal 1개 이상
- Camera 1개
- Missing Script / Missing Prefab 없음
- Build Settings 포함

전부 `[OK]`면 이도윤에게 씬 경로를 알려 주세요.
