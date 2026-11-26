# LDY 코드 감사 보고서

- 작성일: 2026-11-25 (병합 예정일 2026-11-29)
- 대상: `Assets/LDY` 아래 모든 `.cs` (Script 약 16,900줄, Editor 약 8,900줄). 테스트(`Tests/EditMode`)는 지적 대상에서 빼고 사용처 확인에만 썼다.
- 기준 시점: 브랜치 `LDY`의 작업 트리(커밋 안 된 StageSelect 변경 포함).
- 감사 시점에는 코드를 고치지 않았다. 같은 날 11.1절의 병합 전 필수 항목을 고쳤고, 해결한 항목에는 **해결됨 (2026-11-25)**, 일부만 해결한 항목에는 **부분 해결**, 중단 조건 때문에 고치지 않은 항목에는 **보류**를 적었다. 수정 내역은 12절에 있다.

## 읽는 법

- 위치는 `Assets/LDY/` 기준 상대 경로다. 예: `Script/Flow/SceneFlow.cs:44`
- 분류 번호
  - 1R = 실행 중에 코드로 만드는 UI·오브젝트, 1B = 에디터 메뉴(빌드 시점)에서 만드는 것
  - 2 = 하드코딩 (여러 파일에 같은 값이 있으면 `2·중복`)
  - 3 = 구조 (300줄 초과, 책임 3개 이상, 순환 의존, static 상태)
  - 4 = 위험한 탐색 (Find 계열, 문자열로 찾기)
  - 5 = 런타임 비용 (Update 계열의 할당·반복 탐색)
  - 6 = 죽은 코드·폐기된 기능 잔재
  - 7 = 팀원(JJB/HES/HTY) 코드·에셋·이름 의존
- 심각도
  - **높음**: 병합하거나 팀원이 바꾸면 깨지는 곳, 또는 이미 깨진 곳
  - **중간**: 지금은 동작하지만 조건에 따라 오동작하거나 고치는 비용이 큰 곳
  - **낮음**: 정리하면 좋은 수준
- 조사 방법: Update 계열 메서드가 있는 파일 전부와 조립·흐름·브리지·어댑터 파일은 전체를 읽었다. 나머지는 패턴 검색(`new GameObject`, `AddComponent`, `Find*`, 문자열 경로·태그·레이어, 색·수치 리터럴) 결과 주변을 확인했다. 죽은 코드는 "다른 코드에서 타입 이름 참조 0건 + 씬·프리팹·에셋에서 스크립트 GUID 참조 0건"으로 판정했고, 팀원 의존은 팀원 폴더를 읽기만 해서 실제로 존재하는지 확인했다.

---

## 1. 요약

### 1.1 항목별 개수

한 항목이 여러 분류에 걸치면 각 분류에 모두 셌다. 고유 항목은 83건이다(높음 5, 중간 36, 낮음 42).

| 분류 | 건수 | 비고 |
|---|---:|---|
| 1R 실행 중 생성 | 16 | 창·로딩 창·선택 화면 UI 전체가 런타임 생성 |
| 1B 빌드 시점 생성 | 2 | 도구 24개는 따로 목록으로 정리(8.2절). 빌드 시점 생성 자체는 문제가 아니다 |
| 2 하드코딩 | 18 | 이 중 `2·중복` 7건 |
| 3 구조 | 16 | 300줄 초과 클래스 2개, 폴더 간 순환 의존 3쌍 |
| 4 위험한 탐색 | 10 | Update 안에서 매번 호출하는 곳은 1곳(거미 TagPlayerLocator) |
| 5 런타임 비용 | 6 | 매 프레임 할당이 확실한 곳은 1곳(TaskbarView 시계) |
| 6 죽은 코드 | 7 | 소프트웨어 커서·렌더러 프록시 런타임 잔재는 없음. 에디터 쪽 Pink·Proxy 변환기만 남음 |
| 7 팀원 의존 | 18 | 이 중 높음 4건, 1건은 이미 깨짐 |

### 1.2 우선순위 Top 10

기준을 채우는 **높음**은 5건뿐이라, 나머지 5개 자리는 병합에 가장 영향이 큰 **중간** 항목으로 채웠다.

| 순위 | ID | 심각도 | 한 줄 요약 |
|---:|---|---|---|
| 1 | E-02 | 높음 → **해결됨 (2026-11-25)** | 타이틀 셋업이 쓰는 `Assets/JJB/JJB.physicsMaterial2D`가 이미 없다(팀원이 `JJBPM2D`로 바꿈). 정정: 이 경로는 AltPlayer를 **새로 만들 때만** 쓰이고, 기존 AltPlayer 재질은 GUID로 이미 JJBPM2D에 연결돼 있다 |
| 2 | P-08 | 높음 → **해결됨 (2026-11-25)** | 타이틀·선택 화면 플레이어가 JJB 입력 에셋의 액션 이름 `Move/Jump/Interact`를 `FindAction(…, true)`로 찾는다. 이름이 바뀌면 예외가 나서 조작이 안 된다 |
| 3 | P-01 | 높음 → **보류** | 점프 차단이 `JJBPlayerJump.OnDisable`의 내부 구현(플래그 초기화)에 기대고 있다. Input 방식으로 바꾸면 JJB 동작이 달라져서 바꾸지 않았다(12.3절) |
| 4 | F-17 | 높음 → **해결됨 (2026-11-25)** | `"JJB.Script.Level.JJBLevelManager"`를 문자열로 찾아 끈다. 이름이 바뀌면 조용히 안 꺼져서 두 매니저가 함께 동작한다 |
| 5 | F-22 | 높음 → **부분 해결** | 카탈로그가 JJBLevel 안의 루트 이름 `Stage1-Tutorial`, `Stage1-1`~`Stage1-6`을 고정으로 쓴다(지금은 7개 모두 있다). 루트가 없으면 이제 멈추지 않고 선택 화면으로 돌아가지만, 이름에 의존하는 구조는 그대로다 |
| 6 | P-09 | 중간 | 플레이어 프리팹이 3종(`Assets/prefab`, `Assets/Main/02.Prefabs/Player`, `Assets/JJB/Prefab`)이고 경로가 3곳에 흩어져 있다. 병합 기준 프리팹을 정해야 한다 |
| 7 | F-23 / E-11 | 중간 | LDY 빌더가 공용 Build Settings 순서와 TagManager 레이어를 자동으로 고친다 |
| 8 | R-06 | 중간 | Alt↔Title, Alt↔Rope, Rope↔Player가 서로를 참조하고, 모두 Assembly-CSharp에 있어 팀원 코드와 한 덩어리로 컴파일된다 |
| 9 | P-04 | 중간 | 팀원 플레이어에 LDY 컴포넌트 7종을 런타임에 몰래 붙인다. 프리팹을 봐서는 구성이 보이지 않는다 |
| 10 | T-10 | 중간 | 씬 로더와 로딩 화면이 두 벌이다(타이틀은 씬 이름 + LoadingOverlay, 나머지는 경로 + LoadingWindow) |

---

## 2. 타이틀

| ID | 분류 | 위치 | 심각도 | 문제 | 개선 제안 |
|---|---|---|---|---|---|
| T-01 | 1R | `Script/Title/Window/WindowPanel.cs:148-207`, `WindowKit.cs:44-126`, `SliderWidget.cs`, `DropdownWidget.cs` | 중간 | 설정·크레딧 창 전체(프레임, 제목줄, TMP 글자, 발판, 닫기 버튼, 슬라이더, 드롭다운)를 처음 열 때 `new GameObject` + `AddComponent`로 만든다. 씬이나 프리팹에서 보거나 고칠 수 없다 | 창 틀은 에디터 빌더로 프리팹에 굽고, 런타임은 값 바인딩만 한다 |
| T-02 | 1R | `WindowPanel.cs:235-240`, `WindowSideWalls.cs:40-43`, `Crt/ContentBoundsWalls.cs:27-30`, `Window/OneWayPlatform.cs:10-17` | 낮음 | 경계 벽과 단방향 발판 콜라이더를 런타임에 만든다. 레이아웃 계산 결과라 이유는 있다 | 그대로 두되 레이어 결정 로직을 한곳으로 모은다 |
| T-03 | 1R | `Visual/FocusFrame.cs:25-27,76`, `WindowPanel.cs:213` | 낮음 | 포커스 테두리 오브젝트와 임시 `FocusFrameStyle`(ScriptableObject)을 런타임에 만든다 | 기본 FocusFrameStyle 에셋을 하나 만들어 연결한다 |
| T-04 | 1R | `Transition/ZoomMasks.cs:66-70`, `Carrier/LoadingOverlay.cs:80-97`, `Carrier/TransitionCarrier.cs:64-66`, `Carrier/LoadingDots.cs:20` | 낮음 | 가림막 4장, 대체용 모래시계 텍스처, 전환 캐리어를 런타임에 만든다 | 모래시계 대체 텍스처는 지우고 아트를 필수로 한다. 나머지는 그대로 둔다 |
| T-05 | 2 | `Window/WindowStyle.cs:6-99` | 중간 | 창 레이아웃 상수 약 45개, 색 4개(83-86), 정렬 순서 9개가 코드 상수다 | WindowStyle을 ScriptableObject로 빼서 인스펙터에서 조정한다 |
| T-06 | 2 | `Window/CreditPanel.cs:8-21`, `Window/WindowPanel.cs:9-19` | 낮음 | 크레딧 줄 높이·글자 크기·역할 색, 닫기 버튼 크기, 벽 여유값이 상수다 | T-05의 스타일 에셋에 합친다 |
| T-07 | 2·중복 | `Window/WindowServices.cs:67-72` ↔ `Script/StageSelect/StageSelectText.cs:30-35` | 중간 | 설정 문구 6개(전체 음량, 배경음, 효과음, 화면 모드, 전체 화면, 창 모드)가 두 파일에 똑같이 있다 | Flow.Core 같은 공용 위치에 문구 테이블을 하나 둔다 |
| T-08 | 2 | `TitleBootstrap.cs:114` (`"<Keyboard>/s"`), `Input/TitleInputs.cs:33` (`"<Keyboard>/escape"`) | 낮음 | 내려가기·취소 키 바인딩이 코드에 고정돼 있다 | LDY 소유 입력 액션 에셋으로 옮긴다 |
| T-09 | 3 | `Window/WindowPanel.cs` (247줄), `Window/WindowInteraction.cs` (291줄) | 중간 | WindowPanel이 창 생성, 벽 생성, 점프 차단과 행 이동 해제, 포커스 스타일, 열기/닫기 수명까지 맡는다(책임 5개, God Class 후보). WindowInteraction도 300줄에 가깝다 | 창 빌드(WindowBuilder)와 플레이어 제어(RowControl)를 나눈다 |
| T-10 | 3 | `Transition/AsyncSceneLoader.cs:24-34` ↔ `Script/Flow/SceneLoadRunner.cs:33-80`, `Carrier/LoadingOverlay.cs` ↔ `Script/Flow/LoadingWindow.cs` | 중간 | 씬 로더와 로딩 화면이 두 벌이다. 타이틀→선택 화면은 씬 **이름**(`StageSelect`)으로 SceneManager를 직접 부르고 "Cursor.exe 실행 중…"을 띄운다. 나머지 이동은 **경로**로 SceneLoadRunner를 거쳐 "…실행 중..."을 띄운다. 타이틀 전환 중에는 `SceneFlow.IsBusy`가 false다 | 타이틀의 LoadingStep이 SceneFlow(SceneLoadRunner)를 쓰게 하고 로딩 화면을 하나로 합친다 |
| T-11 | 4 | `TitleBootstrap.cs:35` | 낮음 | `FindObjectsByType<MenuIcon>`(비활성 포함)으로 로드된 모든 씬의 아이콘을 모은다 | 아이콘 목록을 직렬화 필드로 받는다 |
| T-12 | 4 | `Visual/TitleVisualApplier.cs:103` (`transform.Find("GroundCheck")`), `Transition/SceneEntryDirector.cs:34` (`FindFirstObjectByType<PlayerInputLock>`) | 낮음 | 문자열 자식 이름 탐색과 씬 전체 검색을 대체 경로로 쓴다 | 직렬화 참조로 바꾼다 |
| T-13 | 5 | `Transition/MonitorZoomRig.cs:46-49` → `:193` | 낮음 | 줌을 유지하는 동안 crt가 연결돼 있지 않으면 매 LateUpdate마다 `GetComponent<CrtMaterialDriver>`를 부른다 | `Play()` 시작 때 한 번만 캐시한다 |
| T-14 | 6 | `Menu/CloseSubScreenAction.cs` | 낮음 | 코드, 씬, 프리팹 어디에서도 쓰지 않는다 | 지운다 |
| T-15 | 6 | `Transition/SceneLoaderBehaviour.cs:9-14` (`LoggingSceneLoader`) | 낮음 | "임시 구현" 주석이 달린 미사용 클래스다. 파일 이름과 클래스 이름이 달라 컴포넌트로 붙일 수도 없다 | 지운다 |
| T-16 | 6 | `Script/Test/TitleClickProbe.cs` (`Assets/Scenes/LDY_TitleScene.unity`의 TitleProbe), `Script/Test/AltLockTester.cs` (LDY_TestScene) | 낮음 | OnGUI 디버그 표시다. EditorOnly 태그라 빌드에는 빠지지만 런타임 어셈블리에 들어 있다 | 디버그 폴더나 에디터 전용 asmdef로 옮긴다 |

---

## 3. 스테이지 선택 / 흐름

| ID | 분류 | 위치 | 심각도 | 문제 | 개선 제안 |
|---|---|---|---|---|---|
| F-01 | 1R | `Script/Flow/FlowBootstrap.cs:17-24`, `Script/Flow/LoadingWindow.cs:18-48`, `Script/Flow/UiFactory.cs:18-41` | 중간 | 로딩 창(Canvas, CanvasScaler, Image·TMP 30여 개)을 BeforeSceneLoad 시점에 코드로 만든다 | LoadingWindow를 프리팹으로 만들고 FlowSettings가 그 프리팹을 참조하게 한다 |
| F-02 | 1R | `Script/StageSelect/StageSelectScreen.cs:64-72` → `Views/TreeView.cs:62-91`, `Views/FileItemView.cs:35-69`, `Views/FilterTabsView.cs:45`, `Views/DialogView.cs:23-47`, `Views/SettingsDialogView.cs:34-118`, `Views/TaskbarView.cs:20-31`, `Views/TitleBarView.cs:13` | 중간 | 선택 화면 내용 전부를 Start에서 UiFactory로 만든다. 씬에는 빈 영역만 구워 둔다(`Editor/Flow/StageSelectSceneBuilder.cs:15` 주석) | 고정 요소(제목줄, 트리 고정 노드, 작업 표시줄, 대화상자)는 빌더가 씬에 굽고, 반복 요소(파일 칸)만 프리팹으로 Instantiate한다 |
| F-03 | 1R | `Views/TreeLineImage.cs:29-51` | 낮음 | 트리 연결선을 런타임에 Texture2D로 직접 그린다 | 그대로 둬도 된다. 모양이 고정이면 빌드 때 PNG로 구워도 된다 |
| F-04 | 1R | `Script/StageSelectBridge/FileBoard.cs:17,29,61-64` | 낮음 | 파일 발판과 벽 콜라이더를 런타임에 만든다. 그리드 레이아웃을 따라가야 해서 이유는 있다 | 그대로 둔다 |
| F-05 | 1R·7 | `Script/Flow/SubStage/StageCameraBinder.cs:20-21,51-59` | 중간 | 팀원 씬의 CinemachineCamera에 Confiner2D를 `AddComponent`하고 범위 콜라이더를 만든다. `layer = 2`는 매직넘버다 | 스테이지 템플릿에 CameraBounds를 필수로 두고 생성 경로를 없앤다. 레이어는 이름 상수로 쓴다 |
| F-06 | 1R·7 | `Script/Flow/SubStage/StageGoalAttacher.cs:8-16`, `Script/Flow/Core/StageObjectNames.cs:18-22` | 중간 | 팀원 씬 오브젝트를 이름(`Stage1_GoalMusicFile`, `Start`/`Spawn`/`StartMarker`)으로 찾아 StageGoal을 런타임에 붙인다 | 골·시작 지점 프리팹에 StageGoal/StageSpawnPoint를 미리 굽는다 |
| F-07 | 2 | `Script/Flow/SceneFlow.cs:59,66,69,128`, `Script/Flow/LoadingWindow.cs:32` | 낮음 | "실행 중...", "스테이지 선택.exe 실행 중...", "타이틀.exe 실행 중..." 같은 표시 문구가 코드에 박혀 있다 | StageSelectText처럼 문구 클래스로 모은다 |
| F-08 | 2·중복 | `Script/Flow/FlowSettings.cs:11-12` ↔ `Editor/Flow/FlowPaths.cs:11-12` ↔ `Editor/TitleSceneSetup.cs:35` | 중간 | 타이틀·선택 화면 씬 경로가 3곳에 있다. 타이틀 씬은 LDY 폴더 밖(`Assets/Scenes`)에 있다 | FlowPaths만 원본으로 두고 FlowSettings 기본값은 빌더가 채우게 한다 |
| F-09 | 2 | `Script/Flow/UiFactory.cs:8,30,31,37` | 낮음 | 기준 해상도 640x360, match 0.5, PPU 100, `"UI"` 레이어 문자열이 고정이다 | UiSkin으로 옮긴다 |
| F-10 | 2 | `LoadingWindow.cs:9-12,36-43`, `Views/DialogView.cs:10-11,36-47`, `Views/SettingsDialogView.cs:11-17,52-53,79-118`, `Views/TaskbarView.cs:20-31`, `Views/FileItemView.cs:13-20,59-61,105`, `Views/TreeView.cs:13-21` | 중간 | uGUI 픽셀 좌표와 크기 수십 개가 코드에 있다 | F-02의 프리팹화와 함께 정리한다 |
| F-11 | 2·중복 | `Views/DialogView.cs:10,23,46-47` ↔ `Views/SettingsDialogView.cs:11,34,52-53` | 낮음 | 창 폭 264, 막 알파 0.15, 확인 버튼 64x18 위치 y8이 두 파일에 똑같이 있다 | 공용 ModalWindow 헬퍼를 만든다 |
| F-12 | 2·중복 | `Script/StageSelectBridge/StageSelectPlayerBootstrap.cs:99-104` ↔ `Script/Title/Window/WindowKit.cs:19-23` | 낮음 | `"Ground"` 레이어 조회와 대체 경고 문구가 중복이다 | 공용 Layers 상수나 헬퍼를 둔다 |
| F-13 | 3 | `Script/Flow/SceneFlow.cs` (136줄) | 중간 | 세션, 기록 저장, ESC 입력(44-48), 재시작, 씬 로드 처리, 카탈로그 조회까지 책임이 6개이고 static Instance도 갖는다 | ESC 입력과 재시작 정책을 별도 컴포넌트로 뺀다 |
| F-14 | 3 | static 상태: `SceneFlow.cs:15`, `StageRoot.cs:9`, `StageSignals.cs:7-8`, `Title/Transition/Carrier/TransitionCarrier.cs:14`, `Player/Death/StageRestart.cs:30` (QuitWatch), `Title/Window/WindowKit.cs:11` (_pixelSprite) | 낮음 | 모두 SubsystemRegistration 초기화가 있다. WindowKit._pixelSprite만 없지만 해는 없다. 전역 이벤트 StageSignals의 구독자는 SceneFlow 하나다 | 그대로 둔다. 팀원 쪽 골·사망 이벤트도 StageSignals로 모은다 |
| F-15 | 4 | `Script/Flow/StageRoot.cs:19`, `Script/Flow/SubStage/SubStageLoader.cs:26` | 중간 → **해결됨 (2026-11-25)**: PrimaryPlayerLocator로 본체를 판별 | `FindGameObjectWithTag("Player")`를 쓴다. `Main/02.Prefabs/Player/PlayerCopy.prefab`도 Player 태그라서, 복사본이 있으면 아무 하나를 시작 지점으로 옮길 수 있다 | PlayerMatch로 진짜 플레이어를 판별하거나 StageRoot에 플레이어 참조를 둔다 |
| F-16 | 4 | `Script/Flow/SubStage/StageCameraBinder.cs:12,28-31` | 중간 | `FindFirstObjectByType<CinemachineCamera>`를 쓰고, 전체 자식에서 이름 `"CameraBounds"`/`"Bounds"`를 찾는다 | 루트에 카메라·범위를 참조하는 컴포넌트를 둔다 |
| F-17 | 4·7 | `Script/Flow/SubStage/ConflictingManagers.cs:8,12-18` | **높음** → **해결됨 (2026-11-25)**: 타입 이름을 FlowSettings로 옮기고, 못 찾으면 비슷한 이름과 함께 경고 1회, Validate에 검사 추가 | 팀원 타입을 `"JJB.Script.Level.JJBLevelManager"` 문자열로 찾아 끄고, 씬 전체 MonoBehaviour를 순회한다. JJB가 이름이나 네임스페이스를 바꾸면 조용히 안 꺼져서 두 매니저가 동시에 루트와 플레이어를 다룬다 | JJB와 합의해 JJBLevelManager에 "외부 흐름 사용" 플래그를 두거나, LDY 흐름이 있으면 스스로 꺼지게 한다 |
| F-18 | 5 | `Views/TaskbarView.cs:37-46` | 중간 → **해결됨 (2026-11-25)** | 매 프레임 `DateTime.Now` + 문자열 보간으로 문자열을 만든 뒤 비교한다. 프레임마다 GC 할당이 생긴다 | 분이 바뀔 때만 문자열을 만든다 |
| F-19 | 5 | `Script/StageSelect/StageSelectModel.cs:59,79,99,109,113-117`, `Script/StageSelectBridge/FilePlatformTarget.cs:32-34` | 낮음 | LINQ와 할당은 선택이 바뀔 때만 생긴다. 다만 발판을 옮길 때마다 Render()가 화면 전체를 다시 그린다 | 선택만 바뀌면 grid.Show 대신 선택 표시만 갱신한다 |
| F-20 | 3 | `StageSelectModel.cs:105`, `StageSelectScreen.cs:128,140-146` | 낮음 | 카탈로그 조회 결과를 null 검사 없이 쓴다(`Find(...).fileName`, `World(...).displayName`) | null 가드를 넣는다 |
| F-21 | 6 | `Script/Flow/Core/StageCatalogRules.cs` | 낮음 | 런타임 코드에서는 안 쓰고 테스트(StageCatalogAssetTests, StagePlanTests)에서만 쓴다 | 테스트 전용이라고 표시하거나 Validator에서 쓴다 |
| F-22 | 7 | `Editor/Flow/StageCatalogBuilder.cs:11-22` → 런타임 `SubStageRoots.Find` | **높음** → **부분 해결 (2026-11-25)**: 루트가 없으면 에러 로그 + "파일을 찾을 수 없습니다" 창 + 선택 화면 복귀. 이름 의존 자체는 그대로 | JJBLevel.unity 안의 루트 이름 `Stage1-Tutorial`, `Stage1-1`~`Stage1-6`이 카탈로그에 고정돼 있다(지금은 7개 모두 있음을 확인). 팀원이 루트 이름을 바꾸거나 씬을 옮기면 그 스테이지에 들어갈 때 루트를 못 찾고 경고만 남긴다 | 루트에 id를 가진 컴포넌트를 붙여 이름 대신 id로 찾는다. 병합 직후 `LDY/Merge/Validate Open Stage`로 다시 확인한다 |
| F-23 | 3 | `Editor/Flow/BuildSettingsSync.cs:10-33` (`StageSelectSceneBuilder.cs:116`에서 자동 호출) | 중간 | 공용 Build Settings 순서를 LDY 타이틀·선택 화면 우선으로 다시 배열한다. 지금 Build에는 LDY 테스트 씬 `RopeTest.unity`도 들어 있다 | 병합 날 한 사람이 Build Settings를 확정하고, 빌더의 자동 호출은 뺀다 |

---

## 4. HUD (+ 스테이지 배경 위젯)

LDY에는 **인게임 HUD 런타임 코드가 없다.** 아래 H-01이 그 공백이고, 나머지는 HUD와 성격이 비슷한 스테이지 배경 위젯(`Script/Background`)이다.

| ID | 분류 | 위치 | 심각도 | 문제 | 개선 제안 |
|---|---|---|---|---|---|
| H-01 | 3 | `Editor/Flow/Merge/StageSceneBuilder.cs:51`, `Editor/Flow/FlowPaths.cs:16` | 중간 | 스테이지 템플릿 빌더는 "HUD 프리팹 없음, 건너뜀" 로그만 남긴다. `ingame_hud` 스프라이트는 UiSkin(로딩 창, 선택 화면 버튼)에서만 쓴다 | 병합 전에 HUD 담당과 범위를 정한다 |
| H-02 | 1R | `Script/Background/NoteSpawner.cs:44-52` | 낮음 | 음표 풀(기본 8개)을 Awake에서 `new GameObject` + SpriteRenderer로 만든다 | 음표 프리팹 하나를 Instantiate한다 |
| H-03 | 2 | `Script/Background/BackgroundSorting.cs:3-6`, `StageBackgroundLoader.cs:7-11` | 낮음 | 정렬 순서(-100~-70)와 기본 파일 이름이 고정이다 | 병합 때 팀원 스프라이트 정렬과 겹치지 않는지 확인한다 |
| H-04 | 3·중복 | `Script/Background/Core/PixelSpace.cs:31` ↔ `Script/StageSelect/Core/PixelSpace.cs:54` | 낮음 | 같은 이름의 PixelSpace 구조체가 두 개 있고 역할도 비슷하다 | 하나로 합치거나 이름을 구분한다 |

---

## 5. 로프

| ID | 분류 | 위치 | 심각도 | 문제 | 개선 제안 |
|---|---|---|---|---|---|
| R-01 | 2 | `Script/Rope/RopeInput.cs:20-33` | 중간 | F/Space/WASD 바인딩이 코드에 고정돼 있고 JJBInput과 따로 논다. 키를 바꾸려면 두 곳을 고쳐야 하고, Space는 JJB Jump와 겹친다 | 줄 전용 액션을 LDY 입력 에셋에 두거나 JJBInput 액션을 재사용한다 |
| R-02 | 2·중복 | `"Player"` 태그 6곳: `Script/Flow/PlayerMatch.cs:8`, `Script/Player/Death/PlayerDeathInstaller.cs:8`, `Script/Rope/RopeHangController.cs:66`, `Script/Spider/Runtime/MouseTrap.cs:7`, `NeedleKiller.cs:6`, `TagPlayerLocator.cs:5` (+ `"Player"` 레이어 `SpiderLeg.cs:83`) | 중간 → **해결됨 (2026-11-25)**: 태그는 `PrimaryPlayerLocator.Tag` 하나로 모음. SpiderLeg의 레이어 문자열은 남아 있음 | 태그 이름이 바뀌면 6곳을 고쳐야 한다. PlayerMatch.DefaultTag가 있는데도 asmdef 경계 때문에 못 쓰고 있다 | 다른 어셈블리를 참조하지 않는 Core(예: LDY.Death.Core)에 PlayerTags 상수를 하나 둔다 |
| R-03 | 4 | `Script/Rope/RopeHangController.cs:65-68` | 낮음 → **부분 해결**: 대체 탐색이 PrimaryPlayerLocator를 거친다. 대체 탐색 자체는 남아 있음 | player가 비어 있으면 `FindWithTag("Player")`로 대신 찾는다 | TryGrab이 잡는 플레이어를 인자로 받게 한다 |
| R-04 | 7 | `Script/Rope/RopeInteractable.cs:1-18` | 중간 | JJBInteractable을 상속하고 `Interact(JJBPlayerInteract)`를 override한다. JJB가 시그니처를 바꾸면 컴파일 에러가 난다 | 병합 체크리스트에 넣고 JJB와 시그니처를 고정하기로 합의한다 |
| R-05 | 1R·7 | `Script/Rope/PlayerSuspendAdapters.cs:44-45` | 중간 | IPlayerControl이 없으면 JJBControlGate를 `AddComponent`한다. RequireComponent 때문에 JJBPlayer와 JJBPlayerJump까지 자동으로 붙는다 | 없으면 경고를 남기고 아무것도 안 하는 싱크를 쓴다 |
| R-06 | 3 (순환) | Rope→Player: `RopeHangController.cs:49,84` → `Script/Player/PlayerPoseRelay.cs:4`, Player→Rope: `Script/Player/PlayerGripRig.cs:12` → `Script/Rope/IRopeGripSource.cs:13`, Alt↔Rope: `Script/Alt/AltModeController.cs:30` ↔ `RopeHangController.cs:29,81` (IPlayerBody), Alt↔Title: `AltModeController.cs:14` (TitleTuning) | 중간 | 폴더끼리 서로를 참조한다. 모두 Assembly-CSharp라 컴파일러가 막지 못하고, 팀원 코드에 컴파일 에러가 하나만 있어도 LDY 전체가 같이 멈춘다 | 계약 인터페이스를 LDY.PlayerAnim 같은 하위 asmdef로 내리고 런타임 폴더마다 asmdef를 둔다(병합 후) |
| R-07 | 5 | `Script/Player/PlayerGripRig.cs:27-28` | 낮음 | 소스가 없는 플레이어(타이틀, 선택 화면)에서 매 LateUpdate마다 `GetComponentInParent`를 두 번 부른다. 인터페이스에 `??=`를 써서 파괴된 Unity 객체도 걸러지지 않는다 | Awake에서 한 번만 찾고, 없으면 컴포넌트를 끈다 |
| R-08 | 3 | `Script/Rope/Logic/RopeCurve.cs:19-305` (약 286줄) | 낮음 | 300줄에 가깝다 | 병합 후 스냅과 샘플링을 나누는 것을 검토한다 |

---

## 6. 거미 / 덫

| ID | 분류 | 위치 | 심각도 | 문제 | 개선 제안 |
|---|---|---|---|---|---|
| S-01 | 1R | `Script/Spider/Runtime/TelegraphMarker.cs:44-76` | 낮음 | 링 스프라이트를 128x128 텍스처로 런타임에 만든다 | 링 PNG 에셋을 만들어 프리팹 자식으로 둔다 |
| S-02 | 1R·2 | `Script/Spider/Runtime/SpiderLeg.cs:74-84` | 낮음 | 잘린 다리에 Rigidbody2D·BoxCollider2D를 런타임에 붙이고, gravityScale 2f와 `"Player"` 레이어 문자열을 쓴다 | 조각 프리팹을 쓰거나 값을 SpiderSettings로 옮긴다 |
| S-03 | 4·5 | `Script/Spider/Runtime/TagPlayerLocator.cs:9-12` (`SpiderController.cs:66`에서 FixedUpdate마다 호출) | 중간 → **해결됨 (2026-11-25)**: 본체 판별 + 0.5초 재탐색 간격 | 플레이어가 없거나 파괴되면(사망 후 재시작 전, 복사 플레이어 교체) FixedUpdate마다 `FindWithTag`를 부른다 | 다시 찾는 간격을 두거나 스폰 이벤트로 플레이어를 넘겨받는다 |
| S-04 | 6 | `Script/Spider/Runtime/TelegraphMarker.cs:15`, `SpiderSettings.cs:105`, `SpiderController.cs:58` | 낮음 | blinkHz/MarkerBlinkHz는 넘겨받기만 하고 쓰지 않는다(주석에 "깜빡이지 않는다") | 설정과 매개변수를 지운다 |
| S-05 | 3 | `Script/Spider/Runtime/SpiderController.cs` (238줄) | 낮음 | 조립, 다리 구동, 반동 계산, 이벤트 중계를 한 클래스가 한다. 계산은 Logic에 맡겨서 당장은 괜찮다 | 다리 구동부(PlaceFeet~DriveLegs)를 SpiderLegRig로 뺀다 |
| S-06 | 2 | `SpiderController.cs:8,165` | 낮음 | FootSnapRise 2f, `i * 1.3f` 같은 매직넘버가 있다 | SpiderSettings로 옮긴다 |

---

## 7. Alt

| ID | 분류 | 위치 | 심각도 | 문제 | 개선 제안 |
|---|---|---|---|---|---|
| A-01 | 1R | `Script/Alt/SpriteAfterimageSpawner.cs:13-28` | 낮음 | Alt에 들어갈 때마다 잔상 GameObject를 만들고 지운다 | 하나를 만들어 재사용한다 |
| A-02 | 2 | `Script/Alt/AltKeyInput.cs:7` | 낮음 | `"<Keyboard>/alt"`가 고정이고 Alt+Tab 같은 OS 조합과 겹친다 | LDY 입력 에셋으로 옮긴다 |
| A-03 | 3 (전역 상태) | `Script/Alt/TimeScaleFreezer.cs:13-14,22`, `Script/Flow/SceneLoadRunner.cs:58` | 중간 | Alt는 전역 `Time.timeScale`을 0으로 만들고, 로더는 1로 강제한다. 지금 팀원 코드에는 timeScale을 바꾸는 곳이 없다(확인함). 팀원이 일시정지나 슬로모를 넣으면 서로 덮어쓴다 | 소유자별로 요청을 모으는 TimeScale 서비스 하나로 합친다 |
| A-04 | 4 | `Script/Alt/AltModeController.cs:22-25` | 낮음 | `GetComponentInChildren<SpriteRenderer>`와 `Camera.main`을 대체 경로로 쓴다 | 직렬화 필드를 필수로 한다 |

---

## 8. 플레이어 (애니메이션, 사망, 어댑터, 타이틀 플레이어)

| ID | 분류 | 위치 | 심각도 | 문제 | 개선 제안 |
|---|---|---|---|---|---|
| P-01 | 7 | `Script/Adapters/JJBControlGate.cs:87-97` | **높음** → **보류**: 중단 조건 해당(12.3절) | `JJBPlayerJump.OnDisable`이 점프 요청 플래그를 지운다는 팀원 내부 구현에 기대, enabled를 껐다 켠다. JJB가 OnDisable을 바꾸면 점프 차단(설정창, 사망, Alt)이 조용히 실패한다 | JJB에 공개 메서드(예: CancelPendingJump)를 요청한다 |
| P-02 | 7 | `Script/Adapters/JJBControlGate.cs:7-8,65` | 중간 | RequireComponent(JJBPlayer, JJBPlayerJump)를 걸고, `JJBPlayer.enabled`를 꺼서 조작을 멈춘다. JJBPlayer가 맡는 일이 늘면 그것도 같이 멈춘다 | JJB에 SetInputEnabled 같은 API를 요청한다 |
| P-03 | 7 | `Script/Adapters/JJBPlayerMotionSource.cs:6,35` | 중간 | `JJBPlayerJump.IsGrounded`와, 비활성일 때 false로 초기화되는 동작에 의존한다 | 병합 체크리스트에 적는다 |
| P-04 | 1R·7 | `Script/Player/Death/PlayerDeathInstaller.cs:16-20`, `Script/Player/DeathInputBlock.cs:14-15,28-36`, `Script/Player/StagePlayerAnimBootstrap.cs:16-17`, `Script/Rope/PlayerSuspendAdapters.cs:49,59` | 중간 | 팀원 플레이어에 PlayerDeath, PlayerInputLock, JJBControlGate, JJBPlayerMotionSource, PlayerPoseRelay, PlayerSuspendHub, PlayerRopeSlot을 런타임에 붙인다. 프리팹을 봐서는 실제 구성이 보이지 않는다 | 병합 후 통합 Player 프리팹에 굽고, 런타임 주입은 검증 경고로 바꾼다 |
| P-05 | 4 | `Script/Player/Death/PlayerDeathInstaller.cs:16-20` | 중간 → **부분 해결**: 복사본 대신 본체에 붙는다. 타이틀·선택 화면에도 붙는 문제는 그대로 | sceneLoaded마다 처음 찾은 `"Player"` 태그 하나에만 PlayerDeath를 붙이고 로드 모드를 무시한다. 타이틀·선택 화면 AltPlayer에도 붙는다. 복사 플레이어를 빼는 것은 의도된 동작이다 | 스테이지 씬(StageRoot나 카탈로그 항목이 있는 씬)에서만 붙인다 |
| P-06 | 5 | `Script/Player/Death/PlayerDeath.cs:62-67` → `PlayerRestraint.cs:41-50` | 낮음 | 스프라이트를 못 찾으면 매 LateUpdate마다 `GetComponentsInChildren`(배열 할당)을 부른다 | 다시 찾는 간격을 두거나, 못 찾으면 끈다 |
| P-07 | 7 | `Script/Player/Death/PlayerRestraint.cs:32-37` | 낮음 | 사망할 때 팀원 Animator를 모두 끈다 | 의도를 병합 체크리스트에 적는다 |
| P-08 | 7 | `Script/Title/Input/TitleInputs.cs:22`, `Script/Title/Input/PlayerWindowInput.cs:24,36`, `Script/TitlePlayer/TitlePlayerInput.cs:8-9,20-21`, `Editor/TitleSceneSetup.cs:30,702` | **높음** → **해결됨 (2026-11-25)**: 이름은 `InputActionNames` 한곳, 못 찾으면 그 기능만 끄고 에러 1회, Validate에 검사 추가 | 타이틀·선택 화면 플레이어가 JJB 소유 `JJBInput.inputactions`의 맵 `"Player"`와 액션 `"Move"/"Jump"/"Interact"`를 `FindAction(…, true)`로 찾는다. 이름이 바뀌면 예외가 나서 타이틀과 선택 화면 조작이 안 된다 | LDY 전용 입력 에셋을 복제해서 쓰거나, `throwIfNotFound=false` + 경고로 바꾼다 |
| P-09 | 2·중복 | `Script/PlayerMerge/Logic/PlayerPrefabPaths.cs:3-5` (런타임 asmdef) ↔ `Editor/Flow/FlowPaths.cs:21-28` ↔ `Editor/TitlePlayer/TitleTuningSync.cs:8` | 중간 | 플레이어 프리팹 경로가 3곳에 있고, 런타임 어셈블리에 에디터 경로가 들어 있다. 프리팹도 3종(`Assets/prefab`, `Assets/Main/02.Prefabs/Player` + PlayerCopy, `Assets/JJB/Prefab`)이며 StageSceneBuilder 기본값은 `Assets/prefab`이다 | 병합 기준 프리팹 하나를 정하고 FlowPaths 한곳만 쓴다 |

---

## 9. 에디터 툴

### 9.1 지적 사항

| ID | 분류 | 위치 | 심각도 | 문제 | 개선 제안 |
|---|---|---|---|---|---|
| E-01 | 1B·3 | `Editor/TitleSceneSetup.cs:26-987` (TitleSceneBuilder 약 960줄) | 중간 | 타이틀 씬 전체(카메라, 책상, 모니터, CRT, 아이콘, 창, 플레이어 프리팹, 레이어)를 코드로 만든다. 300줄을 넘는 God Class이고, float 리터럴 73개, `GameObject.Find` 9회(184, 200, 215-223, 462-464)가 있다 | 단계별 클래스(Desk/Monitor/Icons/Player)로 나누고, 씬이 안정되면 빌더 대신 씬을 원본으로 삼는다 |
| E-02 | 7 (이미 깨짐) | `Editor/TitleSceneSetup.cs:31,696` | **높음** → **해결됨 (2026-11-25)** | PhysicsMaterialPath `"Assets/JJB/JJB.physicsMaterial2D"`가 없다. 팀원 쪽은 지금 `Assets/JJB/JJBPM2D.physicsMaterial2D`다. **정정**: 이 경로는 AltPlayer 프리팹을 새로 만들 때만 쓰인다. 지금 AltPlayer 재질은 GUID(019de775…)로 JJBPM2D에 연결돼 있어 null이 아니다. 프리팹을 지우고 다시 만들 때만 재질이 비게 된다 | 경로를 고치거나 LDY 소유 재질로 바꾸고, 로드에 실패하면 중단한다 |
| E-03 | 1B·3 | `Editor/Flow/StageSelectSceneBuilder.cs` (342줄), `:36-45` | 중간 | 300줄을 넘는다. Assembly-CSharp 타입 5개를 `"LDY.Script.X, Assembly-CSharp"` 문자열로 붙여서, 이름이 바뀌면 조용히 빠진다 | 타입 문자열을 상수 한곳에 모으고 검증 테스트를 두거나, 런타임 쪽을 asmdef로 옮겨 직접 참조한다 |
| E-04 | 4·7 | `Editor/Flow/TitleConnector.cs:9,21-25` | 낮음 | 타입 이름 `"LDY.Script.AsyncSceneLoader"`와 필드 이름 `"sceneName"`을 문자열로 쓴다 | T-10을 통합할 때 지운다 |
| E-05 | 7 | `Editor/Stage/Stage1Variants.cs:27,41,55,79,91,121` (프리팹 이름), `:68-72` (`"Cannon"`, `"bulletPrefab"`), `:89` (`"Square"`, `"Square (1)"`), `:170-171` (`Assets/HES`, `Assets/JJB` 검색) | 중간 | 팀원 프리팹 이름(Glass, Circle, cannon, Guillotine, LRDoor, RotateBlock), 타입 이름, 필드 이름, 자식 이름에 의존한다. 실패하면 조용히 return한다(`:70-72`) | 실패를 report에 남기고, 팀원과 이름을 고정하기로 합의한다 |
| E-06 | 7 | `Editor/Spider/SpiderEditorSupport.cs:74-97` | 중간 | HTY DragObj를 타입 이름 문자열로 찾아 AddComponent한다 | 못 찾으면 빌드를 중단하고 병합 체크리스트에 적는다 |
| E-07 | 7 | `Editor/TitlePlayer/JJBMovementReader.cs:11-27` | 중간 | JJB의 비공개 직렬화 필드 8개(moveSpeed, jumpSpeed, maxJumpHoldTime, extraGravity, gravityDelay, groundCheckSize, groundCheck, groundLayer)를 FindProperty로 읽는다. 이름이 바뀌면 `LDY/Player/Sync Title Tuning From JJB`에서 NRE가 난다 | null을 검사하고 빠진 필드를 보고한다 |
| E-08 | 7 | `Editor/Death/LdyKillHitboxReplacer.cs:2,31` | 낮음 | HES KillHitbox 타입에 컴파일 의존한다 | 일회성 도구라면 병합 후 지운다 |
| E-09 | 6 | `Editor/PlayerVisual/PlayerVisualContext.cs:11`, `Editor/RopeTestSceneSetup.cs:8` | 낮음 → **해결됨 (2026-11-25)**: 메뉴 5개를 삭제하고 CursorPlayerAssigner의 SampleScene 단계도 제거 | 없는 씬 `Assets/LDY/Scenes/SampleScene.unity`를 대상으로 한다. 메뉴 `LDY/Player Visual/Diagnose`·`Fix`(`PlayerVisualMenu.cs:5,10`)와 `LDY/Rope Test/*` 3개(`RopeTestSceneSetup.cs:20,46,62`)가 동작하지 않는다 | 메뉴를 지운다 |
| E-10 | 6 | `Editor/PlayerAnimSetBuilder.cs:12-15,45-55`, `Editor/PlayerVisual/*`, `Editor/PlayerMerge/LegacyVisualSwitch.cs`, `PlayerVisualContext.cs:12` (`"PinkVisual"`), `Editor/TitlePlayer/TitlePlayerConverter.cs:17,40` (`"JJBFacingProxy"`) | 낮음 → **부분 해결 (2026-11-25)**: Pink/V2 메뉴, PlayerVisual 진단·수리, Proxy 변환기 코드를 삭제. 에셋(PlayerAnimSet.asset, V2.asset, aseprite)과 살아 있는 이름(PinkVisual, LegacyVisualSwitch)은 남김 | 예전 핑크 캐릭터와 프록시의 잔재다. `PlayerAnimSet.asset`/`PlayerAnimSetV2.asset`은 어떤 프리팹이나 씬에서도 참조하지 않고 Cursor 세트만 쓴다. AltPlayer에는 PinkVisual이 없다 | Pink/V2 메뉴, PlayerVisual, 변환기를 병합 후 정리한다 |
| E-11 | 3·중복 | `Editor/TitleSceneSetup.cs:951-970` ↔ `Editor/AltTestSceneSetup.cs:102` | 중간 | EnsureLayer 함수가 두 벌이고, 공용 `ProjectSettings/TagManager.asset`을 자동으로 고친다 | 레이어는 병합 때 손으로 확정하고 자동 추가는 없앤다 |
| E-12 | 2·중복 | 9.3절 표 | 중간 | 에셋 경로, 태그, 레이어 문자열이 여러 파일에 중복돼 있다 | FlowPaths처럼 경로 상수 파일 하나로 모은다 |
| E-13 | 3 | `Assets/LDY/Prefab` ↔ `Assets/LDY/Prefabs` | 낮음 | 프리팹 폴더가 두 개다 | 하나로 합친다(GUID가 유지되게 에디터에서 이동한다) |

### 9.2 빌드 시점(에디터 메뉴)에 오브젝트를 만드는 도구 (1B)

런타임 비용은 없고, 결과물(씬·프리팹)이 원본인지 빌더가 원본인지만 정하면 된다. 빌더를 다시 돌리면 손으로 고친 값이 덮이는 도구는 ★로 표시했다.

| 도구 | 만드는 것 |
|---|---|
| `TitleSceneSetup.cs` ★ | 타이틀 씬 전체, AltPlayer 프리팹, 레이어 |
| `Flow/StageSelectSceneBuilder.cs` ★ | 선택 화면 씬의 틀, 플레이어 배치, Build Settings |
| `Flow/Merge/StageSceneBuilder.cs` | 스테이지 템플릿 씬(없는 부분만 채움) |
| `Flow/UiSkinBuilder.cs`, `Flow/FlowSettingsBuilder.cs`, `Flow/StageCatalogBuilder.cs`, `Flow/ThumbnailCapture.cs` | 스킨, 설정, 카탈로그, 썸네일 에셋 |
| `LoadingOverlayPrefabBuilder.cs` | 타이틀 로딩 오버레이 프리팹 |
| `Spider/SpiderPrefabBuilder.cs`, `Spider/TrapPrefabBuilder.cs`, `Spider/SpiderTestSceneBuilder.cs`, `Spider/Stage2Art.cs` | 거미, 덫 프리팹, 테스트 씬 |
| `Stage/Stage1*.cs` (Grid, Placeholders, Pickups, GuitarPendulum, Variants, ArtTestScene, Tiles) | Stage1 타일·프리팹·변형 프리팹 |
| `Background/*PartBuilder.cs` (8개), `BackgroundPrefabBuilder.cs` | 스테이지 배경 프리팹 |
| `AltTestSceneSetup.cs`, `RopeTestSceneSetup.cs` | 테스트 씬(RopeTest는 대상 씬이 없음, E-09) |
| `PlayerAnimSetBuilder.cs`, `CursorCharacter/*`, `PlayerVisual/PlayerGripSetup.cs`, `TitlePlayer/TitlePlayerAssembler.cs` | 애니메이션 세트, 플레이어 비주얼 |
| `PixelStyle/GearAnimationBuilder.cs`, `GearPrefabSwapper.cs`, `Fonts/PixelFontAssetBuilder.cs` | 기어 애니메이션, 픽셀 폰트 에셋 |

### 9.3 여러 파일에 중복된 값

| 값 | 위치 |
|---|---|
| `Assets/LDY/Prefab/AltPlayer.prefab` | `Editor/TitleSceneSetup.cs:33`, `Editor/TitlePlayer/TitlePlayerConverter.cs:12-14`, `Editor/CursorCharacter/CursorPlayerAssigner.cs:7-9`, `Editor/Flow/FlowPaths.cs:22` |
| `Assets/Scenes/LDY_TitleScene.unity` | `Editor/TitleSceneSetup.cs:35`, `Script/Flow/FlowSettings.cs:11`, `Editor/Flow/FlowPaths.cs:11` |
| `Assets/LDY/Scenes/StageSelect.unity` | `Script/Flow/FlowSettings.cs:12`, `Editor/Flow/FlowPaths.cs:12` |
| `Assets/JJB/Prefab/Player.prefab` | `Editor/TitlePlayer/TitleTuningSync.cs:8`, `Script/PlayerMerge/Logic/PlayerPrefabPaths.cs:4`, `Editor/Flow/FlowPaths.cs:27` |
| `Assets/prefab/Player.prefab` | `Script/PlayerMerge/Logic/PlayerPrefabPaths.cs:3`, `Editor/Flow/FlowPaths.cs:21` |
| `Assets/JJB/JJBInput.inputactions` | `Editor/TitleSceneSetup.cs:30`, `Editor/AltTestSceneSetup.cs:10` |
| `Assets/JJB/JJBAudioMixer.mixer` | `Editor/TitleSceneSetup.cs:36`, `Editor/Flow/FlowPaths.cs:18` |
| `Assets/LDY/Data/PlayerAnimSet_Cursor.asset` | `Editor/PlayerAnimSetBuilder.cs:16`, `Editor/Death/PlayerDeathSettingsBuilder.cs:8` |
| `Assets/LDY/Data/RopeSettings.asset`, `RopeGripProfile.asset` | `Editor/PlayerVisual/PlayerGripSetup.cs:7-8`, `Editor/RopeTestSceneSetup.cs:9`, `Editor/CursorCharacter/CursorGripProfileUpdater.cs:5` |
| `Assets/LDY/Scenes/SampleScene.unity` (없는 파일) | `Editor/PlayerVisual/PlayerVisualContext.cs:11`, `Editor/RopeTestSceneSetup.cs:8` |
| `"Player"` 태그 | 런타임 6곳(R-02), 에디터 `RopeTestSceneSetup.cs:71,167` |
| `"Ground"` 레이어 | 런타임 `Script/Title/Window/WindowKit.cs:19`, `Script/StageSelectBridge/StageSelectPlayerBootstrap.cs:100` / 에디터 `TitleSceneSetup.cs:28`, `AltTestSceneSetup.cs:9`, `Flow/Merge/StageSceneBuilder.cs:120-121`, `RopeTestSceneSetup.cs:11`, `Spider/SpiderPrefabBuilder.cs:56`, `Spider/SpiderTestSceneBuilder.cs:11`, `Stage/Stage1GridBuilder.cs:10`, `Stage/Stage1Placeholders.cs:41`, `Stage/Stage1Variants.cs:36,129` |
| 입력 맵 `"Player"` | `Editor/TitleSceneSetup.cs:702`, `Editor/AltTestSceneSetup.cs:65` |
| 설정 문구 6개 | `Script/Title/Window/WindowServices.cs:67-72`, `Script/StageSelect/StageSelectText.cs:30-35` (T-07) |
| 모달 창 치수 | `Views/DialogView.cs`, `Views/SettingsDialogView.cs` (F-11) |
| 로딩 문구 "실행 중" | `Script/Flow/SceneFlow.cs:59,66,69,128`, `Script/Flow/LoadingWindow.cs:32`, `Script/Title/Transition/TransitionSettings.cs:22` |

---

## 10. 팀원 코드 의존 지점 한눈에 보기

모두 팀원 폴더에서 실제로 있는지 확인했다(2026-11-25 기준). "깨질 때"는 팀원이 그 대상을 바꿨을 때 일어나는 일이다.

| 팀 | 의존 대상 | LDY 쪽 위치 | 깨질 때 | 종류 |
|---|---|---|---|---|
| JJB | `JJBPlayer`, `JJBPlayerJump`(+ `IsGrounded`, `OnDisable` 동작) | `Script/Adapters/JJBControlGate.cs`, `Script/Adapters/JJBPlayerMotionSource.cs`, `Script/Player/DeathInputBlock.cs` | 컴파일 에러, 또는 조용히 점프 차단 실패 | 컴파일 + 동작 |
| JJB | `JJBInteractable.Interact(JJBPlayerInteract)` | `Script/Rope/RopeInteractable.cs` | 컴파일 에러 | 컴파일 |
| JJB | `JJBPlayerMovement`/`JJBPlayerJump` 직렬화 필드 8개 | `Editor/TitlePlayer/JJBMovementReader.cs` | 에디터 메뉴 NRE | 문자열 |
| JJB | `JJBPlayerInteract.objectLayer` | `Editor/RopeTestSceneSetup.cs:79` | 경고(어차피 죽은 메뉴) | 문자열 |
| JJB | `JJB.Script.Level.JJBLevelManager` 타입 이름 | `Script/Flow/SubStage/ConflictingManagers.cs:8` | 조용히 안 꺼짐 → 매니저 두 개가 동시에 동작 | 문자열 |
| JJB | `JJBLevel.unity`와 루트 이름 7개 | `Editor/Flow/StageCatalogBuilder.cs:11-22` → 카탈로그 | 스테이지 진입 시 루트를 못 찾음 | 이름 |
| JJB | `JJBInput.inputactions` 맵 `Player`, 액션 `Move/Jump/Interact` | P-08 위치들 | 런타임 예외 | 문자열 |
| JJB | `JJBAudioMixer.mixer` | `Editor/TitleSceneSetup.cs:36`, `Editor/Flow/FlowPaths.cs:18` | 설정 음량이 안 먹음 | 경로 |
| JJB | `JJB.physicsMaterial2D` | `Editor/TitleSceneSetup.cs:31` | **이미 깨짐**(E-02) | 경로 |
| JJB | `Assets/JJB/Prefab/Player.prefab` | P-09 위치들 | 비교·동기화 도구 실패 | 경로 |
| JJB/HES | 프리팹 이름 Glass, Circle, cannon, Guillotine, LRDoor, RotateBlock과 `Cannon.bulletPrefab`, 자식 `Square` | `Editor/Stage/Stage1Variants.cs` | 변형 프리팹이 조용히 빠짐 | 이름 |
| HES | `HES.Script.Object.KillHitbox` | `Editor/Death/LdyKillHitboxReplacer.cs` | 컴파일 에러(에디터) | 컴파일 |
| HTY | `DragObj` 타입 이름 | `Editor/Spider/SpiderEditorSupport.cs:76` | 경고 후 빠짐 | 문자열 |
| Main (공용, 최근 커밋은 hanty100209-cpu) | `Main/02.Prefabs/Player/Player.prefab`, `PlayerCopy.prefab`(둘 다 Player 태그) | `Editor/Flow/FlowPaths.cs:26`, 태그 탐색 전부(F-15, S-03, P-05) | 복사본을 진짜 플레이어로 착각할 수 있음 | 태그 |
| Main (공용) | `Main/05.Fonts`, `Main/04.Sprites` | `Editor/Fonts/PixelFontSources.cs:10`, `Editor/Stage/Stage1Pickups.cs:37` | 폰트·픽업 빌드 실패 | 경로 |
| 공용 | Build Settings, TagManager | `Editor/Flow/BuildSettingsSync.cs`, `TitleSceneSetup.cs:951` | 병합 충돌, 시작 씬이 바뀜 | 프로젝트 설정 |
| 공용 | 팀원 Animator 전체 | `Script/Player/Death/PlayerRestraint.cs:33` | 사망 중 팀원 애니메이션이 꺼짐(의도) | 동작 |
| 공용 | 팀원 씬의 CinemachineCamera | `Script/Flow/SubStage/StageCameraBinder.cs` | Confiner2D가 런타임에 추가됨 | 동작 |

참고(LDY 코드 밖): `Assets/_Recovery/*.unity` 5개가 git에 커밋돼 있고, LDY 타이틀 씬 복사본이라 LDY 스크립트를 참조한다. 병합 전에 지우는 것이 좋다.

---

## 11. 29일 병합 전/후 제안

### 11.1 병합 전에 꼭 고칠 것

코드를 고칠 것과 팀과 합의할 것을 나눴다. 위의 다섯 개는 30분 안에 끝나는 작업이다.

| 순서 | 할 일 | 관련 ID | 종류 |
|---:|---|---|---|
| 1 | 타이틀 셋업의 물리 재질 경로를 `JJBPM2D.physicsMaterial2D`로 고치거나 LDY 재질로 바꾼다. 로드에 실패하면 중단하게 한다 — **해결됨 (2026-11-25)** | E-02 | 코드(1줄) |
| 2 | 입력 액션 이름 의존을 정리한다. `throwIfNotFound=false` + 경고로 바꾸거나 LDY 입력 에셋을 분리한다. 최소한 JJB에 "Move/Jump/Interact, 맵 Player 이름 고정"을 받아 둔다 — **코드 해결됨 (2026-11-25)**, 이름 고정 합의는 남음 | P-08 | 코드 또는 합의 |
| 3 | ConflictingManagers가 대상 타입을 못 찾으면 경고를 남기게 해서, 조용한 실패를 보이는 실패로 바꾼다 — **해결됨 (2026-11-25)** | F-17 | 코드(몇 줄) |
| 4 | 병합 직후 `LDY/Merge/Validate Open Stage`로 JJBLevel 루트 이름 7개를 다시 확인한다 — 루트가 없을 때 멈추지 않고 돌아가는 처리는 **해결됨 (2026-11-25)**, 병합 직후 확인은 남음 | F-22 | 확인 |
| 5 | 병합 기준 플레이어 프리팹 하나를 정하고(`Assets/prefab` / `Assets/Main/02.Prefabs/Player` / `Assets/JJB/Prefab` 중) `FlowPaths.SharedPlayer`를 맞춘다 | P-09, P-04 | 결정 + 코드(1줄) |
| 6 | JJB와 점프·조작 API를 합의한다. `JJBPlayerJump.OnDisable` 초기화 동작, `IsGrounded`, `JJBPlayer.enabled`로 정지하는 방식, `JJBInteractable.Interact` 시그니처를 유지할지 공개 메서드를 추가할지 정한다 — 점프 차단은 Input 방식으로 바꾸면 동작이 달라져 **보류**했다(12.3절). 합의가 더 시급해졌다 | P-01, P-02, P-03, R-04 | 합의 |
| 7 | Build Settings와 TagManager는 병합 당일 한 사람이 확정한다. 확정 뒤에는 `Setup Title Scene`, `Build Stage Select Scene`을 다시 돌리지 않는다고 공지한다(자동으로 설정을 고치기 때문). Build에서 `RopeTest.unity`를 뺄지도 정한다 | F-23, E-11 | 합의 |
| 8 | 10절 표를 팀 병합 체크리스트로 공유한다. 팀원이 이름을 바꾸기 전에 LDY에 알려 달라고 요청한다 | 10절 전체 | 공유 |
| 9 | HUD 담당과 범위를 정한다 | H-01 | 결정 |
| 10 | `Assets/_Recovery`를 커밋에서 뺀다 | 참고 | 정리 |

### 11.2 병합 후로 미뤄도 되는 것

| 묶음 | 할 일 | 관련 ID |
|---|---|---|
| 런타임 UI 프리팹화 | 로딩 창, 선택 화면 고정 요소, 타이틀 창 틀을 프리팹이나 씬에 굽고 런타임은 바인딩만 한다. 레이아웃 상수를 스타일 에셋으로 옮긴다 | T-01, T-05, T-06, F-01, F-02, F-10, F-11 |
| 씬 전환 통합 | 타이틀 전환을 SceneFlow로 보내고 로딩 화면을 하나로 합친다. TitleConnector를 지운다 | T-10, E-04, F-07 |
| 어셈블리 정리 | 런타임 폴더(Title, Alt, Rope, Player, Adapters, Bridge)를 asmdef로 나누고, 순환을 계약 인터페이스로 끊는다. JJB 의존은 Adapters asmdef 하나로 가둔다 | R-06, A-03 |
| 런타임 주입 제거 | 통합 Player 프리팹에 LDY 컴포넌트를 굽고 Installer와 GetOrAdd는 검증만 하게 한다 | P-04, P-05, R-05, F-05, F-06 |
| 탐색 정리 | 태그 탐색을 참조 주입으로 바꾸고, Player 태그·Ground 레이어를 공용 상수로 모은다 | F-15, F-16, S-03, R-02, R-03, F-12, T-11, T-12, A-04 |
| 런타임 비용 | TaskbarView 시계 문자열, PlayerGripRig·PlayerDeath·MonitorZoomRig의 반복 GetComponent를 정리한다 | F-18, R-07, P-06, T-13, F-19 |
| 큰 클래스 분할 | TitleSceneBuilder, StageSelectSceneBuilder, WindowPanel, SceneFlow를 나눈다 | E-01, E-03, T-09, F-13, S-05 |
| 죽은 코드 삭제 | CloseSubScreenAction, LoggingSceneLoader, MarkerBlinkHz, SampleScene 메뉴들, Pink·V2·Proxy 변환기, 테스트 프로브를 지우고 폴더를 정리한다 | T-14, T-15, T-16, S-04, E-09, E-10, E-13 |
| 기타 | null 가드, PixelSpace 이름 정리, 배경 정렬 순서 확인을 한다 | F-20, H-03, H-04 |

---

## 12. 병합 전 필수 항목 수정 내역 (2026-11-25)

규칙: 팀원 코드·씬·프리팹과 ProjectSettings는 고치지 않았다. LDY 프리팹·씬은 이번에 바꾸지 않았다(메뉴로 바꿀 일이 생기지 않음).

검증: 모든 관련 어셈블리를 `dotnet build`로 컴파일해 오류 0개를 확인했다(경고는 기존 HES `ButtonBox.cs` 1건뿐). 새 EditMode 테스트 27개를 Unity 밖 리플렉션 러너로 실행해 모두 통과했다. Unity Test Runner에서 한 번 더 돌려야 한다.

### 12.1 항목별 수정

| 항목 | 수정 내용 | 결과 |
|---|---|---|
| 1. 물리 재질 | 경로 상수를 없애고 설정 SO `TitleSetupSettings`(`Assets/LDY/Editor/Settings/TitleSetupSettings.asset`, 에디터 전용)의 `playerPhysicsMaterial` 참조를 쓴다. 에셋이 없으면 처음 한 번 GUID `019de775…`(JJBPM2D)로 채워 만든다. Setup Title Scene은 새 프리팹과 **기존** AltPlayer 모두에 이 재질을 적용하고, 참조가 비어 있으면 기존 재질을 그대로 두고 경고만 남긴다 | 해결. 현재 AltPlayer 재질은 null이 아니고 이미 JJBPM2D다(프리팹을 고칠 필요 없음) |
| 2. 입력 액션 이름 | 맵·액션 이름을 `InputActionNames`(LDY.TitlePlayer.Core) 한곳에 모았다. `FindAction(…, true)` 대신 `PlayerInputActions.Lookup` → `FindAction(…, false)`을 쓴다. 못 찾으면 "JJBInput에 'Jump' 액션 없음 – 타이틀 입력 비활성" 에러를 에셋·액션마다 1회만 남기고 그 입력만 0/false가 된다. Validate 메뉴에 "타이틀 입력 액션 (AltPlayer)" 검사를 추가했다 | 해결 |
| 3. 점프 차단 | **바꾸지 않음**(중단 조건). 12.3절 | 보류 |
| 4. JJBLevelManager 끄기 | 타입 이름을 `FlowSettings.conflictingManagerTypes`로 옮겼다(기존 에셋은 필드 기본값으로 같은 이름을 가진다). 판별은 순수 함수 `ManagerScan`이 한다. 다중 스테이지 씬에서 못 찾으면 경고를 1회 남기고, `LevelManager`가 들어간 비슷한 컴포넌트 이름도 함께 보여 준다(`similarManagerHint`). Validate 메뉴에 "끌 팀원 매니저 타입" 검사를 추가했다 | 해결 |
| 5-1. 루트 이름 | 다중 스테이지 진입 시 카탈로그의 subRootName이 씬에 없으면 에러 로그 → 로더가 끝난 뒤 선택 화면으로 복귀 → "파일을 찾을 수 없습니다" 창을 띄운다. 세션은 시작하지 않는다 | 해결(이름 의존 자체는 남음) |
| 5-2. 플레이어 찾기 | 새 asmdef `LDY.PlayerLocate`의 `PrimaryPlayerLocator`로 모았다. 순수 규칙 `PrimaryPlayerRule`: 활성 PlayerInput → 이름에 Copy/Clone 없음 → 첫 번째(경고). 아래 표의 런타임 탐색 5곳을 바꿨다 | 해결 |
| 6-1. 시계 | 하루 중 분(0~1439)이 바뀔 때만 문자열을 만든다 | 해결 |
| 6-2. 죽은 메뉴·변환기 | 코드 호출 0건, 에셋 GUID 참조 0건을 확인한 뒤 삭제했다. 살아 있는 도구가 쓰는 부분은 남겼다(아래) | 해결/부분 해결 |

태그로 플레이어를 찾던 LDY 코드 목록

| 위치 | 용도 | 처리 |
|---|---|---|
| `Script/Flow/StageRoot.cs` | 플레이어 존재 경고 | PrimaryPlayerLocator |
| `Script/Flow/SubStage/SubStageLoader.cs` | 시작 지점으로 이동 | PrimaryPlayerLocator |
| `Script/Player/Death/PlayerDeathInstaller.cs` | PlayerDeath 부착 | PrimaryPlayerLocator |
| `Script/Rope/RopeHangController.cs` | player 미연결 시 대체 | PrimaryPlayerLocator |
| `Script/Spider/Runtime/TagPlayerLocator.cs` | 거미 추적 | PrimaryPlayerLocator + 0.5초 재탐색 간격 |
| `Script/Spider/Runtime/MouseTrap.cs`, `NeedleKiller.cs`, `Script/Flow/PlayerMatch.cs` | 충돌한 것이 플레이어인지 확인(찾기 아님) | 태그 상수만 `PrimaryPlayerLocator.Tag`로 통일 |
| `Editor/Flow/Merge/StageSceneBuilder.cs`, `StageSceneValidator.cs` | 편집 중 씬 검사 | 그대로 둠(에디터, 개수 검사) |
| `Editor/RopeTestSceneSetup.cs` | 테스트 씬 구성 | 파일 삭제 |

### 12.2 수정·추가·삭제한 파일

| 구분 | 파일 |
|---|---|
| 추가 | `Script/PlayerLocate/LDY.PlayerLocate.asmdef`, `PrimaryPlayerRule.cs`, `PrimaryPlayerLocator.cs` |
| 추가 | `Script/Flow/Core/ManagerScan.cs` |
| 추가 | `Script/TitlePlayer/Core/InputActionNames.cs`(InputActionNames, OptionalLookup, MissingReport), `Script/TitlePlayer/PlayerInputActions.cs` |
| 추가 | `Editor/TitlePlayer/TitleSetupSettings.cs`, `Editor/TitlePlayer/TitlePlayerPhysics.cs` |
| 추가(테스트) | `Tests/EditMode/PlayerLocate/LDY.PlayerLocate.Tests.asmdef`, `PrimaryPlayerRuleTests.cs`, `Tests/EditMode/TitlePlayer/InputActionLookupTests.cs`, `Tests/EditMode/Flow/ManagerScanTests.cs` |
| 수정(asmdef) | `Script/Flow/LDY.Flow.asmdef`(+LDY.PlayerLocate), `Script/Spider/Runtime/LDY.Spider.asmdef`(+LDY.PlayerLocate), `Editor/Flow/LDY.Flow.Editor.asmdef`(+LDY.TitlePlayer.Core) |
| 수정(런타임) | `Script/Flow/FlowSettings.cs`, `SceneFlow.cs`, `StageRoot.cs`, `PlayerMatch.cs`, `SubStage/ConflictingManagers.cs`, `SubStage/SubStageLoader.cs`, `Script/StageSelect/StageSelectScreen.cs`, `Views/TaskbarView.cs`, `Script/Title/Input/TitleInputs.cs`, `PlayerWindowInput.cs`, `Script/TitlePlayer/TitlePlayerInput.cs`, `Script/Player/Death/PlayerDeathInstaller.cs`, `Script/Rope/RopeHangController.cs`, `Script/Spider/Runtime/TagPlayerLocator.cs`, `MouseTrap.cs`, `NeedleKiller.cs` |
| 수정(에디터) | `Editor/TitleSceneSetup.cs`(재질, 맵 이름 상수, 레거시 변환 호출 → 경고), `AltTestSceneSetup.cs`(맵 이름 상수), `Flow/Merge/StageSceneValidator.cs`(검사 2개), `PlayerAnimSetBuilder.cs`(Pink/V2 제거), `PlayerVisual/PlayerVisualContext.cs`(SampleScene 상수 제거), `CursorCharacter/CursorPlayerAssigner.cs`(SampleScene 단계 제거) |
| 삭제 | `Editor/RopeTestSceneSetup.cs`(메뉴 3개: Setup SampleScene, Upgrade RopeTest To Interact, Assign Player Object Layer), `Editor/PlayerVisual/PlayerVisualMenu.cs`(메뉴 2개: Diagnose, Fix), `PlayerVisualDiagnostics.cs`, `PlayerVisualFixer.cs`, `AsepriteCatalog.cs`, `Editor/TitlePlayer/TitlePlayerConverter.cs`(메뉴 LDY/Player/Convert Title Players, JJBFacingProxy). 각 `.meta` 포함 |
| 삭제(메뉴만) | `LDY/Build Player Anim Set`, `LDY/Build Player Anim Set V2` (`PlayerAnimSetBuilder.cs`) |

일부러 남긴 것

- `PinkVisual` 이름: 공용 Player 프리팹의 비주얼 자식 이름으로 아직 쓰인다(CursorPlayerAssigner, PlayerMerge).
- `LegacyVisualSwitch`: 살아 있는 `LDY/Player/Merge Into One`이 쓴다.
- `player_anim_pinkV2.aseprite`: Cursor Character의 "Match Import Settings"가 임포트 설정 기준으로 쓴다.
- `PlayerAnimSet.asset`, `PlayerAnimSetV2.asset`: 아무도 참조하지 않지만 에셋 삭제는 이번 범위가 아니라 남겼다.
- 변환기 삭제와 함께 `TitleSceneSetup`의 레거시 변환 호출은 "TitlePlayerMotor가 없으면 경고"로 바꿨다. AltPlayer와 LDY_TestScene은 이미 변환돼 있다(JJB 컴포넌트 0, 프록시 0).

### 12.3 점프 차단을 바꾸지 않은 이유 (중단 조건)

JJB 점프 입력의 경로: PlayerInput(SendMessages) → `JJBPlayerInput.OnJump` → `JJBPlayer.HandleJumpKeyPress` → `JJBPlayerJump.RequestJump()`가 `_jumpRequested = true`를 세운다 → 다음 `FixedUpdate`의 `Tick`이 처리한다.

PlayerInput의 Jump 액션을 Disable/Enable하는 방식으로 바꾸면 JJB 플레이어 동작이 다음처럼 달라진다.

1. **이미 들어온 점프 요청을 지울 수 없다.** 지금 `SetSuspended(true)`(로프 잡기, Alt 진입)가 부르는 `ClearPendingJump`는 `OnDisable`로 `_jumpRequested`를 지운다. 액션만 끄면 같은 프레임에 들어온 요청이 남고, 정지가 풀릴 때(Alt 복귀, 로프 놓기) 땅에 있으면 원하지 않은 점프가 나간다. 이 플래그를 지울 공개 API가 JJB에 없고, 팀원 코드는 고칠 수 없다.
2. **지금 방식의 부수 효과도 사라진다.** 차단 중에는 매 FixedUpdate마다 `OnDisable`이 `_timeInAir`를 0으로 돌려서 추가 중력(`ApplyExtraGravity`)이 꺼진다. Input 방식에서는 이 효과가 없다(사망 중에는 몸이 Kinematic이라 영향이 작지만 동작이 같지는 않다).
3. Jump가 Button 타입이고 PlayerInput이 SendMessages라 떼는 순간(canceled)은 전달되지 않는다. 액션을 끄고 켜도 `IsJumpHold`는 정리되지 않는다(지금도 같은 동작이라 차이는 없음).

카운트 방식(여러 차단 요청 합산)도 이 항목의 일부라 함께 보류했다. 제안: JJB에 `JJBPlayerJump.CancelPendingJump()`(요청·홀드 초기화) 같은 공개 메서드 하나를 요청하고, 그 뒤에 LDY 쪽을 카운트 방식 + 해당 API 호출로 바꾼다. 카운트 로직은 순수 함수로 만들어 테스트한다.

### 12.4 메뉴 실행 순서

Unity 에디터로 돌아가면 새 파일과 asmdef가 컴파일된다(`.meta`는 Unity가 만든다).

1. Console에 컴파일 에러가 없는지 확인한다.
2. Test Runner → EditMode → 전체 실행. 새 테스트는 `PrimaryPlayerRuleTests`, `InputActionLookupTests`, `ManagerScanTests`다.
3. `LDY/Title/Setup Title Scene`(선택): 처음 실행하면 `Assets/LDY/Editor/Settings/TitleSetupSettings.asset`이 만들어진다. 콘솔에 "물리 재질 = Assets/JJB/JJBPM2D.physicsMaterial2D"가 찍히는지 본다. AltPlayer 재질은 이미 같아서 프리팹은 바뀌지 않는다.
   - 셋업을 돌리지 않는다면 이 단계는 건너뛴다. 설정 에셋은 셋업이 처음 필요할 때 만들어진다.
4. JJBLevel 씬(`Assets/JJB/JJBLevel.unity`)을 연 상태에서 `LDY/Merge/Validate Open Stage`를 실행한다. "끌 팀원 매니저 타입", "타이틀 입력 액션 (AltPlayer)", 다중 스테이지 루트 7개가 OK인지 본다.
5. FlowSettings(`Assets/LDY/Resources/LDY/FlowSettings.asset`) 인스펙터에서 `Conflicting Manager Types`가 `JJB.Script.Level.JJBLevelManager`인지 확인한다. 한 번 저장하면(값을 건드렸다 되돌리는 정도) 에셋 파일에도 기록된다.

### 12.5 확인 체크리스트

| 구분 | 확인 | 결과 |
|---|---|---|
| 타이틀 | A/D 이동, Space 점프가 전과 같다 | ☐ |
| 타이틀 | E로 아이콘 실행, 설정창에서 Space/S 행 이동·E 조작이 된다 | ☐ |
| 타이틀 | AltPlayer Rigidbody2D 재질이 JJBPM2D다(벽에 붙지 않음) | ☐ |
| 타이틀 | Console에 "…액션 없음 – 타이틀 입력 비활성" 에러가 없다 | ☐ |
| 선택 화면 | 걷기·점프·발판 위 E 실행이 된다. 오른쪽 아래 시계가 분 단위로 바뀐다 | ☐ |
| 점프 차단(변경 없음, 회귀 확인) | 로프를 잡은 동안 Space로 JJB 점프가 나가지 않는다(Space는 줄 점프 놓기) | ☐ |
| 점프 차단 | Alt를 누른 채 점프 → Alt를 떼고 복귀했을 때 엉뚱한 점프가 없다 | ☐ |
| 점프 차단 | 설정 창이 열린 동안 Space는 행 이동만 하고 물리 점프는 없다(타이틀) | ☐ |
| 점프 차단 | 사망 연출 중 점프가 안 된다 | ☐ |
| JJBLevel 진입 | 선택 화면에서 1-T~1-6에 들어가면 해당 루트만 켜지고 시작 지점에 선다. Console에 "JJBLevelManager(…) 런타임에서 끔"이 찍힌다 | ☐ |
| JJBLevel 진입(실패 경로) | (테스트용 복사본 카탈로그에서) subRootName을 없는 이름으로 바꾸고 진입 → 에러 로그 1개, 선택 화면 복귀, "파일을 찾을 수 없습니다" 창 | ☐ |
| PlayerCopy | 복사 스킬로 PlayerCopy를 만든 뒤 사망 → 본체가 사망 연출을 하고 재시작된다 | ☐ |
| PlayerCopy | PlayerCopy가 있는 상태에서 재시작 → 시작 지점으로 옮겨지는 것이 본체다 | ☐ |
| PlayerCopy | Console에 "[Player] 'Player' 태그 N개 중 본체를 가리지 못해…" 경고가 나오면, 본체·복사본의 PlayerInput 활성 상태와 이름을 확인한다 | ☐ |
