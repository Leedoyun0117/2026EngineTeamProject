# 밧줄(Rope) 코드 공부 노트

> 기준 커밋: `67c0587` (브랜치 LDY). 줄 번호는 이 시점 파일 기준입니다. 파일이 바뀌면 줄 번호는 어긋납니다.
> 표기: **[코드]** = 코드에서 직접 읽은 사실, **[의도]** = 구현하면서 정한 설계 의도(코드만 봐서는 안 보임), **[확인 필요]** = 실제 Unity 실행으로 확인 못 한 것.

---

## 1. 한눈에 보기

### 밧줄이 하는 일 (5줄)
1. 플레이어가 줄 근처에서 **F**를 누르면 JJB 상호작용 경로로 줄이 호출되어 **매달린다**.
2. 매달린 동안 플레이어는 **진자**로 움직인다. A/D로 흔들고, W/S로 줄 위 거리(`s`)를 바꾼다.
3. 줄 모양은 **Verlet 시뮬레이션**이 따로 그린다(시각용). 플레이어 위치에는 영향이 없다.
4. **F 또는 Space**로 놓는다. 놓는 순간의 접선 속도가 플레이어에게 넘어가고, Space는 위로 보너스 속도가 더해진다.
5. 놓은 뒤 줄은 속도를 유지한 채 흔들리다가 감쇠로 **천천히 가라앉는다**.

### 클래스 관계도

```
┌──────────────────────────── 팀원 코드 (JJB, 수정 금지) ─────────────────────────────┐
│  PlayerInput(Send Messages) → JJBPlayerInput.OnInteract → JJBPlayer.HandleInteractPressed │
│            → JJBPlayerInteract.Trigger/FindTarget  ──(IInteract.Interact 호출)──┐          │
│  JJBControlGate.SetSuspended  ← (LDY.Script에 있지만 JJB 쪽을 감싸는 어댑터)    │          │
└─────────────────────────────────────────────────────────────────────────────────┼──────────┘
                                                                                  ▼
┌──────────────────────── Unity 쪽 (Assembly-CSharp, LDY.Script) ────────────────────────┐
│ RopeInteractable (JJBInteractable 상속, 얇은 어댑터)                                    │
│        │ IRopeGrabber.TryGrab()                                                         │
│        ▼                                                                                │
│ RopeHangController ──읽음──> RopeSettings (ScriptableObject: 모든 수치)                 │
│   │  │  │  └─ RopeInput (F/Space/WASD 자체 InputAction)                                 │
│   │  │  └──── PlayerSuspendHub.Hub ──> PlayerControlSuspendSink ──> JJBControlGate      │
│   │  └─────── PlayerRopeSlot.Slot (HoldSlot)                                            │
│   │  IRopePose.Points                                                                   │
│   ├──────────> RopeLineRenderer (LineRenderer로 그림)                                   │
│   └──────────> RopeColliderFollower (EdgeCollider2D가 줄을 따라감 → JJB가 감지)        │
│ AltModeController ──> SuspendRequestControl ──> 같은 Hub (정지 요청 합산)              │
└───────────────────────────────────────────┬────────────────────────────────────────────┘
                                            │ 사용 (Unity 컴포넌트 아님, 일반 C# 클래스)
┌──────────────────── 순수 로직 (asmdef: LDY.Rope, Logic/ 폴더) ─────────────────────────┐
│ Pendulum ─ PendulumTuning/State/Input     ClimbSway     ReleaseVelocity                 │
│ RopeSimulation ─┬─ RopeCurve (Verlet)     RopeLengthLimits     RopeReach              │
│                 └─ RopeDampingProfile                                                   │
│ SuspendRequestHub + ISuspendSink          HoldSlot          ReleaseGate                 │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

경계 규칙 **[코드]**: `LDY.Rope.asmdef`의 `references`가 비어 있다(`Logic/LDY.Rope.asmdef:4`). 그래서 Logic 폴더는 Unity 컴포넌트나 JJB 타입을 쓸 수 없고, 쓸 수 있는 건 `UnityEngine`의 `Vector2`, `Mathf` 같은 값 타입뿐이다(`noEngineReferences: false`, 같은 파일 13행).
**[의도]** 이 경계 덕분에 Logic 코드는 Unity 에디터를 켜지 않고도 EditMode 테스트로 검증된다.

---

## 2. 읽는 순서 추천 (쉬운 것 → 어려운 것)

| # | 파일 | 볼 곳(줄) | 왜 이 순서인가 |
|---|---|---|---|
| 1 | `Logic/ReleaseGate.cs` | 전체 1–20 (`Accepts` 16–18) | 20줄. "프레임 번호 비교"만 이해하면 끝 |
| 2 | `Logic/HoldSlot.cs` | `TryAcquire` 6–18, `Release` 20–26 | 소유자 1명 + 쿨다운. 상태 두 개뿐 |
| 3 | `Logic/SuspendRequestHub.cs` | `Request` 18–24, `Release` 26–32 | "소유자 집합이 0↔1이 될 때만 알림" |
| 4 | `Logic/ReleaseVelocity.cs`, `Logic/RopeLengthLimits.cs`, `Logic/RopeReach.cs` | 5–8 / 7–11 / 5–13 | 한 메서드짜리 작은 계산들 |
| 5 | `Logic/ClimbSway.cs` | `Step` 24–33 | sin 파형 + 서서히 커지는 비율 |
| 6 | `Logic/Pendulum.cs` | `PositionOf` 50–52, `TangentVelocity` 54–56, `CreateState` 64–70, **`Step` 72–90**, `LimitReach` 92–100, `LimitAngle` 102–112 | 플레이어 움직임의 핵심 수학 |
| 7 | `Logic/RopeDampingProfile.cs` | 생성자 25–32, `AfterRelease` 37–40 | 놓은 뒤 감쇠가 시간에 따라 커지는 곡선 |
| 8 | `Logic/RopeCurve.cs` | `Step` 124–141, `Integrate` 163–180, `Relax/Constrain` 200–222, `FillRest` 147–161, `Tighten/Pull` 182–198, `SetPin` 87–98 | 가장 어려움. Verlet + 거리 제약 |
| 9 | `Logic/RopeSimulation.cs` | `PrepareGrab` 35–39, `Release` 41–49, `Step` 51–55, `StepHeld` 57–62 | RopeCurve + 감쇠 곡선을 묶는 얇은 조립 |
| 10 | `RopeSettings.cs` | 전체 1–69 (프로퍼티 52–67) | 모든 수치의 집. 어떤 값이 어느 구조체로 가는지 |
| 11 | `RopeInput.cs`, `RopeInteractable.cs`, `PlayerSuspendAdapters.cs` | 13–70 / 7–17 / 4–54 | Unity와 닿는 작은 어댑터들 |
| 12 | `RopeColliderFollower.cs`, `RopeLineRenderer.cs` | 16–28 / 28–42 | 줄 점을 소비하는 두 컴포넌트 |
| 13 | **`RopeHangController.cs`** | `Awake` 37–59, **`TryGrab` 80–104**, **`FixedUpdate` 106–118**, `Hang` 124–133, `Release` 135–147, `ForceRelease` 149–162 | 모든 조각이 모이는 곳. 앞 파일을 알고 읽으면 쉬움 |
| 14 | `Alt/AltModeController.cs`, `Alt/AltState.cs`, `Alt/ReturnState.cs` | 30, 40–43 / 37 / 59–78 | Alt와 밧줄이 겹치는 지점 |
| 15 | (읽기만) JJB: `JJBInteractable.cs`, `JJBPlayerInteract.cs`, `JJBPlayer.cs`, `JJBPlayerInput.cs`, `LDY/Script/Adapters/JJBControlGate.cs` | 5–8 / 14–44 / 44–67 / 34–40 / 40–66 | 밧줄이 기대는 팀원 코드 |
| 16 | `Editor/RopeTestSceneSetup.cs` | `Setup` 21–44, `Upgrade` 47–60, `AssignObjectLayer` 69–90, `AttachInteraction` 92–105 | 테스트 씬을 만드는 에디터 도구 |
| 17 | 테스트 3개 | 7장 참고 | 코드를 읽은 뒤 "이 코드가 무엇을 보장하나" 확인 |

---

## 3. 클래스별 정리

### 3-1. 순수 로직 (`Assets/LDY/Script/Rope/Logic/`, asmdef `LDY.Rope`)

| 클래스 | 역할 한 줄 | 핵심 필드 / 메서드 | 왜 분리했나 (SOLID) | 의존 |
|---|---|---|---|---|
| `Pendulum` (+`PendulumTuning/State/Input`) | 플레이어가 줄에 매달렸을 때의 **진자 물리** 한 틱 계산 | 필드 `_tuning`. `Step`(72–90): 중력/스윙/감쇠/한계 힘을 더해 각속도 갱신. `CreateState`(64–70): 잡은 위치→각도·길이·각속도. `PositionOf/TangentVelocity`(정적) | **SRP**: 계산만 한다. 입력/물리엔진/줄 모양을 모른다 | 없음 (Unity 값 타입만) |
| `ClimbSway` | 오르내릴 때 **작은 좌우 반동** 값 생성 | `_amount`(0→1 서서히), `_phase`. `Step`(24–33) | **SRP**: 진자 코드에 연출 로직이 섞이지 않게 | 없음 |
| `ReleaseVelocity` | 놓을 때 플레이어에게 줄 **이탈 속도** 계산 | `Compute`(5–8): 접선 속도(+위 보너스) | **SRP**, 정적 순수 함수라 테스트 쉬움 | `Pendulum` |
| `RopeLengthLimits` | 플레이어 거리 범위를 줄 전체 길이 안으로 제한 | `Apply`(7–11): max를 총길이×0.8로 제한 | **SRP**. 상수 0.8이 코드에 박혀 있음(5행) | `PendulumTuning` |
| `RopeReach` | 선분 위 **가장 가까운 점** 계산 | `ClosestPoint`(5–13) | **SRP** | 없음 |
| `RopeCurve` | 줄 모양 = **Verlet 점 N개**와 거리 제약 | `_points`/`_previous`(현재/이전 위치), `_rest`(마디별 목표 길이), `_pin`(플레이어에 고정된 점), `_blend`. `Step`(124–141) | **SRP**: 플레이어 위치에 영향 없는 "시각용" 계산만 | `RopeReach` |
| `RopeDampingProfile` | 놓은 뒤 시간에 따라 **감쇠가 커지는 곡선** | `AfterRelease(elapsed)`(37–40) | **SRP**, **OCP**(곡선만 바꾸면 줄 코드는 그대로) | 없음 |
| `RopeSimulation` | `RopeCurve`+`RopeDampingProfile`을 묶는 **줄의 공개 창구** | `PrepareGrab`, `Release`, `Step`, `StepHeld` | **파사드/SRP**: 컨트롤러가 곡선 내부(pin, blend)를 몰라도 되게 | `RopeCurve`, `RopeDampingProfile` |
| `SuspendRequestHub` (+`ISuspendSink`) | **정지 요청 합산**: 요청자가 한 명이라도 있으면 정지 | `_owners`(HashSet). 0→1에서 `SetSuspended(true)`, 1→0에서 `false` | **SRP**, **DIP**: 실제 정지 방법은 `ISuspendSink` 뒤에 숨김 | `ISuspendSink`(인터페이스) |
| `HoldSlot` | 플레이어가 **한 번에 밧줄 하나만** 잡게 하는 슬롯 + 재잡기 쿨다운 | `_owner`, `_blockedUntil` | **SRP** | 없음 |
| `ReleaseGate` | 잡은 프레임의 놓기 입력을 **무시**하는 문지기 | `MarkGrab`, `Accepts`(16–18) | **SRP** | 없음 |

### 3-2. Unity 쪽 (`Assets/LDY/Script/Rope/`, Assembly-CSharp)

| 클래스 | 역할 한 줄 | 핵심 | 왜 분리했나 | 의존 |
|---|---|---|---|---|
| `RopeSettings` (ScriptableObject) | **모든 수치**를 담는 데이터 에셋 | 구조체 필드 `tuning/climbSway/curve/ropeDamping`과 단일 값. `Tuning`(52)은 `RopeLengthLimits`를 거쳐 나감 | **SRP/OCP**: 수치를 코드에서 빼서 인스펙터로 | 순수 로직의 `*Tuning` 구조체들 |
| `RopeHangController` | **조립과 위임**. 입력→진자→줄→플레이어 몸 | `_hanging`, `_state`(진자 상태). `TryGrab`, `FixedUpdate`, `Hang`, `Release` | **SRP**: 계산은 하지 않고 순서만 지휘 | 대부분의 클래스 |
| `IRopeGrabber` / `RopeInteractable` | JJB 상호작용을 받아 `TryGrab`만 부르는 **어댑터** | `Interact`(14–16) | **SRP/DIP**: 진자·줄·입력 로직이 JJB 타입에 오염되지 않게 | `JJBInteractable`(JJB), `IRopeGrabber` |
| `IRopePose` | 줄 점 목록을 읽기 전용으로 노출 | `Points` | **ISP/DIP**: 렌더러와 콜라이더가 컨트롤러 전체를 몰라도 됨 | 없음 |
| `RopeInput` (+`IRopeInput`) | F(놓기)/Space(점프 놓기)/WASD **자체 입력** | `_release/_jump/_move`, 눌린 **프레임 번호** 저장 | **SRP** | Input System |
| `RopeColliderFollower` | `EdgeCollider2D`를 줄 모양으로 갱신(JJB가 감지하게) | `FixedUpdate`(16–28) | **SRP** | `IRopePose` |
| `RopeLineRenderer` | 줄을 `LineRenderer`로 그림(픽셀 스냅) | `LateUpdate`(28–42), `Snap`(53–58) | **SRP** | `IRopePose` |
| `PlayerControlSuspendSink` | `ISuspendSink` → `IPlayerControl.SetSuspended` 연결 | 11–13 | **DIP/어댑터** | `IPlayerControl` |
| `SuspendRequestControl` | **Alt가 쓰는** `IPlayerControl` 모양의 어댑터. 호출을 허브 요청/해제로 바꿈 | `SetSuspended`(25–30) | **LSP/어댑터**: Alt 코드는 그대로 두고 연결만 바꿈 | `SuspendRequestHub` |
| `PlayerSuspendHub` | 플레이어에 붙는 허브 보관 컴포넌트 | `Hub`(38–39), `GetOrAdd`(41–43) | **SRP** | `JJBControlGate` |
| `PlayerRopeSlot` | 플레이어에 붙는 `HoldSlot` 보관 컴포넌트 | `Slot`(48) | **SRP** | `HoldSlot` |

### 3-3. 에디터 / 테스트
- `Editor/RopeTestSceneSetup.cs`: 테스트 씬 구성 도구(메뉴 3개). 런타임에는 포함되지 않는다.
- `Tests/EditMode/Rope/`: 별도 asmdef `LDY.Rope.Tests`(참조는 `LDY.Rope`와 Unity 테스트 러너뿐, `LDY.Rope.Tests.asmdef:4–8`).

---

## 4. 한 프레임의 흐름 3가지

### 4-1. 잡기
1. 플레이어가 F를 누른다. `PlayerInput`이 Send Messages 방식이다 **[코드]** (`Assets/prefab/Player.prefab:192` `m_NotificationBehavior: 0`).
2. `JJBPlayerInput.OnInteract` (`JJBPlayerInput.cs:34–40`) → `OnInteractPressed` 이벤트.
3. `JJBPlayer.HandleInteractPressed` (`JJBPlayer.cs:63–67`) → `JJBPlayerInteract.Trigger` (`JJBPlayerInteract.cs:14–22`).
4. `FindTarget` (24–44): `OverlapCircleAll(interactPoint, interactSize, objectLayer)` 결과에서 **첫 번째** `IInteract`를 고른다(가장 가까운 것이 아님).
5. `RopeInteractable.Interact` (`RopeInteractable.cs:14–16`) → `IRopeGrabber.TryGrab()`.
6. `RopeHangController.TryGrab` (80–104):
   1. 이미 매달렸거나 컴포넌트가 꺼져 있으면 `false`(81–82).
   2. `_slot.TryAcquire` — 다른 줄을 잡고 있거나 쿨다운 중이면 `false`(84–85).
   3. 플레이어 속도를 저장하고(87) `_rope.PrepareGrab(플레이어 위치)`로 줄 위 **가장 가까운 점**을 얻는다(88).
   4. `_hub.Request(this)` — **정지 요청 등록**(91). 요청자가 첫 번째이면 `PlayerControlSuspendSink.SetSuspended(true)` → `JJBControlGate.SetSuspended(true)`(`JJBControlGate.cs:40–50`) → `Apply`가 `JJBPlayer.enabled = false`(65), 대기 중 점프 정리(49).
   5. `_pendulum.CreateState`로 **진자 시작 상태**를 만든다(99), `_sway.Reset()`, `_gate.MarkGrab(프레임, 시각)`, `_hanging = true`(99–102).
7. 같은 프레임에 `RopeInput`의 `_release.performed`도 불려 눌린 프레임 번호를 적어 둔다(`RopeInput.cs:28`). 다음 `FixedUpdate`에서 `ReleaseGate.Accepts`가 `pressFrame > grabFrame`이 아니라서 **거절**한다(`ReleaseGate.cs:17`).

### 4-2. 매달린 동안 (`FixedUpdate`마다)
1. `RopeHangController.FixedUpdate` (106–118): 놓기 입력을 읽고(108–109) 없으면 `Hang(deltaTime)`(114–115).
2. `Hang` (124–133):
   1. `_input.Axis`를 읽는다(125). A/D는 `x`, W/S는 `y`.
   2. `swing = axis.x + _sway.Step(axis.y, dt)` — 오를 때 작은 반동을 더한다(126).
   3. `_pendulum.Step(...)` — 길이 `s` 갱신, 힘 합산, 각속도→각도(127). (`Pendulum.cs:72–90`)
   4. `Pendulum.PositionOf(Anchor, _state)`로 목표 위치(128) → `_body.Position`에 적용, `_body.Velocity = 0`(129–130).
   5. `_rope.StepHeld(Anchor, s, position, dt)` — 줄 시뮬레이션(131).
   6. `_pose?.Report(true, 오르내림 방향)` — 애니메이션 포즈 보고(132).
3. `RopeSimulation.StepHeld` (57–62) → `RopeCurve.SetPin`(점을 플레이어 위치에 고정, 87–98) → `RopeCurve.Step` (124–141): 블렌드 갱신 → `FillRest`(마디 목표 길이) → `Integrate`(관성+중력) → 고정점/anchor 덮어쓰기 → `Relax` 반복(거리 제약) → `Tighten`(직선화).
4. 그리기: `RopeLineRenderer.LateUpdate`(28–42)가 `IRopePose.Points`를 읽어 `LineRenderer`에 넣는다.
5. 콜라이더: `RopeColliderFollower.FixedUpdate`(16–28)가 같은 점들을 `EdgeCollider2D.points`로 넣는다. **[확인 필요]** 두 `FixedUpdate`(컨트롤러/콜라이더)의 실행 순서는 코드에 지정돼 있지 않다(`DefaultExecutionOrder` 없음). 한 틱 늦을 수 있다.

### 4-3. 놓기
1. 매달린 상태에서 F 또는 Space. `JJBPlayer`가 꺼져 있어 JJB 이벤트 구독이 해제된 상태(`JJBPlayer.cs:50–55`)라 JJB는 이 입력을 받지 않는다. `RopeInput`의 `performed`만 프레임 번호를 기록한다(`RopeInput.cs:28–29`).
2. `FixedUpdate`: `ReadRelease`(120–122)가 `_hanging && pressed && _gate.Accepts(...)`를 확인(108–109).
3. 통과하면 `Release(jumped)` (135–147):
   1. `ReleaseVelocity.Compute`로 이탈 속도 계산(136). 접선 속도 + (Space면 `JumpReleaseBonus`만큼 위).
   2. `_hanging = false`, `_rope.Release()`(137–138) — 줄의 고정이 풀리고 `_sinceRelease = 0`(`RopeSimulation.cs:41–49`).
   3. `_hub.Release(this)` — **정지 요청 해제**. 마지막 요청이면 `SetSuspended(false)` → `JJBPlayer.enabled = true`(140).
   4. `finally`: 쿨다운 시작(143), 포즈 보고 해제(144), `_body.Velocity = velocity`(145) — **정지 해제를 먼저 한 뒤** 속도를 준다.
4. 같은 틱의 뒷부분(116–117): `_hanging`이 false이므로 `_rope.Step`(자유 상태)이 불린다. 이후 매 틱 `RopeDampingProfile.AfterRelease`가 감쇠를 서서히 키워 줄이 가라앉는다.

---

## 5. 헷갈리기 쉬운 부분 8가지

### ① FixedUpdate와 scaled time을 쓰는 이유
- **문제**: Alt를 누르면 줄과 진자도 같이 멈춰야 한다. 줄이 Alt 중에 혼자 움직이면 안 된다.
- **원인**: Alt는 `Time.timeScale = 0`으로 시간을 얼린다(`TimeScaleFreezer.cs:8–15`, `AltState.cs:41`). `Update`는 계속 불리지만 `FixedUpdate`는 timeScale 0에서 호출되지 않는다(Unity 동작).
- **해결**: 진자와 줄 계산을 전부 `FixedUpdate`에서 한다(`RopeHangController.cs:106–118`). 그래서 **줄을 멈추는 코드를 따로 안 짜도** Alt 중에는 자동으로 멈추고, 복귀(시간이 돌아옴) 후에는 이어서 계산한다. 입력만 `Update`에서 따로 비운다(75–78, timeScale 0일 때 `_input.Clear()`).

### ② 정지 요청 합산이 필요한 이유
- **문제**: 매달린 동안 Alt를 쓰고 돌아오면 매달림이 풀려 버린다.
- **원인**: Alt는 시작에 `SetSuspended(true)`(`AltState.cs:37`), 끝에 `SetSuspended(false)`(`ReturnState.cs:73`)를 부른다. `JJBControlGate.SetSuspended`는 단순한 true/false 값 하나(`JJBControlGate.cs:40–50`)라서 Alt의 `false`가 줄이 건 정지까지 풀어 버린다.
- **해결**: `SuspendRequestHub`가 **요청자 집합**을 센다(`SuspendRequestHub.cs:9`). 0→1일 때만 정지(22–23), 1→0일 때만 해제(30–31). Alt는 `SuspendRequestControl`을 통해 owner = `AltModeController`로(`AltModeController.cs:30`), 줄은 owner = 자신으로 등록한다. 둘이 겹친 경우는 테스트 `AltAndRope_OverlapKeepsSuspendedUntilBothRelease`가 보장한다.

### ③ 잠금 토큰을 쓰지 않은 이유
- **문제**: 기존 이동 정지 방법인 `InputLockService` 토큰(`PlayerInputLock.Lock.Acquire()`)을 쓰면 매달린 동안 Alt가 막힌다.
- **원인**: `AltKeyInput.Available`이 `_lock.IsLocked`이면 입력을 없는 것으로 친다(`AltKeyInput.cs:16–19`).
- **해결**: 토큰 대신 `JJBControlGate.SetSuspended` 경로(허브)를 쓴다. 잠금과 무관하므로 Alt 입력은 막히지 않는다.

### ④ Alt 복귀 때 위치의 주인은 누구인가
- **문제**: Alt는 복귀할 때 플레이어 위치와 속도를 스냅샷으로 되돌린다. 매달려 있다면 그 위치가 줄과 어긋날 수 있다.
- **원인**: `ReturnState.Exit`가 `_body.Position`과 `_body.Velocity`를 스냅샷으로 복원한다(`ReturnState.cs:64–66`).
- **해결**: Alt 코드는 건드리지 않았다. 매달린 동안의 **위치 주인은 줄(진자)**이라, 복귀 후 다음 `FixedUpdate`의 `Hang`이 자기 상태로 위치를 다시 쓰고 속도를 0으로 만든다(`RopeHangController.cs:128–130`). **[확인 필요]** Alt 중·복귀 직후 실제 화면에서 한 틱 튀는지는 Unity에서 확인하지 못했다.

### ⑤ 진자(플레이어)와 Verlet(줄 모양)의 역할 분리
- **문제**: 줄을 물리 관절로 만들면 플레이어 움직임이 줄 시뮬레이션 오차에 흔들린다.
- **원인**: 줄은 "그려지는 것"이고 플레이어 궤적은 "정확히 제어되어야 하는 것"이라 요구가 다르다.
- **해결**: 플레이어는 `Pendulum`(각도·길이 한 쌍)으로 **계산식**으로만 움직인다. 줄(`RopeCurve`)은 그 결과 위치를 한 점에 **따라가기만** 한다(`RopeSimulation.StepHeld`가 플레이어 위치를 받음, 57–62). 줄은 플레이어 위치 계산에 쓰이지 않는다 **[의도]**. 콜라이더는 JJB가 줄을 "찾기" 위해서만 쓴다.

### ⑥ 줄 전체 길이는 고정, 플레이어는 줄 위 거리(s)만 바꾼다
- **문제**: W/S로 오르내릴 때 줄 전체가 늘어나거나 점이 순간이동하면 어색하다.
- **원인**: 처음엔 고정할 점의 번호를 `s`에 따라 계단식으로 바꿔서 점이 튀었다 **[의도]**.
- **해결**: `s`는 `Pendulum.Step`의 `length`(73)이고 `RopeSettings.totalLength`(27)와 별개다. 고정할 점 번호(`_pin`)는 잡을 때 한 번 정하고 안 바꾼다(`RopeCurve.SetPin` 89–94). 대신 마디별 목표 길이 `_rest`를 `FillRest`(147–161)가 매 틱 계산한다. 윗부분(anchor~플레이어)은 `s ÷ 마디 수`, 아랫부분(꼬리)은 `(총길이 − s) ÷ 마디 수`라서 **합은 항상 총길이**다. 테스트 `ClimbingKeepsTotalLengthAndNeverJumpsPoints`가 보장한다. 플레이어 거리 범위는 총길이의 80%까지(`RopeLengthLimits.cs:5,8`)라 꼬리가 항상 남는다.

### ⑦ 놓은 뒤 줄이 천천히 가라앉는 원리
- **문제**: 놓는 순간 줄을 초기화하면 줄이 갑자기 "철컥" 멈추거나 순간이동한다.
- **원인**: Verlet은 "현재 위치 − 이전 위치"가 곧 속도다(`RopeCurve.cs:67–69`). 위치를 덮어쓰면 속도도 같이 사라진다.
- **해결**:
  1. 놓을 때 `ClearPin`만 한다(`RopeCurve.cs:100–102`). 고정되어 있던 점의 `_points`와 `_previous`는 그대로라 **속도가 유지**된다. 테스트 `Release_KeepsHeldPointVelocity`.
  2. 감쇠는 `RopeDampingProfile.AfterRelease`(37–40)가 만든다. 놓은 직후는 잡은 동안 값에서 시작해 `releaseDamping`으로 부드럽게 이어지고(`releaseBlendTime`), 이후 `settleTime`에 맞춰 더 큰 값으로 서서히 커진다(31행의 공식). 둘 다 `Smooth`(smoothstep, 42–45)로 보간해 갑자기 바뀌는 구간이 없다.
  3. 구체적인 "6", "12" 숫자(`DecayExponent`, 15행)는 "진폭이 약 5%가 되는 시간"을 대략 맞추려고 정한 값이다 **[의도, 근사]**.

### ⑧ ReleaseGate와, 놓기 프레임의 F가 JJB로 새지 않는 이유
- **문제 A (잡을 때)**: 같은 F 입력이 두 군데로 들어간다. JJB 경로(잡기)와 `RopeInput`(놓기). 잡자마자 놓기로 처리될 수 있다.
- **원인 A**: 두 입력 핸들러 모두 F를 받아서 같은 프레임에 불린다. 어느 쪽이 먼저인지는 보장되지 않는다.
- **해결 A**: `ReleaseGate`가 "잡은 프레임"을 기억하고(`MarkGrab`, `RopeHangController.cs:101`), 놓기 입력의 **눌린 프레임 번호**(`RopeInput.cs:28`)가 그보다 **커야** 통과시킨다(`ReleaseGate.cs:17`). 순서가 어느 쪽이든 같은 프레임이면 거절된다. 프레임 번호를 적는 이유는 `FixedUpdate`가 입력이 처리된 프레임보다 나중에 불릴 수 있어서 "지금 프레임"과 비교할 수 없기 때문이다. 추가로 최소 지연 `releaseInputDelay`(0.05초)가 있다.
- **문제 B (놓을 때)**: 놓는 F가 JJB에도 들어가 곧바로 다시 잡혀 버릴까?
- **원인 B**: 놓기 F를 JJB가 받으면 `Interact`가 다시 불린다.
- **해결 B**: 매달리는 동안 `JJBPlayer`는 꺼져 있다. `JJBPlayer.OnDisable`이 `OnInteractPressed` 구독을 해제한다(`JJBPlayer.cs:50–55`). F가 눌리는 순간 JJB는 듣지 않고, 놓기는 그 뒤 `FixedUpdate`에서 일어난다(그때 `JJBPlayer`가 다시 켜진다). 이미 지나간 입력은 JJB가 받지 못한다. 추가로 `HoldSlot`의 재잡기 쿨다운(0.2초)이 있다. 테스트 `RegrabAfterRelease_RespectsCooldownSlot`.

---

## 6. 용어집 (이 프로젝트 코드 기준)

| 용어 | 뜻 |
|---|---|
| **진자(Pendulum)** | 한 점(anchor)에 매달려 좌우로 왔다갔다하는 물체. 여기서는 `Pendulum`이 플레이어를 이 모델로 움직인다 |
| **각도 / 각속도** | `PendulumState.Angle`: 수직 아래(0)에서 얼마나 기울었는지(라디안). `AngularVelocity`: 그 각도가 초당 얼마나 변하는지(rad/s) |
| **접선 속도** | 원 궤적의 접선 방향 속도 = 길이 × 각속도(`Pendulum.cs:54–56`). 놓을 때 플레이어가 날아가는 방향과 빠르기 |
| **s (거리)** | anchor에서 플레이어까지의 거리. `PendulumState.Length`와 같다. 줄 전체 길이(`totalLength`)와 다르다 |
| **소프트 한계 / 하드 한계** | 소프트: 각도가 커질수록 되돌리는 힘이 커지는 구간(`softLimitStartDegrees`~). 하드: 절대 못 넘는 각도(`maxAngleDegrees`, `LimitAngle`) |
| **감쇠(Damping)** | 속도를 깎는 힘. `damping`은 속도에 비례, `quadraticDamping`은 속도의 제곱에 비례해 빠를수록 세게 깎는다(`Pendulum.cs:83`) |
| **Verlet 적분** | 속도를 따로 저장하지 않고 "현재 위치"와 "이전 위치"의 차로 속도를 대신하는 방식. 코드: `_points`, `_previous` |
| **거리 제약(Distance constraint)** | 이웃한 두 점이 일정 거리를 유지하도록 위치를 서로 당기고 미는 보정(`Constrain`, 206–222). 반복(`iterations`)할수록 줄이 덜 늘어난다 |
| **고정점(Pin)** | 움직이지 않고 지정된 위치에 박히는 점. 점 0(anchor)과 `_pin`(플레이어가 잡은 점) |
| **꼬리(Tail)** | 플레이어가 잡은 점 아래, 줄 끝까지의 부분 |
| **ScriptableObject** | 씬에 올리지 않는 데이터 에셋. `RopeSettings`가 그것이라, 수치를 에디터에서 바꾸고 코드는 그대로 둔다 |
| **asmdef** | 어셈블리 정의 파일. `LDY.Rope.asmdef`가 있는 폴더는 따로 컴파일되고 참조도 제한된다 |
| **DIP (의존성 역전)** | 구체 클래스 대신 인터페이스에 의존. 예: `SuspendRequestHub`는 `ISuspendSink`만 알고 `JJBControlGate`를 모른다 |
| **어댑터** | 서로 다른 모양을 연결하는 얇은 클래스. `RopeInteractable`, `SuspendRequestControl`, `PlayerControlSuspendSink` |
| **정지 요청 합산** | 여러 곳(Alt, 줄)이 "멈춰 줘"를 각자 등록·해제하고, 하나라도 있으면 멈춘 상태를 유지하는 방식 |
| **timeScale** | `Time.timeScale`. 0이면 `FixedUpdate`가 안 불린다. Alt가 이것으로 시간을 얼린다 |

---

## 7. 테스트 읽는 법 (EditMode, 총 49개)

테스트는 Unity를 실행하지 않고 순수 로직만 검증한다. 이름이 곧 보장하는 내용이다.

### 7-1. `RopeLogicTests.cs` (29개)

| 테스트 | 보장하는 것 |
|---|---|
| `Step_AdvancesOneTick` | 한 틱에 각속도와 각도가 공식대로 변한다 |
| `Step_DampingBringsPendulumToRest` | 감쇠가 있으면 진자가 결국 멈춘다 |
| `Step_SwingInputPushesInInputDirection` | 오른쪽 입력은 +방향, 왼쪽 입력은 −방향으로 민다 |
| `Step_ClimbClampsToMinLength` | 계속 올라가면 `s`가 최소 길이에서 멈춘다 |
| `Step_DescendClampsToMaxLength` | 계속 내려가면 `s`가 최대 길이에서 멈춘다 |
| `CreateState_ClampsLengthAndKeepsTangentSpeed` | 잡는 위치가 멀면 최대 길이로 잘리고 접선 속도는 유지된다 |
| `PositionOf_HangsBelowAnchorAtZeroAngle` | 각도 0이면 anchor 바로 아래 |
| `Step_MaxAngleLimitsSwingAndStopsOutwardSpeed` | 하드 한계에서 각도가 막히고 바깥쪽 속도가 0이 된다 |
| `Step_SoftLimitKeepsSwingBelowHardLimitAndSlowsGradually` | 소프트 한계가 있으면 없을 때보다 덜 올라가고 하드 한계에 안 닿는다 |
| `SoftLimitPull_GrowsWithAngleAndPointsBack` | 각도가 클수록 되돌리는 힘이 커지고 방향은 안쪽이다 |
| `Step_ResonantPushNeverExceedsNinetyDegrees` | 박자를 맞춰 계속 밀어도 90도를 넘지 않는다 |
| `Step_QuadraticDampingCapsResonantSpeed` | 제곱 감쇠가 박자 밀기 속도를 제한한다(약 11 m/s 미만) |
| `NotClimbing_ProducesNoSway` | 안 오를 땐 흔들림 0 |
| `Climbing_SwaysWithinStrengthAndChangesSign` | 오를 땐 `strength` 안에서 +/−로 왔다갔다 |
| `Climbing_RampsUpSmoothlyInsteadOfStartingAtFullStrength` | 시작 직후엔 작게, 서서히 커진다 |
| `ClimbingRopeSwingStaysSmall` | 올라가는 동안 실제 흔들림이 2~12도 사이(살짝) |
| `Step_ZeroMaxAngleMeansUnlimited` | `maxAngleDegrees`가 0이면 제한 없음 (※ 진자 테스트인데 파일상 `ClimbSwayTests` 클래스 안에 있다, `RopeLogicTests.cs:243`) |
| `AtBottom_MovingRight_ReleasesRight` | 맨 아래에서 오른쪽으로 움직이면 오른쪽으로 놓인다 |
| `AtRightSide_SwingingUp_ReleasesUpward` | 오른쪽 끝에서 위로 올라가는 중이면 위로 놓인다 |
| `JumpAddsUpwardBonusAndKeepsTangent` | Space면 위쪽 보너스가 더해지고 접선 속도는 그대로 |
| `ClosestPoint_ProjectsOntoSegment` | 점을 선분에 수직으로 투영한다 |
| `ClosestPoint_ClampsToEnds` | 선분 밖이면 끝점으로 잘린다 |
| `OnlyOneOwnerAtATime` | 슬롯은 한 소유자만 쓴다 |
| `CooldownBlocksRegrabUntilElapsed` | 놓은 뒤 쿨다운 동안은 못 잡고 지나면 잡는다 |
| `ReleaseByNonOwnerIsIgnored` | 소유자가 아닌 쪽의 해제는 무시 |
| `SingleOwner_SuspendsAndResumes` | 요청자 1명: 정지→해제가 한 번씩 |
| `AltAndRope_OverlapKeepsSuspendedUntilBothRelease` | Alt와 줄이 겹치면 둘 다 풀려야 정지 해제 |
| `DuplicateRequestAndDoubleRelease_DoNotBreakState` | 중복 요청과 이중 해제에도 상태가 안 깨진다 |
| `ReleaseWithoutRequest_DoesNothing` | 요청 없이 해제해도 아무 일 없음 |

### 7-2. `RopeCurveTests.cs` (15개)

| 테스트 | 보장하는 것 |
|---|---|
| `FreeRope_KeepsSegmentLengthAfterIterations` | 자유 상태에서 마디 길이 오차가 3% 이내 |
| `AnchorPoint_NeverMoves` | anchor 점은 절대 안 움직인다 |
| `HeldPoint_FollowsPlayerPosition` | 잡은 점이 플레이어 위치에 정확히 붙는다 |
| `HeldStill_RopeDoesNotJitter` | 가만히 매달려 있을 때 줄이 떨리지 않는다 |
| `HeldRope_UpperSectionIsStraightBetweenAnchorAndPlayer` | anchor~플레이어 구간은 곧은 직선 |
| `ClimbingUpAndDown_KeepsTailNearlyStraight` | 오르내릴 때 꼬리가 거의 꺾이지 않는다(꺾임 합 12도 미만) |
| `HeldTail_StaysWithinReasonableAngleWhileSwinging` | 흔드는 동안 꼬리 각도가 75도 미만 |
| `HeldPinIndex_StaysFixedWhileClimbing` | 오르내려도 고정된 점 번호는 안 바뀐다 |
| `LongIdle_TailHangsStraightDownAndStops` | 오래 두면 꼬리가 아래로 늘어져 멈춘다 |
| `ClimbingKeepsTotalLengthAndNeverJumpsPoints` | 오르내려도 총길이 불변, 점이 한 틱에 0.3 넘게 튀지 않는다 |
| `LengthLimits_ClampMinAndMaxToTotalLength` | 플레이어 거리 범위가 총길이×0.8 안으로 잘린다 |
| `Release_DoesNotSnapPointsInOneTick` | 놓는 순간 점들이 크게 튀지 않는다 |
| `Release_KeepsHeldPointVelocity` | 놓아도 고정됐던 점의 속도가 유지된다 |
| `AfterRelease_AmplitudeShrinksAroundSettleTime` | `settleTime` 즈음에 진폭이 눈에 띄게(≤30%) 줄고 2배 시간엔 ≤10% |
| `ReGrabAfterRelease_ContinuesFromCurrentRopeState` | 놓은 직후 다시 잡아도 줄 상태를 이어서 쓴다 |

### 7-3. `ReleaseGateTests.cs` (5개)

| 테스트 | 보장하는 것 |
|---|---|
| `PressInSameFrameAsGrab_IsIgnored` | 잡은 프레임의 놓기 입력은 무시 |
| `PressBeforeGrabFrame_IsIgnored` | 잡기 이전 프레임의 입력도 무시 |
| `PressFromNextFrame_IsAccepted` | 다음 프레임부터 허용 |
| `MinDelay_BlocksUntilElapsed` | 최소 지연 시간이 지나기 전엔 거절 |
| `RegrabAfterRelease_RespectsCooldownSlot` | 재잡기 쿨다운이 지켜진다(`HoldSlot`) |

> 테스트가 **검증하지 않는 것**: `MonoBehaviour`들(`RopeHangController`, `RopeInteractable`, `RopeColliderFollower`, `RopeLineRenderer`, `PlayerSuspendHub` 등)과 JJB·Alt와의 실제 연동, 에디터 스크립트. 이쪽은 Unity에서 직접 확인해야 한다.

---

## 8. 수정할 때 주의할 점

### 값은 `RopeSettings`에서만 바꾼다
- `RopeSettings.asset`(`Assets/LDY/Data/`)에서 조절하고 코드 안 상수는 건드리지 않는다.
- **에셋 값이 코드 기본값보다 우선한다.** 코드의 `new() {...}` 기본값(`RopeSettings.cs:7–46`)은 에셋에 필드가 아직 없을 때만 쓰인다. 기본값을 바꿔도 이미 저장된 필드는 안 바뀐다.
- 코드에 박힌 상수(SO에 없음): `MaxLengthRatio 0.8`(`RopeLengthLimits.cs:5`), `MaxHeldRatio 0.98`(`RopeCurve.cs:20`), `ClimbThreshold 0.1`(`ClimbSway.cs:13`), `ClimbPoseThreshold 0.1`(`RopeHangController.cs:15`), `DecayExponent 6`(`RopeDampingProfile.cs:15`), `MinPointCount 3`(`RopeCurve.cs:18`).
- `Curve.totalLength`는 숨겨진 필드이고 `RopeSettings.totalLength`가 덮어쓴다(`RopeSettings.cs:54–59`). 구조체만 따로 만들면 총길이를 직접 채워야 한다.

### 건드리면 어디가 깨지나
| 건드리는 곳 | 깨질 수 있는 곳 |
|---|---|
| `SuspendRequestHub` 규칙(0↔1일 때만 알림) | Alt 정지/복귀, 매달림 중 Alt 사용. 테스트 `AltAndRope_OverlapKeepsSuspendedUntilBothRelease` |
| `AltModeController.cs:30`의 `SuspendRequestControl` 연결 | Alt가 다시 `JJBControlGate`를 직접 부르면 매달림이 Alt 복귀 때 풀린다 |
| `RopeHangController.FixedUpdate`를 `Update`로 옮기기 | Alt 중 `timeScale 0`에서 줄이 안 멈춘다 |
| `Release`의 순서(정지 해제 → 속도 적용, 135–147) | 속도를 먼저 주면 `JJBPlayer`가 꺼진 상태라 `JJBPlayerMovement`가 덮어쓰기 전에 값이 지워질 수 있다 **[추측]** |
| `_pose?.Report(...)` (`RopeHangController.cs:132,144,160`) | `PlayerPoseRelay`→`PlayerRenderer`의 Grab 애니메이션 표시. 안 부르면 매달린 포즈가 안 나온다 **[코드: PlayerPoseRelay.cs:4–16]** |
| `ReleaseGate`/`RopeInput`의 프레임 기록 방식 | 잡자마자 놓아지는 이중 처리 재발 |
| `RopeColliderFollower`/레이어 | JJB가 줄을 못 찾는다. 줄 오브젝트 레이어가 `JJBPlayerInteract.objectLayer`에 포함돼야 한다 |
| `RopeCurve.SetPin`(점 번호 고정) | 오르내릴 때 줄이 튄다. 테스트 `HeldPinIndex_StaysFixedWhileClimbing` |
| `RopeLengthLimits` 비율 | 꼬리가 사라지거나 `heldDistance` 상한(`MaxHeldRatio`)과 어긋나 줄이 늘어난다 |
| `LDY.Rope.asmdef`에 참조 추가 | 순수 로직 경계가 무너져 EditMode 테스트가 Unity 타입에 묶인다 |
| JJB 코드(`JJBPlayerInteract`, `JJBControlGate` 등) | 팀원 영역. 이 문서 기준으로는 읽기만 한다 |

---

## 9. 이해 확인 퀴즈

### A. 개념 문제 (코드를 보지 않고)

**Q1. (객관식)** Alt를 누르면 줄이 멈추는 이유로 가장 알맞은 것은?
① 줄 코드가 Alt 키를 감지해 직접 멈춘다 ② `Time.timeScale`이 0이 되어 `FixedUpdate`가 안 불린다 ③ 줄 오브젝트가 비활성화된다 ④ 입력 잠금이 걸려서

**Q2. (단답)** `SuspendRequestHub`가 해결하는 문제를 한 문장으로 쓰세요.

**Q3. (객관식)** W를 눌러 올라갈 때 바뀌는 것은?
① 줄 전체 길이 ② 플레이어가 anchor로부터 떨어진 거리 `s` ③ 줄 마디(점) 개수 ④ 중력

**Q4. (단답)** Verlet 방식에서 한 점의 "속도"는 어떻게 저장·표현되나요?

**Q5. (객관식)** 놓은 뒤 줄이 천천히 가라앉는 방식으로 맞는 것은?
① 놓는 순간 줄 점 속도를 0으로 만들고 원래 자리로 되돌린다 ② 속도를 유지하고 감쇠를 시간에 따라 서서히 키운다 ③ 줄을 삭제하고 새로 만든다 ④ 중력을 0으로 만든다

**Q6. (단답)** 잡는 순간 같은 F 입력이 놓기로 즉시 처리되는 것을 막는 클래스와 그 방법을 쓰세요.

### B. 코드를 보고 답하는 문제

**Q7.** `RopeHangController.cs:106–118`을 보세요. 놓기 입력이 처리된 **바로 그 FixedUpdate**에서 `Hang`이 불리나요? 불린다면/아니라면 그 틱에 줄은 어떤 메서드로 갱신되나요?

**Q8.** `SuspendRequestHub.cs:18–32`와 `PlayerSuspendAdapters.cs:25–30`을 보세요. 줄과 Alt가 모두 요청 중일 때 Alt가 `SetSuspended(false)`를 부르면 `JJBControlGate.SetSuspended`가 불리나요? 이유는?

**Q9.** `Pendulum.cs:73–74`를 보세요. `omega = state.AngularVelocity * state.Length / length`는 W를 눌러 `length`가 줄 때 무엇을 보존하려는 식인가요? (보존되는 양을 쓰세요)

**Q10.** `RopeCurve.cs:147–161`을 보세요. 점이 20개(마디 19개)이고 `_split = 6`, `_heldDistance = 3`, 총길이 6, `Weight`(`_blend`) = 1일 때 **윗부분 마디 하나**와 **꼬리 마디 하나**의 목표 길이를 각각 계산하세요.

---

### 정답과 해설

**Q1. ②.** Alt는 `TimeScaleFreezer.Freeze`로 `timeScale`을 0으로 만든다(`TimeScaleFreezer.cs:14`). `FixedUpdate`는 timeScale 0에서 호출되지 않으니 줄과 진자가 자동으로 멈춘다. ④는 오답이다. 밧줄은 입력 잠금 토큰을 쓰지 않는다(잠금이 있으면 Alt가 오히려 막힌다, `AltKeyInput.cs:18`).

**Q2.** "여러 곳(Alt, 줄)이 각자 정지를 요청하고 해제해도, 요청자가 하나라도 남아 있으면 정지 상태를 유지하게 한다." 마지막 요청자가 빠질 때만 실제 해제가 일어난다.

**Q3. ②.** `Pendulum.Step`의 `length`(73행)가 `s`이고 `PendulumState.Length`와 같다. 줄 전체 길이(`totalLength`)는 W/S로 바뀌지 않는다. 대신 마디별 목표 길이만 `FillRest`로 재배분된다.

**Q4.** "현재 위치 − 이전 위치"가 곧 속도다. `RopeCurve`의 `_points`(현재)와 `_previous`(이전)가 한 쌍이고 `VelocityOf`(67–69)가 그 차를 돌려준다. 그래서 위치를 덮어쓰면 속도도 같이 바뀐다.

**Q5. ②.** 놓을 때 `ClearPin`만 하고 점의 위치/이전 위치는 건드리지 않아 속도가 유지된다. 감쇠는 `RopeDampingProfile.AfterRelease`가 smoothstep 보간으로 서서히 키운다.

**Q6.** `ReleaseGate`. 잡을 때 `MarkGrab(프레임, 시각)`으로 잡은 프레임을 저장하고, 놓기 입력(`RopeInput`이 눌린 프레임 번호를 기록)이 그 프레임**보다 큰** 경우에만 `Accepts`가 `true`를 준다. 같은 프레임이면 거절된다. 최소 지연(`releaseInputDelay`)도 함께 본다.

**Q7.** 아니다. 놓기가 통과하면 `Release(jumped)`가 `_hanging = false`로 만들고(137행), 이어서 `if (_hanging)`(114행)이 거짓이라 `Hang`은 안 불린다. 그 틱의 줄은 `else` 쪽 `_rope.Step(Anchor, deltaTime)`(117행, 자유 상태)로 갱신된다.

**Q8.** 불리지 않는다. `SuspendRequestControl.SetSuspended(false)`가 `_hub.Release(Alt)`를 부르지만(`PlayerSuspendAdapters.cs:29`), 허브는 소유자를 제거한 뒤 `_owners.Count == 0`일 때만 `sink.SetSuspended(false)`를 부른다(`SuspendRequestHub.cs:30–31`). 줄이 아직 소유자로 남아 있어 카운트가 1이라 호출되지 않는다.

**Q9.** 접선 속도(= 길이 × 각속도). `length`가 줄면 같은 접선 속도를 유지하려고 각속도를 `이전 길이 ÷ 새 길이` 배로 키운다. 즉 "올라가도 몸이 가속/감속하지 않는" 접선 속도 보존식이다. (엄밀한 물리의 각운동량 보존과는 다르다. 이건 코드에서 읽은 식의 의미이고, 물리 모델 선택 이유는 **[의도, 확인 필요]**.)

**Q10.** `upperCount = _split = 6`, `tailCount = _rest.Length − upperCount = 19 − 6 = 13`. 윗부분: `_heldDistance / upperCount = 3 / 6 = 0.5`. 꼬리: `(TotalLength − _heldDistance) / tailCount = (6 − 3) / 13 ≈ 0.231`. `Weight = 1`이면 `Lerp(SegmentLength, held, 1) = held`라 그대로다. 합은 `0.5×6 + 0.231×13 = 3 + 3 = 6`으로 총길이와 같다(`CurrentLength`).

---

## 부록: "확인 필요"로 남긴 것 요약

1. `RopeHangController.FixedUpdate`와 `RopeColliderFollower.FixedUpdate`의 실행 순서(콜라이더가 한 틱 늦는지).
2. Alt 복귀 직후 플레이어 위치가 줄에 맞춰질 때 화면에서 한 틱 튀는지.
3. 놓은 뒤 `JJBPlayerMovement.Move`(`rb.linearVelocityX = 입력 × 속도`, `JJBPlayerMovement.cs:14–16`)가 가로 접선 속도를 곧바로 덮어쓰는지(`JJBPlayer.FixedUpdate`와 줄 `FixedUpdate`의 순서에 달림).
4. 물리 모델의 "접선 속도 보존" 선택 이유(Q9)는 설계 의도라 코드에는 주석이 없다.
5. 실제 Unity 실행 결과 전반(이 문서는 코드를 읽고 쓴 것이며, 플레이 모드 동작은 직접 확인하지 않았다).
