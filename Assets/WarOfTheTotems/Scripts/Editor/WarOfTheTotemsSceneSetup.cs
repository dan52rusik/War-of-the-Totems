using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using WarOfTheTotems.Core;

namespace WarOfTheTotems.Editor
{
    public static class WarOfTheTotemsSceneSetup
    {
        private const string SceneFolder = "Assets/WarOfTheTotems/Scenes";
        private const string ScenePath = SceneFolder + "/PrototypeBattlefield.unity";

        [MenuItem("Tools/War of the Totems/Create Prototype Scene")]
        public static void CreatePrototypeScene()
        {
            EnsureFolders();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "PrototypeBattlefield";

            CreateCamera();
            CreateLighting();

            var root = CreateObject("WarOfTheTotems_PrototypeRoot", null, Vector3.zero);
            root.AddComponent<BattlePrototypeState>();
            AddPrototypeBattleController(root);

            var environment = CreateObject("Environment", root.transform, Vector3.zero);
            var gameplay = CreateObject("Gameplay", root.transform, Vector3.zero);
            var uiRoot = CreateObject("UI", root.transform, Vector3.zero);

            BuildEnvironment(environment.transform);
            BuildGameplayMarkers(gameplay.transform);
            BuildUI(uiRoot.transform);
            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));
        }

        [MenuItem("Tools/War of the Totems/Open Prototype Scene")]
        public static void OpenPrototypeScene()
        {
            if (!File.Exists(ScenePath))
            {
                CreatePrototypeScene();
                return;
            }

            EditorSceneManager.OpenScene(ScenePath);
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/WarOfTheTotems");
            Directory.CreateDirectory("Assets/WarOfTheTotems/Art");
            Directory.CreateDirectory("Assets/WarOfTheTotems/Audio");
            Directory.CreateDirectory("Assets/WarOfTheTotems/Materials");
            Directory.CreateDirectory("Assets/WarOfTheTotems/Prefabs");
            Directory.CreateDirectory(SceneFolder);
            Directory.CreateDirectory("Assets/WarOfTheTotems/Scripts");
            Directory.CreateDirectory("Assets/WarOfTheTotems/Scripts/Editor");
            Directory.CreateDirectory("Assets/WarOfTheTotems/Scripts/Runtime");
            Directory.CreateDirectory("Assets/WarOfTheTotems/Scripts/Runtime/Core");
            Directory.CreateDirectory("Assets/WarOfTheTotems/Scripts/Runtime/Gameplay");
            Directory.CreateDirectory("Assets/WarOfTheTotems/Scripts/Runtime/UI");
            Directory.CreateDirectory("Assets/WarOfTheTotems/Textures");
        }

        private static void CreateCamera()
        {
            var cameraGo = CreateObject("Main Camera", null, new Vector3(0f, 0f, -10f));
            cameraGo.tag = "MainCamera";

            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.2f;
            camera.backgroundColor = new Color(0.13f, 0.18f, 0.17f);
            camera.clearFlags = CameraClearFlags.SolidColor;

            cameraGo.AddComponent<AudioListener>();
        }

        private static void CreateLighting()
        {
            var lightGo = CreateObject("Directional Light", null, new Vector3(0f, 2f, -2f));
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.85f;
            light.color = new Color(1f, 0.93f, 0.78f);
            lightGo.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
        }

        private static void BuildEnvironment(Transform parent)
        {
            var battlefield = CreateQuad("Battlefield", parent, new Vector3(0f, 0f, 0f), new Vector3(17.5f, 8.5f, 1f), new Color(0.30f, 0.39f, 0.25f));
            battlefield.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(0.29f, 0.34f, 0.24f);

            CreateQuad("Central Path", parent, new Vector3(0f, -0.45f, -0.02f), new Vector3(14f, 2.2f, 1f), new Color(0.46f, 0.37f, 0.25f));
            CreateQuad("Evolution Zone", parent, new Vector3(0f, -0.45f, -0.04f), new Vector3(2.75f, 2.75f, 1f), new Color(0.34f, 0.72f, 0.62f));

            CreateBase(parent, "Player Base", new Vector3(-6.7f, -0.15f, -0.1f), new Vector3(2.7f, 3.5f, 1f), new Color(0.61f, 0.49f, 0.38f));
            CreateBase(parent, "Enemy Base", new Vector3(6.7f, 0.1f, -0.1f), new Vector3(2.9f, 3.8f, 1f), new Color(0.27f, 0.20f, 0.26f));

            CreateDecoration(parent, "Bone Cluster A", new Vector3(-2.6f, 1.7f, -0.06f), new Vector3(1.2f, 0.35f, 1f), new Color(0.84f, 0.81f, 0.69f), 18f);
            CreateDecoration(parent, "Bone Cluster B", new Vector3(2.75f, -2.05f, -0.06f), new Vector3(1.35f, 0.35f, 1f), new Color(0.84f, 0.81f, 0.69f), -22f);
            CreateDecoration(parent, "Mushroom Grove", new Vector3(-1.35f, -2.1f, -0.06f), new Vector3(0.9f, 1.1f, 1f), new Color(0.40f, 0.90f, 0.73f), 0f);
            CreateDecoration(parent, "Ancient Tree", new Vector3(3.6f, 1.95f, -0.06f), new Vector3(1.0f, 1.8f, 1f), new Color(0.36f, 0.52f, 0.29f), 0f);
        }

        private static void BuildGameplayMarkers(Transform parent)
        {
            CreateUnit(parent, "Base Bearer", new Vector3(-1.0f, -0.45f, -0.2f), new Color(0.90f, 0.81f, 0.58f), "EVOLVING");
            CreateUnit(parent, "Stone Guard Preview", new Vector3(-0.15f, -0.42f, -0.24f), new Color(0.54f, 0.58f, 0.60f), "STONE GUARD");
            CreateUnit(parent, "Shadow Hunter 01", new Vector3(2.45f, -0.2f, -0.2f), new Color(0.33f, 0.18f, 0.42f), "SHADOW");
            CreateUnit(parent, "Shadow Hunter 02", new Vector3(3.25f, -0.7f, -0.2f), new Color(0.33f, 0.18f, 0.42f), "SHADOW");
            CreateUnit(parent, "Shadow Hunter 03", new Vector3(3.95f, 0.15f, -0.2f), new Color(0.33f, 0.18f, 0.42f), "SHADOW");
        }

        private static void BuildUI(Transform parent)
        {
            var canvasGo = CreateObject("Canvas", parent, Vector3.zero);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            var topBar = CreatePanel("TopBar", canvasGo.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(1680f, 88f), new Color(0.11f, 0.14f, 0.11f, 0.82f));
            CreateText("PlayerHealth", topBar.transform, "5 (YOU)", TextAnchor.MiddleLeft, 24, new Vector2(-735f, 0f), new Vector2(190f, 44f));
            CreateText("BoneResource", topBar.transform, "COLLECTING ANCESTRAL BONE: 0", TextAnchor.MiddleLeft, 22, new Vector2(-330f, 0f), new Vector2(500f, 40f));
            CreateText("SparkResource", topBar.transform, "PRIMAL SPARK: 10", TextAnchor.MiddleLeft, 22, new Vector2(150f, 0f), new Vector2(260f, 40f));
            CreateText("Settings", topBar.transform, "SETTINGS", TextAnchor.MiddleCenter, 18, new Vector2(620f, 0f), new Vector2(120f, 34f));
            CreateText("EnemyHealth", topBar.transform, "500 (THEM)", TextAnchor.MiddleRight, 24, new Vector2(735f, 0f), new Vector2(210f, 44f));

            var bottomBar = CreatePanel("BottomBar", canvasGo.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(1680f, 164f), new Color(0.12f, 0.09f, 0.08f, 0.88f));
            CreateText("SparkFooter", bottomBar.transform, "PRIMAL SPARK: 10", TextAnchor.MiddleLeft, 24, new Vector2(-690f, 44f), new Vector2(300f, 40f));
            CreateSummonCard(bottomBar.transform, "Base Bearer (1)", "cost 2", new Vector2(-220f, 8f), new Color(0.57f, 0.43f, 0.29f));
            CreateSummonCard(bottomBar.transform, "Stone Totem (2)", "cost 6", new Vector2(0f, 8f), new Color(0.43f, 0.49f, 0.52f));
            CreateSummonCard(bottomBar.transform, "Beast Totem (3)", "cost 10", new Vector2(220f, 8f), new Color(0.63f, 0.37f, 0.24f));
            CreateText("GameTitle", bottomBar.transform, "WAR OF THE TOTEMS", TextAnchor.MiddleCenter, 28, new Vector2(0f, -48f), new Vector2(520f, 42f));

            var worldOverlay = CreatePanel("EvolutionOverlay", canvasGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -15f), new Vector2(300f, 70f), new Color(0.05f, 0.13f, 0.11f, 0.75f));
            CreateText("EvolutionLabel", worldOverlay.transform, "EVOLVING: STONE GUARD", TextAnchor.UpperCenter, 20, new Vector2(0f, 15f), new Vector2(280f, 28f));
            CreateProgressBar(worldOverlay.transform, new Vector2(0f, -15f), new Vector2(220f, 16f), 0.58f);
        }

        private static GameObject CreateObject(string name, Transform parent, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            return go;
        }

        private static GameObject CreateQuad(string name, Transform parent, Vector3 position, Vector3 scale, Color color)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetParent(parent, false);
            quad.transform.localPosition = position;
            quad.transform.localScale = scale;

            var collider = quad.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            var renderer = quad.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.sharedMaterial.color = color;
            return quad;
        }

        private static void CreateBase(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            var baseGo = CreateQuad(name, parent, position, scale, color);
            CreateDecoration(baseGo.transform, "Totem Accent", new Vector3(0f, 0.35f, -0.01f), new Vector3(0.55f, 1.55f, 1f), color * 1.18f, 0f);
        }

        private static void CreateDecoration(Transform parent, string name, Vector3 position, Vector3 scale, Color color, float rotationZ)
        {
            var go = CreateQuad(name, parent, position, scale, color);
            go.transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
        }

        private static void CreateUnit(Transform parent, string name, Vector3 position, Color color, string label)
        {
            var body = CreateQuad(name, parent, position, new Vector3(0.55f, 0.85f, 1f), color);
            CreateDecoration(body.transform, "Weapon", new Vector3(0.38f, -0.1f, -0.01f), new Vector3(0.14f, 0.7f, 1f), color * 0.8f, -18f);
            CreateWorldLabel(parent, $"{name} Label", position + new Vector3(0f, 0.75f, -0.02f), label);
        }

        private static void CreateWorldLabel(Transform parent, string name, Vector3 position, string text)
        {
            var labelGo = CreateObject(name, parent, position);
            var mesh = labelGo.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.fontSize = 36;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.characterSize = 0.08f;
            mesh.color = new Color(0.96f, 0.94f, 0.77f);
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            var go = CreateObject(name, parent, Vector3.zero);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            var image = go.AddComponent<Image>();
            image.color = color;
            return go;
        }

        private static void CreateText(string name, Transform parent, string content, TextAnchor alignment, int fontSize, Vector2 anchoredPosition, Vector2 size)
        {
            var go = CreateObject(name, parent, Vector3.zero);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            var text = go.AddComponent<Text>();
            text.text = content;
            text.alignment = alignment;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.color = new Color(0.95f, 0.92f, 0.84f);
        }

        private static void CreateSummonCard(Transform parent, string title, string cost, Vector2 anchoredPosition, Color color)
        {
            var card = CreatePanel(title, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, new Vector2(190f, 78f), color);
            CreateText($"{title}Title", card.transform, title, TextAnchor.MiddleCenter, 16, new Vector2(0f, 12f), new Vector2(170f, 28f));
            CreateText($"{title}Cost", card.transform, cost, TextAnchor.MiddleCenter, 18, new Vector2(0f, -18f), new Vector2(120f, 24f));
            card.AddComponent<Button>();
        }

        private static void CreateProgressBar(Transform parent, Vector2 anchoredPosition, Vector2 size, float value)
        {
            var background = CreatePanel("ProgressBackground", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, size, new Color(0.11f, 0.20f, 0.16f, 0.95f));
            var fillRect = CreatePanel("ProgressFill", background.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(size.x * Mathf.Clamp01(value), size.y - 4f), new Color(0.39f, 0.82f, 0.68f, 1f)).GetComponent<RectTransform>();
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.anchorMin = new Vector2(0f, 0.5f);
            fillRect.anchorMax = new Vector2(0f, 0.5f);
            fillRect.anchoredPosition = new Vector2(2f, 0f);
        }

        private static void CreateEventSystem()
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        private static void AddPrototypeBattleController(GameObject root)
        {
            var controllerType = System.Type.GetType("WarOfTheTotems.Gameplay.PrototypeBattleController, Assembly-CSharp");
            if (controllerType != null)
            {
                root.AddComponent(controllerType);
            }
        }
    }
}
