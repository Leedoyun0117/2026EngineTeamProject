using System.Collections;
using LDY.Death;
using LDY.Flow.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace LDY.Flow {
    // 선택 화면 → 스테이지 → 클리어/종료 → 선택 화면. 선택 화면을 거치지 않고 스테이지 씬을 바로 실행해도 동작한다.
    public sealed class SceneFlow : MonoBehaviour, IStageRestarter {
        private readonly StageSession _session = new StageSession();
        private FlowSettings _settings;
        private SceneLoadRunner _loader;
        private ConflictingManagers _managers;
        private string _pendingStageId;
        private string _loadFailureId;

        public static SceneFlow Instance { get; private set; }

        public StageCatalog Catalog => _settings.catalog;
        public UiSkin Skin => _settings.skin;
        public ProgressService Progress { get; private set; }
        public string CurrentStageId => _session.StageId;
        public string LastStageId { get; private set; }

        // 방금 클리어하고 선택 화면으로 돌아온 스테이지. 다른 스테이지에 들어가거나 타이틀로 가면 지운다.
        public string LastClearedId { get; private set; }
        public bool IsBusy => _loader.IsBusy;

        public void Initialize(FlowSettings settings, ProgressService progress, SceneLoadRunner loader) {
            Instance = this;
            _settings = settings;
            Progress = progress;
            _loader = loader;
            _managers = new ConflictingManagers(settings.conflictingManagerTypes, settings.similarManagerHint);
            SceneManager.sceneLoaded += HandleSceneLoaded;
            StageSignals.Cleared += HandleCleared;
            StageSignals.PlayerLost += HandlePlayerLost;
        }

        private void OnDestroy() {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            StageSignals.Cleared -= HandleCleared;
            StageSignals.PlayerLost -= HandlePlayerLost;
            if (Instance == this) Instance = null;
        }

        private void Update() {
            var keyboard = Keyboard.current;
            if (_session.IsActive && !IsBusy && keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                ReturnToSelect();
        }

        public StageStatus StatusOf(string id) {
            var index = Catalog.IndexOf(id);
            return index < 0 ? StageStatus.NotReady : UnlockPolicy.StatusOf(Catalog.Slots(), index, Progress.Model);
        }

        public bool TryEnterStage(string id) {
            var entry = Catalog.Find(id);
            var status = StatusOf(id);
            if (entry == null || (status != StageStatus.Open && status != StageStatus.Cleared)) return false;
            if (!_loader.Load(entry.scenePath, entry.fileName + " 실행 중...")) return false;
            _pendingStageId = id;
            LastStageId = id;
            LastClearedId = null;
            return true;
        }

        public bool ReturnToSelect() => _loader.Load(_settings.selectScenePath, "스테이지 선택.exe 실행 중...");

        public bool ReturnToTitle() {
            if (!_loader.Load(_settings.titleScenePath, "타이틀.exe 실행 중...")) return false;
            LastClearedId = null;
            return true;
        }

        public void ReloadProgress() => Progress.Reload();

        // 들어가려던 스테이지를 열지 못하고 선택 화면으로 돌아왔으면 그 id를 한 번 돌려준다(선택 화면이 안내 창을 띄운다).
        public bool TryTakeLoadFailure(out string stageId) {
            stageId = _loadFailureId;
            _loadFailureId = null;
            return stageId != null;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode) {
            if (mode != LoadSceneMode.Single) return;
            var requested = _pendingStageId != null;
            var entry = requested ? Catalog.Find(_pendingStageId) : StandaloneEntry(scene);
            var id = entry != null ? entry.id : RootId(scene);
            _pendingStageId = null;
            if (entry != null && entry.UsesSubRoot && !SubStageLoader.Prepare(scene, entry, Catalog, requested, _managers) &&
                requested) {
                FailBackToSelect(entry.id);
                return;
            }

            if (string.IsNullOrEmpty(id)) {
                _session.End();
                return;
            }

            _session.Begin(id);
        }

        // 다중 스테이지 루트를 못 찾은 경우. 멈추지 않고 선택 화면으로 돌아간다.
        private void FailBackToSelect(string stageId) {
            _session.End();
            _loadFailureId = stageId;
            StartCoroutine(ReturnToSelectWhenIdle());
        }

        // sceneLoaded 시점에는 로더가 아직 바빠서(Load가 거절됨) 끝난 뒤에 연다.
        private IEnumerator ReturnToSelectWhenIdle() {
            while (IsBusy) yield return null;
            if (!ReturnToSelect()) Debug.LogWarning("[Flow] 선택 화면을 열 수 없어 이 씬에 머문다.");
        }

        // 바로 실행한 씬: StageRoot id → 씬 하나짜리 항목 → 켜져 있는 다중 스테이지 루트 순으로 찾는다.
        private StageEntry StandaloneEntry(Scene scene) {
            var rootId = RootId(scene);
            if (rootId != null) return Catalog.Find(rootId);
            return Catalog.FindWholeScene(scene.path) ?? SubStageLoader.DetectActive(scene, Catalog);
        }

        private static string RootId(Scene scene) =>
            StageRoot.Current != null && StageRoot.Current.gameObject.scene == scene ? StageRoot.Current.StageId : null;

        private void HandleCleared() {
            if (IsBusy) return;
            if (_session.IsActive && Catalog.Find(_session.StageId) != null) {
                var seconds = _session.Elapsed;
                var improved = Progress.RecordClear(_session.StageId, seconds);
                LastStageId = _session.StageId;
                LastClearedId = _session.StageId;
                Debug.Log($"[Flow] {_session.StageId} 클리어 {StageNaming.FormatTime(seconds)}{(improved ? " (최고 기록)" : "")}");
            }
            else {
                Debug.Log("[Flow] 카탈로그에 없는 씬에서 골 도착. 기록 없이 선택 화면으로 간다.");
            }

            _session.End();
            if (!ReturnToSelect()) Debug.LogWarning("[Flow] 선택 화면을 열 수 없어 이 씬에 머문다.");
        }

        // 사망 연출이 이미 끝난 뒤의 요청이라 추가 대기 없이 다시 연다.
        public void RequestRestart() => Restart(0f);

        private void HandlePlayerLost() => Restart(_settings.restartDelaySeconds);

        private void Restart(float delay) {
            if (IsBusy) return;
            var scene = SceneManager.GetActiveScene();
            var entry = _session.IsActive ? Catalog.Find(_session.StageId) : null;
            var caption = (entry != null ? entry.fileName : scene.name) + " 다시 실행 중...";
            _pendingStageId = _session.StageId;
            if (!_loader.Load(scene.path, caption, delay)) _pendingStageId = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;
    }
}
