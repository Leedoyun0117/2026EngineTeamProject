# Player 프리팹 비교 (조사만, 합치기 전)

## 후보
| 경로 | 브랜치 | 마지막 커밋 | GUID | 판단 |
|---|---|---|---|---|
| Assets/JJB/Prefab/Player.prefab | LDY, base, origin/JJB 등 | press101-spare, 2026-11-21 | 49ba53f6... | 지빈이 것 (JJB 폴더, 작성자 press101-spare) |
| Assets/prefab/Player.prefab | LDY, base, origin/JJB 등 | Leedoyun, 2026-11-21 | 65b933f5... | 내 것 (LDY 컴포넌트와 PlayerAnimSet_Cursor 포함, 작성자 Leedoyun) |
| Assets/LDY/Prefab/AltPlayer.prefab | 전 브랜치 | Leedoyun | 05b60116... | 타이틀용 별도 프리팹 (이번 합치기 대상 아님) |
| Assets/HTY/02.Prefabs/Player_HTY.prefab | LDY, base, origin/HTY | hanty100209-cpu | 18c5c0e3... | HTY 것 (대상 아님) |

두 프리팹 모두 현재 브랜치(LDY)에 있어 브랜치 간 가져오기는 필요 없다. 지빈이 프리팹의 최신본은 origin/JJB와 같은 6431c34 계열이다.

## 비교
| 항목 | 지빈이 (JJB) | 내 것 (prefab) |
|---|---|---|
| 루트 태그/레이어/스케일 | Player / 3 / 1 | 같음 |
| Rigidbody2D | 질량 1, 중력 1, 보간 0, 충돌감지 0, Constraints 4 (회전 고정) | 같음, 단 Constraints 0 |
| CapsuleCollider2D | 오프셋 (0.017, 0.257), 크기 (0.729, 1.514) | 오프셋 (0, 0), 크기 (0.74, 0.61) |
| JJBPlayerMovement / Jump | moveSpeed 5, jumpSpeed 16, groundLayer 64 등 | 같음 |
| PlayerInput | 같은 InputSystem_Actions | 같음 |
| JJBPlayerInteract | objectLayer 512, interactPoint (0, 0.5) | objectLayer 0 (깨짐), interactPoint (0, 0) |
| JJBPlayerAbsorb + AbsorbPoint | 있음 (absorbLayer 1024) | 없음 |
| GroundCheck 위치 | (0, -0.68) | (0, -0.27) |
| Visual | 있음. 스프라이트 0.39x0.61, 위치 (0, -0.55), JJBPlayerRenderer, Animator 켜짐 | 있음. 스프라이트와 Animator 꺼짐, 위치 (0, 0), JJBPlayerRenderer |
| PinkVisual | 없음 | 있음. 위치 (0, -0.305), PlayerRenderer(PlayerAnimSet_Cursor), StagePlayerAnimBootstrap, PlayerGripRig, GripPivot/Sprite(0.42x0.51) |
| JJBControlGate, PlayerInputLock | 없음 | 없음 (AltPlayer에만 있음) |
| PlayerSuspendHub, PlayerRopeSlot, PlayerPoseRelay, JJBPlayerMotionSource | 없음 | 없음. 런타임에 자동으로 붙음 |
| Alt 관련 (AltModeController, PlayerAbilityGate) | 없음 | 스테이지 프리팹에는 없음 (AltPlayer에만 있음) |
| Missing Script / 빈 필드 | 없음 | objectLayer 0 |

지빈이 말과 달리 지빈이 프리팹에도 Visual이 있다.

## 참조
- Assets/prefab/Player.prefab: HES/Scene/Test, LDY/Scenes/RopeTest, _Recovery/0 (5)
- Assets/JJB/Prefab/Player.prefab: JJB/JJBScene, LDY/Scenes/Stage1_ArtTest, LDY/Scenes/Stage2_SpiderTest
- 경로 하드코딩: PlayerVisualContext.cs(prefab), SpiderEditorSupport.cs(JJB), Stage1ArtTestScene.cs(JJB), RopeTestSceneSetup.cs(문자열)
- Addressables, Resources 로드: 없음

## 씬 오버라이드
- RopeTest: 인스턴스 스케일 2, objectLayer 512, SortingOrder 6
- 나머지 씬: 위치와 이름만 덮어씀 (JJBScene은 m_Sprite 비움 포함)

## 질문
1. 콜라이더가 지빈이 값 그대로면 높이 1.51, 내 PinkVisual 스프라이트는 높이 0.51이라 크기가 안 맞는다. 지빈이 콜라이더를 쓰고 PinkVisual을 키울까, 아니면 콜라이더를 내 값으로 둘까.
2. 지빈이 콜라이더를 쓰면 GroundCheck, InteractPoint, AbsorbPoint 위치도 지빈이 값으로 가져간다. 괜찮은가.
3. 콜라이더 바닥(-0.5)에 맞춰 PinkVisual y를 자동 계산할까, 현재 -0.305를 유지할까. PlayerGripRig(로프 그립) 값도 콜라이더 높이에 영향을 받는다.
4. RopeTest의 스케일 2 오버라이드를 유지하나, 합친 뒤에는 제거하나.
5. 지빈이 Visual(JJBPlayerRenderer, Animator)을 내 쪽처럼 꺼 두고 PinkVisual만 쓰면 되나.
6. JJBControlGate(PlayerInputLock 자동 추가)를 스테이지 프리팹에 직접 넣을까, 로프가 쓸 때 RequireComponent로 붙게 둘까.
7. Alt 컴포넌트를 스테이지 프리팹에도 넣나.
