#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine.SceneManagement;
using TMPro;
using LowAmmo.Player;
using LowAmmo.Weapon;
using LowAmmo.Puzzle;
using LowAmmo.Environment;
using LowAmmo.Level;
using LowAmmo.UI;

namespace LowAmmo.Editor
{
    public static class LevelBuilder
    {
        private const string ScenesPath = "Assets/_Game/Scenes";
        private const string PrefabsPath = "Assets/_Game/Prefabs";
        private const string SpritesPath = "Assets/_Game/Sprites";

        private static Sprite LoadSprite(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/{name}.png");
        }

        private static GameObject LoadPrefab(string subpath)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/{subpath}.prefab");
        }

        [MenuItem("LowAmmo/Build All 5 Levels")]
        public static void BuildAllLevels()
        {
            if (!Directory.Exists(ScenesPath))
            {
                Directory.CreateDirectory(ScenesPath);
            }

            BuildLevel01();
            BuildLevel02();
            BuildLevel03();
            BuildLevel04();
            BuildLevel05();

            // Setup Build Settings
            string[] scenePaths = new string[]
            {
                $"{ScenesPath}/Level01.unity",
                $"{ScenesPath}/Level02.unity",
                $"{ScenesPath}/Level03.unity",
                $"{ScenesPath}/Level04.unity",
                $"{ScenesPath}/Level05.unity"
            };

            EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[scenePaths.Length];
            for (int i = 0; i < scenePaths.Length; i++)
            {
                buildScenes[i] = new EditorBuildSettingsScene(scenePaths[i], true);
            }
            EditorBuildSettings.scenes = buildScenes;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[LevelBuilder] All 5 levels built and added to Build Settings successfully!");
        }

        private static GameObject CreateGroundBlock(string name, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name);
            go.layer = LayerMask.NameToLayer("Ground");
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("ground");
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = size;
            sr.sortingOrder = 1;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = size;

            return go;
        }

        private static (LevelManager, GameObject, CameraFollow2D) SetupBaseScene(string levelName, int ammo, string nextScene)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Level Manager
            var lmObj = new GameObject("LevelManager");
            var lm = lmObj.AddComponent<LevelManager>();
            var lmSo = new SerializedObject(lm);
            lmSo.FindProperty("levelName").stringValue = levelName;
            lmSo.FindProperty("startingAmmo").intValue = ammo;
            lmSo.FindProperty("nextSceneName").stringValue = nextScene;
            lmSo.ApplyModifiedProperties();

            // Camera
            var camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.10f, 0.14f);
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000f;
            camObj.transform.position = new Vector3(0f, 2f, -10f);

            var camFollow = camObj.AddComponent<CameraFollow2D>();
            var camFollowSo = new SerializedObject(camFollow);
            camFollowSo.FindProperty("clampY").boolValue = true;
            camFollowSo.FindProperty("minY").floatValue = 1.5f;
            camFollowSo.ApplyModifiedProperties();

            // Universal 2D Directional Light
            var lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.0f;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // EventSystem
            var esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // UI HUDCanvas
            var hudPrefab = LoadPrefab("UI/HUDCanvas");
            var hudInstance = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab);
            hudInstance.name = "HUDCanvas";

            // Bottom Pit DeathZone
            var deathPrefab = LoadPrefab("Environment/DeathZone");
            var deathObj = (GameObject)PrefabUtility.InstantiatePrefab(deathPrefab);
            deathObj.transform.position = new Vector3(0f, -8f, 0f);
            var deathCol = deathObj.GetComponent<BoxCollider2D>();
            deathCol.size = new Vector2(50f, 2f);

            return (lm, hudInstance, camFollow);
        }

        private static GameObject SpawnPlayer(Vector3 position, CameraFollow2D camFollow)
        {
            var playerPrefab = LoadPrefab("Player/Player");
            var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.name = "Player";
            player.transform.position = position;

            if (camFollow != null)
            {
                camFollow.SetTarget(player.transform);
            }

            return player;
        }

        public static void BuildLevel01()
        {
            var (lm, hud, camFollow) = SetupBaseScene("Level 1 - First Shot", 2, "Level02");
            var player = SpawnPlayer(new Vector3(-6f, 0.5f, 0f), camFollow);

            // Ground floors
            CreateGroundBlock("Floor_Main", new Vector2(0f, -1f), new Vector2(24f, 1.5f));
            CreateGroundBlock("Wall_Left", new Vector2(-9.5f, 4f), new Vector2(1.5f, 10f));
            CreateGroundBlock("Wall_Right", new Vector2(9.5f, 4f), new Vector2(1.5f, 10f));
            CreateGroundBlock("Ceiling", new Vector2(0f, 8.5f), new Vector2(24f, 1.5f));

            // Pressure Plate
            var platePrefab = LoadPrefab("PuzzleObjects/PressurePlate");
            var plateObj = (GameObject)PrefabUtility.InstantiatePrefab(platePrefab);
            plateObj.transform.position = new Vector3(0f, -0.25f, 0f);
            var plate = plateObj.GetComponent<PressurePlate>();

            // Crate suspended by rope
            var cratePrefab = LoadPrefab("PuzzleObjects/Crate");
            var crateObj = (GameObject)PrefabUtility.InstantiatePrefab(cratePrefab);
            crateObj.transform.position = new Vector3(0f, 4f, 0f);
            var crateRb = crateObj.GetComponent<Rigidbody2D>();
            crateRb.bodyType = RigidbodyType2D.Kinematic; // Held in place until rope cut

            // Rope
            var ropePrefab = LoadPrefab("PuzzleObjects/ShootableRope");
            var ropeObj = (GameObject)PrefabUtility.InstantiatePrefab(ropePrefab);
            ropeObj.transform.position = new Vector3(0f, 5.2f, 0f);
            var rope = ropeObj.GetComponent<ShootableRope>();

            var ropeSo = new SerializedObject(rope);
            ropeSo.FindProperty("connectedBody").objectReferenceValue = crateRb;
            ropeSo.ApplyModifiedProperties();

            // Door blocking exit
            var doorPrefab = LoadPrefab("PuzzleObjects/Door");
            var doorObj = (GameObject)PrefabUtility.InstantiatePrefab(doorPrefab);
            doorObj.transform.position = new Vector3(5f, 0.75f, 0f);
            var door = doorObj.GetComponent<DoorController>();

            // Exit
            var exitPrefab = LoadPrefab("Environment/LevelExit");
            var exitObj = (GameObject)PrefabUtility.InstantiatePrefab(exitPrefab);
            exitObj.transform.position = new Vector3(7.5f, 0.5f, 0f);

            // Wire PressurePlate -> Door
            UnityEventTools.AddPersistentListener(plate.onPressed, door.OpenDoor);
            UnityEventTools.AddPersistentListener(plate.onReleased, door.CloseDoor);

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), $"{ScenesPath}/Level01.unity");
            Debug.Log("[LevelBuilder] Level 1 generated!");
        }

        public static void BuildLevel02()
        {
            var (lm, hud, camFollow) = SetupBaseScene("Level 2 - The Wrong Rope", 2, "Level03");
            var player = SpawnPlayer(new Vector3(-6f, 3.5f, 0f), camFollow);

            // Layout: High ledge for player, deep pit, and two ropes
            // Upper ledge
            CreateGroundBlock("UpperLedge", new Vector2(-6f, 2f), new Vector2(6f, 1.5f));
            CreateGroundBlock("Wall_Left", new Vector2(-9.5f, 5f), new Vector2(1.5f, 10f));
            CreateGroundBlock("Wall_Right", new Vector2(10.5f, 5f), new Vector2(1.5f, 14f));
            CreateGroundBlock("Ceiling", new Vector2(0f, 9.5f), new Vector2(24f, 1.5f));

            // Lower right floor where exit is
            CreateGroundBlock("LowerFloor", new Vector2(6f, -2.5f), new Vector2(8f, 1.5f));

            // Chute for Crate A (correct rope): drops onto Pressure Plate
            var platePrefab = LoadPrefab("PuzzleObjects/PressurePlate");
            var plateObj = (GameObject)PrefabUtility.InstantiatePrefab(platePrefab);
            plateObj.transform.position = new Vector3(-1f, -1.75f, 0f);
            var plate = plateObj.GetComponent<PressurePlate>();
            CreateGroundBlock("PlateFloor", new Vector2(-1f, -2.5f), new Vector2(3.5f, 1.5f));

            // Rope A & Crate A (Correct choice)
            var cratePrefab = LoadPrefab("PuzzleObjects/Crate");
            var crateA = (GameObject)PrefabUtility.InstantiatePrefab(cratePrefab);
            crateA.name = "Crate_A_Correct";
            crateA.transform.position = new Vector3(-1f, 5f, 0f);
            var crateARb = crateA.GetComponent<Rigidbody2D>();
            crateARb.bodyType = RigidbodyType2D.Kinematic;

            var ropePrefab = LoadPrefab("PuzzleObjects/ShootableRope");
            var ropeA = (GameObject)PrefabUtility.InstantiatePrefab(ropePrefab);
            ropeA.name = "Rope_A";
            ropeA.transform.position = new Vector3(-1f, 6.2f, 0f);
            var ropeAScript = ropeA.GetComponent<ShootableRope>();
            var ropeASo = new SerializedObject(ropeAScript);
            ropeASo.FindProperty("connectedBody").objectReferenceValue = crateARb;
            ropeASo.ApplyModifiedProperties();

            // Rope B & Crate B (The Wrong Rope: drops into bottomless pit)
            var crateB = (GameObject)PrefabUtility.InstantiatePrefab(cratePrefab);
            crateB.name = "Crate_B_Wrong";
            crateB.transform.position = new Vector3(2.5f, 5f, 0f);
            var crateBRb = crateB.GetComponent<Rigidbody2D>();
            crateBRb.bodyType = RigidbodyType2D.Kinematic;

            var ropeB = (GameObject)PrefabUtility.InstantiatePrefab(ropePrefab);
            ropeB.name = "Rope_B";
            ropeB.transform.position = new Vector3(2.5f, 6.2f, 0f);
            var ropeBScript = ropeB.GetComponent<ShootableRope>();
            var ropeBSo = new SerializedObject(ropeBScript);
            ropeBSo.FindProperty("connectedBody").objectReferenceValue = crateBRb;
            ropeBSo.ApplyModifiedProperties();

            // Bridge door that opens when Plate is triggered
            var doorPrefab = LoadPrefab("PuzzleObjects/Door");
            var doorObj = (GameObject)PrefabUtility.InstantiatePrefab(doorPrefab);
            doorObj.transform.position = new Vector3(3.5f, -1.75f, 0f);
            var door = doorObj.GetComponent<DoorController>();

            // Wire PressurePlate -> Door
            UnityEventTools.AddPersistentListener(plate.onPressed, door.OpenDoor);
            UnityEventTools.AddPersistentListener(plate.onReleased, door.CloseDoor);

            // Exit
            var exitPrefab = LoadPrefab("Environment/LevelExit");
            var exitObj = (GameObject)PrefabUtility.InstantiatePrefab(exitPrefab);
            exitObj.transform.position = new Vector3(8f, -1.25f, 0f);

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), $"{ScenesPath}/Level02.unity");
            Debug.Log("[LevelBuilder] Level 2 generated!");
        }

        public static void BuildLevel03()
        {
            var (lm, hud, camFollow) = SetupBaseScene("Level 3 - The Fire Room", 3, "Level04");
            var player = SpawnPlayer(new Vector3(-6f, 0.5f, 0f), camFollow);

            // Left floor
            CreateGroundBlock("Floor_Left", new Vector2(-5f, -1f), new Vector2(7f, 1.5f));
            CreateGroundBlock("Wall_Left", new Vector2(-9.5f, 4f), new Vector2(1.5f, 10f));
            CreateGroundBlock("Wall_Right", new Vector2(9.5f, 4f), new Vector2(1.5f, 10f));
            CreateGroundBlock("Ceiling", new Vector2(0f, 8.5f), new Vector2(24f, 1.5f));

            // Fire Pit floor (lower)
            CreateGroundBlock("FireFloor", new Vector2(0f, -1.8f), new Vector2(4.5f, 1f));

            // Fire Hazard in the pit
            var firePrefab = LoadPrefab("PuzzleObjects/FireHazard");
            var fireObj = (GameObject)PrefabUtility.InstantiatePrefab(firePrefab);
            fireObj.transform.position = new Vector3(0f, -0.9f, 0f);
            var fire = fireObj.GetComponent<FireHazard>();

            // Right floor where exit is
            CreateGroundBlock("Floor_Right", new Vector2(5.5f, -1f), new Vector2(6f, 1.5f));

            // Pipe & Valve overhead
            var valvePrefab = LoadPrefab("PuzzleObjects/ShootableValve");
            var valveObj = (GameObject)PrefabUtility.InstantiatePrefab(valvePrefab);
            valveObj.transform.position = new Vector3(0f, 4.5f, 0f);
            var valve = valveObj.GetComponent<ShootableValve>();

            // Water Source under pipe
            var waterPrefab = LoadPrefab("PuzzleObjects/WaterSource");
            var waterObj = (GameObject)PrefabUtility.InstantiatePrefab(waterPrefab);
            waterObj.transform.position = new Vector3(0f, 2.5f, 0f);
            var water = waterObj.GetComponent<WaterSource>();

            var waterSo = new SerializedObject(water);
            waterSo.FindProperty("targetFireHazard").objectReferenceValue = fire;
            waterSo.ApplyModifiedProperties();

            // Wire Valve -> WaterSource
            UnityEventTools.AddPersistentListener(valve.onValveOpened, water.StartWaterFlow);

            // Exit
            var exitPrefab = LoadPrefab("Environment/LevelExit");
            var exitObj = (GameObject)PrefabUtility.InstantiatePrefab(exitPrefab);
            exitObj.transform.position = new Vector3(7f, 0.5f, 0f);

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), $"{ScenesPath}/Level03.unity");
            Debug.Log("[LevelBuilder] Level 3 generated!");
        }

        public static void BuildLevel04()
        {
            var (lm, hud, camFollow) = SetupBaseScene("Level 4 - The Counterweight", 3, "Level05");
            var player = SpawnPlayer(new Vector3(-7f, 0.5f, 0f), camFollow);

            // Left ledge
            CreateGroundBlock("Floor_Left", new Vector2(-6.5f, -1f), new Vector2(5f, 1.5f));
            CreateGroundBlock("Wall_Left", new Vector2(-9.5f, 4f), new Vector2(1.5f, 10f));
            CreateGroundBlock("Wall_Right", new Vector2(9.5f, 4f), new Vector2(1.5f, 10f));
            CreateGroundBlock("Ceiling", new Vector2(0f, 8.5f), new Vector2(24f, 1.5f));

            // Wide gap from -4 to 3 (Bottom pit with deathzone at -8)

            // Right ledge with Exit
            CreateGroundBlock("Floor_Right", new Vector2(6.5f, -1f), new Vector2(5f, 1.5f));
            var exitPrefab = LoadPrefab("Environment/LevelExit");
            var exitObj = (GameObject)PrefabUtility.InstantiatePrefab(exitPrefab);
            exitObj.transform.position = new Vector3(7.5f, 0.5f, 0f);

            // Moving Platform: initially down at (-0.5, -4f), target at (-0.5f, -0.75f)
            var platformPrefab = LoadPrefab("PuzzleObjects/MovingPlatform");
            var platformObj = (GameObject)PrefabUtility.InstantiatePrefab(platformPrefab);
            platformObj.transform.position = new Vector3(-0.5f, -4f, 0f);
            var platform = platformObj.GetComponent<MovingPlatform>();

            var destPoint = new GameObject("PlatformDestination");
            destPoint.transform.position = new Vector3(-0.5f, -0.75f, 0f);

            var platSo = new SerializedObject(platform);
            platSo.FindProperty("endPoint").objectReferenceValue = destPoint.transform;
            platSo.FindProperty("moveSpeed").floatValue = 4.5f;
            platSo.ApplyModifiedProperties();

            // Counterweight hanging high
            var cratePrefab = LoadPrefab("PuzzleObjects/Crate");
            var weightObj = (GameObject)PrefabUtility.InstantiatePrefab(cratePrefab);
            weightObj.name = "Counterweight";
            weightObj.transform.position = new Vector3(-0.5f, 5.5f, 0f);
            var weightRb = weightObj.GetComponent<Rigidbody2D>();
            weightRb.bodyType = RigidbodyType2D.Kinematic;
            weightRb.mass = 10f;

            // Rope holding counterweight
            var ropePrefab = LoadPrefab("PuzzleObjects/ShootableRope");
            var ropeObj = (GameObject)PrefabUtility.InstantiatePrefab(ropePrefab);
            ropeObj.transform.position = new Vector3(-0.5f, 6.7f, 0f);
            var rope = ropeObj.GetComponent<ShootableRope>();

            var ropeSo = new SerializedObject(rope);
            ropeSo.FindProperty("connectedBody").objectReferenceValue = weightRb;
            ropeSo.ApplyModifiedProperties();

            // Wire Rope Cut -> Platform moves to destination
            UnityEventTools.AddPersistentListener(rope.onRopeCut, platform.MoveToDestination);

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), $"{ScenesPath}/Level04.unity");
            Debug.Log("[LevelBuilder] Level 4 generated!");
        }

        public static void BuildLevel05()
        {
            var (lm, hud, camFollow) = SetupBaseScene("Level 5 - Chain Reaction", 4, "Level01");
            var player = SpawnPlayer(new Vector3(-7f, 4.5f, 0f), camFollow);

            // Upper tier (Player start & Rope 1)
            CreateGroundBlock("UpperTier", new Vector2(-6f, 3f), new Vector2(7f, 1.5f));
            CreateGroundBlock("Wall_Left", new Vector2(-10.5f, 4f), new Vector2(1.5f, 14f));
            CreateGroundBlock("Wall_Right", new Vector2(10.5f, 4f), new Vector2(1.5f, 14f));
            CreateGroundBlock("Ceiling", new Vector2(0f, 9.5f), new Vector2(24f, 1.5f));

            // Lower tier
            CreateGroundBlock("LowerFloor_Left", new Vector2(-4f, -1.5f), new Vector2(8f, 1.5f));
            CreateGroundBlock("LowerFloor_Right", new Vector2(6.5f, -1.5f), new Vector2(6f, 1.5f));

            // Pressure Plate on Lower Left
            var platePrefab = LoadPrefab("PuzzleObjects/PressurePlate");
            var plateObj = (GameObject)PrefabUtility.InstantiatePrefab(platePrefab);
            plateObj.transform.position = new Vector3(-2f, -0.75f, 0f);
            var plate = plateObj.GetComponent<PressurePlate>();

            // Crate 1 suspended above Pressure Plate
            var cratePrefab = LoadPrefab("PuzzleObjects/Crate");
            var crateObj = (GameObject)PrefabUtility.InstantiatePrefab(cratePrefab);
            crateObj.transform.position = new Vector3(-2f, 5f, 0f);
            var crateRb = crateObj.GetComponent<Rigidbody2D>();
            crateRb.bodyType = RigidbodyType2D.Kinematic;

            // Rope 1
            var ropePrefab = LoadPrefab("PuzzleObjects/ShootableRope");
            var ropeObj = (GameObject)PrefabUtility.InstantiatePrefab(ropePrefab);
            ropeObj.transform.position = new Vector3(-2f, 6.2f, 0f);
            var rope = ropeObj.GetComponent<ShootableRope>();
            var ropeSo = new SerializedObject(rope);
            ropeSo.FindProperty("connectedBody").objectReferenceValue = crateRb;
            ropeSo.ApplyModifiedProperties();

            // Machine Door shielding the Valve
            var doorPrefab = LoadPrefab("PuzzleObjects/Door");
            var doorObj = (GameObject)PrefabUtility.InstantiatePrefab(doorPrefab);
            doorObj.transform.position = new Vector3(1.5f, 3.5f, 0f);
            var door = doorObj.GetComponent<DoorController>();

            // Wire PressurePlate -> Machine Door (Opens to expose valve)
            UnityEventTools.AddPersistentListener(plate.onPressed, door.OpenDoor);

            // Valve behind the door
            var valvePrefab = LoadPrefab("PuzzleObjects/ShootableValve");
            var valveObj = (GameObject)PrefabUtility.InstantiatePrefab(valvePrefab);
            valveObj.transform.position = new Vector3(3f, 3.5f, 0f);
            var valve = valveObj.GetComponent<ShootableValve>();

            // Water Source connected to Valve
            var waterPrefab = LoadPrefab("PuzzleObjects/WaterSource");
            var waterObj = (GameObject)PrefabUtility.InstantiatePrefab(waterPrefab);
            waterObj.transform.position = new Vector3(1.5f, 1.5f, 0f);
            var water = waterObj.GetComponent<WaterSource>();

            // Fire Hazard blocking the exit path on lower floor
            var firePrefab = LoadPrefab("PuzzleObjects/FireHazard");
            var fireObj = (GameObject)PrefabUtility.InstantiatePrefab(firePrefab);
            fireObj.transform.position = new Vector3(1.5f, -0.75f, 0f);
            var fire = fireObj.GetComponent<FireHazard>();

            var waterSo = new SerializedObject(water);
            waterSo.FindProperty("targetFireHazard").objectReferenceValue = fire;
            waterSo.ApplyModifiedProperties();

            // Wire Valve -> Water
            UnityEventTools.AddPersistentListener(valve.onValveOpened, water.StartWaterFlow);

            // Exit on the right
            var exitPrefab = LoadPrefab("Environment/LevelExit");
            var exitObj = (GameObject)PrefabUtility.InstantiatePrefab(exitPrefab);
            exitObj.transform.position = new Vector3(7.5f, -0.25f, 0f);

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), $"{ScenesPath}/Level05.unity");
            Debug.Log("[LevelBuilder] Level 5 generated!");
        }
    }
}
#endif
