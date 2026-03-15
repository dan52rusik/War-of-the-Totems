using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using WarOfTheTotems.Core;
using WarOfTheTotems.Core.Data;
using WarOfTheTotems.Core.State;
using WarOfTheTotems.Systems;
using WarOfTheTotems.UI;
using WarOfTheTotems.Units;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WarOfTheTotems.Gameplay
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class PrototypeBattleController : MonoBehaviour, IBattleCallbacks
    {
        private const float PlayerSpawnX = -5.5f;
        private const float EnemySpawnX = 5.5f;
        private const float LaneY = -0.45f;
        private const float EvolutionZoneX = 0f;
        private const float EvolutionZoneRadius = 1.1f;
        private const float PlayerBaseX = -6.1f;
        private const float EnemyBaseX = 6.1f;

        private readonly List<Unit> units = new();

        private BattlePrototypeState state = null!; // Оставим пока для levels и evolution
        private MetaProgressionState metaState = null!;
        private readonly ProgressionSystem progression = new();
        private readonly BattleSessionState session = new();
        private readonly BattleSystem battleSystem = new();
        private readonly BattleUiManager battleUi = new();

        private Transform gameplayRoot = null!;
        private Canvas mainCanvas = null!;
        private Text playerHealthText = null!;
        private Text enemyHealthText = null!;
        private Text boneText = null!;
        private Text sparkTopText = null!;
        private Text sparkBottomText = null!;
        private Text controlsHintText = null!;
        private Text evolutionLabelText = null!;
        private Text defeatMessageText = null!;
        private Text defeatTitleText = null!;
        private Text battleMenuTitleText = null!;
        private Text battleMenuDescriptionText = null!;
        private Text battleLevelStatusText = null!;
        private Text battleLevelTitleText = null!;
        private Text battleLevelEnemyText = null!;
        private Text battleLevelPlayerText = null!;
        private Text modeTitleText = null!;
        private Text unitsBalanceText = null!;
        private Text unitsUpgradeCostText = null!;
        private Text unitsHealthValueText = null!;
        private Text unitsDamageValueText = null!;
        private Text unitsDamageCostText = null!;
        private Text unitsManaValueText = null!;
        private Text unitsManaCostText = null!;
        private RectTransform evolutionPanel = null!;
        private RectTransform evolutionFill = null!;
        private RectTransform defeatPanel = null!;
        private RectTransform unitsPanel = null!;
        private RectTransform hubPanel = null!;
        private RectTransform hubCardPanel = null!;
        private RectTransform levelSelectPanel = null!;
        private RectTransform topBarPanel = null!;
        private RectTransform battleHudPanel = null!;
        private RectTransform bottomNavPanel = null!;
        private Transform playerBaseWorldFill = null!;
        private Transform enemyBaseWorldFill = null!;
        private TextMesh playerBaseWorldText = null!;
        private TextMesh enemyBaseWorldText = null!;
        private Button baseSummonButton = null!;
        private Button stoneSummonButton = null!;
        private Button beastSummonButton = null!;
        private Button defeatUnitsButton = null!;
        private Button unitsUpgradeButton = null!;
        private Button unitsDamageUpgradeButton = null!;
        private Button unitsManaUpgradeButton = null!;
        private Button resetProgressButton = null!;
        private Button unitsBattleButton = null!;
        private Button hubBattleButton = null!;
        private Button levelStartButton = null!;
        private Button levelPrevButton = null!;
        private Button levelNextButton = null!;
        private Button navHomeButton = null!;
        private Button navUnitsButton = null!;
        private Button navBattleButton = null!;

        private float sparkAccumulator;
        private float enemyWaveTimer;
        private int playerBaseMaxHealth;
        private int enemyBaseMaxHealth;
        private bool battleEnded;
        private bool playerWon;
        private bool unitsOpenedOnce;
        private int selectedLevelIndex;
        private ScreenId screenMode;
        private LevelDefinition activeLevel;

        public float EvolutionDuration => state.evolutionDuration;
        public int BaseBearerBonusHealth => progression.BonusHealth;
        public bool StoneTotemUnlocked => activeLevel.allowStoneTotem;
        public bool BeastTotemUnlocked => activeLevel.allowBeastTotem;



        private void Awake()
        {
            EnsureCoreReferences();
            CleanupSceneDuplicates();
            gameplayRoot = EnsureChild("Gameplay");
            ClearGameplayMarkers();
            EnsureUiScaffold();
            EnsureBaseHealthOverlays();
            EnsureEventSystem();
            BindRuntimeSystems();
            BindUi();

            if (Application.isPlaying)
            {
                playerBaseMaxHealth = Mathf.Max(1, state.playerBaseHealth);
                enemyBaseMaxHealth = Mathf.Max(1, state.enemyBaseHealth);
            }
            else
            {
                ApplyEditorPreview();
            }
        }

        private void OnDestroy()
        {
            battleSystem.OnEnemySpawnRequested -= SpawnEnemyFromWave;
            battleSystem.OnBattleEnded -= HandleBattleEnded;
            progression.OnChanged -= RefreshUi;
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                return;
            }

#if UNITY_EDITOR
            EditorApplication.delayCall += EditorRebuild;
#endif
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

#if UNITY_EDITOR
            // DestroyImmediate and SetActive are forbidden inside OnValidate.
            // Defer everything to the next editor tick.
            EditorApplication.delayCall += EditorRebuild;
#endif
        }

#if UNITY_EDITOR
        private void EditorRebuild()
        {
            EditorApplication.delayCall -= EditorRebuild;

            // The component may have been destroyed while waiting.
            if (this == null || gameObject == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                return;
            }

            EnsureCoreReferences();
            CleanupSceneDuplicates();
            gameplayRoot = EnsureChild("Gameplay");
            ClearGameplayMarkers();
            EnsureUiScaffold();
            EnsureBaseHealthOverlays();
            BindUi();
            ApplyEditorPreview();
        }
#endif

        private void EnsureCoreReferences()
        {
            state = GetComponent<BattlePrototypeState>();
            if (state == null)
            {
                state = gameObject.AddComponent<BattlePrototypeState>();
            }

            metaState = GetComponent<MetaProgressionState>();
            if (metaState == null)
            {
                metaState = gameObject.AddComponent<MetaProgressionState>();
            }

            progression.Bind(metaState, state.levels.Length);
            progression.Load();
            
            // Заглушка, выпилим старый позже, так как все мета-данные теперь в metaState
            // LoadMetaProgress(); 

            selectedLevelIndex = Mathf.Clamp(selectedLevelIndex, 0, Mathf.Max(0, state.levels.Length - 1));
            activeLevel = GetSelectedLevel();
            playerBaseMaxHealth = Mathf.Max(1, state.playerBaseHealth);
            enemyBaseMaxHealth = Mathf.Max(1, state.enemyBaseHealth);
        }

        private void BindRuntimeSystems()
        {
            battleSystem.OnEnemySpawnRequested -= SpawnEnemyFromWave;
            battleSystem.OnEnemySpawnRequested += SpawnEnemyFromWave;
            battleSystem.OnBattleEnded -= HandleBattleEnded;
            battleSystem.OnBattleEnded += HandleBattleEnded;
            progression.OnChanged -= RefreshUi;
            progression.OnChanged += RefreshUi;
        }

        private void BindUi()
        {
            battleUi.Bind(
                mainCanvas,
                () => SpawnPlayerUnit(TotemType.None),
                () => SpawnPlayerUnit(TotemType.Stone),
                () => SpawnPlayerUnit(TotemType.Beast),
                OpenUnitsAfterDefeat,
                UpgradeBaseBearerHealth,
                UpgradeBaseBearerDamage,
                UpgradePrimalSparkCapacity,
                ResetProgress,
                OpenLevelSelect,
                OpenPrimaryBattleEntry,
                StartSelectedLevel,
                SelectPreviousLevel,
                SelectNextLevel,
                () => SwitchScreen(ScreenId.Hub),
                () => SwitchScreen(ScreenId.Units),
                OpenLevelSelect);
            battleUi.BindBaseHealthOverlays(playerBaseWorldFill, playerBaseWorldText, enemyBaseWorldFill, enemyBaseWorldText);
        }

        private void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            RefreshUi();
            SetEvolutionUI(false, string.Empty, 0f);
            battleUi.SetResultVisible(false);
            SwitchScreen(ScreenId.Hub);
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (screenMode != ScreenId.Battle)
            {
                RefreshUi();
                return;
            }

            if (battleEnded)
            {
                RefreshUi();
                return;
            }

            RegenerateSpark(Time.deltaTime);
            TickUnits(Time.deltaTime);
            battleSystem.Tick(Time.deltaTime);
            HandleInput();
            RefreshUi();
        }

        private void HandleInput()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && screenMode == ScreenId.Battle)
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

        public Unit FindClosestEnemy(Unit requester, float range)
        {
            Unit best = null;
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

        public bool IsAtEnemyBase(Unit unit)
        {
            return unit.Team == TeamSide.Player
                ? unit.transform.position.x >= EnemyBaseX
                : unit.transform.position.x <= PlayerBaseX;
        }

        public void DamageBase(TeamSide side, int amount)
        {
            if (side == TeamSide.Player)
            {
                battleSystem.DamagePlayerBase(amount);
            }
            else
            {
                battleSystem.DamageEnemyBase(amount);
            }
        }

        public void NotifyUnitKilled(Unit deadUnit)
        {
            if (deadUnit.Team == TeamSide.Enemy)
            {
                session.GainBone(1);
                progression.AwardCoins(1);
            }

            units.Remove(deadUnit);
        }

        public void NotifyUnitEvolved(Unit unit)
        {
            var stats = UnitFactory.BuildStats(unit.Team, unit.AssignedTotem, session, progression);
            unit.ReinitializeStats(stats);
        }

        public void SetEvolutionUI(bool visible, string label, float progress)
        {
            battleUi.SetEvolution(visible, label, progress);
            state.evolvingLabel = label;
            state.evolutionProgress = progress;
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

        private void SpawnEnemyFromWave(int waveIndex, int waveSize)
        {
            var startOffset = -(waveSize - 1) * 0.1f;
            var position = new Vector3(EnemySpawnX + startOffset + (waveIndex * 0.2f), LaneY, 0f);
            SpawnUnit(TeamSide.Enemy, TotemType.Shadow, true, position);
        }

        private void SpawnPlayerUnit(TotemType totem)
        {
            if (!CanAfford(totem))
            {
                return;
            }

            var option = GetSummonOption(totem);
            session.TrySpendSpark(option.sparkCost);
            session.TrySpendBone(option.boneCost);
            SpawnUnit(TeamSide.Player, totem, false, new Vector3(PlayerSpawnX, LaneY, 0f));
        }

        private void SpawnUnit(TeamSide team, TotemType totem, bool evolved, Vector3 position)
        {
            var unitRoot = new GameObject($"{team}_{totem}_{units.Count}");
            unitRoot.transform.SetParent(gameplayRoot, false);
            unitRoot.transform.position = position;

            CreateUnitVisual(unitRoot.transform);
            var unit = unitRoot.AddComponent<Unit>();
            var stats = UnitFactory.BuildStats(team, totem, session, progression);
            unit.Initialize(this, team, totem, stats, evolved);
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
    mainCanvas = EnsureCanvas(uiRoot);

    // 1. БОЕВОЙ UI (Верх и Низ)
    var topBar = EnsurePanel("TopBar", mainCanvas.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(1800f, 88f), new Color(0.11f, 0.14f, 0.11f, 0.82f));
    topBarPanel = topBar;
    EnsureText("PlayerHealth", topBar, "5 (YOU)", TextAnchor.MiddleLeft, 24, new Vector2(-800f, 0f), new Vector2(190f, 44f));
    EnsureText("BoneResource", topBar, "BONE: 0", TextAnchor.MiddleLeft, 22, new Vector2(-400f, 0f), new Vector2(220f, 40f));
    EnsureText("SparkResource", topBar, "SPARK: 2/2", TextAnchor.MiddleLeft, 22, new Vector2(-100f, 0f), new Vector2(220f, 40f));
    EnsureText("ControlsHint", topBar, string.Empty, TextAnchor.MiddleCenter, 18, new Vector2(300f, 0f), new Vector2(420f, 34f));
    EnsureText("EnemyHealth", topBar, "500 (THEM)", TextAnchor.MiddleRight, 24, new Vector2(800f, 0f), new Vector2(210f, 44f));
    var legacySettings = FindText("Settings");
    if (legacySettings != null) legacySettings.text = string.Empty;

    var bottomBar = EnsurePanel("BottomBar", mainCanvas.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 82f), new Vector2(1800f, 164f), new Color(0.12f, 0.09f, 0.08f, 0.88f));
    EnsureText("SparkFooter", bottomBar, "ONLY BASE BEARER IS UNLOCKED", TextAnchor.MiddleLeft, 24, new Vector2(-610f, 44f), new Vector2(480f, 40f));
    EnsureSummonCard(bottomBar, "Base Bearer (1)", "2 SP", new Vector2(-220f, 8f), new Color(0.57f, 0.43f, 0.29f));
    EnsureSummonCard(bottomBar, "Stone Totem (2)", "LOCKED", new Vector2(0f, 8f), new Color(0.43f, 0.49f, 0.52f));
    EnsureSummonCard(bottomBar, "Beast Totem (3)", "LOCKED", new Vector2(220f, 8f), new Color(0.63f, 0.37f, 0.24f));
    EnsureText("GameTitle", bottomBar, "WAR OF THE TOTEMS", TextAnchor.MiddleCenter, 28, new Vector2(0f, -48f), new Vector2(520f, 42f));
    battleHudPanel = bottomBar.GetComponent<RectTransform>();

    var overlay = EnsurePanel("EvolutionOverlay", mainCanvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 100f), new Vector2(300f, 70f), new Color(0.05f, 0.13f, 0.11f, 0.75f));
    EnsureText("EvolutionLabel", overlay, "EVOLVING: STONE GUARD", TextAnchor.UpperCenter, 20, new Vector2(0f, 15f), new Vector2(280f, 28f));
    EnsureProgressBar(overlay, new Vector2(0f, -15f), new Vector2(220f, 16f), 0.58f);

    // 2. ЭКРАН ПОРАЖЕНИЯ
    var defeat = EnsurePanel("DefeatOverlay", mainCanvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600f, 250f), new Color(0.08f, 0.06f, 0.06f, 0.94f));
    EnsureText("DefeatTitle", defeat, "ПОРАЖЕНИЕ", TextAnchor.MiddleCenter, 34, new Vector2(0f, 70f), new Vector2(280f, 42f));
    EnsureText("DefeatMessage", defeat, "НУЖНО УЛУЧШИТЬ ЗДОРОВЬЕ БОЙЦА.", TextAnchor.MiddleCenter, 22, new Vector2(0f, 15f), new Vector2(550f, 64f));
    EnsureButton("DefeatUnitsButton", defeat, "К УЛУЧШЕНИЯМ", new Vector2(0f, -60f), new Vector2(240f, 52f), new Color(0.37f, 0.49f, 0.31f));

    // 3. ХАБ (ГЛАВНЫЙ ЭКРАН) — тёмный backdrop + красивая центрированная карточка
    // Backdrop на весь экран
    var hubBackdrop = EnsurePanel("HubOverlay", mainCanvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.04f, 0.06f, 0.10f, 0.92f));
    hubBackdrop.anchorMin = Vector2.zero;
    hubBackdrop.anchorMax = Vector2.one;
    hubBackdrop.sizeDelta = Vector2.zero;
    hubBackdrop.anchoredPosition = Vector2.zero;
    hubPanel = hubBackdrop;

    // Карточка — по центру экрана, не перекрывает нижний нав (смещена вверх)
    var hubCard = EnsurePanel("HubCard", hubBackdrop, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(680f, 560f), new Color(0.09f, 0.12f, 0.18f, 1f));
    hubCardPanel = hubCard;

    // Цветовая полоса-акцент сверху карточки
    var hubAccent = EnsurePanel("HubAccent", hubCard, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(0f, 6f), new Color(0.22f, 0.72f, 0.95f, 1f));
    hubAccent.anchorMin = new Vector2(0f, 1f);
    hubAccent.anchorMax = new Vector2(1f, 1f);
    hubAccent.pivot = new Vector2(0.5f, 1f);
    hubAccent.sizeDelta = new Vector2(0f, 6f);
    hubAccent.anchoredPosition = Vector2.zero;

    // Заголовок
    EnsureText("HubModeTitle", hubCard, "БАЗА ИГРОКА", TextAnchor.MiddleCenter, 46, new Vector2(0f, 200f), new Vector2(600f, 60f));
    // Подзаголовок
    EnsureText("HubMessage", hubCard, "Здесь ты готовишься к бою и усиливаешь своё племя", TextAnchor.MiddleCenter, 22, new Vector2(0f, 140f), new Vector2(600f, 36f));

    // Разделитель
    var hubDivider = EnsurePanel("HubDivider", hubCard, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 95f), new Vector2(520f, 2f), new Color(0.22f, 0.72f, 0.95f, 0.25f));

    // Блок «Поле боя» — две базы с линией между ними
    var hubBasePlayer = EnsurePanel("HubBasePlayer", hubCard, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-200f, 30f), new Vector2(160f, 80f), new Color(0.12f, 0.40f, 0.22f, 1f));
    EnsureText("HubBasePlayerLabel", hubBasePlayer, "ТВОЯ\nБАЗА", TextAnchor.MiddleCenter, 20, Vector2.zero, new Vector2(150f, 70f));

    var hubBaseLine = EnsurePanel("HubBaseLine", hubCard, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(100f, 4f), new Color(0.22f, 0.72f, 0.95f, 0.5f));
    EnsureText("HubRoad", hubCard, "⟵────────────────⟶", TextAnchor.MiddleCenter, 18, new Vector2(0f, 8f), new Vector2(120f, 24f));

    var hubBaseEnemy = EnsurePanel("HubBaseEnemy", hubCard, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(200f, 30f), new Vector2(160f, 80f), new Color(0.40f, 0.12f, 0.14f, 1f));
    EnsureText("HubBaseEnemyLabel", hubBaseEnemy, "БАЗА\nВРАГА", TextAnchor.MiddleCenter, 20, Vector2.zero, new Vector2(150f, 70f));

    // Большая кнопка «В БОЙ»
    EnsureButton("HubBattleButton", hubCard, "В  БОЙ", new Vector2(0f, -160f), new Vector2(400f, 90f), new Color(0.85f, 0.42f, 0.10f, 1f));
    // Увеличиваем шрифт лейбла кнопки
    var hubBtnLabel = hubCard.Find("HubBattleButton")?.Find("HubBattleButtonLabel")?.GetComponent<Text>();
    if (hubBtnLabel != null) hubBtnLabel.fontSize = 34;

    var levelsBackdrop = EnsurePanel("LevelsOverlay", mainCanvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.05f, 0.07f, 0.10f, 0.92f));
    levelsBackdrop.anchorMin = Vector2.zero;
    levelsBackdrop.anchorMax = Vector2.one;
    levelsBackdrop.sizeDelta = Vector2.zero;
    levelsBackdrop.anchoredPosition = Vector2.zero;
    levelSelectPanel = levelsBackdrop;

    var levelsCard = EnsurePanel("LevelsCard", levelsBackdrop, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 35f), new Vector2(920f, 560f), new Color(0.11f, 0.12f, 0.16f, 0.98f));
    var levelsAccent = EnsurePanel("LevelsAccent", levelsCard, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(0f, 6f), new Color(0.84f, 0.46f, 0.12f, 1f));
    levelsAccent.anchorMin = new Vector2(0f, 1f);
    levelsAccent.anchorMax = new Vector2(1f, 1f);
    levelsAccent.pivot = new Vector2(0.5f, 1f);
    levelsAccent.sizeDelta = new Vector2(0f, 6f);
    levelsAccent.anchoredPosition = Vector2.zero;
    battleMenuTitleText = EnsureText("BattleMenuTitle", levelsCard, "ВЫБОР УРОВНЯ", TextAnchor.MiddleCenter, 38, new Vector2(0f, 220f), new Vector2(420f, 50f));
    battleMenuDescriptionText = EnsureText("BattleMenuDescription", levelsCard, "После обучения доступен первый бой", TextAnchor.MiddleCenter, 20, new Vector2(0f, 180f), new Vector2(680f, 32f));
    levelPrevButton = EnsureButton("LevelPrevButton", levelsCard, "<", new Vector2(-300f, 10f), new Vector2(70f, 70f), new Color(0.20f, 0.23f, 0.30f, 1f));
    levelNextButton = EnsureButton("LevelNextButton", levelsCard, ">", new Vector2(300f, 10f), new Vector2(70f, 70f), new Color(0.20f, 0.23f, 0.30f, 1f));
    var levelOneCard = EnsurePanel("Level01Card", levelsCard, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(340f, 220f), new Color(0.19f, 0.22f, 0.18f, 1f));
    battleLevelTitleText = EnsureText("Level01Title", levelOneCard, "УРОВЕНЬ 1", TextAnchor.MiddleCenter, 30, new Vector2(0f, 62f), new Vector2(240f, 40f));
    battleLevelEnemyText = EnsureText("Level01Enemy", levelOneCard, "Башня врага: 1 уровень", TextAnchor.MiddleCenter, 20, new Vector2(0f, 18f), new Vector2(240f, 28f));
    battleLevelPlayerText = EnsureText("Level01Player", levelOneCard, "У тебя только 1 базовый боец", TextAnchor.MiddleCenter, 18, new Vector2(0f, -16f), new Vector2(260f, 28f));
    battleLevelStatusText = EnsureText("Level01Status", levelOneCard, "ОТКРЫТ", TextAnchor.MiddleCenter, 18, new Vector2(0f, -56f), new Vector2(180f, 24f));
    levelStartButton = EnsureButton("Level01StartButton", levelsCard, "В БОЙ", new Vector2(0f, -180f), new Vector2(300f, 72f), new Color(0.84f, 0.42f, 0.10f, 1f));

    // 4. МЕНЮ ПРОКАЧКИ (ЮНИТЫ) — всё строим сразу в правильных родителях
    // Backdrop на весь экран
    var unitsBackdrop = EnsurePanel("UnitsOverlay", mainCanvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.05f, 0.07f, 0.10f, 0.92f));
    unitsBackdrop.anchorMin = Vector2.zero;
    unitsBackdrop.anchorMax = Vector2.one;
    unitsBackdrop.sizeDelta = Vector2.zero;
    unitsBackdrop.anchoredPosition = Vector2.zero;
    unitsPanel = unitsBackdrop;

    // Карточка — центрирована, смещена вверх чтобы не перекрывать навигацию
    var unitsCard = EnsurePanel("UnitsCard", unitsBackdrop, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 35f), new Vector2(920f, 600f), new Color(0.90f, 0.86f, 0.74f, 0.98f));

    // Цветная полоса-акцент сверху карточки
    var unitsAccent = EnsurePanel("UnitsAccent", unitsCard, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(0f, 6f), new Color(0.82f, 0.48f, 0.14f, 1f));
    unitsAccent.anchorMin = new Vector2(0f, 1f);
    unitsAccent.anchorMax = new Vector2(1f, 1f);
    unitsAccent.pivot = new Vector2(0.5f, 1f);
    unitsAccent.sizeDelta = new Vector2(0f, 6f);
    unitsAccent.anchoredPosition = Vector2.zero;

    // Заголовок и баланс монет
    modeTitleText = EnsureText("UnitsTitle", unitsCard, "Улучшения Базы", TextAnchor.MiddleCenter, 38, new Vector2(-60f, 240f), new Vector2(420f, 54f));
    unitsBalanceText = EnsureText("UnitsBalance", unitsCard, "COINS: 0", TextAnchor.MiddleRight, 26, new Vector2(345f, 240f), new Vector2(180f, 40f));
    EnsureText("UnitsSubtitle", unitsCard, "Укрепи основного бойца перед следующей тренировкой", TextAnchor.MiddleCenter, 20, new Vector2(0f, 188f), new Vector2(700f, 32f));

    // Карточки юнитов
    var selectedCard = EnsurePanel("UnitsSelectedCard", unitsCard, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-250f, 80f), new Vector2(210f, 150f), new Color(0.23f, 0.29f, 0.21f, 0.96f));
    EnsureText("UnitsSelectedName", selectedCard, "Пещерный\nЧеловек", TextAnchor.MiddleCenter, 26, new Vector2(0f, 16f), new Vector2(185f, 72f));
    EnsureText("UnitsSelectedStatus", selectedCard, "АКТИВЕН", TextAnchor.MiddleCenter, 17, new Vector2(0f, -48f), new Vector2(140f, 24f));

    var lockedCard1 = EnsurePanel("UnitsLockedCard1", unitsCard, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), new Vector2(210f, 150f), new Color(0.40f, 0.39f, 0.35f, 0.92f));
    EnsureText("UnitsLocked1", lockedCard1, "Тотем Камня\n(Закрыто)", TextAnchor.MiddleCenter, 20, Vector2.zero, new Vector2(185f, 64f));

    var lockedCard2 = EnsurePanel("UnitsLockedCard2", unitsCard, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(250f, 80f), new Vector2(210f, 150f), new Color(0.38f, 0.31f, 0.28f, 0.92f));
    EnsureText("UnitsLocked2", lockedCard2, "Тотем Зверя\n(Закрыто)", TextAnchor.MiddleCenter, 20, Vector2.zero, new Vector2(185f, 64f));

    // Карточка характеристик
    var statsCard = EnsurePanel("UnitsStatsCard", unitsCard, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -105f), new Vector2(560f, 270f), new Color(0.96f, 0.92f, 0.82f, 1f));
    EnsureText("UnitsStatsTitle", statsCard, "Характеристики", TextAnchor.MiddleCenter, 24, new Vector2(0f, 72f), new Vector2(220f, 32f));
    EnsureText("UnitsUpgradeLabel", statsCard, "Здоровье:", TextAnchor.MiddleRight, 20, new Vector2(-60f, 16f), new Vector2(240f, 36f));
    unitsHealthValueText = EnsureText("UnitsHealthValue", statsCard, "10", TextAnchor.MiddleLeft, 22, new Vector2(130f, 16f), new Vector2(120f, 36f));
    EnsureText("UnitsUpgradeCostLabel", statsCard, "Цена:", TextAnchor.MiddleRight, 19, new Vector2(-60f, -28f), new Vector2(240f, 32f));
    unitsUpgradeCostText = EnsureText("UnitsUpgradeCost", statsCard, "5", TextAnchor.MiddleLeft, 20, new Vector2(130f, -28f), new Vector2(120f, 32f));
    EnsureText("UnitsDamageLabel", statsCard, "Урон:", TextAnchor.MiddleRight, 20, new Vector2(-60f, -72f), new Vector2(240f, 32f));
    unitsDamageValueText = EnsureText("UnitsDamageValue", statsCard, "1", TextAnchor.MiddleLeft, 22, new Vector2(130f, -72f), new Vector2(60f, 32f));
    EnsureText("UnitsDamageCostLabel", statsCard, "Цена:", TextAnchor.MiddleRight, 19, new Vector2(25f, -72f), new Vector2(80f, 28f));
    unitsDamageCostText = EnsureText("UnitsDamageCost", statsCard, "4", TextAnchor.MiddleLeft, 20, new Vector2(95f, -72f), new Vector2(60f, 28f));
    EnsureButton("UnitsDamageUpgradeButton", statsCard, "ATK +", new Vector2(180f, -72f), new Vector2(110f, 40f), new Color(0.73f, 0.42f, 0.16f, 1f));
    EnsureText("UnitsManaLabel", statsCard, "Мана:", TextAnchor.MiddleRight, 20, new Vector2(-60f, -112f), new Vector2(240f, 32f));
    unitsManaValueText = EnsureText("UnitsManaValue", statsCard, "2", TextAnchor.MiddleLeft, 22, new Vector2(130f, -112f), new Vector2(60f, 32f));
    EnsureText("UnitsManaCostLabel", statsCard, "Цена:", TextAnchor.MiddleRight, 19, new Vector2(25f, -112f), new Vector2(80f, 28f));
    unitsManaCostText = EnsureText("UnitsManaCost", statsCard, "4", TextAnchor.MiddleLeft, 20, new Vector2(95f, -112f), new Vector2(60f, 28f));
    EnsureButton("UnitsManaUpgradeButton", statsCard, "MANA +", new Vector2(180f, -112f), new Vector2(110f, 40f), new Color(0.29f, 0.50f, 0.70f, 1f));

    // Кнопка улучшения
    EnsureButton("UnitsUpgradeButton", unitsCard, "УЛУЧШИТЬ", new Vector2(0f, -250f), new Vector2(320f, 76f), new Color(0.72f, 0.40f, 0.12f, 1f));
    var upgBtnLabel = unitsCard.Find("UnitsUpgradeButton")?.Find("UnitsUpgradeButtonLabel")?.GetComponent<Text>();
    if (upgBtnLabel != null) { upgBtnLabel.fontSize = 24; upgBtnLabel.color = new Color(0.98f, 0.95f, 0.88f); }
    EnsureButton("ResetProgressButton", unitsCard, "RESET", new Vector2(0f, -330f), new Vector2(220f, 52f), new Color(0.50f, 0.24f, 0.18f, 1f));

    var statsRect = statsCard.GetComponent<RectTransform>();
    if (statsRect != null)
    {
        statsRect.anchoredPosition = new Vector2(0f, -105f);
        statsRect.sizeDelta = new Vector2(560f, 270f);
    }

    SetRectTransform(statsCard.Find("UnitsStatsTitle") as RectTransform, new Vector2(0f, 106f), new Vector2(320f, 32f));

    // ===== Сетка статистик: [Метка | Значение | Цена: | ЦенаЧисло | Кнопка] =====
    // Карточка 560пх, центр (0,0), диапазоны X:
    // Метка  (MiddleRight): center=-95, w=155 → rect [-172.5 .. -17.5], правый край=-17.5
    // Значение (MiddleLeft): center=18, w=50  → rect [-7 .. 43], левый край=-7  → зазор=10.5px ✓
    // "Cost:" (MiddleRight): center=88, w=68  → rect [54 .. 122], правый край=122
    // ЦенаЧисло (MiddleLeft): center=138, w=46 → rect [115 .. 161], левый край=115 → зазор=7px ✓
    // Кнопка (center):       center=218, w=100 → rect [168 .. 268], левый край=168 → зазор=7px ✓
    // Строки: y=40 (HP), y=-12 (ATK), y=-64 (MANA)

    // --- Строка HP (y=40) ---
    SetRectTransform(statsCard.Find("UnitsUpgradeLabel")     as RectTransform, new Vector2(-95f,  40f), new Vector2(155f, 30f));
    SetRectTransform(statsCard.Find("UnitsHealthValue")      as RectTransform, new Vector2( 18f,  40f), new Vector2( 50f, 30f));
    SetRectTransform(statsCard.Find("UnitsUpgradeCostLabel") as RectTransform, new Vector2( 88f,  40f), new Vector2( 68f, 26f));
    SetRectTransform(statsCard.Find("UnitsUpgradeCost")      as RectTransform, new Vector2(138f,  40f), new Vector2( 46f, 26f));
    // HP+ кнопка — в unitsCard; y = statsCard_y(-105) + row_y(40) = -65
    SetRectTransform(unitsCard.Find("UnitsUpgradeButton")    as RectTransform, new Vector2(218f, -65f), new Vector2(100f, 36f));
    var compactHpLabel = unitsCard.Find("UnitsUpgradeButton")?.Find("UnitsUpgradeButtonLabel")?.GetComponent<Text>();
    if (compactHpLabel != null) { compactHpLabel.text = "HP +"; compactHpLabel.fontSize = 17; }

    // --- Строка ATK (y=-12) ---
    SetRectTransform(statsCard.Find("UnitsDamageLabel")         as RectTransform, new Vector2(-95f, -12f), new Vector2(155f, 30f));
    SetRectTransform(statsCard.Find("UnitsDamageValue")         as RectTransform, new Vector2( 18f, -12f), new Vector2( 50f, 30f));
    SetRectTransform(statsCard.Find("UnitsDamageCostLabel")     as RectTransform, new Vector2( 88f, -12f), new Vector2( 68f, 26f));
    SetRectTransform(statsCard.Find("UnitsDamageCost")          as RectTransform, new Vector2(138f, -12f), new Vector2( 46f, 26f));
    SetRectTransform(statsCard.Find("UnitsDamageUpgradeButton") as RectTransform, new Vector2(218f, -12f), new Vector2(100f, 36f));

    // --- Строка MANA (y=-64) ---
    SetRectTransform(statsCard.Find("UnitsManaLabel")           as RectTransform, new Vector2(-95f, -64f), new Vector2(155f, 30f));
    SetRectTransform(statsCard.Find("UnitsManaValue")           as RectTransform, new Vector2( 18f, -64f), new Vector2( 50f, 30f));
    SetRectTransform(statsCard.Find("UnitsManaCostLabel")       as RectTransform, new Vector2( 88f, -64f), new Vector2( 68f, 26f));
    SetRectTransform(statsCard.Find("UnitsManaCost")            as RectTransform, new Vector2(138f, -64f), new Vector2( 46f, 26f));
    SetRectTransform(statsCard.Find("UnitsManaUpgradeButton")   as RectTransform, new Vector2(218f, -64f), new Vector2(100f, 36f));

    // RESET под статкардом
    SetRectTransform(unitsCard.Find("ResetProgressButton")      as RectTransform, new Vector2(0f, -255f), new Vector2(200f, 46f));


    // Цвета текста для экрана Юнитов
    SetTextColor("UnitsTitle", new Color(0.20f, 0.16f, 0.12f));
    SetTextColor("UnitsBalance", new Color(0.82f, 0.48f, 0.14f));
    SetTextColor("UnitsSubtitle", new Color(0.39f, 0.34f, 0.28f));
    SetTextColor("UnitsSelectedName", new Color(0.96f, 0.94f, 0.86f));
    SetTextColor("UnitsSelectedStatus", new Color(0.62f, 0.90f, 0.52f));
    SetTextColor("UnitsLocked1", new Color(0.84f, 0.82f, 0.78f));
    SetTextColor("UnitsLocked2", new Color(0.84f, 0.82f, 0.78f));
    SetTextColor("UnitsStatsTitle", new Color(0.26f, 0.20f, 0.14f));
    SetTextColor("UnitsUpgradeLabel", new Color(0.20f, 0.18f, 0.16f));
    SetTextColor("UnitsUpgradeCostLabel", new Color(0.32f, 0.29f, 0.24f));
    SetTextColor("UnitsHealthValue", new Color(0.16f, 0.58f, 0.26f));
    SetTextColor("UnitsUpgradeCost", new Color(0.82f, 0.48f, 0.14f));
    SetTextColor("UnitsDamageLabel", new Color(0.20f, 0.18f, 0.16f));
    SetTextColor("UnitsDamageCostLabel", new Color(0.32f, 0.29f, 0.24f));
    SetTextColor("UnitsDamageValue", new Color(0.72f, 0.33f, 0.18f));
    SetTextColor("UnitsDamageCost", new Color(0.82f, 0.48f, 0.14f));
    SetTextColor("UnitsManaLabel", new Color(0.20f, 0.18f, 0.16f));
    SetTextColor("UnitsManaCostLabel", new Color(0.32f, 0.29f, 0.24f));
    SetTextColor("UnitsManaValue", new Color(0.18f, 0.40f, 0.70f));
    SetTextColor("UnitsManaCost", new Color(0.82f, 0.48f, 0.14f));

    // Hub цвета текста
    SetTextColor("HubModeTitle", new Color(0.95f, 0.97f, 1.0f));
    SetTextColor("HubMessage", new Color(0.65f, 0.78f, 0.90f));
    SetTextColor("HubRoad", new Color(0.40f, 0.68f, 0.90f));
    SetTextColor("HubBasePlayerLabel", new Color(0.55f, 0.95f, 0.70f));
    SetTextColor("HubBaseEnemyLabel", new Color(0.95f, 0.60f, 0.60f));

    // 5. НИЖНЯЯ НАВИГАЦИЯ (Всегда поверх остальных окон)
    var nav = EnsurePanel("BottomNav", mainCanvas.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(700f, 100f), new Color(0.16f, 0.16f, 0.19f, 0.98f));
    bottomNavPanel = nav.GetComponent<RectTransform>();
    EnsureButton("NavHomeButton", nav, "ДОМ", new Vector2(-220f, 0f), new Vector2(200f, 80f), new Color(0.35f, 0.38f, 0.45f));
    EnsureButton("NavUnitsButton", nav, "ЮНИТЫ", new Vector2(0f, 0f), new Vector2(200f, 80f), new Color(0.35f, 0.38f, 0.45f));
    EnsureButton("NavBattleButton", nav, "БИТВА", new Vector2(220f, 0f), new Vector2(200f, 80f), new Color(0.35f, 0.38f, 0.45f));

    // Навигация рисуется поверх всех окон
    bottomNavPanel.SetAsLastSibling();
}

        private void WireUi()
        {
            playerHealthText = FindText("PlayerHealth");
            enemyHealthText = FindText("EnemyHealth");
            boneText = FindText("BoneResource");
            sparkTopText = FindText("SparkResource");
            sparkBottomText = FindText("SparkFooter");
            evolutionLabelText = FindText("EvolutionLabel");
            controlsHintText = FindText("ControlsHint");
            if (controlsHintText != null) controlsHintText.gameObject.SetActive(false);
            var settingsText = FindText("Settings");
            if (settingsText != null) settingsText.gameObject.SetActive(false);
            defeatMessageText = FindText("DefeatMessage");
            defeatTitleText = FindText("DefeatTitle");
            battleMenuTitleText = FindText("BattleMenuTitle");
            battleMenuDescriptionText = FindText("BattleMenuDescription");
            battleLevelTitleText = FindText("Level01Title");
            battleLevelEnemyText = FindText("Level01Enemy");
            battleLevelPlayerText = FindText("Level01Player");
            battleLevelStatusText = FindText("Level01Status");
            unitsBalanceText = FindText("UnitsBalance");
            unitsUpgradeCostText = FindText("UnitsUpgradeCost");
            unitsHealthValueText = FindText("UnitsHealthValue");
            unitsDamageValueText = FindText("UnitsDamageValue");
            unitsDamageCostText = FindText("UnitsDamageCost");
            unitsManaValueText = FindText("UnitsManaValue");
            unitsManaCostText = FindText("UnitsManaCost");

            var evolutionOverlay = FindRect("EvolutionOverlay");
            evolutionPanel = evolutionOverlay;
            evolutionFill = FindRect("ProgressFill");
            defeatPanel = FindRect("DefeatOverlay");
            unitsPanel = FindRect("UnitsOverlay");
            hubPanel = FindRect("HubOverlay");
            hubCardPanel = FindRect("HubCard");
            levelSelectPanel = FindRect("LevelsOverlay");
            topBarPanel = FindRect("TopBar");
            battleHudPanel = FindRect("BottomBar");
            bottomNavPanel = FindRect("BottomNav");

            baseSummonButton = BindButton(new[] { "Base Bearer (1)", "Base Bearer" }, () => SpawnPlayerUnit(TotemType.None));
            stoneSummonButton = BindButton(new[] { "Stone Totem (2)", "Stone Totem Summon" }, () => SpawnPlayerUnit(TotemType.Stone));
            beastSummonButton = BindButton(new[] { "Beast Totem (3)", "Beast Totem Summon" }, () => SpawnPlayerUnit(TotemType.Beast));
            defeatUnitsButton = BindButton(new[] { "DefeatUnitsButton" }, OpenUnitsAfterDefeat);
            unitsUpgradeButton = BindButton(new[] { "UnitsUpgradeButton" }, UpgradeBaseBearerHealth);
            unitsDamageUpgradeButton = BindButton(new[] { "UnitsDamageUpgradeButton" }, UpgradeBaseBearerDamage);
            unitsManaUpgradeButton = BindButton(new[] { "UnitsManaUpgradeButton" }, UpgradePrimalSparkCapacity);
            resetProgressButton = BindButton(new[] { "ResetProgressButton" }, ResetProgress);
            unitsBattleButton = BindButton(new[] { "UnitsBattleButton" }, OpenLevelSelect);
            hubBattleButton = BindButton(new[] { "HubBattleButton" }, OpenPrimaryBattleEntry);
            levelStartButton = BindButton(new[] { "Level01StartButton" }, StartSelectedLevel);
            levelPrevButton = BindButton(new[] { "LevelPrevButton" }, SelectPreviousLevel);
            levelNextButton = BindButton(new[] { "LevelNextButton" }, SelectNextLevel);
            navHomeButton = BindButton(new[] { "NavHomeButton" }, () => SwitchScreen(ScreenId.Hub));
            navUnitsButton = BindButton(new[] { "NavUnitsButton" }, () => SwitchScreen(ScreenId.Units));
            navBattleButton = BindButton(new[] { "NavBattleButton" }, OpenLevelSelect);

            SetPanelVisible(defeatPanel, false);
            SetPanelVisible(unitsPanel, false);
        }

        private Button BindButton(string[] objectNames, UnityEngine.Events.UnityAction action)
        {
            if (mainCanvas == null) return null;

            Button target = null;
            var allButtons = mainCanvas.GetComponentsInChildren<Button>(true);
            foreach (var btn in allButtons)
            {
                for (var i = 0; i < objectNames.Length; i++)
                {
                    if (btn.gameObject.name == objectNames[i])
                    {
                        target = btn;
                        break;
                    }
                }
                if (target != null) break;
            }

            if (target == null)
            {
                return null;
            }

            var image = target.GetComponent<Image>();
            if (image != null)
            {
                var colors = target.colors;
                colors.normalColor = image.color;
                colors.highlightedColor = image.color * 1.08f;
                colors.pressedColor = image.color * 0.88f;
                colors.selectedColor = image.color;
                target.colors = colors;
                target.targetGraphic = image;
            }

            target.onClick.RemoveAllListeners();
            target.onClick.AddListener(action);
            return target;
        }

        private void RefreshUi()
        {
            if (playerHealthText != null)
            {
                playerHealthText.text = $"{session.PlayerBaseHealth} (YOU)";
            }

            if (enemyHealthText != null)
            {
                enemyHealthText.text = $"{session.EnemyBaseHealth} (THEM)";
            }

            if (boneText != null)
            {
                boneText.text = $"BONE: {session.AncestralBone}";
            }

            if (sparkTopText != null)
            {
                sparkTopText.text = $"SPARK: {session.PrimalSpark}/{session.PrimalSparkMax}";
            }

            if (sparkBottomText != null)
            {
                sparkBottomText.text = screenMode == ScreenId.Battle
                    ? $"PRIMAL SPARK: {session.PrimalSpark}/{session.PrimalSparkMax}"
                    : string.Empty;
            }

            if (controlsHintText != null)
            {
                controlsHintText.text = screenMode == ScreenId.Battle
                    ? "Призови одного бойца. После поражения открой улучшения."
                    : "Открой Юниты и улучши здоровье после тренировки.";
            }

            if (defeatTitleText != null)
            {
                defeatTitleText.text = playerWon ? "ПОБЕДА!" : "ПОРАЖЕНИЕ";
            }

            if (defeatMessageText != null)
            {
                if (playerWon)
                {
                    defeatMessageText.text = "БАЗА ВРАГА УНИЧТОЖЕНА! МОНЕТЫ ПОЛУЧЕНЫ.";
                }
                else
                {
                    defeatMessageText.text = unitsOpenedOnce
                        ? "ПОТРАТЬ МОНЕТЫ НА ЗДОРОВЬЕ И ПОПРОБУЙ СНОВА."
                        : "ВРАГ ПОБЕДИЛ. ОТКРОЙ ЮНИТЫ И УЛУЧШИ ЗДОРОВЬЕ.";
                }
            }

            if (unitsBalanceText != null)
            {
                unitsBalanceText.text = $"COINS: {progression.Coins}";
            }

            if (unitsUpgradeCostText != null)
            {
                unitsUpgradeCostText.text = $"{progression.HealthUpgradeCost}";
            }

            if (unitsHealthValueText != null)
            {
                unitsHealthValueText.text = $"{5 + progression.BonusHealth}";
            }

            if (unitsDamageValueText != null)
            {
                unitsDamageValueText.text = $"{1 + progression.BonusDamage}";
            }

            if (unitsDamageCostText != null)
            {
                unitsDamageCostText.text = $"{progression.DamageUpgradeCost}";
            }

            if (unitsManaValueText != null)
            {
                unitsManaValueText.text = $"{GetSelectedLevel().startingSpark + progression.SparkBonus}";
            }

            if (unitsManaCostText != null)
            {
                unitsManaCostText.text = $"{progression.SparkUpgradeCost}";
            }

            if (battleMenuTitleText != null)
            {
                battleMenuTitleText.text = progression.TutorialCompleted ? "ВЫБОР УРОВНЯ" : "ОБУЧЕНИЕ";
            }

            if (battleMenuDescriptionText != null)
            {
                battleMenuDescriptionText.text = progression.TutorialCompleted
                    ? $"{activeLevel.title}: {activeLevel.description}"
                    : "Сначала пройди показательный бой, затем откроется уровень 1.";
            }

            if (battleLevelStatusText != null)
            {
                battleLevelStatusText.text = progression.TutorialCompleted ? "ОТКРЫТ" : "ОТКРОЕТСЯ ПОСЛЕ ОБУЧЕНИЯ";
            }

            if (levelStartButton != null)
            {
                levelStartButton.interactable = progression.TutorialCompleted;
            }

            if (battleMenuTitleText != null) battleMenuTitleText.text = "ВЫБОР УРОВНЯ";
            if (battleMenuDescriptionText != null) battleMenuDescriptionText.text = $"{GetSelectedLevel().title}: {GetSelectedLevel().description}";
            if (battleLevelStatusText != null) battleLevelStatusText.text = "ОТКРЫТ";
            var selectedLevel = GetSelectedLevel();
            var selectedLevelNumber = selectedLevelIndex + 1;
            var levelUnlocked = IsLevelUnlocked(selectedLevelIndex);

            if (battleMenuTitleText != null) battleMenuTitleText.text = "ВЫБОР УРОВНЯ";
            if (battleMenuDescriptionText != null) battleMenuDescriptionText.text = selectedLevel.description;
            if (battleLevelTitleText != null) battleLevelTitleText.text = selectedLevel.title.ToUpperInvariant();
            if (battleLevelEnemyText != null) battleLevelEnemyText.text = $"Башня врага: {selectedLevel.enemyBaseHealth} HP";
            if (battleLevelPlayerText != null) battleLevelPlayerText.text = levelUnlocked
                ? $"Награда: {selectedLevel.rewardCoins} мон."
                : $"Откроется после уровня {Mathf.Max(1, selectedLevelNumber - 1)}";
            if (battleLevelStatusText != null) battleLevelStatusText.text = levelUnlocked ? "ОТКРЫТ" : "ЗАКРЫТ";
            if (levelStartButton != null) levelStartButton.interactable = levelUnlocked;
            if (levelPrevButton != null) levelPrevButton.interactable = selectedLevelIndex > 0;
            if (levelNextButton != null) levelNextButton.interactable = selectedLevelIndex < state.levels.Length - 1;

            RefreshBaseHealthOverlay(playerBaseWorldFill, playerBaseWorldText, session.PlayerBaseHealth, playerBaseMaxHealth);
            RefreshBaseHealthOverlay(enemyBaseWorldFill, enemyBaseWorldText, session.EnemyBaseHealth, enemyBaseMaxHealth);
            RefreshSummonUi();
        }

        private void EnsureEventSystem()
        {
            var eventSystems = FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (eventSystems.Length > 0)
            {
                var primaryEventSystem = eventSystems[0];
                for (var i = 1; i < eventSystems.Length; i++)
                {
                    DestroyObject(eventSystems[i].gameObject);
                }

                if (primaryEventSystem.GetComponent<InputSystemUIInputModule>() == null)
                {
                    primaryEventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                }

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
                if (Application.isPlaying)
                {
                    Destroy(gameplayRoot.GetChild(i).gameObject);
                }
                else
                {
                    DestroyImmediate(gameplayRoot.GetChild(i).gameObject);
                }
            }
        }

        private Transform EnsureChild(string childName)
        {
            Transform child = null;
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var candidate = transform.GetChild(i);
                if (candidate.name != childName)
                {
                    continue;
                }

                if (child == null)
                {
                    child = candidate;
                    continue;
                }

                DestroyObject(candidate.gameObject);
            }

            if (child != null)
            {
                return child;
            }

            var go = new GameObject(childName);
            go.transform.SetParent(transform, false);
            return go.transform;
        }

        private void CleanupSceneDuplicates()
        {
            EnsureChild("UI");
            EnsureChild("Gameplay");

            var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Canvas primaryCanvas = null;
            for (var i = 0; i < canvases.Length; i++)
            {
                if (canvases[i].transform.IsChildOf(transform))
                {
                    primaryCanvas = canvases[i];
                    break;
                }
            }

            if (primaryCanvas == null && canvases.Length > 0)
            {
                primaryCanvas = canvases[0];
            }

            for (var i = 0; i < canvases.Length; i++)
            {
                if (canvases[i] != primaryCanvas)
                {
                    DestroyObject(canvases[i].gameObject);
                }
            }

            var eventSystems = FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 1; i < eventSystems.Length; i++)
            {
                DestroyObject(eventSystems[i].gameObject);
            }
        }

        private Canvas EnsureCanvas(Transform uiRoot)
        {
            var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Canvas canvas = null;
            for (var i = 0; i < canvases.Length; i++)
            {
                if (canvases[i].transform.IsChildOf(uiRoot))
                {
                    canvas = canvases[i];
                    break;
                }
            }

            if (canvas == null && canvases.Length > 0)
            {
                canvas = canvases[0];
            }

            if (canvas == null)
            {
                var canvasGo = new GameObject("Canvas");
                canvasGo.transform.SetParent(uiRoot, false);
                canvas = canvasGo.AddComponent<Canvas>();
            }
            else if (canvas.transform.parent != uiRoot)
            {
                canvas.transform.SetParent(uiRoot, false);
            }

            canvas.name = "Canvas";
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            if (canvas.GetComponent<GraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            }

            for (var i = 0; i < canvases.Length; i++)
            {
                if (canvases[i] != canvas)
                {
                    DestroyObject(canvases[i].gameObject);
                }
            }

            return canvas;
        }

        private static void DestroyObject(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }

        private Text FindText(string objectName)
        {
            if (mainCanvas == null) return null;
            var texts = mainCanvas.GetComponentsInChildren<Text>(true);
            foreach (var t in texts)
            {
                if (t.gameObject.name == objectName) return t;
            }
            return null;
        }

        private RectTransform FindRect(string objectName)
        {
            if (mainCanvas == null) return null;
            var rects = mainCanvas.GetComponentsInChildren<RectTransform>(true);
            foreach (var r in rects)
            {
                if (r.gameObject.name == objectName) return r;
            }
            return null;
        }

        private void EnsureBaseHealthOverlays()
        {
            var playerRoot = EnsureBaseOverlay(gameplayRoot, "PlayerBaseWorldHp", new Vector3(-6.7f, 2.15f, -0.3f), new Color(0.52f, 0.88f, 0.70f), out var playerFill, out var playerText);
            var enemyRoot = EnsureBaseOverlay(gameplayRoot, "EnemyBaseWorldHp", new Vector3(6.7f, 2.2f, -0.3f), new Color(0.94f, 0.46f, 0.49f), out var enemyFill, out var enemyText);

            playerBaseWorldFill = playerFill;
            enemyBaseWorldFill = enemyFill;
            playerBaseWorldText = playerText;
            enemyBaseWorldText = enemyText;
        }

        private GameObject EnsureBaseOverlay(Transform parent, string rootName, Vector3 position, Color fillColor, out Transform fill, out TextMesh valueText)
        {
            var root = parent.Find(rootName)?.gameObject ?? new GameObject(rootName);
            root.transform.SetParent(parent, true);
            root.transform.position = position;

            var title = EnsureWorldText(root.transform, "Title", "BASE HP", new Vector3(0f, 0.32f, 0f), 28, 0.05f, new Color(0.96f, 0.93f, 0.83f));
            title.anchor = TextAnchor.MiddleCenter;

            valueText = EnsureWorldText(root.transform, "Value", "0 / 0", new Vector3(0f, 0.16f, 0f), 28, 0.05f, Color.white);
            valueText.anchor = TextAnchor.MiddleCenter;

            var barRoot = root.transform.Find("BarRoot");
            if (barRoot == null)
            {
                barRoot = new GameObject("BarRoot").transform;
                barRoot.SetParent(root.transform, false);
            }

            barRoot.localPosition = Vector3.zero;
            EnsureQuad("BarBack", barRoot, Vector3.zero, new Vector3(1.4f, 0.16f, 1f), new Color(0.14f, 0.16f, 0.14f));
            fill = EnsureQuad("BarFill", barRoot, new Vector3(-0.35f, 0f, -0.01f), new Vector3(0.7f, 0.11f, 1f), fillColor).transform;
            fill.localScale = new Vector3(1f, 1f, 1f);
            return root;
        }

        private static void RefreshBaseHealthOverlay(Transform fill, TextMesh valueText, int current, int maximum)
        {
            if (fill == null || valueText == null)
            {
                return;
            }

            var ratio = maximum <= 0 ? 0f : Mathf.Clamp01((float)current / maximum);
            fill.localScale = new Vector3(ratio, 1f, 1f);
            fill.localPosition = new Vector3(-0.7f + (0.7f * ratio), 0f, -0.01f);
            valueText.text = $"{current} / {maximum}";
        }

        private void RefreshSummonUi()
        {
            UpdateSummonCard(baseSummonButton, "Base Bearer (1)Cost", GetCostLabel(TotemType.None), CanAfford(TotemType.None));
            UpdateSummonCard(stoneSummonButton, "Stone Totem (2)Cost", GetCostLabel(TotemType.Stone), CanAfford(TotemType.Stone));
            UpdateSummonCard(beastSummonButton, "Beast Totem (3)Cost", GetCostLabel(TotemType.Beast), CanAfford(TotemType.Beast));

            if (stoneSummonButton != null)
            {
                stoneSummonButton.gameObject.SetActive(StoneTotemUnlocked);
            }

            if (beastSummonButton != null)
            {
                beastSummonButton.gameObject.SetActive(BeastTotemUnlocked);
            }

            if (unitsUpgradeButton != null)
            {
                unitsUpgradeButton.interactable = state.upgradeCoins >= state.baseBearerHealthUpgradeCost;
            }

            if (unitsDamageUpgradeButton != null)
            {
                unitsDamageUpgradeButton.interactable = state.upgradeCoins >= state.baseBearerDamageUpgradeCost;
            }

            if (unitsManaUpgradeButton != null)
            {
                unitsManaUpgradeButton.interactable = state.upgradeCoins >= state.primalSparkUpgradeCost;
            }
        }

        private void UpdateSummonCard(Button button, string costTextName, string costLabel, bool canAfford)
        {
            var costText = FindText(costTextName);
            if (costText != null)
            {
                costText.text = costLabel;
            }

            if (button == null)
            {
                return;
            }

            button.interactable = canAfford;
            if (button.targetGraphic is Image image)
            {
                var baseColor = image.color;
                image.color = canAfford ? new Color(baseColor.r, baseColor.g, baseColor.b, 1f) : new Color(baseColor.r * 0.55f, baseColor.g * 0.55f, baseColor.b * 0.55f, 0.8f);
            }
        }

        private string GetCostLabel(TotemType totem)
        {
            var option = GetSummonOption(totem);
            return option.boneCost > 0
                ? $"{option.sparkCost} SP + {option.boneCost} BN"
                : $"{option.sparkCost} SP";
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
                if (Application.isPlaying)
                {
                    Destroy(collider);
                }
                else
                {
                    collider.enabled = false;
                }
            }

            var renderer = quad.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.sharedMaterial.color = color;
            return quad;
        }

        private static GameObject EnsureQuad(string name, Transform parent, Vector3 position, Vector3 scale, Color color)
        {
            var quad = parent.Find(name)?.gameObject ?? CreateQuad(name, parent, position, scale, color);
            quad.transform.SetParent(parent, false);
            quad.transform.localPosition = position;
            quad.transform.localScale = scale;
            quad.transform.localRotation = Quaternion.identity;

            if (quad.TryGetComponent<MeshRenderer>(out var renderer))
            {
                if (Application.isPlaying)
                    renderer.material.color = color;
                else
                    renderer.sharedMaterial.color = color;
            }

            return quad;
        }

        private static TextMesh EnsureWorldText(Transform parent, string name, string content, Vector3 localPosition, int fontSize, float charSize, Color color)
        {
            var go = parent.Find(name)?.gameObject ?? new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;

            var text = go.GetComponent<TextMesh>();
            if (text == null)
            {
                text = go.AddComponent<TextMesh>();
            }

            text.text = content;
            text.fontSize = fontSize;
            text.characterSize = charSize;
            text.color = color;
            text.alignment = TextAlignment.Center;
            return text;
        }

        private static RectTransform EnsurePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            var go = parent.Find(name)?.gameObject ?? new GameObject(name);
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
            var go = parent.Find(name)?.gameObject ?? new GameObject(name);
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

        private static void SetRectTransform(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }

        private void RegenerateSpark(float deltaTime)
        {
            if (session.PrimalSpark >= session.PrimalSparkMax)
            {
                sparkAccumulator = 0f;
                return;
            }

            sparkAccumulator += activeLevel.sparkRegenPerSecond * deltaTime;
            if (sparkAccumulator < 1f)
            {
                return;
            }

            var gained = Mathf.FloorToInt(sparkAccumulator);
            sparkAccumulator -= gained;
            session.GainSpark(gained);
        }

        private bool CanAfford(TotemType totem)
        {
            if (totem == TotemType.Stone && !StoneTotemUnlocked) return false;
            if (totem == TotemType.Beast && !BeastTotemUnlocked) return false;
            var option = GetSummonOption(totem);
            return session.PrimalSpark >= option.sparkCost && session.AncestralBone >= option.boneCost;
        }

        private SummonOption GetSummonOption(TotemType totem)
        {
            return totem switch
            {
                TotemType.Stone => state.summonOptions[1],
                TotemType.Beast => state.summonOptions[2],
                _ => state.summonOptions[0],
            };
        }

        private void ApplyEditorPreview()
        {
            battleEnded = false;
            session.BeginBattle(activeLevel, progression.SparkBonus);

            SetEvolutionUI(true, "EVOLVING: STONE GUARD", 0.58f);
            battleUi.SetResultVisible(false);

            if (stoneSummonButton != null) stoneSummonButton.gameObject.SetActive(true);
            if (beastSummonButton != null) beastSummonButton.gameObject.SetActive(true);

            screenMode = ScreenId.Hub;
            ApplyScreenVisibility();
            RefreshUi();
        }

        private void HandleBattleEnded(bool didPlayerWin)
        {
            if (battleEnded)
            {
                return;
            }

            battleEnded = true;
            playerWon = didPlayerWin;

            if (didPlayerWin)
            {
                if (!progression.TutorialCompleted)
                {
                    progression.MarkTutorialComplete();
                }

                if (activeLevel.id != "tutorial")
                {
                    progression.UnlockLevel(selectedLevelIndex + 1);
                }

                progression.AwardCoins(Mathf.Max(1, activeLevel.rewardCoins));
            }

            ShowBattleResult();
        }

        private void ShowBattleResult()
        {
            battleUi.SetResultVisible(true);
        }

        private void OpenUnitsAfterDefeat()
        {
            unitsOpenedOnce = true;
            battleUi.SetResultVisible(false);
            SwitchScreen(ScreenId.Units);
        }

        private void UpgradeBaseBearerHealth()
        {
            if (progression.TryUpgradeHealth())
            {
                RefreshUi();
            }
        }

        private void UpgradeBaseBearerDamage()
        {
            if (progression.TryUpgradeDamage())
            {
                RefreshUi();
            }
        }

        private void UpgradePrimalSparkCapacity()
        {
            if (progression.TryUpgradeSpark())
            {
                RefreshUi();
            }
        }

        private void ResetProgress()
        {
            progression.ResetAll();
            selectedLevelIndex = 0;
            activeLevel = GetSelectedLevel();
            SwitchScreen(ScreenId.Hub);
        }

        private void OpenPrimaryBattleEntry()
        {
            if (progression.TutorialCompleted)
            {
                OpenLevelSelect();
                return;
            }

            StartTutorialBattle();
        }

        private void OpenLevelSelect()
        {
            activeLevel = GetSelectedLevel();
            SwitchScreen(ScreenId.Levels);
        }

        private void SelectPreviousLevel()
        {
            selectedLevelIndex = Mathf.Max(0, selectedLevelIndex - 1);
            activeLevel = GetSelectedLevel();
            RefreshUi();
        }

        private void SelectNextLevel()
        {
            selectedLevelIndex = Mathf.Min(Mathf.Max(0, state.levels.Length - 1), selectedLevelIndex + 1);
            activeLevel = GetSelectedLevel();
            RefreshUi();
        }

        private void StartSelectedLevel()
        {
            if (!IsLevelUnlocked(selectedLevelIndex))
            {
                return;
            }

            selectedLevelIndex = Mathf.Clamp(selectedLevelIndex, 0, Mathf.Max(0, state.levels.Length - 1));
            activeLevel = GetSelectedLevel();
            StartLevelBattle(activeLevel);
        }

        private void StartTutorialBattle()
        {
            activeLevel = new LevelDefinition(
                id: "tutorial",
                title: "Обучение",
                description: "Показательный бой",
                rewardCoins: 5,
                playerBaseHealth: 5,
                enemyBaseHealth: 500,
                startingSpark: 10,
                sparkRegenPerSecond: 1f,
                enemyWaveSize: 3,
                enemyWaveInterval: 8f,
                playerBaseBearerHealth: 14,
                playerBaseBearerDamage: 1,
                enemyShadowHealth: 0,
                enemyShadowDamage: 0,
                allowStoneTotem: false,
                allowBeastTotem: false);

            StartLevelBattle(activeLevel);
        }

        private void StartLevelBattle(LevelDefinition level)
        {
            ApplyLevelState(level);
            ResetBattleState();
            SwitchScreen(ScreenId.Battle);
            battleSystem.StartBattle(session, level);
        }

        private void ApplyLevelState(LevelDefinition level)
        {
            activeLevel = level;
            session.BeginBattle(level, progression.SparkBonus);

            playerBaseMaxHealth = Mathf.Max(1, level.playerBaseHealth);
            enemyBaseMaxHealth = Mathf.Max(1, level.enemyBaseHealth);
        }

        private LevelDefinition GetSelectedLevel()
        {
            if (state.levels == null || state.levels.Length == 0)
            {
                return default;
            }

            var definition = state.levels[Mathf.Clamp(selectedLevelIndex, 0, state.levels.Length - 1)];
            return new LevelDefinition(
                definition.id, definition.title, definition.description,
                definition.rewardCoins, definition.playerBaseHealth, definition.enemyBaseHealth,
                definition.startingSpark, definition.sparkRegenPerSecond, definition.enemyWaveSize,
                definition.enemyWaveInterval, definition.playerBaseBearerHealth, definition.playerBaseBearerDamage,
                definition.enemyShadowHealth, definition.enemyShadowDamage, definition.allowStoneTotem, definition.allowBeastTotem);
        }

        private bool IsLevelUnlocked(int levelIndex)
        {
            return progression.IsLevelUnlocked(levelIndex);
        }

        // Удалили LoadMetaProgress и SaveMetaProgress (теперь в ProgressionSystem)

        private static Button EnsureButton(string name, Transform parent, string label, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            var rect = EnsurePanel(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, size, color);
            EnsureText($"{name}Label", rect, label, TextAnchor.MiddleCenter, 20, Vector2.zero, size);
            var button = rect.GetComponent<Button>();
            if (button == null)
            {
                button = rect.gameObject.AddComponent<Button>();
            }

            button.targetGraphic = rect.GetComponent<Image>();
            return button;
        }

        private static void SetPanelVisible(RectTransform panel, bool visible)
        {
            if (panel != null)
            {
                panel.gameObject.SetActive(visible);
            }
        }

        private void SwitchScreen(ScreenId mode)
        {
            screenMode = mode;
            ApplyScreenVisibility();
            RefreshUi();
        }

        private void ApplyScreenVisibility()
        {
            SetPanelVisible(hubPanel, screenMode == ScreenId.Hub);
            SetPanelVisible(hubCardPanel, screenMode == ScreenId.Hub && !progression.TutorialCompleted);
            SetPanelVisible(levelSelectPanel, screenMode == ScreenId.Levels);
            SetPanelVisible(unitsPanel, screenMode == ScreenId.Units);
            SetPanelVisible(topBarPanel, screenMode == ScreenId.Battle);
            SetPanelVisible(battleHudPanel, screenMode == ScreenId.Battle);
            SetPanelVisible(bottomNavPanel, screenMode != ScreenId.Battle);
            SetPanelVisible(evolutionPanel, screenMode == ScreenId.Battle && Application.isPlaying);
            if (gameplayRoot != null && Application.isPlaying)
            {
                gameplayRoot.gameObject.SetActive(screenMode == ScreenId.Battle);
            }
            if (screenMode == ScreenId.Hub)
            {
                SetPanelVisible(defeatPanel, false);
            }
        }

        private void ResetBattleState()
        {
            battleEnded = false;
            playerWon = false;
            unitsOpenedOnce = false;
            sparkAccumulator = 0f;
            enemyWaveTimer = 0f;
            // BeginBattle sets health and resources now
            ClearGameplayMarkers();
            EnsureBaseHealthOverlays();
            SetPanelVisible(defeatPanel, false);
        }

        private void SetTextColor(string name, Color color)
        {
            if (mainCanvas == null) return;
            var texts = mainCanvas.GetComponentsInChildren<Text>(true);
            foreach (var t in texts)
            {
                if (t.name == name)
                {
                    t.color = color;
                    break;
                }
            }
        }
    }
}
