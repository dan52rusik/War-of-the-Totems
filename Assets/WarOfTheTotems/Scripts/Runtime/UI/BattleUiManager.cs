using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WarOfTheTotems.Core;
using WarOfTheTotems.Core.Data;

namespace WarOfTheTotems.UI
{
    public readonly struct BattleUiSnapshot
    {
        public BattleUiSnapshot(
            ScreenId screen,
            int playerBaseHealth,
            int playerBaseMaxHealth,
            int enemyBaseHealth,
            int enemyBaseMaxHealth,
            int ancestralBone,
            int primalSpark,
            int primalSparkMax,
            bool playerWon,
            bool unitsOpenedOnce,
            bool tutorialCompleted,
            int coins,
            int healthUpgradeCost,
            int baseBearerHealth,
            int baseBearerDamage,
            int damageUpgradeCost,
            int sparkCapacity,
            int sparkUpgradeCost,
            LevelDefinition selectedLevel,
            int selectedLevelIndex,
            int totalLevels,
            bool levelUnlocked,
            bool stoneTotemUnlocked,
            bool beastTotemUnlocked,
            string baseCostLabel,
            string stoneCostLabel,
            string beastCostLabel,
            bool canAffordBase,
            bool canAffordStone,
            bool canAffordBeast,
            bool canUpgradeHealth,
            bool canUpgradeDamage,
            bool canUpgradeSpark)
        {
            Screen = screen;
            PlayerBaseHealth = playerBaseHealth;
            PlayerBaseMaxHealth = playerBaseMaxHealth;
            EnemyBaseHealth = enemyBaseHealth;
            EnemyBaseMaxHealth = enemyBaseMaxHealth;
            AncestralBone = ancestralBone;
            PrimalSpark = primalSpark;
            PrimalSparkMax = primalSparkMax;
            PlayerWon = playerWon;
            UnitsOpenedOnce = unitsOpenedOnce;
            TutorialCompleted = tutorialCompleted;
            Coins = coins;
            HealthUpgradeCost = healthUpgradeCost;
            BaseBearerHealth = baseBearerHealth;
            BaseBearerDamage = baseBearerDamage;
            DamageUpgradeCost = damageUpgradeCost;
            SparkCapacity = sparkCapacity;
            SparkUpgradeCost = sparkUpgradeCost;
            SelectedLevel = selectedLevel;
            SelectedLevelIndex = selectedLevelIndex;
            TotalLevels = totalLevels;
            LevelUnlocked = levelUnlocked;
            StoneTotemUnlocked = stoneTotemUnlocked;
            BeastTotemUnlocked = beastTotemUnlocked;
            BaseCostLabel = baseCostLabel;
            StoneCostLabel = stoneCostLabel;
            BeastCostLabel = beastCostLabel;
            CanAffordBase = canAffordBase;
            CanAffordStone = canAffordStone;
            CanAffordBeast = canAffordBeast;
            CanUpgradeHealth = canUpgradeHealth;
            CanUpgradeDamage = canUpgradeDamage;
            CanUpgradeSpark = canUpgradeSpark;
        }

        public ScreenId Screen { get; }
        public int PlayerBaseHealth { get; }
        public int PlayerBaseMaxHealth { get; }
        public int EnemyBaseHealth { get; }
        public int EnemyBaseMaxHealth { get; }
        public int AncestralBone { get; }
        public int PrimalSpark { get; }
        public int PrimalSparkMax { get; }
        public bool PlayerWon { get; }
        public bool UnitsOpenedOnce { get; }
        public bool TutorialCompleted { get; }
        public int Coins { get; }
        public int HealthUpgradeCost { get; }
        public int BaseBearerHealth { get; }
        public int BaseBearerDamage { get; }
        public int DamageUpgradeCost { get; }
        public int SparkCapacity { get; }
        public int SparkUpgradeCost { get; }
        public LevelDefinition SelectedLevel { get; }
        public int SelectedLevelIndex { get; }
        public int TotalLevels { get; }
        public bool LevelUnlocked { get; }
        public bool StoneTotemUnlocked { get; }
        public bool BeastTotemUnlocked { get; }
        public string BaseCostLabel { get; }
        public string StoneCostLabel { get; }
        public string BeastCostLabel { get; }
        public bool CanAffordBase { get; }
        public bool CanAffordStone { get; }
        public bool CanAffordBeast { get; }
        public bool CanUpgradeHealth { get; }
        public bool CanUpgradeDamage { get; }
        public bool CanUpgradeSpark { get; }
    }

    public sealed class BattleUiManager
    {
        private Canvas canvas = null!;
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
        private Button levelStartButton = null!;
        private Button levelPrevButton = null!;
        private Button levelNextButton = null!;
        private Button unitsUpgradeButton = null!;
        private Button unitsDamageUpgradeButton = null!;
        private Button unitsManaUpgradeButton = null!;

        public void Bind(
            Canvas uiCanvas,
            Action onBaseSummon,
            Action onStoneSummon,
            Action onBeastSummon,
            Action onDefeatUnits,
            Action onUpgradeHealth,
            Action onUpgradeDamage,
            Action onUpgradeSpark,
            Action onResetProgress,
            Action onUnitsBattle,
            Action onHubBattle,
            Action onStartLevel,
            Action onPrevLevel,
            Action onNextLevel,
            Action onNavHome,
            Action onNavUnits,
            Action onNavBattle)
        {
            canvas = uiCanvas;

            playerHealthText = FindText("PlayerHealth");
            enemyHealthText = FindText("EnemyHealth");
            boneText = FindText("BoneResource");
            sparkTopText = FindText("SparkResource");
            sparkBottomText = FindText("SparkFooter");
            controlsHintText = FindText("ControlsHint");
            evolutionLabelText = FindText("EvolutionLabel");
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

            evolutionPanel = FindRect("EvolutionOverlay");
            evolutionFill = FindRect("ProgressFill");
            defeatPanel = FindRect("DefeatOverlay");
            unitsPanel = FindRect("UnitsOverlay");
            hubPanel = FindRect("HubOverlay");
            levelSelectPanel = FindRect("LevelsOverlay");
            topBarPanel = FindRect("TopBar");
            battleHudPanel = FindRect("BottomBar");
            bottomNavPanel = FindRect("BottomNav");

            if (controlsHintText != null)
            {
                controlsHintText.gameObject.SetActive(false);
            }

            var settingsText = FindText("Settings");
            if (settingsText != null)
            {
                settingsText.gameObject.SetActive(false);
            }

            baseSummonButton = BindButton(new[] { "Base Bearer (1)", "Base Bearer" }, onBaseSummon);
            stoneSummonButton = BindButton(new[] { "Stone Totem (2)", "Stone Totem Summon" }, onStoneSummon);
            beastSummonButton = BindButton(new[] { "Beast Totem (3)", "Beast Totem Summon" }, onBeastSummon);
            BindButton(new[] { "DefeatUnitsButton" }, onDefeatUnits);
            unitsUpgradeButton = BindButton(new[] { "UnitsUpgradeButton" }, onUpgradeHealth);
            unitsDamageUpgradeButton = BindButton(new[] { "UnitsDamageUpgradeButton" }, onUpgradeDamage);
            unitsManaUpgradeButton = BindButton(new[] { "UnitsManaUpgradeButton" }, onUpgradeSpark);
            BindButton(new[] { "ResetProgressButton" }, onResetProgress);
            BindButton(new[] { "UnitsBattleButton" }, onUnitsBattle);
            BindButton(new[] { "HubBattleButton" }, onHubBattle);
            levelStartButton = BindButton(new[] { "Level01StartButton" }, onStartLevel);
            levelPrevButton = BindButton(new[] { "LevelPrevButton" }, onPrevLevel);
            levelNextButton = BindButton(new[] { "LevelNextButton" }, onNextLevel);
            BindButton(new[] { "NavHomeButton" }, onNavHome);
            BindButton(new[] { "NavUnitsButton" }, onNavUnits);
            BindButton(new[] { "NavBattleButton" }, onNavBattle);

            SetResultVisible(false);
            UiBuilder.SetVisible(unitsPanel, false);
        }

        public void BindBaseHealthOverlays(
            Transform playerFill,
            TextMesh playerText,
            Transform enemyFill,
            TextMesh enemyText)
        {
            playerBaseWorldFill = playerFill;
            playerBaseWorldText = playerText;
            enemyBaseWorldFill = enemyFill;
            enemyBaseWorldText = enemyText;
        }

        public void SetEvolution(bool visible, string label, float progress)
        {
            UiBuilder.SetVisible(evolutionPanel, visible);

            if (evolutionLabelText != null)
            {
                evolutionLabelText.text = label;
            }

            if (evolutionFill != null)
            {
                var fillSize = evolutionFill.sizeDelta;
                fillSize.x = 216f * Mathf.Clamp01(progress);
                evolutionFill.sizeDelta = fillSize;
            }
        }

        public void SetResultVisible(bool visible)
        {
            UiBuilder.SetVisible(defeatPanel, visible);
        }

        public void SetScreen(ScreenId screen, bool isPlaying, Transform gameplayRoot)
        {
            UiBuilder.SetVisible(hubPanel, screen == ScreenId.Hub || screen == ScreenId.Intro);
            UiBuilder.SetVisible(levelSelectPanel, screen == ScreenId.Levels);
            UiBuilder.SetVisible(unitsPanel, screen == ScreenId.Units);
            UiBuilder.SetVisible(topBarPanel, screen == ScreenId.Battle);
            UiBuilder.SetVisible(battleHudPanel, screen == ScreenId.Battle);
            UiBuilder.SetVisible(bottomNavPanel, screen != ScreenId.Battle && screen != ScreenId.Intro);
            UiBuilder.SetVisible(evolutionPanel, screen == ScreenId.Battle && isPlaying);

            if (gameplayRoot != null && isPlaying)
            {
                gameplayRoot.gameObject.SetActive(screen == ScreenId.Battle);
            }

            if (screen == ScreenId.Hub || screen == ScreenId.Intro)
            {
                SetResultVisible(false);
            }
        }

        public void Refresh(BattleUiSnapshot snapshot)
        {
            if (playerHealthText != null)
            {
                playerHealthText.text = $"{snapshot.PlayerBaseHealth} (YOU)";
            }

            if (enemyHealthText != null)
            {
                enemyHealthText.text = $"{snapshot.EnemyBaseHealth} (THEM)";
            }

            if (boneText != null)
            {
                boneText.text = $"BONE: {snapshot.AncestralBone}";
            }

            if (sparkTopText != null)
            {
                sparkTopText.text = $"SPARK: {snapshot.PrimalSpark}/{snapshot.PrimalSparkMax}";
            }

            if (sparkBottomText != null)
            {
                sparkBottomText.text = snapshot.Screen == ScreenId.Battle
                    ? $"PRIMAL SPARK: {snapshot.PrimalSpark}/{snapshot.PrimalSparkMax}"
                    : string.Empty;
            }

            if (controlsHintText != null)
            {
                controlsHintText.text = snapshot.Screen == ScreenId.Battle
                    ? "Призови одного бойца. После поражения открой улучшения."
                    : "Открой Юниты и улучши здоровье после тренировки.";
            }

            if (defeatTitleText != null)
            {
                defeatTitleText.text = snapshot.PlayerWon ? "ПОБЕДА!" : "ПОРАЖЕНИЕ";
            }

            if (defeatMessageText != null)
            {
                defeatMessageText.text = snapshot.PlayerWon
                    ? "БАЗА ВРАГА УНИЧТОЖЕНА! МОНЕТЫ ПОЛУЧЕНЫ."
                    : snapshot.UnitsOpenedOnce
                        ? "ПОТРАТЬ МОНЕТЫ НА ЗДОРОВЬЕ И ПОПРОБУЙ СНОВА."
                        : "ВРАГ ПОБЕДИЛ. ОТКРОЙ ЮНИТЫ И УЛУЧШИ ЗДОРОВЬЕ.";
            }

            if (unitsBalanceText != null)
            {
                unitsBalanceText.text = $"COINS: {snapshot.Coins}";
            }

            if (unitsUpgradeCostText != null)
            {
                unitsUpgradeCostText.text = $"{snapshot.HealthUpgradeCost}";
            }

            if (unitsHealthValueText != null)
            {
                unitsHealthValueText.text = $"{snapshot.BaseBearerHealth}";
            }

            if (unitsDamageValueText != null)
            {
                unitsDamageValueText.text = $"{snapshot.BaseBearerDamage}";
            }

            if (unitsDamageCostText != null)
            {
                unitsDamageCostText.text = $"{snapshot.DamageUpgradeCost}";
            }

            if (unitsManaValueText != null)
            {
                unitsManaValueText.text = $"{snapshot.SparkCapacity}";
            }

            if (unitsManaCostText != null)
            {
                unitsManaCostText.text = $"{snapshot.SparkUpgradeCost}";
            }

            if (battleMenuTitleText != null)
            {
                battleMenuTitleText.text = "ВЫБОР УРОВНЯ";
            }

            if (battleMenuDescriptionText != null)
            {
                battleMenuDescriptionText.text = snapshot.TutorialCompleted
                    ? snapshot.SelectedLevel.description
                    : "Сначала пройди показательный бой, затем откроется уровень 1.";
            }

            if (battleLevelTitleText != null)
            {
                battleLevelTitleText.text = snapshot.SelectedLevel.title.ToUpperInvariant();
            }

            if (battleLevelEnemyText != null)
            {
                battleLevelEnemyText.text = $"Башня врага: {snapshot.SelectedLevel.enemyBaseHealth} HP";
            }

            if (battleLevelPlayerText != null)
            {
                battleLevelPlayerText.text = snapshot.LevelUnlocked
                    ? $"Награда: {snapshot.SelectedLevel.rewardCoins} мон."
                    : $"Откроется после уровня {Mathf.Max(1, snapshot.SelectedLevelIndex)}";
            }

            if (battleLevelStatusText != null)
            {
                battleLevelStatusText.text = snapshot.LevelUnlocked ? "ОТКРЫТ" : "ЗАКРЫТ";
            }

            if (levelStartButton != null)
            {
                levelStartButton.interactable = snapshot.LevelUnlocked;
            }

            if (levelPrevButton != null)
            {
                levelPrevButton.interactable = snapshot.SelectedLevelIndex > 0;
            }

            if (levelNextButton != null)
            {
                levelNextButton.interactable = snapshot.SelectedLevelIndex < snapshot.TotalLevels - 1;
            }

            RefreshBaseHealthOverlay(playerBaseWorldFill, playerBaseWorldText, snapshot.PlayerBaseHealth, snapshot.PlayerBaseMaxHealth);
            RefreshBaseHealthOverlay(enemyBaseWorldFill, enemyBaseWorldText, snapshot.EnemyBaseHealth, snapshot.EnemyBaseMaxHealth);

            UpdateSummonCard(baseSummonButton, "Base Bearer (1)Cost", snapshot.BaseCostLabel, snapshot.CanAffordBase);
            UpdateSummonCard(stoneSummonButton, "Stone Totem (2)Cost", snapshot.StoneCostLabel, snapshot.CanAffordStone);
            UpdateSummonCard(beastSummonButton, "Beast Totem (3)Cost", snapshot.BeastCostLabel, snapshot.CanAffordBeast);

            if (stoneSummonButton != null)
            {
                stoneSummonButton.gameObject.SetActive(snapshot.StoneTotemUnlocked);
            }

            if (beastSummonButton != null)
            {
                beastSummonButton.gameObject.SetActive(snapshot.BeastTotemUnlocked);
            }

            if (unitsUpgradeButton != null)
            {
                unitsUpgradeButton.interactable = snapshot.CanUpgradeHealth;
            }

            if (unitsDamageUpgradeButton != null)
            {
                unitsDamageUpgradeButton.interactable = snapshot.CanUpgradeDamage;
            }

            if (unitsManaUpgradeButton != null)
            {
                unitsManaUpgradeButton.interactable = snapshot.CanUpgradeSpark;
            }
        }

        private Text FindText(string objectName)
        {
            if (canvas == null)
            {
                return null;
            }

            var texts = canvas.GetComponentsInChildren<Text>(true);
            foreach (var text in texts)
            {
                if (text.gameObject.name == objectName)
                {
                    return text;
                }
            }

            return null;
        }

        private RectTransform FindRect(string objectName)
        {
            if (canvas == null)
            {
                return null;
            }

            var rects = canvas.GetComponentsInChildren<RectTransform>(true);
            foreach (var rect in rects)
            {
                if (rect.gameObject.name == objectName)
                {
                    return rect;
                }
            }

            return null;
        }

        private Button BindButton(string[] objectNames, Action action)
        {
            if (canvas == null)
            {
                return null;
            }

            Button target = null;
            var buttons = canvas.GetComponentsInChildren<Button>(true);
            foreach (var button in buttons)
            {
                for (var i = 0; i < objectNames.Length; i++)
                {
                    if (button.gameObject.name != objectNames[i])
                    {
                        continue;
                    }

                    target = button;
                    break;
                }

                if (target != null)
                {
                    break;
                }
            }

            if (target == null)
            {
                return null;
            }

            if (target.TryGetComponent<Image>(out var image))
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
            target.onClick.AddListener(() => action());
            return target;
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
            if (button.targetGraphic is not Image image)
            {
                return;
            }

            var baseColor = image.color;
            image.color = canAfford
                ? new Color(baseColor.r, baseColor.g, baseColor.b, 1f)
                : new Color(baseColor.r * 0.55f, baseColor.g * 0.55f, baseColor.b * 0.55f, 0.8f);
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
    }
}
