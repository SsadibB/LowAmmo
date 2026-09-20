#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using TMPro;
using LowAmmo.Player;
using LowAmmo.Weapon;
using LowAmmo.Puzzle;
using LowAmmo.Environment;
using LowAmmo.Level;
using LowAmmo.UI;

namespace LowAmmo.Editor
{
    public static class GameBuilder
    {
        private const string PrefabsPath = "Assets/_Game/Prefabs";
        private const string SpritesPath = "Assets/_Game/Sprites";

        private static Sprite LoadSprite(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/{name}.png");
        }

        private static void EnsureFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
        }

        [MenuItem("LowAmmo/Build All Prefabs")]
        public static void BuildAllPrefabs()
        {
            EnsureFolder($"{PrefabsPath}/Player");
            EnsureFolder($"{PrefabsPath}/PuzzleObjects");
            EnsureFolder($"{PrefabsPath}/Environment");
            EnsureFolder($"{PrefabsPath}/UI");

            BuildPlayerPrefab();
            BuildCratePrefab();
            BuildRopePrefab();
            BuildPressurePlatePrefab();
            BuildDoorPrefab();
            BuildValvePrefab();
            BuildWaterSourcePrefab();
            BuildFireHazardPrefab();
            BuildMovingPlatformPrefab();
            BuildBreakableGlassPrefab();
            BuildShootableSwitchPrefab();
            BuildDeathZonePrefab();
            BuildExitPrefab();
            BuildHUDCanvasPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[GameBuilder] All prefabs built successfully!");
        }

        public static GameObject BuildPlayerPrefab()
        {
            var go = new GameObject("Player");
            go.tag = "Player";
            go.layer = LayerMask.NameToLayer("Default");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("player");
            sr.sortingOrder = 10;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 3f;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 1.4f);
            col.offset = new Vector2(0f, 0f);

            var groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(go.transform);
            groundCheck.transform.localPosition = new Vector3(0f, -0.7f, 0f);

            var controller = go.AddComponent<PlayerController>();
            var pDeath = go.AddComponent<PlayerDeath>();

            // Setup serialized fields for PlayerController
            var so = new SerializedObject(controller);
            so.FindProperty("groundCheckPoint").objectReferenceValue = groundCheck.transform;
            so.FindProperty("groundCheckSize").vector2Value = new Vector2(0.7f, 0.2f);
            so.FindProperty("groundLayer").intValue = 1 << LayerMask.NameToLayer("Ground");
            so.FindProperty("bodySpriteRenderer").objectReferenceValue = sr;
            so.FindProperty("moveSpeed").floatValue = 8f;
            so.FindProperty("jumpForce").floatValue = 14f;
            so.ApplyModifiedProperties();

            // Gun Pivot & Gun
            var gunObj = new GameObject("Gun");
            gunObj.transform.SetParent(go.transform);
            gunObj.transform.localPosition = new Vector3(0.2f, 0f, 0f);

            var gunSr = gunObj.AddComponent<SpriteRenderer>();
            gunSr.sprite = LoadSprite("gun");
            gunSr.sortingOrder = 11;

            var firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(gunObj.transform);
            firePoint.transform.localPosition = new Vector3(0.45f, 0.05f, 0f);

            var tracerObj = new GameObject("BulletTracer");
            tracerObj.transform.SetParent(gunObj.transform);
            var lr = tracerObj.AddComponent<LineRenderer>();
            lr.startWidth = 0.05f;
            lr.endWidth = 0.02f;
            lr.useWorldSpace = true;
            lr.enabled = false;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.sortingOrder = 20;
            lr.startColor = new Color(1f, 0.95f, 0.4f, 1f);
            lr.endColor = new Color(1f, 0.5f, 0.1f, 0.2f);
            var tracer = tracerObj.AddComponent<BulletTracer>();

            var gun = gunObj.AddComponent<GunController>();
            var gunSo = new SerializedObject(gun);
            gunSo.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
            gunSo.FindProperty("gunSpriteRenderer").objectReferenceValue = gunSr;
            gunSo.FindProperty("bulletTracer").objectReferenceValue = tracer;
            // Raycast hits everything except layer 2 (Ignore Raycast) and maybe player itself
            gunSo.FindProperty("hitLayers").intValue = ~(1 << 2);
            gunSo.ApplyModifiedProperties();

            string path = $"{PrefabsPath}/Player/Player.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        public static GameObject BuildCratePrefab()
        {
            var go = new GameObject("Crate");
            go.layer = LayerMask.NameToLayer("Shootable");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("crate");
            sr.sortingOrder = 5;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.mass = 3f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f);

            string path = $"{PrefabsPath}/PuzzleObjects/Crate.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        public static GameObject BuildRopePrefab()
        {
            var go = new GameObject("ShootableRope");
            go.layer = LayerMask.NameToLayer("Shootable");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("rope");
            sr.sortingOrder = 4;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.4f, 1f);

            var rope = go.AddComponent<ShootableRope>();

            string path = $"{PrefabsPath}/PuzzleObjects/ShootableRope.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        public static GameObject BuildPressurePlatePrefab()
        {
            var go = new GameObject("PressurePlate");
            go.layer = LayerMask.NameToLayer("Ground");

            // Base trigger
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.4f, 0.4f);
            col.offset = new Vector2(0f, 0.1f);

            // Plate Visual / Top
            var topObj = new GameObject("PlateTop");
            topObj.transform.SetParent(go.transform);
            topObj.transform.localPosition = Vector3.zero;

            var sr = topObj.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("plate");
            sr.sortingOrder = 3;

            // Plate static collider so player can stand on it
            var staticCol = go.AddComponent<BoxCollider2D>();
            staticCol.isTrigger = false;
            staticCol.size = new Vector2(1.5f, 0.2f);
            staticCol.offset = new Vector2(0f, -0.1f);

            var plate = go.AddComponent<PressurePlate>();
            var so = new SerializedObject(plate);
            so.FindProperty("plateTop").objectReferenceValue = topObj.transform;
            so.FindProperty("pressDepth").floatValue = 0.15f;
            so.ApplyModifiedProperties();

            string path = $"{PrefabsPath}/PuzzleObjects/PressurePlate.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        public static GameObject BuildDoorPrefab()
        {
            var go = new GameObject("Door");
            go.layer = LayerMask.NameToLayer("Ground");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("door");
            sr.sortingOrder = 3;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.75f, 2f);

            var door = go.AddComponent<DoorController>();
            var so = new SerializedObject(door);
            so.FindProperty("openOffset").vector3Value = new Vector3(0f, 2.5f, 0f);
            so.FindProperty("moveSpeed").floatValue = 5f;
            so.ApplyModifiedProperties();

            string path = $"{PrefabsPath}/PuzzleObjects/Door.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        public static GameObject BuildValvePrefab()
        {
            var go = new GameObject("ShootableValve");
            go.layer = LayerMask.NameToLayer("Shootable");

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;

            var wheelObj = new GameObject("WheelVisual");
            wheelObj.transform.SetParent(go.transform);
            wheelObj.transform.localPosition = Vector3.zero;

            var sr = wheelObj.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("valve");
            sr.sortingOrder = 5;

            var valve = go.AddComponent<ShootableValve>();
            var so = new SerializedObject(valve);
            so.FindProperty("wheelTransform").objectReferenceValue = wheelObj.transform;
            so.FindProperty("rotationAngle").floatValue = 180f;
            so.ApplyModifiedProperties();

            string path = $"{PrefabsPath}/PuzzleObjects/ShootableValve.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        public static GameObject BuildWaterSourcePrefab()
        {
            var go = new GameObject("WaterSource");

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.5f, 3f);

            var visualObj = new GameObject("WaterVisual");
            visualObj.transform.SetParent(go.transform);
            visualObj.transform.localPosition = Vector3.zero;

            var sr = visualObj.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("water");
            sr.sortingOrder = 4;

            var water = go.AddComponent<WaterSource>();
            var so = new SerializedObject(water);
            so.FindProperty("waterFlowVisual").objectReferenceValue = visualObj;
            so.FindProperty("waterCollider").objectReferenceValue = col;
            so.ApplyModifiedProperties();

            string path = $"{PrefabsPath}/PuzzleObjects/WaterSource.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        public static GameObject BuildFireHazardPrefab()
        {
            var go = new GameObject("FireHazard");

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.8f, 1f);

            var visualObj = new GameObject("FireVisual");
            visualObj.transform.SetParent(go.transform);
            visualObj.transform.localPosition = Vector3.zero;

            var sr = visualObj.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("fire");
            sr.sortingOrder = 4;

            var fire = go.AddComponent<FireHazard>();
            var so = new SerializedObject(fire);
            so.FindProperty("fireVisual").objectReferenceValue = visualObj;
            so.ApplyModifiedProperties();

            string path = $"{PrefabsPath}/PuzzleObjects/FireHazard.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        public static GameObject BuildMovingPlatformPrefab()
        {
            var go = new GameObject("MovingPlatform");
            go.layer = LayerMask.NameToLayer("Ground");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("platform");
            sr.sortingOrder = 3;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2f, 0.5f);

            var platform = go.AddComponent<MovingPlatform>();

            string path = $"{PrefabsPath}/PuzzleObjects/MovingPlatform.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        public static GameObject BuildBreakableGlassPrefab()
        {
            var go = new GameObject("BreakableGlass");
            go.layer = LayerMask.NameToLayer("Shootable");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("glass");
            sr.sortingOrder = 4;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.5f, 1.5f);

            var glass = go.AddComponent<BreakableGlass>();

            string path = $"{PrefabsPath}/PuzzleObjects/BreakableGlass.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        public static GameObject BuildShootableSwitchPrefab()
        {
            var go = new GameObject("ShootableSwitch");
            go.layer = LayerMask.NameToLayer("Shootable");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("switch");
            sr.sortingOrder = 4;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.75f, 1f);

            var leverObj = new GameObject("Lever");
            leverObj.transform.SetParent(go.transform);
            leverObj.transform.localPosition = new Vector3(0f, 0.1f, 0f);

            var sw = go.AddComponent<ShootableSwitch>();
            var so = new SerializedObject(sw);
            so.FindProperty("switchLever").objectReferenceValue = leverObj.transform;
            so.ApplyModifiedProperties();

            string path = $"{PrefabsPath}/PuzzleObjects/ShootableSwitch.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        public static GameObject BuildDeathZonePrefab()
        {
            var go = new GameObject("DeathZone");

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(10f, 1f);

            go.AddComponent<DeathZone>();

            string path = $"{PrefabsPath}/Environment/DeathZone.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        public static GameObject BuildExitPrefab()
        {
            var go = new GameObject("Exit");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("exit");
            sr.sortingOrder = 2;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 1.5f);

            var exit = go.AddComponent<LevelExit>();
            var so = new SerializedObject(exit);
            so.FindProperty("exitRenderer").objectReferenceValue = sr;
            so.ApplyModifiedProperties();

            string path = $"{PrefabsPath}/Environment/LevelExit.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        public static GameObject BuildHUDCanvasPrefab()
        {
            var canvasObj = new GameObject("HUDCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // Always on top of all 2D sprites

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1.0f;

            canvasObj.AddComponent<GraphicRaycaster>();
            var hud = canvasObj.AddComponent<HUDController>();

            // Top Bar
            var topBar = CreateUIElement("TopBar", canvasObj.transform);
            SetRectTransform(topBar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(0, 100));
            var topBarImg = topBar.AddComponent<Image>();
            topBarImg.color = new Color(0.1f, 0.12f, 0.15f, 0.75f);

            // Ammo Text
            var ammoObj = CreateUIElement("AmmoText", topBar.transform);
            SetRectTransform(ammoObj, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(300, 60));
            var ammoText = ammoObj.AddComponent<TextMeshProUGUI>();
            ammoText.text = "AMMO: 2 / 2";
            ammoText.fontSize = 32;
            ammoText.color = Color.white;
            ammoText.alignment = TextAlignmentOptions.MidlineLeft;

            // Level Name Text
            var levelObj = CreateUIElement("LevelNameText", topBar.transform);
            SetRectTransform(levelObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(400, 60));
            var levelText = levelObj.AddComponent<TextMeshProUGUI>();
            levelText.text = "LEVEL 1";
            levelText.fontSize = 32;
            levelText.color = new Color(1f, 0.85f, 0.3f);
            levelText.alignment = TextAlignmentOptions.Center;

            // Timer Text
            var timerObj = CreateUIElement("TimerText", topBar.transform);
            SetRectTransform(timerObj, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-240, 0), new Vector2(160, 60));
            var timerText = timerObj.AddComponent<TextMeshProUGUI>();
            timerText.text = "00:00";
            timerText.fontSize = 28;
            timerText.color = new Color(0.8f, 0.85f, 0.9f);
            timerText.alignment = TextAlignmentOptions.MidlineRight;

            // Restart Button
            var restartBtnObj = CreateButton("RestartButton", topBar.transform, "RESTART (R)", new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-40, 0), new Vector2(160, 48));
            var restartBtn = restartBtnObj.GetComponent<Button>();

            // Level Complete Panel
            var completePanel = CreateUIElement("LevelCompletePanel", canvasObj.transform);
            SetRectTransform(completePanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500, 320));
            var compImg = completePanel.AddComponent<Image>();
            compImg.color = new Color(0.12f, 0.18f, 0.15f, 0.95f);

            var compTitleObj = CreateUIElement("CompleteTitle", completePanel.transform);
            SetRectTransform(compTitleObj, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(400, 60));
            var compTitle = compTitleObj.AddComponent<TextMeshProUGUI>();
            compTitle.text = "LEVEL COMPLETE!";
            compTitle.fontSize = 36;
            compTitle.color = new Color(0.3f, 1f, 0.5f);
            compTitle.alignment = TextAlignmentOptions.Center;

            var nextBtnObj = CreateButton("NextLevelButton", completePanel.transform, "NEXT LEVEL", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 70), new Vector2(240, 60));
            var nextBtn = nextBtnObj.GetComponent<Button>();

            // Game Over Panel
            var overPanel = CreateUIElement("GameOverPanel", canvasObj.transform);
            SetRectTransform(overPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500, 320));
            var overImg = overPanel.AddComponent<Image>();
            overImg.color = new Color(0.2f, 0.1f, 0.1f, 0.95f);

            var overTitleObj = CreateUIElement("GameOverTitle", overPanel.transform);
            SetRectTransform(overTitleObj, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(400, 60));
            var overTitle = overTitleObj.AddComponent<TextMeshProUGUI>();
            overTitle.text = "FAILED / OUT OF AMMO";
            overTitle.fontSize = 32;
            overTitle.color = new Color(1f, 0.35f, 0.35f);
            overTitle.alignment = TextAlignmentOptions.Center;

            var retryBtnObj = CreateButton("RetryButton", overPanel.transform, "RETRY (R)", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 70), new Vector2(240, 60));
            var retryBtn = retryBtnObj.GetComponent<Button>();

            // Wire serialized fields on HUDController
            var hudSo = new SerializedObject(hud);
            hudSo.FindProperty("ammoText").objectReferenceValue = ammoText;
            hudSo.FindProperty("levelNameText").objectReferenceValue = levelText;
            hudSo.FindProperty("timerText").objectReferenceValue = timerText;
            hudSo.FindProperty("restartButton").objectReferenceValue = restartBtn;
            hudSo.FindProperty("levelCompletePanel").objectReferenceValue = completePanel;
            hudSo.FindProperty("nextLevelButton").objectReferenceValue = nextBtn;
            hudSo.FindProperty("gameOverPanel").objectReferenceValue = overPanel;
            hudSo.FindProperty("retryButton").objectReferenceValue = retryBtn;
            hudSo.ApplyModifiedProperties();

            // Set panels inactive by default in prefab
            completePanel.SetActive(false);
            overPanel.SetActive(false);

            string path = $"{PrefabsPath}/UI/HUDCanvas.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(canvasObj, path);
            Object.DestroyImmediate(canvasObj);
            return prefab;
        }

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.transform.localScale = Vector3.one; // Strictly enforced (1, 1, 1)
            return go;
        }

        private static void SetRectTransform(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.localScale = Vector3.one; // STRICT (1, 1, 1)
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            rt.localScale = Vector3.one;
        }

        private static GameObject CreateButton(string name, Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var btnObj = CreateUIElement(name, parent);
            SetRectTransform(btnObj, anchorMin, anchorMax, pivot, anchoredPos, sizeDelta);

            var img = btnObj.AddComponent<Image>();
            img.color = new Color(0.25f, 0.3f, 0.38f, 1f);

            var btn = btnObj.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.35f, 0.42f, 0.52f, 1f);
            colors.pressedColor = new Color(0.18f, 0.22f, 0.28f, 1f);
            btn.colors = colors;

            var textObj = CreateUIElement("Label", btnObj.transform);
            SetRectTransform(textObj, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var txt = textObj.AddComponent<TextMeshProUGUI>();
            txt.text = label;
            txt.fontSize = 20;
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.Center;

            return btnObj;
        }
    }
}
#endif
