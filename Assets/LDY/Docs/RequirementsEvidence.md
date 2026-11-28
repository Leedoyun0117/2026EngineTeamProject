# 과제 요구사항 근거: 예외 처리 · 파일 입출력

범위: `Assets/LDY` 아래 `.cs` (Tests 제외, 단 근거로 테스트를 일부 인용).
구분: **[R]** 런타임(빌드에 포함, `Script/`) / **[E]** 에디터 전용(`Editor/`).
조사 기준 커밋: `fbfe99e`. 코드는 수정하지 않았다.

JSON은 Unity `JsonUtility`나 Newtonsoft를 쓰지 않는다. 직접 만든 파서 `Script/Background/Core/JsonParser.cs`를 쓰고, 이 파서는 형식이 틀리면 `FormatException`("JSON 위치 N: …")을 던진다(`JsonParser.cs:133`). 각 기능이 이 예외를 자기 도메인 예외(`ProgressFormatException`, `StageFileException`, `BackgroundDataException`)로 바꿔 다시 던진다.

---

## 1. 파일 입출력 사용처

### 1-1. 표

| 파일:라인 | R/W | 형식 | 경로 | 구분 | 기능 |
|---|---|---|---|---|---|
| `Script/Flow/Core/JsonProgressStore.cs:21-23` | 읽기 | JSON | `persistentDataPath/progress.json` (`ProgressService.cs:15`) | R | 스테이지 클리어·최고기록·해금 진행도 불러오기 |
| `Script/Flow/Core/JsonProgressStore.cs:32-38` | 쓰기 | JSON | 위와 같음 + `.tmp` | R | 진행도 저장 (임시 파일에 쓰고 바꿔치기) |
| `Script/Flow/Core/JsonProgressStore.cs:41-45` | 이동 | JSON | `progress.json.corrupt-yyyyMMdd-HHmmss` | R | 손상 파일 백업 |
| `Script/Adapters/MixerAudioVolume.cs:22, 55, 63-73` | 읽기/쓰기 | PlayerPrefs | 레지스트리/플랫폼 저장소 | R | 채널별 볼륨 저장·복원·초기화 |
| `Script/Title/Settings/ScreenModeSetting.cs:26-33` | 읽기/쓰기 | PlayerPrefs | 위와 같음 | R | 화면 모드 저장·복원 |
| `Script/Flow/SceneLoadRunner.cs:27` | 존재 확인 | – | `Assets/...unity` | R(에디터 실행 시 의미) | 로드할 씬 파일이 있는지 확인 |
| `Editor/StageImport/Jobs/ImportSource.cs:37` | 읽기 | JSON | `Assets/LDY/StageData/stages.json` 또는 `Assets/LDY/Data/stages1.json` (`StageImportPaths.cs:7,15`) | E | 레벨 툴 내보내기 파일 → 스테이지 씬 자동 생성 |
| `Editor/StageImport/Jobs/ImportSource.cs:63`, `StageImportPaths.cs:36` | 존재 확인 | – | 위와 같음 | E | 설정 경로에 없으면 후보 경로 탐색 |
| `Editor/StageImport/Jobs/SceneNaming.cs:17`, `ImportJob.cs:33` | 존재 확인 | – | `Assets/LDY/Scenes/Imported/*.unity` | E | 같은 이름 씬이 있으면 `_v2`, `_v3` 이름 짓기 |
| `Editor/Background/BackgroundSources.cs:21-35` | 읽기 | JSON | `StageBackgroundSource.JsonPath` (에셋 폴더) | E | 스테이지 배경 레이아웃 읽기 + 참조 파일 존재 검사 |
| `Editor/Flow/UiSkinBuilder.cs:133-138` | 읽기 | JSON | `<스킨 폴더>/manifest.json` | E | 9-slice 테두리(`border_LBRT`) 읽기 |
| `Editor/PlayerAnimSetBuilder.cs:75` | 읽기 | 바이너리(.aseprite) | 에셋 폴더 | E | aseprite 헤더에서 프레임·태그 읽기 |
| `Editor/CursorCharacter/CursorCleanReimport.cs:40-70` | 읽기/쓰기/삭제 | 바이너리 | 원본 .aseprite + 백업 경로 | E | 백업 → 삭제 → 다시 쓰기 → 바이트 비교 검증 → 재임포트 |
| `Editor/PlayerMerge/PlayerCompareReport.cs:16-17` | 쓰기 | 텍스트(Markdown, UTF-8 no BOM) | `PlayerPrefabPaths.DiffReport` | E | 플레이어 프리팹 비교 리포트 |
| `Editor/PlayerMerge/SceneSwap/CopyReferenceFinder.cs:23-25` | 읽기 | 텍스트(.cs) | `Assets/**/*.cs` | E | 코드 안의 참조 검색 |
| `Editor/Flow/ThumbnailCapture.cs:75, 103` | 쓰기 | PNG | 썸네일 폴더 | E | 스테이지 썸네일 캡처 |
| `Editor/Background/BackgroundArtImporter.cs:24-28` | 쓰기 | PNG | `WhitePixelPath` | E | 1px 흰색 텍스처 생성 |
| `Editor/CursorCharacter/CursorClipDumper.cs:48-49` | 쓰기 | PNG | `OutputPath` | E | 애니메이션 클립 시트 덤프 |
| `Editor/Stage/Stage1Tiles.cs:111-118` | 쓰기 | PNG | 타일 폴더 | E | 없는 타일 텍스처 생성 |
| `Editor/Stage/Stage1Art.cs:105` | 읽기 | PNG | `pngPath` | E | PNG를 텍스처로 로드 |
| `Editor/TitleSceneSetup.cs:852` | 쓰기 | PNG | `SpriteFolder` | E | 타이틀 스프라이트 생성 |
| `Editor/Stage/Stage1Pickups.cs:75`, `Stage1SetupMenu.cs:119` | 목록 | – | 에셋 폴더 | E | 폴더 안 파일 나열 |
| 기타 `Directory.CreateDirectory` (`PlayerDeathSettingsBuilder.cs:19`, `TitlePlayerAssembler.cs:17`, `TitleSceneSetup.cs:198/620/711` 등) | 폴더 생성 | – | 에셋 폴더 | E | 에셋 저장 전 폴더 보장 |

정리하면 **런타임 파일 I/O는 진행도 JSON 하나 + PlayerPrefs 두 곳**이고, 나머지는 에디터 툴이다.

### 1-2. 대표 흐름: `JsonProgressStore` (저장 → 불러오기 → 손상 복구)

1. **경로**: `ProgressService.DefaultPath = Path.Combine(Application.persistentDataPath, "progress.json")` (`ProgressService.cs:15`). 빌드/에디터 모두 쓰기 가능한 사용자 폴더다.
2. **저장** (`JsonProgressStore.cs:32-39`)
   - 폴더가 없으면 만든다.
   - `ProgressJson.Write`로 직렬화한 내용을 `progress.json.tmp`에 먼저 쓴다.
   - 기존 파일을 지우고 `.tmp`를 `progress.json`으로 옮긴다. 쓰는 도중 꺼져도 이전 파일은 남는다.
   - 호출부 `ProgressService.Save`(`:43-52`)가 `IOException`을 잡아 `Debug.LogError`로 남긴다. 게임은 멈추지 않고, 메모리의 진행도는 그대로 유지된다.
3. **불러오기** (`:20-29`)
   - 파일이 없으면 빈 `ProgressModel`을 돌려준다(첫 실행).
   - 있으면 `ProgressJson.Read`로 파싱한다. 문법 오류(`FormatException`)는 `ProgressFormatException`으로 바뀌고(`ProgressJson.cs:39-44`), 키 누락, 타입 불일치, `version` 불일치도 같은 예외가 된다(`:48, 69, 75`).
4. **손상 복구** (`:25-28`, `:41-45`)
   - `ProgressFormatException`을 잡으면 손상된 파일을 `progress.json.corrupt-시각`으로 **옮겨 보존**하고 빈 진행도로 시작한다.
   - `ProgressService.Load`(`:36-41`)가 `LastBackupPath`를 보고 경고 로그에 백업 위치를 남긴다.
5. **테스트 근거**: `Tests/EditMode/Flow/JsonProgressStoreTests.cs`. 잘린 JSON(`:64-73`), version 99(`:89-95`), 키 누락(`:100-106`) 각각에서 백업 파일이 생기고 원본 내용이 보존되는지, 저장 후 `.tmp`가 남지 않는지(`:52`) 검증한다.

---

## 2. try-catch 사용처

평가 기준: **좋음** = 구체 예외 + 복구/로그, **주의** = `Exception` 전체를 잡음, **나쁨** = 빈 catch, 조용히 무시.

### 2-1. catch가 있는 곳

| 파일:라인 | 구분 | 잡는 예외 | catch 안에서 하는 일 | 평가 |
|---|---|---|---|---|
| `Script/Flow/Core/JsonProgressStore.cs:22-28` | R | `ProgressFormatException` | 복구: 손상 파일 백업 후 빈 진행도 반환 | 좋음 |
| `Script/Flow/ProgressService.cs:44-49` | R | `IOException` | 로그(`LogError`) 후 계속 진행 | 좋음 |
| `Script/Flow/Core/ProgressJson.cs:39-44` | R | `FormatException` | 도메인 예외로 바꿔 다시 던짐 | 좋음 |
| `Script/Background/Core/JsonNode.cs:21-27` | R | `FormatException` | `BackgroundDataException`으로 바꿔 다시 던짐(출처 이름 포함) | 좋음 |
| `Script/Player/Logic/AsepriteTagParser.cs:18-25` | R | `IndexOutOfRange` / `Argument` / `FormatException` (예외 필터) | `TryParse` 패턴: `false` + 오류 메시지 반환 | 좋음 |
| `Script/StageImport/Core/CursorStageParser.cs:13-18` | R | `Exception` (단, `StageFileException` 제외 필터) | `StageFileException`으로 바꿔 다시 던짐 | 주의 |
| `Script/Alt/AltStateMachine.cs:24-29` | R | `Exception` | 로그(`LogException`). 상태 전환은 계속(의도적, 주석 있음) | 주의 |
| `Script/Title/Transition/TransitionFailureHandler.cs:50-55` | R | `Exception` | 로그. 실패 처리 단계를 끝까지 진행(의도적) | 주의 |
| `Script/Title/Transition/TransitionSequence.cs:33-38` | R | `Exception` | `onError` 콜백(`context.Fail` 또는 `LogException`)으로 넘기고 코루틴 중단 | 주의 (코루틴 경계라 불가피) |
| `Script/Title/Window/PlayerRowMover.cs:99-114` | R | 전체(`catch` 무타입) | 정리(`Finish()`) 후 `throw;`로 **다시 던짐** | 좋음 (삼키지 않음) |
| `Editor/StageImport/Jobs/ImportSource.cs:36-42` | E | `StageFileException` | 로그(`log.Warn`) 후 `null` 반환 → 임포트 전체 중단 | 좋음 |
| `Editor/Background/BackgroundSources.cs:26-33` | E | `BackgroundDataException` | 로그 후 `false` 반환 → 해당 배경 건너뜀 | 좋음 |
| `Editor/PlayerMerge/CopyVariantBuilder.cs:57-64` | E | `InvalidOperationException` | 노트 기록. 핵심 컴포넌트(SkillManager) 실패면 `ok=false` | 좋음 |
| `Editor/PlayerMerge/SerializedValueCopier.cs:26-31` | E | `NotSupported` / `InvalidOperationException` (필터) | 노트 기록 후 다음 프로퍼티 | 좋음 |

**빈 catch나 조용히 무시하는 catch(나쁨)는 없다.**

### 2-2. try-finally만 있는 곳 (정리 보장용, 잡지 않음)

- 런타임: `Alt/ReturnState.cs:60-70` (시간 배율·입력 잠금 해제), `Rope/RopeHangController.cs:125/193/211` (조작 슬롯 반납), `Title/Transition/MonitorZoomRig.cs:83-95`, `SceneEntryDirector.cs:60-77`, `TransitionFailureHandler.cs:29-46` (토큰 반납), `PlayerRowMover.cs:121-135` (중첩 finally로 물리·토큰 복원).
- 에디터: `StageImportMenu.cs:50-60` (진행 바 닫기), `ThumbnailCapture.cs:89-108` (RenderTexture 해제·카메라 복원), `CopyVariantBuilder.cs:40-46` / `TempPrefabFactory.cs:17-26` / `RopePrefab.cs:38-71` / `SpriteFeetProbe.cs` 등 (프리뷰 씬·프리팹 편집 닫기), `FlowMenu.cs`, `Stage1SetupMenu.cs` 등 (`AssetDatabase.StartAssetEditing/StopAssetEditing` 짝 맞추기).

### 2-3. 직접 던지는 곳 (입력 검증)

- 도메인 예외: `ProgressJson.cs:48,69,75`, `CursorStageParser.cs:23-108`, `JsonNode.cs:35-83`, `StageBackgroundLoader.cs:46`, `WidgetNormalizer.cs:91,105`. 모두 JSON 경로(`stages[3].cells` 같은)를 메시지에 넣는다.
- 인자 검증 `ArgumentException`/`ArgumentNullException`: `BackgroundFit.cs:9-10`, `FrameClock.cs:10-11`, `PixelSpace.cs:37-38`, `VisualizerModel.cs:9-10`, `PixelFontSize.cs:6`, `JsonParser.cs:12`, `JsonProgressStore.cs:13`.
- `CursorCleanReimport.cs:71`: 백업 바이트 비교 실패 시 `IOException`.

---

## 3. 부족한 점과 개선 제안 (제안만, 수정 안 함)

### 3-1. 파일 입출력인데 예외 처리가 빠진 곳

| 위치 | 문제 | 제안 |
|---|---|---|
| `JsonProgressStore.cs:23` (Load) | `ProgressFormatException`만 잡는다. 파일이 잠겨 있거나 권한이 없으면 `IOException`/`UnauthorizedAccessException`이 `ProgressService` 생성자까지 올라가 진행도 서비스가 만들어지지 않는다. | `catch (Exception e) when (e is IOException or UnauthorizedAccessException)` → 로그 후 빈 진행도(이때는 백업하지 않음). |
| `JsonProgressStore.cs:44` (Backup) | catch 블록 안에서 `File.Move`가 실패하면 새 예외가 그대로 나간다. | Backup을 try로 감싸고 실패 시 `LastBackupPath = null` + 경고 로그. |
| `JsonProgressStore.cs:37-38` (Save) | `Delete` 후 `Move` 사이에 꺼지면 `.tmp`만 남는데, Load는 `.tmp`를 보지 않아 진행도가 사라진 것처럼 보인다. | `File.Replace(temp, _path, backup)`을 쓰거나, Load에서 본 파일이 없고 `.tmp`가 있으면 `.tmp`를 읽기. |
| `ProgressService.cs:47` | `UnauthorizedAccessException`은 `IOException`의 하위 클래스가 아니라서 잡히지 않는다. | 필터에 `UnauthorizedAccessException` 추가. |
| `ImportSource.cs:37` | `ReadAllText`의 `IOException`(파일 잠김)은 잡지 않는다. 씬을 만들기 전이라 빈 씬은 생기지 않지만 메뉴가 예외로 끝난다. | 같은 try에 `IOException` catch 추가 → `log.Warn` 후 `null`. |
| `BackgroundSources.cs:27` | 위와 같음(`IOException` 미처리). | 같음. |
| `UiSkinBuilder.cs:138` | `manifest.json`이 손상되면 `JsonParser`의 `FormatException`이 그대로 나가 스킨 빌드 전체가 멈춘다. try가 없다. | `FormatException`을 잡아 `log.Warn("manifest 손상")` 후 빈 결과 반환(파일 없을 때와 같은 처리). |
| `PlayerAnimSetBuilder.cs:75` | `ReadAllBytes`의 `IOException`이 처리되지 않는다(파싱 오류는 `TryParse`가 처리함). | `IOException`을 잡아 같은 `LogError` 경로로 보내기. |
| `MixerAudioVolume.cs`, `ScreenModeSetting.cs:33` | PlayerPrefs는 예외를 거의 던지지 않지만, 저장된 int가 enum 범위를 벗어나면 그대로 캐스팅된다. | `Enum.IsDefined`로 검사, 범위 밖이면 기본값. |
| PNG 쓰기 여러 곳 (`ThumbnailCapture.cs:75`, `Stage1Tiles.cs:118` 등) | 에디터 툴이라 영향은 작지만 쓰기 실패가 메뉴 전체를 멈춘다. | 우선순위 낮음. 반복 처리(썸네일 여러 장) 쪽만 항목별 try + 경고 로그. |

### 3-2. 주의로 평가된 곳의 고치는 방법

- `CursorStageParser.cs:16`: `JsonParser`가 던지는 건 `FormatException`뿐이니 `catch (FormatException e)`로 좁힌다(`ProgressJson`, `JsonNode`와 같은 모양).
- `AltStateMachine.cs:27`: 의도된 방어(Exit 실패해도 전환)라 유지해도 된다. 발표 때 "로그를 남기고 상태 일관성을 우선한다"고 설명하면 된다.
- `TransitionFailureHandler.cs:53`: 실패 처리 중의 best-effort라 유지. 같은 설명.
- `TransitionSequence.cs:36` (`SafeEnumerator`): 코루틴의 `MoveNext` 경계에서는 어떤 예외가 올지 모르기 때문에 `Exception` 전체가 맞다. 유지.

### 3-3. 스테이지 임포터에서 빈 씬이 생길 수 있는 지점

예외를 **삼키는** catch는 임포터에 없다. 대신 예외가 **처리되지 않아서** 반쯤 만든 씬이 남는 경로와, 경고만 남기고 넘어가서 내용이 빈 씬이 나오는 경로가 있다.

1. **`StageImporter.cs:34` → `:47-53`** (가장 중요)
   - `StageSceneBuilder.Build`가 템플릿 씬을 **먼저 저장**하고(`StageSceneBuilder.cs`의 `SaveScene`), 빌드 설정 추가와 카탈로그 연결까지 끝낸 뒤 돌아온다.
   - 그 뒤 `Steps[i].step.Apply`(타일·프리팹·시작/골…)에서 예외가 나면 catch가 없어 `StageImportMenu.Run`의 `finally`(진행 바 닫기)만 실행되고 예외가 올라간다.
   - 결과적으로 **템플릿만 있는 빈 씬이 디스크에 저장되고 카탈로그에도 연결된 상태**로 남는다. 다음 실행 때는 `SceneExists`라 건너뛰므로(`StageImporter.cs:24`) 자동으로 고쳐지지 않는다. 남은 job도 모두 중단된다.
   - 제안: step 루프를 try로 감싸 실패 시 `log.Warn` + 해당 씬 삭제(`AssetDatabase.DeleteAsset`)나 이름에 `_failed` 표시, 카탈로그 연결은 step이 모두 성공한 뒤로 미루기, job 단위 catch로 다음 맵은 계속 진행하기.
2. **`TilemapStep.cs:14-17`**: 템플릿에 `Grid`가 없으면 경고만 남기고 return한다. 타일이 하나도 없는 씬이 "성공"으로 저장된다.
3. **`TilemapStep.cs:35-38`**: 대응표(`StageTileMap`)에 키가 없으면 그 칸을 건너뛰고 개수만 경고한다. 대응표가 비어 있으면 전 칸이 빠져 빈 씬이 된다.
   - 2·3번 제안: 놓은 타일이 0개이거나 빠진 비율이 크면 `Import`가 `false`를 돌려주고 경고를 Error로 올리기.
4. **`ImportSource.cs:39-41`**: JSON 오류는 경고 후 `null`이라 씬을 만들기 전에 멈춘다. **이 경로는 안전하다**(빈 씬이 생기지 않음).

---

## 4. 발표용 요약

1. 런타임에서는 `persistentDataPath/progress.json`(스테이지 진행도)을 JSON으로 읽고 쓰고, 볼륨·화면 모드는 PlayerPrefs에 저장한다.
2. 저장은 `.tmp`에 먼저 쓴 뒤 바꿔치기해서 도중에 꺼져도 이전 파일이 남고, 저장 실패(`IOException`)는 로그만 남기고 게임은 계속된다.
3. 불러오다 JSON이 깨졌거나 키·버전이 맞지 않으면 `ProgressFormatException`을 잡아 원본을 `.corrupt-시각`으로 백업하고 빈 진행도로 시작한다(EditMode 테스트로 검증).
4. 에디터에서는 레벨 툴 JSON(`stages.json`)과 배경 JSON을 읽어 씬·프리팹을 자동 생성한다. 파싱 오류는 JSON 경로가 담긴 도메인 예외로 바꿔 경고하고 그 항목만 중단한다.
5. 빈 catch는 없다. 남은 과제는 권한·잠금 예외 처리와, 씬 생성 도중 실패했을 때 반쯤 만든 씬을 정리하는 것이다.
