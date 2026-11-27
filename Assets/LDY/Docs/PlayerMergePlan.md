# 플레이어 프리팹 통합 조사 및 계획

- 작성일: 2026-11-26 / 기준 커밋: `9195db1` (LDY 브랜치)
- 범위: 조사와 계획만 담았다. 이 문서를 쓰면서 프리팹, 씬, 코드, 에셋은 하나도 고치지 않았다.
- 표기: **A** = 지빈 `Assets/JJB/Prefab/Player.prefab`, **B** = 태영 `Assets/Main/02.Prefabs/Player/Player.prefab`, **B'** = 태영 `PlayerCopy.prefab`, **C** = 공용(LDY) `Assets/prefab/Player.prefab`

---

## 0. 요약

### 핵심 발견

1. **세 프리팹 사이에 Variant 관계는 없다.** 모두 독립 프리팹이다(`m_SourcePrefab` 없음).
   - **B'(PlayerCopy)는 C를 파일째 복사한 것**이다. 내용은 C와 같고, fileID만 새로 생성됐으며 루트 이름(`PlayerCopy`)과 루트 위치만 다르다.
   - **B는 C의 구성에 HTY 스킬 묶음을 더한 것**이다(Skills 자식, SkillManager, Light, FixedJoint 등).
2. **이동, 점프, 상호작용은 세 프리팹 모두 같은 JJB 스크립트(같은 GUID)를 쓴다.** 차이는 직렬화 값과 자식 위치뿐이다. 코드를 합칠 필요는 없고, 값과 구성만 하나로 정하면 된다.
3. **A는 11-23 커밋(`35ee473`)에서 루트 스케일이 2로 바뀌었다.** 콜라이더(0.436×0.536)와 자식 위치도 이 스케일을 기준으로 다시 잡혔다.
   - 반면 프리팹 기본값인 `GroundCheck`(-0.68)와 `Visual`(-0.55)은 예전 값 그대로다. `JJBLevel` 씬 오버라이드(-0.065 / 0)만 맞는 값이다.
   - **그래서 오버라이드가 없는 `Stage1_ArtTest`에서는 A의 발 판정과 그림이 몸에서 1u 이상 아래에 있다.**
4. **LDY의 기존 `LDY/Player/Merge Into One` 메뉴는 A가 스케일 1이던 시절을 기준으로 짜여 있다.** 지금 그대로 실행하면 다음과 같이 깨진다.
   - A의 로컬 콜라이더 값이 스케일 1인 C에 복사돼 콜라이더가 절반 크기가 된다.
   - `GroundCheck`가 -0.68로 옮겨진다.
   - **도구를 고치기 전에는 실행하면 안 된다.**
5. **기능 버그**
   - **흡수(E)**: B, C에는 `JJBPlayerAbsorb`가 없다. 그래서 E를 누르면 `JJBPlayer.HandleAbsorbPressed`에서 NullReferenceException이 난다. A에는 Absorb가 있지만 필수 짝인 `JJBPlayerGimmick`이 프리팹에 없어서 아무 일도 일어나지 않는다. **흡수는 어느 프리팹에서도 동작하지 않는다.**
   - **상호작용(F)**: B, C의 `objectLayer`가 0이라 F 상호작용이 안 된다. 이 때문에 로프도 잡을 수 없다. `RopeTest`만 씬 오버라이드(512)로 살려 두었다.
   - **회전 고정**: B, C의 Rigidbody2D `Constraints`가 0이라 캡슐이 넘어질 수 있다. A는 4(회전 고정)다.
6. **애니메이션**
   - `player_anim_cursor`(52프레임, 16태그)는 `cursor_character_anim`(16프레임, 5태그)의 **완전한 상위집합**이다. 겹치는 프레임은 픽셀 경계까지 같다.
   - 피벗은 둘 다 캔버스 하단 중앙이다. 차이는 PPU(100 대 40)뿐이다.
   - **A의 Animator를 움직이는 코드는 없다.** 파라미터가 0개이고, `JJBPlayerRenderer.FilpX` 호출은 주석 처리돼 있다. 그래서 A는 idle 애니메이션만 반복하고 좌우로 뒤집히지 않는다.
7. **크기 기준이 둘이다.**
   - A: PPU 100 × 스케일 2 = 실효 PPU 50. 캐릭터 높이 **1.02u**.
   - C: PPU 40. 캐릭터 높이 **1.275u**(25% 더 큼).
   - `JJBLevel`(빌드 씬) 레벨은 1.02u를 기준으로 맞춰져 있다. **어느 PPU로 통일할지 먼저 정해야 한다(§9의 결정 1).**
8. **PlayerCopy 관련**
   - 태그가 `Player`이고 `PlayerInput`이 켜져 있다. 그래서 본체와 같은 키 입력을 받는다. 점프는 `SetPlay(false)`로 막히지 않으므로 복사본이 본체와 같이 점프한다.
   - `PrimaryPlayerLocator`는 이름 규칙(`Copy`/`Clone`)으로 본체를 정상적으로 고른다.
   - 다만 `StageGoal`, `StageDeathZone`, HES 기믹은 복사본도 플레이어로 인정한다. 복사본이 골에 닿으면 스테이지가 클리어된다.
9. **참조 수**
   - C의 GUID를 최종 파일로 유지하면 **바꿀 씬은 4개**다(JJBLevel, JJBScene, Stage1_ArtTest, Main_HTY).
   - A를 유지해도 4개지만, LDY 코드 상수 변경이 더 많다.
   - B를 유지하면 6개다.

### 추천

**안 1 변형(§7-1)을 추천한다.**
- 파일과 GUID는 **C(`65b933f5…`)를 최종본으로 유지**한다.
- 게임플레이 값은 **A 기준**으로 맞춘다.
- 비주얼은 **C의 PinkVisual(player_anim_cursor)** 를 쓴다.
- HTY 스킬은 **B의 Skills 자식을 옮겨 온다.**
- PlayerCopy는 **최종본의 Variant**로 새로 만든다.

C의 GUID를 쓰는 씬 3개는 손대지 않아도 되고, 기존 LDY 병합 도구도 대부분 다시 쓸 수 있다.

---

## 1. 대상과 관계

| | 경로 | GUID | 마지막 수정 | 담당 | Variant 여부 |
|---|---|---|---|---|---|
| A | `Assets/JJB/Prefab/Player.prefab` | `49ba53f6c4983bd4a816ae3f76cd321b` | `35ee473` press101-spare 11-23 | 지빈 | 독립 |
| B | `Assets/Main/02.Prefabs/Player/Player.prefab` | `094dd1f1fae102a499bf2c1dea40bd1c` | `035bc7d` hanty100209-cpu 11-24 | 태영 | 독립 (C의 구성 + HTY) |
| B' | `Assets/Main/02.Prefabs/Player/PlayerCopy.prefab` | `a915d2995e793da4e8ed90059226964e` | `f2dd373` 11-23 | 태영 | 독립 (C의 파일 복사본) |
| C | `Assets/prefab/Player.prefab` | `65b933f5f82ec99458c601f66f4f7bd2` | Leedoyun | LDY | 독립 |

- B'를 C와 diff하면 차이는 세 가지뿐이다. 모든 fileID가 다르고, 루트 이름이 `PlayerCopy`이며, 루트 위치가 (1.18, -0.46)이다. 컴포넌트와 값은 모두 같다.
- 참고: `Assets/LDY/Debug/player_prefab_diff.md`(11-23)는 A의 스케일이 바뀌기 전에 쓴 보고서라 **A 수치가 지금과 다르다.** 이 문서가 그 보고서를 대체한다.

---

## 2. 계층 비교

```
A (JJB)                         C (공용) = B'(PlayerCopy)        B (Main/HTY)
Player  [scale 2]               Player  [scale 1]                Player  [scale 1, pos (1.23,-0.12)]
├─ Visual (-0.55)               ├─ Visual (0)                    ├─ Visual (0)
├─ GroundCheck (-0.68)          ├─ GroundCheck (-0.27)           ├─ GroundCheck (-0.27)
├─ InteractPoint (0.25)         ├─ PinkVisual (-0.305)           ├─ PinkVisual (-0.305)
└─ AbsorbPoint (0.25)           │  └─ GripPivot                  │  └─ GripPivot
                                │     └─ Sprite                  │     └─ Sprite
                                └─ InteractPoint (0)             ├─ InteractPoint (0)
                                                                 ├─ Player Light (0.367)  [layer 0]
                                                                 └─ Skills (0.376)        [layer 0]
```
(괄호 안 숫자는 localPosition.y. 별도 표시가 없으면 모든 오브젝트가 layer 3 `Player`이고, 루트만 tag `Player`다.)

---

## 3. 컴포넌트와 직렬화 값 비교

### 3-1. 루트

| 컴포넌트 / 필드 | A | C (=B') | B | 비고 |
|---|---|---|---|---|
| Transform.scale | **(2,2,1)** | (1,1,1) | (1,1,1) | A만 2 |
| Rigidbody2D.Constraints | **4 (회전 고정)** | 0 | 0 | B, C는 회전이 풀려 있음 ⚠ |
| Rigidbody2D 나머지 | 질량 1, 중력 1, 재질 `JJBPM2D` | 같음 | 같음 | |
| CapsuleCollider2D.offset | (-0.0045, 0.2637) | (0, 0) | (0, 0) | |
| CapsuleCollider2D.size | (0.436, 0.536) → **월드 0.872×1.072** | (0.74, 0.61) | (0.74, 0.61) | §5-2 참고 |
| JJBPlayer | 있음 | 있음 | 있음 | 같은 스크립트 |
| JJBPlayerInput | 있음 | 있음 | 있음 | |
| JJBPlayerMovement.moveSpeed | 5 | 5 | 5 | |
| JJBPlayerJump.jumpSpeed | **11** | 16 | 16 | 차이 큼 |
| JJBPlayerJump.maxJumpHoldTime | 0.5 | 0.5 | 0.5 | |
| JJBPlayerJump.groundCheckSize | **(0.5, 0.25)** | (1, 0.25) | (1, 0.25) | C/B는 콜라이더 폭(0.74)보다 넓어 벽 옆을 바닥으로 볼 수 있음 |
| JJBPlayerJump.groundLayer | 64 (Ground) | 64 | 64 | |
| JJBPlayerJump.extraGravity / gravityDelay | 30 / 0.15 | 같음 | 같음 | |
| JJBPlayerInteract.objectLayer | **512 (Object)** | **0 ⚠** | **0 ⚠** | B, C는 F가 아무것도 잡지 못함 |
| JJBPlayerInteract.interactSize | 1 | 1 | 1 | |
| JJBPlayerAbsorb | 있음 (absorbLayer 1024 Gimmick, size 1) | **없음** | **없음** | B, C는 E를 누르면 NRE |
| JJBPlayerGimmick | **없음** | 없음 | 없음 | Absorb의 필수 짝. 모든 프리팹에 없음 |
| PlayerInput | `JJBInput.inputactions`, map `Player` | 같음 | 같음 | |
| FixedJoint2D | - | - | 있음 (꺼짐) | HTY DragSkill용 |
| SkillManager_HTY | - | - | `_skillSoList`=SkillListSo_HTY, `_utilityCompo`=Utility_HTY | |
| TargetSelector | - | - | `_detectRange` 5, `_targetLayer` 비어 있음 | 다른 코드에서 쓰지 않음 |
| Utility_HTY | - | - | `_player`=루트, `_pointerPrefab`=Pointer_HTY | |

### 3-2. 자식

| 자식 / 컴포넌트 | A | C (=B') | B |
|---|---|---|---|
| Visual.SpriteRenderer | **켜짐**, cursor_character_anim 프레임, Order 0 | 꺼짐, KBG `Apple.png`(임시) | 꺼짐, Apple.png |
| Visual.Animator | **켜짐**, cursor_character_anim 컨트롤러 | 꺼짐, `player_anim (2).aseprite` 컨트롤러 | 꺼짐, 같음 |
| Visual.JJBPlayerRenderer | 있음 | 있음 | 있음 |
| PinkVisual.PlayerRenderer | - | animSet `PlayerAnimSet_Cursor`, target=Sprite, `flipSprite` **0**, `resolveSourcesOnAwake` 0 | 같음 |
| PinkVisual.StagePlayerAnimBootstrap | - | 있음 | 있음 |
| PinkVisual.PlayerGripRig | - | profile `RopeGripProfile`, settings `RopeSettings` | 같음 |
| Sprite.SpriteRenderer | - | player_anim_cursor 프레임, Order **0** | 같음 |
| Skills (9개 컴포넌트) | - | - | CopyPlayer(→B'), CursorLighting, CuttingSkill(Cut 20), DragSkill(Drag 24), OpacityPlayer(Opacity 21), ReScaleObjSkill(Rescale 22), StopObjSkill(Stop 23), TpPlayer |
| Player Light (Light2D) | - | - | 꺼짐, 반경 4 |

- SpriteRenderer 머티리얼 `a97c1056…`은 Assets 안에 없다. 패키지 기본 머티리얼(URP Sprite-Lit-Default로 추정)이고, 세 프리팹이 모두 같은 것을 쓴다.
- `JJBControlGate`, `PlayerInputLock`, `PlayerAbilityGate`, `AltModeController`는 **세 프리팹 모두에 없다**. 아래 컴포넌트들은 런타임에 붙는다.
  - `JJBPlayerMotionSource`와 `PlayerPoseRelay`: `StagePlayerAnimBootstrap`이 붙인다.
  - `PlayerInputLock`: RequireComponent 때문에 자동으로 붙는다.
  - `PlayerSuspendHub`, `PlayerRopeSlot`, `JJBControlGate`: 로프가 처음 쓰일 때 붙는다.
  - `PlayerDeath`: `PlayerDeathInstaller`가 씬을 로드할 때 붙인다.

---

## 4. 기능별 담당

| 기능 | 담당 컴포넌트 (위치) | A | B | C | 겹침 / 충돌 |
|---|---|---|---|---|---|
| 이동 | `JJBPlayerMovement` (루트) | ✓ | ✓ | ✓ | 조작을 끄는 방법이 둘이다. HTY는 `SetPlay(false)` + `Rb.simulated`를 쓰고, LDY는 `JJBControlGate`가 `JJBPlayer.enabled`를 끈다. 동시에 걸리면 서로 모르고 풀어 버릴 수 있다 |
| 점프 | `JJBPlayerJump` (루트) | ✓ jump 11 | ✓ 16 | ✓ 16 | Space 키를 `RopeInput`(RopeJump)도 쓴다. 로프에 매달린 동안에는 JJBControlGate가 JJBPlayer를 꺼서 충돌하지 않는다 |
| 상호작용 F | `JJBPlayerInteract` + InteractPoint | ✓ 512 | ✗ 0 | ✗ 0 | 로프 잡기가 F 상호작용(`RopeInteractable : JJBInteractable`)을 거친다. 로프 놓기도 F(`RopeInput`)다 |
| 흡수 E | `JJBPlayerAbsorb` + AbsorbPoint (+`JJBPlayerGimmick` 필요) | △ Gimmick이 없어 동작 안 함 | ✗ NRE | ✗ NRE | |
| 스킬 (HTY) | Skills 자식 + 루트의 SkillManager_HTY / Utility_HTY / TargetSelector / FixedJoint2D | - | ✓ | - | Y/R/Q+Shift 키를 `Keyboard.current`로 직접 읽는다. LDY의 `PlayerInputLock`(사망, Alt, 컷씬 잠금)을 거치지 않는다. R은 `StageManager_HTY`(재시작 추정)도 쓴다 |
| 로프 | 루트에 런타임으로 붙는 `PlayerSuspendHub`/`PlayerRopeSlot`/`JJBControlGate`, PinkVisual의 `PlayerGripRig` | - | ✓ (PinkVisual) | ✓ | RopeHangController가 `PrimaryPlayerLocator`로 본체를 찾는다 |
| Alt | `AltModeController` (+`PlayerAbilityGate`) | - | - | - (타이틀용 `AltPlayer.prefab`에만 있음) | `playerSprite`를 연결하지 않으면 `GetComponentInChildren<SpriteRenderer>()`가 **꺼진 Visual의 SpriteRenderer**를 잡는다. `StagePlayerAnimBootstrap`은 PlayerRenderer에 alt로 `null`을 넘긴다 |
| 사망 | `PlayerDeath` (런타임 설치), `PlayerKill.Request` | 런타임 | 런타임 | 런타임 | HES `KillHitbox`는 `Destroy(other.gameObject)`로 플레이어를 바로 파괴해 LDY 사망 연출을 건너뛴다 |
| 비주얼 / 애니메이션 | A: Visual(Animator). B, C: PinkVisual(PlayerRenderer, 코드 구동) | Animator | PlayerRenderer | PlayerRenderer | B, C의 Visual은 꺼진 채 남아 있다. `JJBPlayer.PlayerRenderer`가 Visual의 `JJBPlayerRenderer`를 참조하므로 지우면 안 되고, 꺼 두는 방식을 유지해야 한다 |
| 좌우 반전 | A: 없음(FilpX 주석). C: `PlayerRenderer.flipSprite`=0 | ✗ | ✗? | ✗? | B, C도 프리팹 값으로는 반전이 꺼져 있다. 의도인지 확인 필요 |

---

## 5. 비주얼

### 5-1. 애니메이션 에셋 2종 (.aseprite 바이너리와 .meta를 직접 읽은 결과)

| 항목 | `cursor_character_anim.aseprite` | `player_anim_cursor.aseprite` |
|---|---|---|
| GUID | `e4c5d2c44274d3146a2266125daf90e3` | `91c8d95151a21924b8afb82002d35d6b` |
| 쓰는 곳 | A의 Visual (SpriteRenderer + Animator 컨트롤러) | C, B, B'의 PinkVisual/Sprite, `PlayerAnimSet_Cursor.asset`(16개 clip) |
| 캔버스 | 64×64, 레이어 `art` 1개 | 64×64, 레이어 `art` 1개 |
| 프레임 수 | 16 | 52 |
| 태그(프레임 수) | idle 4, walk 6, jump 2, fall 2, land 2 | idle 4, walk 6, **run 6**, **sit_down 3, sit_loop 1, stand_up 3**, jump 2, fall 2, **grab 1, rope_hang 2, rope_climb 4, rope_climb_down 4, push 4**, land 2, **trapped 5, death 3** |
| PPU | **100** | **40** |
| 피벗 | `defaultPivotAlignment 7`(하단 중앙), `defaultPivotSpace 0`(캔버스 기준) | 같음 |
| 발 위치 | 불투명 최하단 행 = 캔버스 바닥에서 1px 위 (idle) | 같음 |
| 캐릭터 크기 (idle 불투명 영역) | 42×51px | 42×51px |
| 그 밖의 임포트 설정 | 같음 | 같음 (meta는 GUID와 PPU 두 줄만 다름) |

- 프레임 대응:
  - cursor 0–9(idle, walk) = player 0–9
  - cursor 10–13(jump, fall) = player 23–26
  - cursor 14–15(land) = player 42–43
  - 각 프레임의 불투명 경계와 duration이 모두 같다. **`cursor_character_anim`을 버려도 잃는 그림은 없다.**
- 주의: `CursorPpuStep`(Merge Into One의 선행 단계)은 `player_anim_cursor`의 PPU를 `PlayerArt.PixelsPerUnit = 40`으로 강제한다.

### 5-2. 콜라이더와 발 위치

모두 루트 기준 월드 단위로 환산했다.

| | A (프리팹 기본값) | A (`JJBLevel` 오버라이드) | C / B |
|---|---|---|---|
| 캐릭터 높이 | 51px / 100 × 2 = **1.02u** | 1.02u | 51 / 40 = **1.275u** |
| 콜라이더 | 0.872 × 1.072, 바닥 **-0.009** | 같음 | 0.74 × 0.61, 바닥 **-0.305** |
| 그림 바닥(발) | Visual -0.55 × 2 + 1px = **-1.08** ⚠ | Visual 0 → **+0.02** ✓ | PinkVisual -0.305 + 1px = **-0.28** ✓ |
| 발과 콜라이더 바닥의 차이 | 1.07u 어긋남 | 약 1px | 약 1px |
| GroundCheck 상자 (y 범위) | -0.68 × 2 = -1.36 → **-1.49 ~ -1.24** ⚠ | -0.065 × 2 → -0.26 ~ -0.01 ✓ | -0.27 → -0.40 ~ -0.15 ✓ (폭 1.0 > 콜라이더 0.74) |
| 콜라이더와 그림 크기 비교 | 높이 1.07 / 1.02, 폭 0.87 / 0.84 ✓ | ✓ | **높이 0.61 / 1.275**: 콜라이더가 그림의 아래 절반만 덮는다. 폭 0.74 / 1.05 |

**정리**
- A는 `JJBLevel` 오버라이드가 있을 때만 맞는다. `Stage1_ArtTest`는 오버라이드가 없어 발 판정이 몸에서 1.2u 아래에 있다. 그래서 공중에서도 점프할 수 있고, 그림은 땅 아래로 묻힌다.
- C/B는 발은 맞지만 콜라이더가 너무 작다. 머리가 천장과 벽을 통과한다.
- `RopeTest`는 C에 스케일 2 오버라이드를 걸어서 쓰고 있다(캐릭터 2.55u).

---

## 6. 참조 목록 (GUID 기준)

### 6-1. 씬 (프리팹 인스턴스)

| 씬 | 빌드 포함 | 참조 프리팹 | 인스턴스 오버라이드 |
|---|---|---|---|
| `Assets/JJB/JJBLevel.unity` | ✓ | A | 위치, 이름, **GroundCheck.y -0.065, Visual.y 0** |
| `Assets/JJB/JJBScene.unity` | | A | 위치, 이름, Visual.m_Sprite (다른 스프라이트 `6ad3a2db…`) |
| `Assets/LDY/Scenes/Stage1_ArtTest.unity` | | A | 위치 (0, 2), 이름 |
| `Assets/HTY/00.Scenes/Main_HTY.unity` | | B | 위치, 이름 `Player (1)`, 지금은 없는 컴포넌트(`7642081104254461444`)를 가리키는 RemovedComponents가 남아 있음 |
| `Assets/HES/Scene/Test.unity` | | C | 위치, 이름 |
| `Assets/LDY/Scenes/RopeTest.unity` | ✓ | C | 위치, 이름, **objectLayer 512, SortingOrder 6, 스케일 2** |
| `Assets/LDY/Scenes/Stage2_SpiderTest.unity` | | C | 위치, 이름, 스케일 1 |
| `Assets/_Recovery/0 (5).unity` | | C | RopeTest와 같음 (복구 파일이라 무시 가능) |

- 씬 안의 다른 오브젝트가 플레이어 인스턴스 내부(stripped 오브젝트)를 직렬화해서 참조하는 경우는 **0건**이다. 카메라와 로프 등은 런타임에 `PrimaryPlayerLocator`로 찾는다.
- 빌드 씬은 `LDY_TitleScene`, `StageSelect`, `JJBLevel`, `RopeTest` 4개다. 플레이어가 놓인 빌드 씬은 JJBLevel(A)과 RopeTest(C)다.

### 6-2. 프리팹과 SO

| 참조하는 곳 | 대상 |
|---|---|
| B의 `Skills/CopyPlayer_HTY._playerPrefab` | B' (`a915d299…`, 루트 fileID `8283665974918895059`) |
| ScriptableObject | 플레이어 프리팹을 참조하는 SO는 없음 |

### 6-3. 코드 (경로 문자열)

| 파일 | 내용 |
|---|---|
| `Assets/LDY/Script/PlayerMerge/Logic/PlayerPrefabPaths.cs` | `Shared` = C, `Jibin` = A |
| `Assets/LDY/Editor/Flow/FlowPaths.cs` | `SharedPlayer` = C (`CreateStageSceneWindow`가 사용), `KnownPlayers` = C, B, A (`StageSceneValidator`가 사용) |
| `Assets/LDY/Editor/TitlePlayer/TitleTuningSync.cs` | `JibinPlayer` = A (타이틀 플레이어 튜닝값의 원본) |
| `Assets/LDY/Editor/PlayerMerge/SceneInstanceReplacer.cs` | 교체 대상 씬이 `Stage1_ArtTest`, `Stage2_SpiderTest`로 고정돼 있음. JJBLevel, JJBScene, Main_HTY가 빠져 있음 |

### 6-4. 최종본을 무엇으로 하느냐에 따른 변경 수

| 최종 GUID | 바꿔야 할 씬 | 바꿔야 할 코드 상수 | 기타 |
|---|---|---|---|
| **C 유지** | **4** (JJBLevel, JJBScene, Stage1_ArtTest, Main_HTY) | `TitleTuningSync.JibinPlayer`, `FlowPaths.KnownPlayers`(정리), `PlayerPrefabPaths.Jibin`(병합이 끝나면 불필요), `SceneInstanceReplacer` 목록 | CopyPlayer 필드 1곳 |
| A 유지 | 4 (Test, RopeTest, Stage2, Main_HTY) | 위의 항목 + `PlayerPrefabPaths.Shared`, `FlowPaths.SharedPlayer`. 병합 도구가 C에 쓰도록 짜여 있어 방향을 뒤집어야 함 | CopyPlayer 1곳, LDY 도구 대부분 수정 |
| B 유지 | 6 | 위의 항목 모두 | |

---

## 7. 태그 / 레이어 / PlayerCopy

- 레이어 번호: 3 Player, 6 Ground, 9 Object, 10 Gimmick, 20 Cut, 21 Opacity, 22 Rescale, 23 Stop, 24 Drag. 세 프리팹의 레이어 마스크 값은 모두 이 이름과 맞는다(0으로 비어 있는 objectLayer만 예외).
- B'(PlayerCopy)는 루트 tag가 `Player`, layer가 3이다. 런타임 이름은 `PlayerCopy(Clone)`이 된다.

**PrimaryPlayerLocator에 미치는 영향**
- 본체와 복사본 모두 `PlayerInput`이 켜져 있어서 입력 규칙으로는 둘 다 후보로 남는다.
- 다음 단계인 이름 규칙이 `Copy`/`Clone`을 걸러 내므로 **본체를 정상적으로 고른다**(Reason=Name, 경고 없음). `PrimaryPlayerRuleTests`에도 이 경우가 들어 있다.
- 주의할 점:
  - 최종 프리팹이나 씬 인스턴스 이름에 `Copy`나 `Clone`이 들어가면 본체가 걸러진다.
  - `Main_HTY`의 `Player (1)` 같은 이름은 괜찮다.

**PlayerCopy가 실제로 일으키는 문제**
1. 같은 키 입력을 받는다. `SetPlay(false)`는 이동만 막으므로 **복사본도 Space로 같이 점프**한다. F와 E 입력도 함께 받는다.
2. `PlayerMatch`(태그 또는 레이어 비교)를 쓰는 `StageGoal`과 `StageDeathZone`이 복사본도 플레이어로 본다.
   - **복사본이 골에 닿으면 스테이지가 클리어된다.**
   - 데스존에 닿으면 `PlayerKill`이 복사본을 파괴한다(PlayerDeath가 없으므로).
3. 복사본이 파괴되면 `CopyPlayer_HTY._copyObj`가 파괴된 오브젝트를 가리키게 된다. 그 상태에서 Paste를 하면, 본체가 `Rb.simulated=false`로 멈춰 있을 수 있다.
4. HES의 `ButtonParent`, `FallingBlock`, `SwitchElevator`, JJB의 `Gear`도 `CompareTag("Player")`로 복사본에 반응한다. 버튼을 누르게 하는 것이 복사 스킬의 의도라면 이건 문제가 아니다.
5. 최종 프리팹에 Alt나 사망 컴포넌트를 구워 넣으면 Variant인 복사본도 그대로 물려받는다. **복사본 Variant에서는 반드시 끄거나 제거해야 한다.**

**태그 처리 제안**
- 복사본은 tag를 `Untagged`(또는 새 태그 `PlayerCopy`)로 바꾸고, layer는 3을 유지한다.
- 복사본도 버튼을 눌러야 한다면 HES 버튼 쪽이 레이어나 `PlayerMatch`로 판정하게 바꾼다. 골과 데스존은 `PlayerMatch.playerLayer`를 비워서 태그로만 판정하게 한다.
- 태그를 유지하는 쪽을 고르면, 대신 `StageGoal`에 본체인지 확인하는 단계(`PrimaryPlayerLocator`)를 더한다.

---

## 8. 통합 계획

### 안 1. 파일 하나 (추천: 파일은 C, 값은 A, 비주얼은 C, 스킬은 B)

**결과**
- `Assets/prefab/Player.prefab`(GUID `65b933f5…` 유지) 한 개가 모든 씬의 플레이어가 된다.
- `PlayerCopy`는 이 파일의 Variant(`Assets/Main/02.Prefabs/Player/PlayerCopy_Variant.prefab` 등, 새 GUID)로 새로 만든다.
- A와 B는 모든 참조를 옮긴 뒤 `Assets/_Deprecated/`로 옮기거나 삭제한다.

**최종 구성**
- 루트: 스케일 1. Rigidbody2D Constraints 4.
- 값: jumpSpeed와 groundCheckSize는 A 값, objectLayer 512, `JJBPlayerAbsorb`와 `JJBPlayerGimmick`을 추가한다.
- 콜라이더: §9의 결정 1에 따른다.
- 자식: Visual(꺼짐, JJBPlayerRenderer 유지), PinkVisual(player_anim_cursor), GroundCheck, InteractPoint, AbsorbPoint, Skills(B에서 이동), Player Light(B에서 이동).
- 루트 컴포넌트: B의 SkillManager_HTY, Utility_HTY, FixedJoint2D, (TargetSelector는 쓰지 않으므로 뺄지 태영 확인) + `PlayerInputLock`, `JJBControlGate`, `PlayerAbilityGate`, (Alt를 스테이지에서 쓸 경우) `AltModeController`(playerSprite = PinkVisual/GripPivot/Sprite).

| 장점 | 단점 |
|---|---|
| C를 쓰는 씬 3개(Test, RopeTest, Stage2)는 손대지 않아도 된다 | 지빈이 `Assets/JJB/Prefab` 대신 공용 파일을 고쳐야 한다. 담당 경계가 흐려진다 |
| LDY 도구(`PlayerPrefabMerger`, `SceneInstanceReplacer`, `ReferenceRemapper`)를 약간만 고쳐 재사용할 수 있다 | 한 파일을 셋이 고치므로 `.prefab` YAML 머지 충돌 위험이 크다 |
| 스테이지 씬이 모두 같은 플레이어를 쓴다. 테스트 결과가 서로 맞는다 | 스킬 테스트용 설정(예: 스킬 모두 켬)이 본 게임에 섞인다 |

**수정해야 할 팀원 파일**

| 담당 | 파일 | 할 일 |
|---|---|---|
| 지빈 | `Assets/JJB/JJBLevel.unity`, `Assets/JJB/JJBScene.unity` | 인스턴스를 C로 교체한다. GroundCheck와 Visual 오버라이드는 제거한다(이제 프리팹 기본값이 맞으므로) |
| 지빈 | `Assets/JJB/Prefab/Player.prefab` | 은퇴(이동 또는 삭제) |
| 지빈 | (선택) `JJBPlayer.cs` | `PlayerAbsorb` null 가드를 추가한다(Absorb가 없는 Variant 대비) |
| 태영 | `Assets/HTY/00.Scenes/Main_HTY.unity` | 인스턴스를 C로 교체한다. 남은 RemovedComponents 항목을 정리한다 |
| 태영 | `Assets/Main/02.Prefabs/Player/Player.prefab`, `PlayerCopy.prefab` | Skills와 Light를 C로 옮긴 뒤 은퇴한다. PlayerCopy는 Variant로 대체한다 |
| 태영 | `CopyPlayer_HTY.cs` | **`using UnityEditor.Searcher;`를 삭제한다**(런타임 스크립트라 빌드가 깨짐). 복사본이 파괴됐을 때의 처리를 추가한다 |
| HES | `Assets/HES/Scene/Test.unity` | 변경 없음(이미 C) |
| HES | `KillHitbox.cs` | (권장) `Destroy` 대신 `LDY.Death.PlayerKill.Request` 사용 |
| LDY | `Assets/prefab/Player.prefab` | 병합 결과를 저장한다 |
| LDY | `Stage1_ArtTest.unity` | A에서 C로 교체 |
| LDY | `RopeTest.unity` | 스케일 2, objectLayer, SortingOrder 오버라이드 제거 |
| LDY | `PlayerPrefabMerger`/`ChildSync`/`ComponentSync` | A의 스케일 2를 환산하도록 수정 |
| LDY | `SceneInstanceReplacer` | 교체 대상 목록 확장 |
| LDY | `FlowPaths.KnownPlayers`, `TitleTuningSync.JibinPlayer`, `PlayerPrefabPaths` | 최종본 경로로 정리 |

### 안 2. 공통 베이스 + 담당별 Variant

**결과**
- `Assets/prefab/Player.prefab`(C의 GUID 유지)을 **베이스**로 둔다. 베이스에는 이동, 점프, 상호작용, 흡수, PinkVisual, 로프, 잠금만 넣는다.
- Variant는 다음과 같이 나눈다.
  - `Assets/Main/02.Prefabs/Player/Player_Skill.prefab`: 베이스 + Skills + Light + FixedJoint. 태영 소유, 스킬 테스트용.
  - `Assets/Main/02.Prefabs/Player/PlayerCopy.prefab`: 베이스 + 입력/Alt/잠금 제거, tag 변경. 태영 소유.
  - (선택) `Assets/JJB/Prefab/Player_JJB.prefab`: 지빈 레벨 테스트용 Variant. 오버라이드는 0개를 목표로 한다.

| 장점 | 단점 |
|---|---|
| 각자 자기 Variant만 고치므로 머지 충돌이 줄어든다 | 본 게임에 스킬이 들어간다면 결국 "스킬 Variant"가 본 플레이어가 된다. 어느 Variant가 정식인지 정해야 한다 |
| 베이스를 고치면 모든 Variant에 반영된다 | Variant 오버라이드가 쌓이면 무엇이 진짜 값인지 다시 흐려진다. 정기적으로 Validate가 필요하다 |
| 스킬처럼 실험 중인 기능을 본 게임 플레이어와 분리할 수 있다 | `StageSceneValidator.KnownPlayers`에 Variant 경로를 모두 넣어야 한다. 검증기는 가장 가까운 인스턴스 루트, 즉 Variant 경로를 돌려준다 |
| PlayerCopy를 Variant로 만들면 "원본과 같은 구성 + 필요한 것만 끔"이 구조적으로 보장된다 | 새 GUID가 생기므로 Main_HTY의 참조(1곳)와 CopyPlayer 필드를 교체해야 한다 |

**수정해야 할 팀원 파일**: 안 1과 같다. 차이점은 세 가지다.
- 태영의 Skills와 Light가 C가 아니라 `Player_Skill` Variant로 간다. `Main_HTY`는 `Player_Skill`로 교체한다.
- 지빈의 `JJBLevel`을 베이스로 둘지 `Player_JJB` Variant로 둘지 고를 수 있다.
- LDY는 `FlowPaths.KnownPlayers`에 Variant 경로를 추가한다.

### 안 3. A의 파일을 최종본으로 (비교용, 비추천)

A(`49ba53f6…`)에 C의 PinkVisual과 B의 Skills를 넣는다.

| 장점 | 단점 |
|---|---|
| 지빈의 빌드 씬 `JJBLevel`의 GUID가 그대로라, 오버라이드를 유지한 채 바로 쓸 수 있다 | RopeTest(빌드 씬), Stage2, HES Test, Main_HTY 이렇게 **4개 씬**을 교체해야 한다 |
| 담당이 명확하다(이동 담당 = 파일 소유자) | LDY 병합 도구와 경로 상수가 모두 C에 쓰도록 짜여 있어 대부분 다시 짜야 한다. 스케일 2를 유지하면 로프 그립과 Alt 잔상 계산이 `lossyScale`을 타므로 다시 검증해야 한다 |

### 비교 요약

| | 안 1 | 안 2 | 안 3 |
|---|---|---|---|
| 바꿀 씬 수 | 4 | 4 (+Variant별 1) | 4 |
| 새 GUID | PlayerCopy Variant 1개 | Variant 2~3개 | PlayerCopy Variant 1개 |
| 머지 충돌 위험 | 높음 | 낮음 | 높음 |
| LDY 도구 재사용 | 높음 | 높음 | 낮음 |
| 스킬과 본 게임 분리 | ✗ | ✓ | ✗ |

**권장 조합**: 안 1을 기본으로 하되 PlayerCopy만 Variant로 만든다(안 2의 일부만 채택). 스킬을 본 게임에 넣을지 아직 확정되지 않았다면 안 2로 시작한다.

---

## 9. 애니메이션 통일 (player_anim_cursor 하나로)

### 현재 상태
- **JJB(A)의 Animator에는 파라미터가 하나도 없다.** aseprite 임포터가 만든 컨트롤러(state 5개: idle, walk, jump, fall, land)를 쓰지만, JJB 코드 어디서도 `Animator`를 부르지 않는다. 그래서 기본 state만 계속 재생된다.
- LDY의 `PlayerRenderer`는 Animator 없이 `SpriteFramePlayer`로 프레임을 직접 넘긴다. 상태는 `JJBPlayerMotionSource` 어댑터가 JJB 컴포넌트를 **읽기만 해서** 가져온다.

| PlayerRenderer가 쓰는 값 | 출처 (JJB) |
|---|---|
| 속도, 방향 | `Rigidbody2D.linearVelocity` |
| 땅에 있는지 | `JJBPlayerJump.IsGrounded` (Jump가 꺼져 있으면 접촉 법선으로 대신 판정) |
| 입력 잠금 | `PlayerInputLock` |
| 로프 / 잡기 | `PlayerPoseRelay` (로프 시스템이 채움) |

### 제안
1. **Animator 방식은 버리고 PlayerRenderer 방식으로 통일한다.**
   - Visual의 SpriteRenderer와 Animator를 끈 상태(C 방식)를 최종으로 한다.
   - Visual 오브젝트는 `JJBPlayer.PlayerRenderer` 참조 때문에 남긴다.
   - JJB 쪽에서 "파라미터를 맞추는" 작업은 필요 없다.
2. JJB가 나중에 애니메이션 상태를 직접 정하고 싶다면 Animator 파라미터를 만들지 않는다. 대신 `IPlayerMotionSource`/`IPlayerPoseSink` 같은 LDY 인터페이스에 값을 넘긴다. 예를 들어 흡수 모션이 필요하면 `PlayerAnimId`에 Absorb를 추가하고 `PlayerPoseRelay`로 요청한다.
3. 태그 이름은 두 에셋이 소문자로 같으므로(idle/walk/jump/fall/land), Animator를 써야 하는 다른 곳(예: 컷씬)이 생겨도 clip 이름을 그대로 대응시킬 수 있다.
4. `cursor_character_anim.aseprite`는 다음 조건을 확인한 뒤 은퇴한다.
   - A의 참조가 사라진 것
   - `TitleTuningSync`나 타이틀(AltPlayer)이 쓰지 않는 것
   - 은퇴는 삭제하지 말고 `_Deprecated`로 옮긴다.
5. **좌우 반전**: `PlayerRenderer.flipSprite`가 프리팹에서 0이다. 켤지 결정해야 한다(`PlayerGripRig`는 `sprite.flipX`를 읽어 반전을 반영한다).
6. **Alt 상태 연결**: `StagePlayerAnimBootstrap`이 alt에 `null`을 넘긴다. 스테이지에 Alt를 넣는다면 `GetComponent<IAltStatus>()`를 넘기도록 고쳐야 한다(LDY).

### 결정 1. PPU / 크기

| 선택 | 캐릭터 높이 | 콜라이더 (스케일 1 기준) | 영향 |
|---|---|---|---|
| **PPU 50** | 1.02u (지금 JJBLevel과 같음) | A의 월드 값 그대로: size (0.872, 1.072), offset y 0.527 | JJBLevel 레벨을 다시 맞출 필요가 없다. `PlayerArt.PixelsPerUnit` 40→50, `RopeGripProfile`을 다시 빌드해야 한다(`CursorCharacter/4. Update Rope Grip Profile`) |
| PPU 40 (예시안) | 1.275u | 약 (1.09, 1.34), offset y 약 0.66 (A 비율 × 1.25) | JJBLevel의 통로, 점프 높이, 천장 높이를 다시 확인해야 한다. jumpSpeed 11이 부족할 수 있다 |

두 경우 모두 **루트 스케일은 1**로 둔다. 로프 그립, Alt 잔상, RopeTest 오버라이드가 스케일 1을 전제로 한다. 콜라이더 높이를 정하면 `VisualPlacer`가 PinkVisual의 y를 콜라이더 바닥에 맞춰 자동으로 계산한다.

---

## 10. 병합 순서

### 원칙: GUID 유지
- **최종 파일(C)은 지우거나 다시 만들지 않고, 그 파일 안에서 수정한다.** 그래야 GUID와 내부 fileID가 유지된다. C를 쓰는 씬 3개의 오버라이드도 그대로 산다.
- `.meta`를 직접 고쳐 GUID를 바꿔치기하지 않는다. Variant로 바꾸면 내부 fileID가 바뀌어 씬 오버라이드가 고아가 되므로 이득이 없다.
- 씬의 프리팹 교체는 `SceneInstanceReplacer`(내부적으로 새 인스턴스 생성 + `ReferenceRemapper`)를 쓴다. 또는 Unity 6의 Hierarchy 우클릭 → **Prefab → Replace…**(`PrefabUtility.ReplacePrefabAssetOfPrefabInstance`)를 쓴다. 이름과 위치 오버라이드가 보존되는지 반드시 확인한다.
- 은퇴하는 A, B, B' 파일은 **모든 참조가 0인지 확인한 다음** `_Deprecated`로 옮긴다. 옮기면 GUID가 유지돼서, 빠뜨린 참조가 있어도 깨지지 않는다. 한 스프린트 뒤에 삭제한다.
- 한 단계에 한 사람만 프리팹 파일을 만진다. 각 단계는 별도 커밋으로 하고, 다음 단계는 `base`에 머지한 뒤 시작한다.

### 단계

| # | 담당 | 할 일 | 끝난 조건 |
|---|---|---|---|
| 0 | 전원 | §11 결정 사항 확정(PPU, 안 1/2, Alt 포함 여부, PlayerCopy 태그, jumpSpeed) | 결정을 이 문서에 기록 |
| 1 | 태영 | `CopyPlayer_HTY.cs`의 `using UnityEditor.Searcher;` 삭제. Skills 컴포넌트 목록과 필요한 루트 컴포넌트 확정(TargetSelector와 FixedJoint2D가 필요한지) | 빌드 컴파일 통과 |
| 2 | 지빈 | 최종 게임플레이 값 확정: jumpSpeed, groundCheckSize, 콜라이더(결정 1 기준). `JJBPlayer.HandleAbsorbPressed` null 가드. `JJBPlayerGimmick`을 흡수에 필수로 붙일지 결정(RequireComponent 권장) | 값 목록을 이 문서 §12에 기록 |
| 3 | LDY | 병합 도구 수정: A의 스케일 2를 환산(콜라이더, 자식 위치 × 2). `JJBLevel` 오버라이드 값(GroundCheck -0.065, Visual 0)을 기준으로 사용. Gimmick과 Absorb 추가. Skills와 Light 복사(B → C). `SceneInstanceReplacer` 목록 확장. 실행 전 dry-run 출력 | `LDY/Player/Compare Prefabs` 결과를 검토 |
| 4 | LDY | `LDY/Player/Merge Into One` 실행 → C 저장(GUID 유지) | §10-검증 체크리스트 1~3 통과 |
| 5 | 태영 | (안 2라면 `Player_Skill` Variant 생성) `PlayerCopy` Variant 생성: PlayerInput, JJBPlayerInput, Alt, PlayerAbilityGate, Skills 끄기 또는 제거, tag 변경. B의 CopyPlayer 필드를 새 Variant로 다시 연결 | 복사 스킬 테스트 |
| 6 | 각 씬 담당 | 씬 인스턴스 교체: JJBLevel, JJBScene(지빈), Main_HTY(태영), Stage1_ArtTest(LDY). 교체 후 남은 오버라이드 정리(`OverrideCleaner`). RopeTest 스케일 2 제거(LDY) | 각 씬에서 Play 테스트 |
| 7 | LDY | 경로 상수 정리(`FlowPaths`, `TitleTuningSync`, `PlayerPrefabPaths`). A, B, B'를 `_Deprecated`로 이동 | GUID 검색으로 참조 0건 확인 |
| 8 | 전원 | 빌드 씬(JJBLevel, RopeTest) 통합 플레이 테스트 | 체크리스트 전체 통과 |

### 검증

**메뉴**
- 지금 있는 메뉴:
  - `LDY/Player/Compare Prefabs`: 두 프리팹의 직렬화 값 diff를 `Assets/LDY/Debug/player_prefab_diff.md`로 출력한다.
  - `LDY/Merge/Validate Open Stage`: 열린 스테이지 씬에서 Player 1개, 알려진 프리팹 인스턴스, Goal, Camera 등을 확인한다.
- **새로 만들 것을 제안: `LDY/Player/Validate`**(LDY 담당). 최종 프리팹과 모든 Variant에 대해 다음을 자동으로 확인한다.
  - Missing Script 0개
  - 레이어 마스크가 0이 아님: `objectLayer`, `absorbLayer`, `groundLayer` (`MergePolicy.IsBrokenMask` 재사용)
  - `JJBPlayerAbsorb`가 있으면 `JJBPlayerGimmick`도 있음
  - 루트 스케일이 1이고 Rigidbody2D Constraints에 회전 고정 포함
  - 콜라이더 바닥과 PinkVisual 발 위치 차이가 1px 이하 (`FeetAlignment` 재사용)
  - GroundCheck 상자가 콜라이더 바닥 ±0.1u 안에 있고, 폭이 콜라이더 폭 이하
  - `AltModeController.playerSprite`가 PinkVisual Sprite를 가리킴(Alt를 넣을 경우)
  - PlayerCopy Variant: tag가 `Player`가 아니거나(결정에 따라), PlayerInput이 꺼져 있음
  - 프로젝트 전체 씬에서 A, B, B'의 GUID 참조가 0건(7단계 이후)

**수동 체크리스트** (각 씬에서 Play)
1. [ ] 이동(A/D), 점프(Space 짧게/길게), 착지할 때 발이 땅에 붙음(1px 이내)
2. [ ] 벽에 붙은 상태에서 점프가 안 됨(GroundCheck 폭 확인)
3. [ ] 머리가 천장을 통과하지 않음(콜라이더 높이)
4. [ ] F로 상호작용 오브젝트 반응, F로 로프 잡기와 놓기, 로프 위에서 W/S 오르내리기와 Space 점프
5. [ ] E로 흡수(Gimmick 레이어 오브젝트), 예외 없음
6. [ ] HTY 스킬: Y로 사용, R, Q+Shift 드롭. 복사 스킬에서 복사본이 Space에 같이 점프하지 않음
7. [ ] 복사본이 골에 닿아도 클리어되지 않음(또는 의도대로 동작)
8. [ ] Alt 모드 진입과 복귀(넣는 경우). 잔상이 PinkVisual 스프라이트로 나옴
9. [ ] 데스존, 트랩, 거미에 사망 연출 후 재시작. HES KillHitbox에서도 같은 동작
10. [ ] 애니메이션: idle → walk → run, jump, fall, land, 앉기(5초 대기), 로프 hang/climb, push, death
11. [ ] Console에 `[Player] 'Player' 태그 n개 중 본체를 가리지 못해` 경고 없음
12. [ ] 빌드(Development)가 컴파일 에러 없이 성공

---

## 11. 결정이 필요한 것

1. **PPU 50(지금 크기 유지)과 PPU 40(25% 확대) 중 무엇으로 할지** (§9의 결정 1). 지빈(레벨)과 LDY(아트)가 함께 정한다.
2. **안 1(파일 하나)과 안 2(베이스 + Variant) 중 무엇으로 할지.** 핵심은 스킬이 본 게임에 들어가는지다.
3. jumpSpeed를 11(A)과 16(B/C) 중 무엇으로 할지. 결정 1과 함께 정한다.
4. Alt를 스테이지 플레이어에 넣을지(`AltModeController`, `PlayerAbilityGate`). 넣는다면 HTY 스킬도 `IAbilityGate`와 `PlayerInputLock`을 거치게 할지.
5. PlayerCopy의 tag를 `Player`로 유지할지, 바꿀지. 바꾼다면 HES 버튼류의 판정 방식도 함께 정한다.
6. 좌우 반전(`flipSprite`)을 켤지.
7. HES `KillHitbox`를 LDY 사망 흐름(`PlayerKill`)으로 바꿀지.

## 12. 결정 기록

| # | 결정 | 날짜 | 결정자 |
|---|---|---|---|
| | | | |

---

### 부록: 조사 방법
- 프리팹과 씬 YAML에서 GUID와 fileID를 직접 비교했다. 스크립트 GUID는 `.meta`에서 찾아 경로로 바꿨다.
- `.aseprite`는 바이너리 헤더, 레이어, 태그, cel 청크를 읽고, 압축 cel을 풀어 프레임별 불투명 경계를 계산했다.
- 씬 인스턴스 오버라이드는 `PrefabInstance.m_Modification`을 추출했다. 내부 참조는 stripped 오브젝트를 기준으로 셌다.
- Unity 에디터를 실행하지 않았으므로 **런타임 동작(NRE, 복사본 점프 등)은 코드를 읽고 추론한 것**이다. §10 체크리스트로 확인해야 한다.
- Unity 버전: 6000.3.16f1. 이 .md 파일의 `.meta`는 에디터를 열면 자동으로 생성된다.
