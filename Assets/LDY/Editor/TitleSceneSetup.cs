using System.Collections.Generic;
using System.IO;
using JJB.Script;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.Script.Editor
{
    public static class TitleSceneSetup
    {
        [MenuItem("LDY/Title/Setup Title Scene")]
        public static void Setup()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            new TitleSceneBuilder().Build();
        }
    }

    internal sealed class TitleSceneBuilder
    {
        private const string DeskLayerName = "Desk";
        private const string GroundLayerName = "Ground";
        private const string PlayerLayerName = "Player";
        private const string InputAssetPath = "Assets/JJB/JJBInput.inputactions";
        private const string PhysicsMaterialPath = "Assets/JJB/JJB.physicsMaterial2D";
        private const string SpriteFolder = "Assets/LDY/Sprites";
        private const string PrefabPath = "Assets/LDY/Prefab/AltPlayer.prefab";
        private const string MaterialPath = "Assets/LDY/Shader/CrtScreen.mat";
        private const string ScenePath = "Assets/Scenes/LDY_TitleScene.unity";
        private const string ArtSetPath = TitleArtImporter.Folder + "/TitleArtSet.asset";

        // 모니터 쿼드와 겹쳐야 하는 monitor_frame의 화면 구멍(이미지 왼쪽 위 기준 픽셀).
        private static readonly Rect MonitorHolePx = new Rect(48f, 56f, 480f, 360f);
        // room_background의 책상 윗면 선: 이미지 위에서부터의 픽셀.
        private const float RoomDeskLinePx = 520f;
        private const float DeskTopY = -2.4f;
        // 메인 카메라 높이의 절반. 방 배경(1280x720)이 16:9 화면에 정확히 맞는다.
        private const float RoomOrthoSize = 6.6f;
        // 책상 쪽 오브젝트(모니터, 본체, 키보드, 마우스)만 배경 대비 줄여서 카메라 안에 여유 있게 들인다.
        private const float DeskScale = 0.85f;
        private static readonly Vector2 MonitorCenter = new Vector2(-2f, 2f);
        private static readonly Vector2 MonitorQuadSize = new Vector2(8.8f, 6.6f) * DeskScale;
        private static readonly Vector2 TowerCenter = new Vector2(7.5f, -1f);

        // 발판 높이(화면 비율)를 이 점프 높이 기준으로 잡았다. 값을 바꾸면 아이콘 anchor도 다시 확인할 것.
        // 점프 정점은 약 3.97유닛(임펄스 14, 0.15초 뒤 추가 중력 30). 바닥에서 올라가는 발판은 윗면이 3.5 이하여야 한다.
        private const float PlayerJumpSpeed = 14f;

        private const float ContentX = 200f;
        private const float ContentOrthoSize = 5.4f;
        private const float ContentWidth = ContentOrthoSize * 2f * 4f / 3f;
        private const float ContentHeight = ContentOrthoSize * 2f;
        private const float FloorY = -ContentOrthoSize;

        private int _desk;
        private int _ground;
        private int _playerLayer;
        private Sprite _square;
        private Sprite _circle;
        private TMP_FontAsset _font;
        private TitleArtSet _art;
        // 책상 쪽 모든 도트가 같은 크기로 보이도록, 화면 구멍 1픽셀이 차지하는 월드 길이를 기준으로 삼는다.
        private float _pixel;
        // 배경 1픽셀의 월드 길이. 카메라 높이 / 배경 높이(px)
        private float _roomPixel;
        private float _roomCenterY;

        private sealed class MonitorScreenParts
        {
            public Transform Quad;
            public MonitorSpace Space;
            public MonitorPointerSource Pointer;
            public CrtScreen Screen;
        }

        private sealed class IconDefinition
        {
            public string Id;
            public string Label;
            public Vector2 Anchor;
            public Sprite Symbol;
            public Color SymbolTint;
        }

        public void Build()
        {
            _desk = EnsureLayer(DeskLayerName);
            _ground = EnsureLayer(GroundLayerName);
            _playerLayer = EnsureLayer(PlayerLayerName);
            if (_desk < 0 || _ground < 0 || _playerLayer < 0)
            {
                Debug.LogError("필요한 레이어(Desk/Ground/Player)를 만들 수 없습니다 (빈 레이어 슬롯 없음).");
                return;
            }

            _art = LoadArtSet();
            if (_art == null)
                return;

            _pixel = MonitorQuadSize.x / MonitorHolePx.width;
            _roomPixel = RoomOrthoSize * 2f / _art.roomBackground.rect.height;
            _roomCenterY = DeskTopY + (RoomDeskLinePx - _art.roomBackground.rect.height * 0.5f) * _roomPixel;
            _square = EnsureSprite("Square", 4, 4, FilterMode.Point, (u, v) => Color.white);
            _circle = EnsureSprite("Circle", 64, 64, FilterMode.Bilinear, TempShapes.Circle);
            Sprite wallpaper = _art.screenWallpaper;
            Sprite arrow = _art.playerCursor;

            Material crtMaterial = EnsureCrtMaterial();
            GameObject playerPrefab = EnsurePlayerPrefab(arrow);
            if (crtMaterial == null || playerPrefab == null)
                return;

            _font = FindKoreanFont();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Camera mainCam = CreateMainCamera();
            Camera contentCam = CreateContentCamera();

            var systems = new GameObject("TitleSystems");
            var clock = systems.AddComponent<TitleClock>();
            var subScreens = systems.AddComponent<SubScreenController>();
            var transition = systems.AddComponent<GameStartTransition>();
            var loader = systems.AddComponent<LoggingSceneLoader>();
            var zoom = systems.AddComponent<MonitorZoomRig>();
            var bootstrap = systems.AddComponent<TitleBootstrap>();

            MonitorScreenParts monitor = CreateDesk(clock, crtMaterial, mainCam, contentCam);
            var icons = new[]
            {
                new IconDefinition { Id = "Setting", Label = "Setting.png", Anchor = new Vector2(0.2f, 0.235f), Symbol = _art.symbolSetting, SymbolTint = Color.white },
                new IconDefinition { Id = "Credit", Label = "Credit.jpg", Anchor = new Vector2(0.78f, 0.48f), Symbol = _art.symbolCredit, SymbolTint = Color.white },
                new IconDefinition { Id = "GameStart", Label = "GameStart", Anchor = new Vector2(0.5f, 0.198f), Symbol = _art.symbolGameStart, SymbolTint = Color.white },
                new IconDefinition { Id = "Quit", Label = "나가기", Anchor = new Vector2(0.86f, 0.16f), Symbol = _art.symbolExit, SymbolTint = Color.white }
            };
            GameObject player = CreateContent(contentCam, subScreens, transition, playerPrefab, monitor, wallpaper,
                arrow, icons);
            CreateDeskMouse(clock, player.transform);

            Wire(transition, "zoomRig", zoom);
            Wire(transition, "sceneLoader", loader);
            Wire(zoom, "viewCamera", mainCam);
            Wire(zoom, "screenQuad", monitor.Quad);
            Wire(mainCam.gameObject.AddComponent<DeskCameraFit>(), "background",
                GameObject.Find("RoomBackground").GetComponent<SpriteRenderer>());
            Wire(bootstrap, "player", player);
            Wire(bootstrap, "subScreens", subScreens);
            Wire(bootstrap, "transition", transition);
            Wire(bootstrap, "monitorSpace", monitor.Space);
            Wire(player.GetComponent<AltModeController>(), "pointerOverride", monitor.Pointer);

            CreateProbe(player);

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), ScenePath);
            Selection.activeGameObject = GameObject.Find("TitleVisual");
            Debug.Log("타이틀 씬 세팅 완료 (Assets/Scenes/LDY_TitleScene.unity). 이미지/배치는 TitleVisual 오브젝트의 " +
                      "TitleVisualSettings에서 교체하세요. Play 후 A/D, Space, E, Alt+클릭, ESC로 확인합니다.");
            WarnMissingGlyphs();
        }

        // 설정 에셋을 불러오거나 만들고, 비어 있는 슬롯을 Art/Title 폴더의 같은 이름 PNG로 채운다.
        // 이미 연결된 슬롯은 건드리지 않으므로 셋업을 다시 실행해도 손으로 바꾼 연결이 유지된다.
        private static TitleArtSet LoadArtSet()
        {
            TitleArtImporter.ReimportAll();

            var art = AssetDatabase.LoadAssetAtPath<TitleArtSet>(ArtSetPath);
            if (art == null)
            {
                art = ScriptableObject.CreateInstance<TitleArtSet>();
                AssetDatabase.CreateAsset(art, ArtSetPath);
            }

            var so = new SerializedObject(art);
            var missing = new List<string>();
            foreach (var (field, file) in ArtSlots)
            {
                SerializedProperty slot = so.FindProperty(field);
                if (slot.objectReferenceValue == null)
                    slot.objectReferenceValue = FindArtSprite(file);

                if (slot.objectReferenceValue == null)
                    missing.Add($"{field} ({file}.png)");
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();

            if (missing.Count == 0)
                return art;

            Debug.LogError($"타이틀 에셋 슬롯이 비어 있습니다: {string.Join(", ", missing)}\n" +
                           $"{TitleArtImporter.Folder} 아래에 PNG를 넣거나 {ArtSetPath}의 슬롯에 직접 연결한 뒤 다시 실행하세요.");
            return null;
        }

        private static Sprite FindArtSprite(string fileName)
        {
            foreach (string guid in AssetDatabase.FindAssets($"{fileName} t:Sprite", new[] { TitleArtImporter.Folder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == fileName)
                    return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }

            return null;
        }

        // TitleArtSet 필드 이름 -> 자동 연결에 쓰는 PNG 파일 이름
        private static readonly (string field, string file)[] ArtSlots =
        {
            ("roomBackground", "room_background"), ("monitorFrame", "monitor_frame"), ("pcTower", "pc_tower"),
            ("fanLarge", "fan_large"), ("fanSmall", "fan_small"), ("keyboard", "keyboard"), ("mouse", "mouse"),
            ("screenWallpaper", "screen_wallpaper"), ("iconTile", "icon_tile"), ("symbolSetting", "symbol_setting"),
            ("symbolCredit", "symbol_credit"), ("symbolGameStart", "symbol_gamestart"), ("symbolExit", "symbol_exit"),
            ("playerCursor", "player_cursor")
        };

        private Camera CreateMainCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
            cam.orthographic = true;
            cam.orthographicSize = RoomOrthoSize;
            // 카메라를 배경 중심에 둔다. 16:9가 아니면 DeskCameraFit이 런타임에 크기를 다시 맞춘다.
            cam.transform.position = new Vector3(0f, _roomCenterY, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.07f, 0.07f);
            cam.cullingMask = 1 << _desk;
            cam.depth = 0f;
            return cam;
        }

        private Camera CreateContentCamera()
        {
            var go = new GameObject("MonitorContentCamera");
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = ContentOrthoSize;
            cam.transform.position = new Vector3(ContentX, 0f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.04f, 0.06f);
            cam.cullingMask = ~(1 << _desk);
            cam.depth = -10f;
            return cam;
        }

        private MonitorScreenParts CreateDesk(TitleClock clock, Material crtMaterial, Camera mainCam,
            Camera contentCam)
        {
            var desk = new GameObject("Desk").transform;

            // 책상 윗면 선(이미지 위에서 520px)이 DeskTopY에 오도록 배경 높이를 맞춘다.
            PlaceSprite("RoomBackground", desk, _art.roomBackground, new Vector2(0f, _roomCenterY), -20, _roomPixel);

            var quad = new GameObject("MonitorScreen") { layer = _desk };
            quad.transform.SetParent(desk, false);
            quad.transform.position = new Vector3(MonitorCenter.x, MonitorCenter.y, 0f);
            quad.transform.localScale = new Vector3(MonitorQuadSize.x, MonitorQuadSize.y, 1f);
            quad.AddComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
            var meshRenderer = quad.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = crtMaterial;
            meshRenderer.sortingOrder = 2;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;

            // 프레임의 화면 구멍 중심이 쿼드 중심에 오도록 프레임을 놓는다. 쿼드보다 앞(order 3)에 그린다.
            Sprite frame = _art.monitorFrame;
            Vector2 holeFromImageCenter = new Vector2(
                MonitorHolePx.center.x - frame.rect.width * 0.5f,
                frame.rect.height * 0.5f - MonitorHolePx.center.y) * _pixel;
            SpriteRenderer frameRenderer = PlaceSprite("MonitorFrame", desk, frame, MonitorCenter - holeFromImageCenter, 3);
            float frameBottom = frameRenderer.transform.position.y - frame.rect.height * 0.5f * _pixel;

            // 받침대는 기존 임시 도형을 쓰되 프레임 바로 아래에 보이도록 위치를 잡는다.
            const float tuck = 0.1f;
            Block("MonitorStand", desk, new Vector2(MonitorCenter.x, frameBottom - 0.35f * DeskScale + tuck),
                new Vector2(4f, 0.7f) * DeskScale,
                new Color(0.227f, 0.239f, 0.267f), -1, _desk);
            float baseTop = frameBottom - 0.7f * DeskScale + tuck;
            Block("MonitorBase", desk, new Vector2(MonitorCenter.x, baseTop - 0.2f * DeskScale),
                new Vector2(6f, 0.4f) * DeskScale,
                new Color(0.141f, 0.149f, 0.169f), -1, _desk);
            float baseBottom = baseTop - 0.4f * DeskScale;

            var screen = quad.AddComponent<CrtScreen>();
            var driver = quad.AddComponent<CrtMaterialDriver>();
            var space = quad.AddComponent<MonitorSpace>();
            var pointer = quad.AddComponent<MonitorPointerSource>();
            Wire(screen, "contentCamera", contentCam);
            Wire(driver, "screenRenderer", meshRenderer);
            Wire(driver, "screen", screen);
            Wire(driver, "clock", clock);
            Wire(space, "viewCamera", mainCam);
            Wire(space, "contentCamera", contentCam);
            Wire(space, "screenQuad", quad.transform);
            Wire(space, "crt", driver);
            Wire(pointer, "space", space);

            CreateKeyboard(desk, baseBottom);
            CreateCase(desk, clock);

            return new MonitorScreenParts { Quad = quad.transform, Space = space, Pointer = pointer, Screen = screen };
        }

        // 키보드는 기존 X 위치에 두고, 윗면이 받침대 바닥 바로 아래에 오게 한다.
        private void CreateKeyboard(Transform desk, float baseBottom)
        {
            Sprite keyboard = _art.keyboard;
            float y = baseBottom - 0.15f - keyboard.rect.height * 0.5f * _pixel;
            PlaceSprite("Keyboard", desk, keyboard, new Vector2(MonitorCenter.x, y), 4);
        }

        private void CreateCase(Transform desk, TitleClock clock)
        {
            var root = new GameObject("Case") { layer = _desk };
            root.transform.SetParent(desk, false);
            root.transform.position = new Vector3(TowerCenter.x, TowerCenter.y, 0f);

            Sprite tower = _art.pcTower;
            PlaceSprite("Body", root.transform, tower, Vector2.zero, 0);

            // 팬 중심은 본체 이미지(256x512) 기준 픽셀 좌표
            CreateFan("CaseFan", root.transform, clock, _art.fanLarge, TowerPx(tower, 128f, 232f), 540f);
            CreateFan("GpuFan", root.transform, clock, _art.fanSmall, TowerPx(tower, 128f, 408f), 760f);

            // 본체 그림에 그려진 전원/디스크 LED(16x16px) 위에 점멸용 원을 겹친다.
            float ledSize = 16f * _pixel;
            CreateLed("PowerLed", root.transform, clock, TowerPx(tower, 56f, 96f), ledSize, Color.green,
                new Color(0.05f, 0.2f, 0.05f), true);
            CreateLed("DiskLed", root.transform, clock, TowerPx(tower, 84f, 96f), ledSize, new Color(1f, 0.65f, 0.1f),
                new Color(0.25f, 0.15f, 0.03f), false);

            root.AddComponent<AudioSource>().playOnAwake = false;
            Wire(root.AddComponent<FanSoundLoop>(), "clock", clock);
        }

        // 이미지 왼쪽 위 기준 픽셀 -> 이미지 중심 기준 로컬 좌표(월드 길이)
        private Vector2 TowerPx(Sprite sprite, float px, float py)
        {
            return new Vector2(px - sprite.rect.width * 0.5f, sprite.rect.height * 0.5f - py) * _pixel;
        }

        // 팬 스프라이트의 중심이 회전 중심이므로 오브젝트를 그대로 돌리면 본체 이미지의 지정 좌표를 축으로 돈다.
        private void CreateFan(string name, Transform parent, TitleClock clock, Sprite sprite, Vector2 pos, float speed)
        {
            SpriteRenderer fan = PlaceSprite(name, parent, sprite, pos, 1);
            var spinner = fan.gameObject.AddComponent<FanSpinner>();
            Wire(spinner, "clock", clock);
            SetFloat(spinner, "degreesPerSecond", speed);
        }

        private void CreateLed(string name, Transform parent, TitleClock clock, Vector2 pos, float size, Color on,
            Color off, bool steady)
        {
            SpriteRenderer led = Circle(name, parent, pos, size, on, 2);
            var blinker = led.gameObject.AddComponent<LedBlinker>();
            Wire(blinker, "clock", clock);
            Wire(blinker, "led", led);
            SetColor(blinker, "onColor", on);
            SetColor(blinker, "offColor", off);
            SetBool(blinker, "steady", steady);
        }

        // 마우스 기준점: 가로는 키보드 오른쪽 끝과 본체 왼쪽 끝의 중앙, 세로는 책상 윗면 선과 화면 아래 끝의 중간.
        // 둘 다 Renderer bounds와 카메라 값에서 계산하므로 배치가 바뀌어도 따라간다.
        private void CreateDeskMouse(TitleClock clock, Transform player)
        {
            Transform desk = GameObject.Find("Desk").transform;
            var keyboard = desk.Find("Keyboard").GetComponent<SpriteRenderer>();
            var tower = desk.Find("Case/Body").GetComponent<SpriteRenderer>();
            float x = (keyboard.bounds.max.x + tower.bounds.min.x) * 0.5f;
            float screenBottom = _roomCenterY - RoomOrthoSize;
            float y = (DeskTopY + screenBottom) * 0.5f;
            SpriteRenderer mouse = PlaceSprite("DeskMouse", desk, _art.mouse, new Vector2(x, y), 5);
            var follower = mouse.gameObject.AddComponent<DeskMouseFollower>();
            Wire(follower, "clock", clock);
            Wire(follower, "source", player);
            Wire(follower, "leftObstacle", keyboard);
            Wire(follower, "rightObstacle", tower);
        }

        // 스프라이트 PPU와 상관없이 원본 1픽셀이 _pixel 월드 길이가 되도록 스케일을 정한다.
        private SpriteRenderer PlaceSprite(string name, Transform parent, Sprite sprite, Vector2 localPosition, int order,
            float pixel = 0f)
        {
            var go = new GameObject(name) { layer = _desk };
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            float scale = (pixel > 0f ? pixel : _pixel) * sprite.pixelsPerUnit;
            go.transform.localScale = new Vector3(scale, scale, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return sr;
        }

        private GameObject CreateContent(Camera contentCam, SubScreenController subScreens,
            GameStartTransition transition, GameObject playerPrefab, MonitorScreenParts monitor, Sprite wallpaperSprite,
            Sprite playerSprite, IconDefinition[] icons)
        {
            var content = new GameObject("MonitorContent").transform;
            content.position = new Vector3(ContentX, 0f, 0f);

            SpriteRenderer wallpaper = Block("Wallpaper", content, Vector2.zero, new Vector2(ContentWidth, ContentHeight),
                Color.white, -100, 0);
            TextMeshPro logoLabel = Text("Logo", content, Vector2.zero, "Cursor", 18f, Color.white, 2, new Vector2(10f, 3f));
            SpriteRenderer logoSprite = Block("LogoSprite", content, Vector2.zero, Vector2.one, Color.white, 2, 0);
            logoSprite.sprite = null;
            SpriteRenderer underline = Block("LogoUnderline", content, Vector2.zero, Vector2.one, Color.white, 1, 0);

            var walls = new GameObject("ScreenBounds") { layer = _ground };
            walls.transform.SetParent(content, false);
            Wire(walls.AddComponent<ContentBoundsWalls>(), "contentCamera", contentCam);

            var slots = new List<(IconDefinition definition, GameObject icon)>();
            foreach (IconDefinition definition in icons)
            {
                GameObject icon = CreateIcon(content, $"Icon_{definition.Id}", false, false);
                slots.Add((definition, icon));
            }

            SubScreenPanel settingPanel = CreatePanel(content, "Panel_Setting", "Setting", subScreens);
            SubScreenPanel creditPanel = CreatePanel(content, "Panel_Credit", "Credit", subScreens);
            WireArray(subScreens, "panels", new Object[] { settingPanel, creditPanel });

            foreach ((IconDefinition definition, GameObject icon) in slots)
            {
                MenuActionBehaviour action = definition.Id switch
                {
                    "GameStart" => CreateStartAction(icon, transition),
                    "Setting" => CreateOpenAction(icon, subScreens, settingPanel),
                    "Credit" => CreateOpenAction(icon, subScreens, creditPanel),
                    _ => icon.AddComponent<QuitAction>()
                };
                Wire(icon.GetComponent<MenuIcon>(), "action", action);
            }

            var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);

            var visual = new GameObject("TitleVisual");
            var settings = visual.AddComponent<TitleVisualSettings>();
            var applier = visual.AddComponent<TitleVisualApplier>();
            Wire(applier, "contentCamera", contentCam);
            Wire(applier, "crtScreen", monitor.Screen);
            Wire(applier, "wallpaper", wallpaper);
            Wire(applier, "logoLabel", logoLabel);
            Wire(applier, "logoSprite", logoSprite);
            Wire(applier, "underline", underline);
            Wire(applier, "player", player);

            underline.sprite = _square;
            Wire(settings, "wallpaperSprite", wallpaperSprite);
            Wire(settings, "playerSprite", playerSprite);
            FillIconSlots(settings, slots);

            applier.Apply(true);
            return player;
        }

        private void FillIconSlots(TitleVisualSettings settings, List<(IconDefinition definition, GameObject icon)> slots)
        {
            var so = new SerializedObject(settings);
            SerializedProperty array = so.FindProperty("icons");
            array.arraySize = slots.Count;
            for (int i = 0; i < slots.Count; i++)
            {
                SerializedProperty element = array.GetArrayElementAtIndex(i);
                IconDefinition definition = slots[i].definition;
                element.FindPropertyRelative("id").stringValue = definition.Id;
                element.FindPropertyRelative("visual").objectReferenceValue = slots[i].icon.GetComponent<MenuIconVisual>();
                element.FindPropertyRelative("labelText").stringValue = definition.Label;
                element.FindPropertyRelative("anchor").vector2Value = definition.Anchor;
                element.FindPropertyRelative("tileSprite").objectReferenceValue = _art.iconTile;
                element.FindPropertyRelative("tileTint").colorValue = Color.white;
                element.FindPropertyRelative("symbolSprite").objectReferenceValue = definition.Symbol;
                element.FindPropertyRelative("symbolTint").colorValue = definition.SymbolTint;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static MenuActionBehaviour CreateStartAction(GameObject icon, GameStartTransition transition)
        {
            var action = icon.AddComponent<StartGameAction>();
            Wire(action, "transition", transition);
            return action;
        }

        private static MenuActionBehaviour CreateOpenAction(GameObject icon, SubScreenController controller,
            SubScreenPanel panel)
        {
            var action = icon.AddComponent<OpenSubScreenAction>();
            Wire(action, "controller", controller);
            Wire(action, "panel", panel);
            return action;
        }

        private SubScreenPanel CreatePanel(Transform content, string name, string title, SubScreenController controller)
        {
            var root = new GameObject(name);
            root.transform.SetParent(content, false);
            root.transform.localPosition = new Vector3(0f, 0.2f, 0f);

            Block("Background", root.transform, Vector2.zero, new Vector2(9f, 6f), new Color(0.07f, 0.08f, 0.12f), 10, 0);
            Text("Title", root.transform, new Vector2(0f, 2.3f), title, 9f, Color.white, 11, new Vector2(8f, 1.5f));
            var contentRoot = new GameObject("ContentRoot");
            contentRoot.transform.SetParent(root.transform, false);

            var panel = root.AddComponent<SubScreenPanel>();
            Wire(panel, "contentRoot", contentRoot.transform);

            GameObject close = CreateIcon(root.transform, "CloseButton", true, true);
            close.transform.localPosition = new Vector3(3.2f, -3.5f, 0f);
            var visual = close.GetComponent<MenuIconVisual>();
            visual.SetContent(new TitleIconSlot { labelText = "Close", tileSprite = _square, tileTint = new Color(0.85f, 0.4f, 0.4f) });
            visual.Layout(new Vector2(1.4f, 0.7f), 0f, 0.1f, 3f);

            var closeAction = close.AddComponent<CloseSubScreenAction>();
            Wire(closeAction, "controller", controller);
            Wire(close.GetComponent<MenuIcon>(), "action", closeAction);
            root.SetActive(false);
            return panel;
        }

        // 아이콘 루트 아래에 발판(Platform: 콜라이더, Tile, Symbol), 판정 영역, 이름 라벨을 만든다.
        // 크기와 내용은 MenuIconVisual이 정하므로 여기서는 구조만 만든다. 액션은 호출 측에서 붙인다.
        private GameObject CreateIcon(Transform parent, string name, bool labelInside, bool usableWhileBlocked)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);

            var platformGo = new GameObject("Platform") { layer = _ground };
            platformGo.transform.SetParent(root.transform, false);
            var platform = platformGo.AddComponent<BoxCollider2D>();

            SpriteRenderer tile = Block("Tile", platformGo.transform, Vector2.zero, Vector2.one, Color.black, 5, 0);
            SpriteRenderer symbol = Block("Symbol", platformGo.transform, Vector2.zero, Vector2.one, Color.white, 6, 0);
            symbol.sprite = null;

            var hit = new GameObject("HitArea");
            hit.transform.SetParent(root.transform, false);
            var hitCollider = hit.AddComponent<BoxCollider2D>();
            hitCollider.isTrigger = true;

            TextMeshPro label = Text("Label", root.transform, Vector2.zero, "", 3.2f, Color.white, 7, new Vector2(3f, 0.4f));

            var visual = root.AddComponent<MenuIconVisual>();
            Wire(visual, "platform", platform);
            Wire(visual, "hitArea", hitCollider);
            Wire(visual, "tile", tile);
            Wire(visual, "symbol", symbol);
            Wire(visual, "label", label);
            SetBool(visual, "labelInside", labelInside);

            var menuIcon = root.AddComponent<MenuIcon>();
            Wire(menuIcon, "platform", platform);
            Wire(menuIcon, "hitArea", hitCollider);
            Wire(menuIcon, "iconRenderer", tile);
            Wire(menuIcon, "label", label);
            SetBool(menuIcon, "usableWhileBlocked", usableWhileBlocked);
            return root;
        }

        // 빌드에 포함되지 않도록 EditorOnly 태그를 단 별도 오브젝트에 둔다.
        private static void CreateProbe(GameObject player)
        {
            var go = new GameObject("TitleProbe") { tag = "EditorOnly" };
            Wire(go.AddComponent<TitleClickProbe>(), "alt", player.GetComponent<AltModeController>());
        }

        private GameObject EnsurePlayerPrefab(Sprite sprite)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existing != null)
            {
                ApplyJumpTuning();
                return existing;
            }

            var go = new GameObject("AltPlayer") { layer = _playerLayer };
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 20;

            go.AddComponent<BoxCollider2D>().size = Vector2.one;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.sharedMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(PhysicsMaterialPath);

            var groundCheck = new GameObject("GroundCheck").transform;
            groundCheck.SetParent(go.transform, false);
            groundCheck.localPosition = new Vector3(0f, -0.5f, 0f);

            var playerInput = go.AddComponent<PlayerInput>();
            var inputSo = new SerializedObject(playerInput);
            inputSo.FindProperty("m_Actions").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputAssetPath);
            inputSo.FindProperty("m_DefaultActionMap").stringValue = "Player";
            inputSo.ApplyModifiedPropertiesWithoutUndo();

            go.AddComponent<JJBPlayerInput>();
            var movement = go.AddComponent<JJBPlayerMovement>();
            var jump = go.AddComponent<JJBPlayerJump>();
            go.AddComponent<JJBPlayer>();

            SetFloat(movement, "moveSpeed", 5f);
            SetFloat(jump, "jumpSpeed", PlayerJumpSpeed);
            Wire(jump, "groundCheck", groundCheck);
            var jumpSo = new SerializedObject(jump);
            jumpSo.FindProperty("groundCheckSize").vector2Value = new Vector2(0.9f, 0.12f);
            jumpSo.FindProperty("groundLayer").intValue = 1 << _ground;
            jumpSo.ApplyModifiedPropertiesWithoutUndo();

            go.AddComponent<PlayerInputLock>();
            go.AddComponent<PlayerAbilityGate>();
            go.AddComponent<JJBControlGate>();
            go.AddComponent<AltModeController>();

            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        // 이미 만들어진 프리팹도 발판 배치 기준 점프 속도로 맞춘다.
        private static void ApplyJumpTuning()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            var jump = root.GetComponent<JJBPlayerJump>();
            if (jump != null)
            {
                SetFloat(jump, "jumpSpeed", PlayerJumpSpeed);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }

            PrefabUtility.UnloadPrefabContents(root);
        }

        private static Material EnsureCrtMaterial()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (existing != null)
                return existing;

            Shader shader = Shader.Find("LDY/CrtScreen");
            if (shader == null)
            {
                Debug.LogError("LDY/CrtScreen 셰이더를 찾을 수 없습니다. 셰이더 컴파일 에러를 확인하세요.");
                return null;
            }

            var material = new Material(shader);
            AssetDatabase.CreateAsset(material, MaterialPath);
            return material;
        }

        private static TMP_FontAsset FindKoreanFont()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:TMP_FontAsset"))
            {
                var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (font != null && SupportsGlyph(font, '나'))
                    return font;
            }

            return null;
        }

        private static bool SupportsGlyph(TMP_FontAsset font, char c)
        {
            if (font.HasCharacter(c))
                return true;

            if (font.fallbackFontAssetTable != null)
            {
                foreach (TMP_FontAsset fallback in font.fallbackFontAssetTable)
                {
                    if (fallback != null && fallback.HasCharacter(c))
                        return true;
                }
            }

            return false;
        }

        // 라벨에 쓰는 글자가 폰트에 없으면 깨져 보이므로 셋업 때 어떤 글자가 문제인지 알려준다.
        private void WarnMissingGlyphs()
        {
            TMP_FontAsset font = _font != null ? _font : TMP_Settings.defaultFontAsset;
            var settings = Object.FindFirstObjectByType<TitleVisualSettings>();
            if (font == null || settings == null)
                return;

            var texts = new List<string> { settings.logoText };
            foreach (TitleIconSlot slot in settings.icons)
                texts.Add(slot.labelText);

            var missing = new HashSet<char>();
            foreach (string text in texts)
            {
                foreach (char c in text)
                {
                    if (!char.IsWhiteSpace(c) && !SupportsGlyph(font, c))
                        missing.Add(c);
                }
            }

            if (missing.Count == 0)
                return;

            Debug.LogWarning($"[Title] 폰트 '{font.name}'에 없는 글자: {string.Join(" ", missing)} (라벨이 □로 깨져 보입니다).\n" +
                             "한글 폰트 에셋 만들기: 1) Window > TextMeshPro > Font Asset Creator  2) Source Font File에 한글 폰트(.ttf)를 지정 " +
                             "(예: 프로젝트에 넣은 NanumGothic 등)  3) Character Set을 Unicode Range(Hex)로 두고 Hangul Syllables(AC00-D7AF) 또는 " +
                             "Custom Characters에 사용할 글자 입력  4) Atlas Population Mode를 Dynamic으로 두고 Generate/Save  " +
                             "5) 저장한 에셋이 프로젝트에 있으면 이 셋업을 다시 실행해 자동으로 라벨에 적용합니다.");
        }

        private static Sprite EnsureSprite(string name, int size, int pixelsPerUnit, FilterMode filter,
            System.Func<float, float, Color> paint)
        {
            return EnsureSprite(name, size, size, pixelsPerUnit, filter, paint);
        }

        private static Sprite EnsureSprite(string name, int width, int height, int pixelsPerUnit, FilterMode filter,
            System.Func<float, float, Color> paint)
        {
            string path = $"{SpriteFolder}/{name}.png";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null)
                return existing;

            Directory.CreateDirectory(SpriteFolder);
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    pixels[y * width + x] = paint((x + 0.5f) / width, (y + 0.5f) / height);
            }

            texture.SetPixels(pixels);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = filter;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private SpriteRenderer Block(string name, Transform parent, Vector2 position, Vector2 size, Color color,
            int order, int layer)
        {
            var go = new GameObject(name) { layer = layer };
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _square;
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }

        private SpriteRenderer Circle(string name, Transform parent, Vector2 position, float diameter, Color color,
            int order)
        {
            var go = new GameObject(name) { layer = _desk };
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = new Vector3(diameter, diameter, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _circle;
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }

        private TextMeshPro Text(string name, Transform parent, Vector2 position, string text, float fontSize,
            Color color, int order, Vector2 rectSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;

            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            tmp.sortingOrder = order;
            tmp.rectTransform.sizeDelta = rectSize;
            if (_font != null)
                tmp.font = _font;
            return tmp;
        }

        private static void Wire(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireArray(Object target, string field, Object[] values)
        {
            var so = new SerializedObject(target);
            SerializedProperty array = so.FindProperty(field);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloat(Object target, string field, float value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetBool(Object target, string field, bool value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetColor(Object target, string field, Color value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).colorValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static int EnsureLayer(string layerName)
        {
            int existing = LayerMask.NameToLayer(layerName);
            if (existing >= 0)
                return existing;

            var tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty slot = layers.GetArrayElementAtIndex(i);
                if (!string.IsNullOrEmpty(slot.stringValue))
                    continue;

                slot.stringValue = layerName;
                tagManager.ApplyModifiedPropertiesWithoutUndo();
                return i;
            }

            return -1;
        }
    }
}
