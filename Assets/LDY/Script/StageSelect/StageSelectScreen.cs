using System;
using System.Collections.Generic;
using LDY.Flow;
using LDY.Flow.Core;
using UnityEngine;

namespace LDY.StageSelect {
    // 뷰와 모델을 잇는 진입점. 상태 판정은 모델(=Core)에서 받고 여기서는 이벤트만 연결한다.
    public sealed class StageSelectScreen : MonoBehaviour, IFileBoardHost {
        [SerializeField] private TitleBarView titleBar;
        [SerializeField] private AddressBarView addressBar;
        [SerializeField] private TreeView tree;
        [SerializeField] private FilterTabsView tabs;
        [SerializeField] private FileGridView grid;
        [SerializeField] private StatusBarView statusBar;
        [SerializeField] private TaskbarView taskbar;
        [SerializeField] private DialogView dialog;
        [SerializeField] private SettingsDialogView settings;
        [Tooltip("IStageSelectSettings 구현 (StageSelectSettings)")] [SerializeField]
        private MonoBehaviour settingsSource;
        [SerializeField] private StageSelectInput input;

        private SceneFlow _flow;
        private StageSelectModel _model;

        private ProgressModel Progress => _flow.Progress.Model;
        private bool SettingsOpen => settings != null && settings.IsOpen;
        private bool ModalOpen => dialog.IsOpen || SettingsOpen;
        private bool HasSettings => settings != null && settingsSource is IStageSelectSettings;

        public bool IsReady => _model != null;
        public bool Blocking => IsReady && (ModalOpen || _flow.IsBusy);
        public string SelectedId => _model?.SelectedId;
        public IReadOnlyList<string> VisibleIds => grid.VisibleIds;
        public RectTransform FileArea => grid.Area;

        public event Action LayoutChanged {
            add => grid.LayoutChanged += value;
            remove => grid.LayoutChanged -= value;
        }

        // 걷는 플레이어가 올라선 파일. 클릭 선택과 같은 경로다.
        public void Select(string id) {
            if (!IsReady || id == _model.SelectedId) return;
            _model.Select(id);
            Render();
        }

        public void Execute(string id) {
            if (IsReady) Run(id);
        }

        private void Start() {
            _flow = SceneFlow.Instance;
            if (_flow == null) {
                Debug.LogError("[StageSelect] 흐름 서비스가 없다. LDY/Flow/Build Flow Assets 실행 필요", this);
                return;
            }

            var ui = new UiFactory(_flow.Skin);
            _model = new StageSelectModel(_flow.Catalog);
            _model.Focus(_flow.LastClearedId, Progress);

            titleBar.Build(ui, StageSelectText.WindowTitle);
            addressBar.Build(ui);
            tree.Build(ui, _model.Worlds());
            tabs.Build(ui);
            grid.Build(ui);
            statusBar.Build(ui);
            taskbar.Build(ui, StageSelectText.TaskTitle);
            dialog.Build(ui);
            if (HasSettings) settings.Build(ui, (IStageSelectSettings)settingsSource);
            Subscribe();
            Render();
        }

        private void OnDestroy() {
            if (_flow != null) _flow.Progress.Changed -= HandleProgressChanged;
        }

        private void Subscribe() {
            titleBar.CloseClicked += GoToTitle;
            taskbar.StartClicked += GoToTitle;
            tree.WorldClicked += OpenWorld;
            tree.SettingsClicked += OpenSettings;
            tabs.Changed += mode => {
                _model.SetFilter(mode, Progress);
                Render();
            };
            grid.Selected += id => {
                _model.Select(id);
                Render();
            };
            grid.Executed += Run;
            input.Moved += step => {
                if (ModalOpen) return;
                _model.MoveSelection(step);
                Render();
            };
            input.Executed += () => {
                if (SettingsOpen) settings.Close();
                else if (dialog.IsOpen) dialog.Close();
                else if (_model.SelectedId != null) Run(_model.SelectedId);
            };
            input.Back += () => {
                if (SettingsOpen) settings.Close();
                else if (dialog.IsOpen) dialog.Close();
                else GoToTitle();
            };
            _flow.Progress.Changed += HandleProgressChanged;
        }

        private void HandleProgressChanged() {
            _model.Refresh(Progress);
            Render();
        }

        private void Render() {
            addressBar.Show(_model.Address());
            tree.Show(_model.Worlds());
            tabs.Show(_model.Filter);
            grid.Show(_model.Items(Progress), _model.SelectedId);
            statusBar.Show(_model.Summary());
        }

        private void OpenWorld(int world) {
            if (_model.IsWorldLocked(world)) {
                ShowDenied(_flow.Catalog.World(world).displayName, _model.FirstIdOf(world));
                return;
            }

            _model.SetWorld(world, Progress);
            Render();
        }

        private void Run(string id) {
            if (_flow.IsBusy || ModalOpen) return;
            _model.Select(id);
            Render();
            var entry = _flow.Catalog.Find(id);
            switch (_model.StatusOf(id)) {
                case StageStatus.Locked:
                    ShowDenied(entry.fileName, id);
                    return;
                case StageStatus.NotReady:
                    dialog.Show(entry.fileName, StageSelectText.FileNotFound + "\n" + entry.fileName, _flow.Skin.iconError);
                    return;
                default:
                    _flow.TryEnterStage(id);
                    return;
            }
        }

        private void ShowDenied(string title, string lockedId) {
            var prerequisite = _model.PrerequisiteFileName(lockedId);
            var message = prerequisite == null
                ? StageSelectText.AccessDenied
                : StageSelectText.AccessDenied + "\n" + string.Format(StageSelectText.RunFirstFormat, prerequisite);
            dialog.Show(title, message, _flow.Skin.iconError);
        }

        private void OpenSettings() {
            if (ModalOpen) return;
            if (HasSettings) settings.Open();
            else dialog.Show(StageSelectText.SettingsFile, StageSelectText.SettingsMissing, _flow.Skin.iconError);
        }

        private void GoToTitle() {
            if (!_flow.IsBusy && !ModalOpen) _flow.ReturnToTitle();
        }
    }
}
