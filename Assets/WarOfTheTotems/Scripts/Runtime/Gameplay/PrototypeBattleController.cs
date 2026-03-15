using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using WarOfTheTotems.Core;

namespace WarOfTheTotems.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class PrototypeBattleController : MonoBehaviour
    {
        private const float PlayerSpawnX = -5.5f;
        private const float EnemySpawnX = 5.5f;
        private const float LaneY = -0.45f;
        private const float EvolutionZoneX = 0f;
        private const float EvolutionZoneRadius = 1.1f;
        private const float PlayerBaseX = -6.1f;
        private const float EnemyBaseX = 6.1f;

        private readonly List<PrototypeUnit> units = new();

        private BattlePrototypeState state = null!;
        private Transform gameplayRoot = null!;
        private Canvas mainCanvas = null!;
        private Text playerHealthText = null!;
        private Text enemyHealthText = null!;
        private Text boneText = null!;
        private Text sparkTopText = null!;
        private Text sparkBottomText = null!;
        private Text evolutionLabelText = null!;
        private RectTransform evolutionPanel = null!;
        private RectTransform evolutionFill = null!;

        private float sparkAccumulator;
        private float enemyWaveTimer;

        public float EvolutionDuration => state.evolutionDuration;

        private void Awake()
        {
            state = GetComponent<BattlePrototypeState>();
            if (state == null)
            {
                state = gameObject.AddComponent<BattlePrototypeState>();
            }

            gameplayRoot = EnsureChild("Gameplay");
            ClearGameplayMarkers();
            EnsureUiScaffold();
            WireUi();
            EnsureEventSystem();
        }

        private void Start()
        {
            SpawnEnemyWave();
            RefreshUi();
            SetEvolutionUI(false, string.Empty, 0f);
        }

        private void Update()
        {
            RegenerateSpark(Time.deltaTime);
            TickUnits(Time.deltaTime);
            HandleEnemyWaves(Time.deltaTime);
            HandleInput();
            RefreshUi();
        }

        private void HandleInput()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.digit1Key.wasPressedThisFrame) SpawnPlayerUnit(TotemType.None);
                if (keyboard.digit2Key.wasPressedThisFrame) SpawnPlayerUnit(TotemType.Stone);
                if (keyboard.digit3Key.wasPressedThisFrame) SpawnPlayerUnit(TotemType.Beast);
            }
#endif
        }

        public bool IsInsideEvolutionZone(Vector3 worldPosition)
        {
            return Mathf.Abs(worldPosition.x - EvolutionZoneX) <= EvolutionZoneRadius;
        }

        public PrototypeUnit FindClosestEnemy(PrototypeUnit requester, float range)
        {
            PrototypeUnit best = null;
            var bestDistance = float.MaxValue;
            const float laneTolerance = 0.1f;

            var forward = requester.Team == TeamSide.Player ? 1f : -1f;

            for (var i = 0; i < units.Count; i++)
            {
                var candidate = units[i];
                if (candidate == null || candidate.IsDead || candidate.Team == requester.Team)
                {
                    continue;
                }

                var dirX = candidate.transform.position.x - requester.transform.position.x;
                if (dirX * forward < 0f)
                {
                    continue;
                }

                var distance = Mathf.Abs(dirX);
                var laneGap = Mathf.Abs(candidate.transform.position.y - requester.transform.position.y);
                if (laneGap <= laneTolerance && distance <= range && distance < bestDistance)
                {
                    best = candidate;
                    bestDistance = distance;
                }
            }

            return best;
        }

        public bool IsAtEnemyBase(PrototypeUnit unit)
        {
            return unit.Team == TeamSide.Player
                ? unit.transform.position.x >= EnemyBaseX
                : unit.transform.position.x <= PlayerBaseX;
        }

        public void DamageBase(TeamSide side, int amount)
        {
            if (side == TeamSide.Player)
            {
                state.playerBaseHealth = Mathf.Max(0, state.playerBaseHealth - amount);
            }
            else
            {
                state.enemyBaseHealth = Mathf.Max(0, state.enemyBaseHealth - amount);
            }
        }

        public void NotifyUnitKilled(PrototypeUnit deadUnit)
        {
            if (deadUnit.Team == TeamSide.Enemy)
            {
                state.ancestralBone += 1;
            }

            units.Remove(deadUnit);
        }

        public void SetEvolutionUI(bool visible, string label, float progress)
        {
            if (evolutionPanel == null || evolutionLabelText == null || evolutionFill == null)
            {
                return;
            }

            evolutionPanel.gameObject.SetActive(visible);
            evolutionLabelText.text = label;
            var fillSize = evolutionFill.sizeDelta;
            fillSize.x = 216f * Mathf.Clamp01(progress);
            evolutionFill.sizeDelta = fillSize;

            state.evolvingLabel = label;
            state.evolutionProgress = progress;
        }

        private void RegenerateSpark(float deltaTime)
        {
            sparkAccumulator += state.primalSparkRegenPerSecond * deltaTime;
            if (sparkAccumulator < 1f)
            {
                return;
            }

            var gained = Mathf.FloorToInt(sparkAccumulator);
            sparkAccumulator -= gained;
            state.primalSpark += gained;
        }

        private void HandleEnemyWaves(float deltaTime)
        {
            enemyWaveTimer += deltaTime;
            if (enemyWaveTimer < state.enemyWaveInterval)
            {
                return;
            }

            enemyWaveTimer = 0f;
            SpawnEnemyWave();
        }

        private void TickUnits(float deltaTime)
        {
            for (var i = units.Count - 1; i >= 0; i--)
            {
                var unit = units[i];
                if (unit == null || unit.IsDead)
                {
                    units.RemoveAt(i);
                    continue;
                }

                unit.Tick(deltaTime);
            }
        }

        private void SpawnEnemyWave()
        {
            for (var i = 0; i < state.enemyWaveSize; i++)
            {
                SpawnUnit(TeamSide.Enemy, TotemType.Shadow, true, new Vector3(EnemySpawnX + i * 0.2f, LaneY, 0f));
            }
        }

        private void SpawnPlayerUnit(TotemType totem)
        {
            var cost = GetCost(totem);
            if (state.primalSpark < cost)
            {
                return;
            }

            state.primalSpark -= cost;
            SpawnUnit(TeamSide.Player, totem, false, new Vector3(PlayerSpawnX, LaneY, 0f));
        }

        private int GetCost(TotemType totem)
        {
            return totem switch
            {
                TotemType.Stone => state.summonOptions[1].cost,
                TotemType.Beast => state.summonOptions[2].cost,
                _ => state.summonOptions[0].cost,
            };
        }

        private void SpawnUnit(TeamSide team, TotemType totem, bool evolved, Vector3 position)
        {
            var unitRoot = new GameObject($"{team}_{totem}_{units.Count}");
            unitRoot.transform.SetParent(gameplayRoot, false);
            unitRoot.transform.position = position;

            CreateUnitVisual(unitRoot.transform);
            var unit = unitRoot.AddComponent<PrototypeUnit>();
            unit.Initialize(this, team, totem, evolved);
            units.Add(unit);
        }

        private void CreateUnitVisual(Transform parent)
        {
            var visual = new GameObject("Visual");
            visual.transform.SetParent(parent, false);

            CreateQuad("LegBack", visual.transform, new Vector3(-0.12f, -0.38f, 0.01f), new Vector3(0.12f, 0.34f, 1f), Color.white);
            CreateQuad("LegFront", visual.transform, new Vector3(0.12f, -0.38f, 0f), new Vector3(0.12f, 0.34f, 1f), Color.white);
            CreateQuad("Torso", visual.transform, new Vector3(0f, -0.02f, 0f), new Vector3(0.42f, 0.52f, 1f), Color.white);
            CreateQuad("Arm", visual.transform, new Vector3(0.22f, -0.04f, -0.01f), new Vector3(0.10f, 0.34f, 1f), Color.white);
            CreateQuad("Head", visual.transform, new Vector3(0f, 0.34f, -0.02f), new Vector3(0.28f, 0.28f, 1f), Color.white);
            CreateQuad("Weapon", visual.transform, new Vector3(0.38f, -0.02f, -0.03f), new Vector3(0.12f, 0.54f, 1f), Color.gray, -18f);

            var healthRoot = new GameObject("HealthBar");
            healthRoot.transform.SetParent(parent, false);

            CreateQuad("Background", healthRoot.transform, Vector3.zero, new Vector3(0.62f, 0.08f, 1f), new Color(0.15f, 0.17f, 0.16f));
            var fill = CreateQuad("Fill", healthRoot.transform, new Vector3(-0.155f, 0f, -0.01f), new Vector3(0.31f, 0.055f, 1f), new Color(0.39f, 0.86f, 0.68f));
            fill.transform.localScale = new Vector3(0.62f, 0.055f, 1f);

            var hpGo = new GameObject("HealthValue");
            hpGo.transform.SetParent(parent, false);
            var hpText = hpGo.AddComponent<TextMesh>();
            hpText.fontSize = 30;
            hpText.characterSize = 0.055f;
            hpText.anchor = TextAnchor.MiddleCenter;
            hpText.alignment = TextAlignment.Center;
            hpText.color = new Color(0.42f, 0.95f, 0.78f);

            var labelGo = new GameObject("NameLabel");
            labelGo.transform.SetParent(parent, false);
            var label = labelGo.AddComponent<TextMesh>();
            label.fontSize = 26;
            label.characterSize = 0.05f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = new Color(0.95f, 0.93f, 0.83f);
        }

        private void EnsureUiScaffold()
        {
            var uiRoot = EnsureChild("UI");
            mainCanvas = FindFirstObjectByType<Canvas>();
            if (mainCanvas == null)
            {
                var canvasGo = new GameObject("Canvas");
                canvasGo.transform.SetParent(uiRoot, false);
                mainCanvas = canvasGo.AddComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            var topBar = EnsurePanel("TopBar", mainCanvas.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(1680f, 88f), new Color(0.11f, 0.14f, 0.11f, 0.82f));
            EnsureText("PlayerHealth", topBar, "5 (YOU)", TextAnchor.MiddleLeft, 24, new Vector2(-735f, 0f), new Vector2(190f, 44f));
            EnsureText("BoneResource", topBar, "COLLECTING ANCESTRAL BONE: 0", TextAnchor.MiddleLeft, 22, new Vector2(-330f, 0f), new Vector2(500f, 40f));
            EnsureText("SparkResource", topBar, "PRIMAL SPARK: 10", TextAnchor.MiddleLeft, 22, new Vector2(150f, 0f), new Vector2(260f, 40f));
            EnsureText("Settings", topBar, "SETTINGS", TextAnchor.MiddleCenter, 18, new Vector2(620f, 0f), new Vector2(120f, 34f));
            EnsureText("EnemyHealth", topBar, "500 (THEM)", TextAnchor.MiddleRight, 24, new Vector2(735f, 0f), new Vector2(210f, 44f));

            var bottomBar = EnsurePanel("BottomBar", mainCanvas.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(1680f, 164f), new Color(0.12f, 0.09f, 0.08f, 0.88f));
            EnsureText("SparkFooter", bottomBar, "PRIMAL SPARK: 10", TextAnchor.MiddleLeft, 24, new Vector2(-690f, 44f), new Vector2(300f, 40f));
            EnsureSummonCard(bottomBar, "Base Bearer (1)", "cost 2", new Vector2(-220f, 8f), new Color(0.57f, 0.43f, 0.29f));
            EnsureSummonCard(bottomBar, "Stone Totem (2)", "cost 6", new Vector2(0f, 8f), new Color(0.43f, 0.49f, 0.52f));
            EnsureSummonCard(bottomBar, "Beast Totem (3)", "cost 10", new Vector2(220f, 8f), new Color(0.63f, 0.37f, 0.24f));
            EnsureText("GameTitle", bottomBar, "WAR OF THE TOTEMS", TextAnchor.MiddleCenter, 28, new Vector2(0f, -48f), new Vector2(520f, 42f));

            var overlay = EnsurePanel("EvolutionOverlay", mainCanvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -15f), new Vector2(300f, 70f), new Color(0.05f, 0.13f, 0.11f, 0.75f));
            EnsureText("EvolutionLabel", overlay, "EVOLVING: STONE GUARD", TextAnchor.UpperCenter, 20, new Vector2(0f, 15f), new Vector2(280f, 28f));
            EnsureProgressBar(overlay, new Vector2(0f, -15f), new Vector2(220f, 16f), 0.58f);
        }

        private void WireUi()
        {
            playerHealthText = FindText("PlayerHealth");
            enemyHealthText = FindText("EnemyHealth");
            boneText = FindText("BoneResource");
            sparkTopText = FindText("SparkResource");
            sparkBottomText = FindText("SparkFooter");
            evolutionLabelText = FindText("EvolutionLabel");

            var evolutionOverlay = FindRect("EvolutionOverlay");
            evolutionPanel = evolutionOverlay;
            evolutionFill = FindRect("ProgressFill");

            BindButton(new[] { "Base Bearer (1)", "Base Bearer" }, () => SpawnPlayerUnit(TotemType.None));
            BindButton(new[] { "Stone Totem (2)", "Stone Totem Summon" }, () => SpawnPlayerUnit(TotemType.Stone));
            BindButton(new[] { "Beast Totem (3)", "Beast Totem Summon" }, () => SpawnPlayerUnit(TotemType.Beast));
        }

        private void BindButton(string[] objectNames, UnityEngine.Events.UnityAction action)
        {
            GameObject target = null;
            for (var i = 0; i < objectNames.Length; i++)
            {
                target = GameObject.Find(objectNames[i]);
                if (target != null)
                {
                    break;
                }
            }

            if (target == null)
            {
                return;
            }

            var button = target.GetComponent<Button>();
            if (button == null)
            {
                button = target.AddComponent<Button>();
            }

            var image = target.GetComponent<Image>();
            if (image != null)
            {
                var colors = button.colors;
                colors.normalColor = image.color;
                colors.highlightedColor = image.color * 1.08f;
                colors.pressedColor = image.color * 0.88f;
                colors.selectedColor = image.color;
                button.colors = colors;
                button.targetGraphic = image;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private void RefreshUi()
        {
            if (playerHealthText != null)
            {
                playerHealthText.text = $"{state.playerBaseHealth} (YOU)";
            }

            if (enemyHealthText != null)
            {
                enemyHealthText.text = $"{state.enemyBaseHealth} (THEM)";
            }

            if (boneText != null)
            {
                boneText.text = $"COLLECTING ANCESTRAL BONE: {state.ancestralBone}";
            }

            if (sparkTopText != null)
            {
                sparkTopText.text = $"PRIMAL SPARK: {state.primalSpark}";
            }

            if (sparkBottomText != null)
            {
                sparkBottomText.text = $"PRIMAL SPARK: {state.primalSpark}";
            }
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<InputSystemUIInputModule>();
        }

        private void ClearGameplayMarkers()
        {
            for (var i = gameplayRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(gameplayRoot.GetChild(i).gameObject);
            }
        }

        private Transform EnsureChild(string childName)
        {
            var child = transform.Find(childName);
            if (child != null)
            {
                return child;
            }

            var go = new GameObject(childName);
            go.transform.SetParent(transform, false);
            return go.transform;
        }

        private Text FindText(string objectName)
        {
            return GameObject.Find(objectName)?.GetComponent<Text>();
        }

        private RectTransform FindRect(string objectName)
        {
            return GameObject.Find(objectName)?.GetComponent<RectTransform>();
        }

        private static GameObject CreateQuad(string name, Transform parent, Vector3 position, Vector3 scale, Color color, float rotationZ = 0f)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetParent(parent, false);
            quad.transform.localPosition = position;
            quad.transform.localScale = scale;
            quad.transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);

            var collider = quad.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var renderer = quad.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.sharedMaterial.color = color;
            return quad;
        }

        private static RectTransform EnsurePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            var go = GameObject.Find(name) ?? new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = go.AddComponent<RectTransform>();
            }

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            var image = go.GetComponent<Image>();
            if (image == null)
            {
                image = go.AddComponent<Image>();
            }

            image.color = color;
            return rect;
        }

        private static Text EnsureText(string name, Transform parent, string content, TextAnchor alignment, int fontSize, Vector2 anchoredPosition, Vector2 size)
        {
            var go = GameObject.Find(name) ?? new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = go.AddComponent<RectTransform>();
            }

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            var text = go.GetComponent<Text>();
            if (text == null)
            {
                text = go.AddComponent<Text>();
            }

            text.text = content;
            text.alignment = alignment;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.color = new Color(0.95f, 0.92f, 0.84f);
            return text;
        }

        private static void EnsureSummonCard(Transform parent, string title, string cost, Vector2 anchoredPosition, Color color)
        {
            var card = EnsurePanel(title, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, new Vector2(190f, 78f), color);
            EnsureText($"{title}Title", card, title, TextAnchor.MiddleCenter, 16, new Vector2(0f, 12f), new Vector2(170f, 28f));
            EnsureText($"{title}Cost", card, cost, TextAnchor.MiddleCenter, 18, new Vector2(0f, -18f), new Vector2(120f, 24f));

            if (card.GetComponent<Button>() == null)
            {
                card.gameObject.AddComponent<Button>();
            }
        }

        private static void EnsureProgressBar(Transform parent, Vector2 anchoredPosition, Vector2 size, float value)
        {
            var background = EnsurePanel("ProgressBackground", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, size, new Color(0.11f, 0.20f, 0.16f, 0.95f));
            var fill = EnsurePanel("ProgressFill", background, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(2f, 0f), new Vector2(size.x * Mathf.Clamp01(value), size.y - 4f), new Color(0.39f, 0.82f, 0.68f, 1f));
            fill.pivot = new Vector2(0f, 0.5f);
            fill.anchorMin = new Vector2(0f, 0.5f);
            fill.anchorMax = new Vector2(0f, 0.5f);
        }
    }
}
