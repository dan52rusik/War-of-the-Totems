using System;
using UnityEngine;
using UnityEngine.UI;
using WarOfTheTotems.Core;

namespace WarOfTheTotems.UI
{
    /// <summary>
    /// HUD во время боя — ресурсы, здоровье баз, кнопки призыва, эволюция.
    /// </summary>
    public sealed class BattleHud
    {
        public event Action? OnBaseBearer;
        public event Action? OnStoneTotem;
        public event Action? OnBeastTotem;

        private RectTransform topBar   = null!;
        private RectTransform bottomBar= null!;

        private Text? playerHpText;
        private Text? enemyHpText;
        private Text? sparkText;
        private Text? boneText;
        private Text? sparkFooterText;
        private Text? evolutionLabel;

        private RectTransform? evolutionPanel;
        private RectTransform? evolutionFill;

        private Button? baseBtn;
        private Button? stoneBtn;
        private Button? beastBtn;

        // ----------------------------------------------------------------
        // Построение UI
        // ----------------------------------------------------------------

        public (RectTransform top, RectTransform bottom) Build(Transform canvasRoot)
        {
            topBar = UiBuilder.EnsurePanel("TopBar", canvasRoot,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -24f), new Vector2(1800f, 88f),
                new Color(0.11f, 0.14f, 0.11f, 0.86f));

            playerHpText = UiBuilder.EnsureText("PlayerHealth", topBar,
                "5 (YOU)", TextAnchor.MiddleLeft, 24,
                new Vector2(-800f, 0f), new Vector2(190f, 44f));

            boneText = UiBuilder.EnsureText("BoneResource", topBar,
                "BONE: 0", TextAnchor.MiddleLeft, 22,
                new Vector2(-400f, 0f), new Vector2(220f, 40f));

            sparkText = UiBuilder.EnsureText("SparkResource", topBar,
                "SPARK: 2/2", TextAnchor.MiddleLeft, 22,
                new Vector2(-100f, 0f), new Vector2(220f, 40f));

            enemyHpText = UiBuilder.EnsureText("EnemyHealth", topBar,
                "500 (THEM)", TextAnchor.MiddleRight, 24,
                new Vector2(800f, 0f), new Vector2(210f, 44f));

            // Нижняя панель с кнопками призыва
            bottomBar = UiBuilder.EnsurePanel("BottomBar", canvasRoot,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 82f), new Vector2(1800f, 164f),
                new Color(0.12f, 0.09f, 0.08f, 0.88f));

            sparkFooterText = UiBuilder.EnsureText("SparkFooter", bottomBar,
                "SPARK: 2/2", TextAnchor.MiddleLeft, 24,
                new Vector2(-610f, 44f), new Vector2(300f, 40f));

            BuildSummonCard(bottomBar, "Base Bearer (1)", new Vector2(-220f, 8f), new Color(0.57f, 0.43f, 0.29f), out baseBtn);
            BuildSummonCard(bottomBar, "Stone Totem (2)",  new Vector2(   0f, 8f), new Color(0.43f, 0.49f, 0.52f), out stoneBtn);
            BuildSummonCard(bottomBar, "Beast Totem (3)",  new Vector2( 220f, 8f), new Color(0.63f, 0.37f, 0.24f), out beastBtn);

            baseBtn?.onClick.AddListener( () => OnBaseBearer?.Invoke());
            stoneBtn?.onClick.AddListener(() => OnStoneTotem?.Invoke());
            beastBtn?.onClick.AddListener(() => OnBeastTotem?.Invoke());

            // Панель эволюции (по центру экрана)
            var evoPanel = UiBuilder.EnsurePanel("EvolutionOverlay", canvasRoot,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -15f), new Vector2(300f, 70f),
                new Color(0.05f, 0.13f, 0.11f, 0.80f));
            evolutionPanel = evoPanel;

            evolutionLabel = UiBuilder.EnsureText("EvolutionLabel", evoPanel,
                "EVOLVING", TextAnchor.UpperCenter, 20,
                new Vector2(0f, 15f), new Vector2(280f, 28f));

            var evoBg = UiBuilder.EnsurePanel("ProgressBackground", evoPanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -15f), new Vector2(220f, 16f),
                new Color(0.11f, 0.20f, 0.16f, 0.95f));

            evolutionFill = UiBuilder.EnsurePanel("ProgressFill", evoBg,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(2f, 0f), new Vector2(0f, 12f),
                new Color(0.39f, 0.82f, 0.68f));
            evolutionFill.pivot = new Vector2(0f, 0.5f);
            evolutionFill.anchorMin = new Vector2(0f, 0.5f);
            evolutionFill.anchorMax = new Vector2(0f, 0.5f);

            SetEvolution(false, string.Empty, 0f);

            return (topBar, bottomBar);
        }

        // ----------------------------------------------------------------
        // Обновление данных
        // ----------------------------------------------------------------

        public void RefreshResources(int spark, int sparkMax, int bone)
        {
            if (sparkText       != null) sparkText.text       = $"SPARK: {spark}/{sparkMax}";
            if (sparkFooterText != null) sparkFooterText.text = $"SPARK: {spark}/{sparkMax}";
            if (boneText        != null) boneText.text        = $"BONE: {bone}";
        }

        public void RefreshBases(int playerHp, int playerMax, int enemyHp, int enemyMax)
        {
            if (playerHpText != null) playerHpText.text = $"{playerHp} (YOU)";
            if (enemyHpText  != null) enemyHpText.text  = $"{enemyHp} (THEM)";
        }

        public void SetEvolution(bool active, string label, float progress)
        {
            UiBuilder.SetVisible(evolutionPanel, active);
            if (evolutionLabel != null) evolutionLabel.text = label;
            if (evolutionFill  != null)
            {
                var s = evolutionFill.sizeDelta;
                s.x   = 216f * Mathf.Clamp01(progress);
                evolutionFill.sizeDelta = s;
            }
        }

        public void SetSummonInteractable(TotemType totem, bool canAfford)
        {
            switch (totem)
            {
                case TotemType.None:   if (baseBtn  != null) baseBtn.interactable  = canAfford; break;
                case TotemType.Stone:  if (stoneBtn != null) stoneBtn.interactable = canAfford; break;
                case TotemType.Beast:  if (beastBtn != null) beastBtn.interactable = canAfford; break;
            }
        }

        public void SetSummonVisible(TotemType totem, bool visible)
        {
            switch (totem)
            {
                case TotemType.Stone: if (stoneBtn != null) stoneBtn.gameObject.SetActive(visible); break;
                case TotemType.Beast: if (beastBtn != null) beastBtn.gameObject.SetActive(visible); break;
            }
        }

        public void UpdateSummonCostText(string cardName, string costLabel)
        {
            var costT = bottomBar.Find($"{cardName}/{cardName}Cost")?.GetComponent<Text>();
            if (costT != null) costT.text = costLabel;
        }

        // ----------------------------------------------------------------
        // Приватный хелпер
        // ----------------------------------------------------------------

        private static void BuildSummonCard(RectTransform parent, string name, Vector2 pos, Color color, out Button btn)
        {
            var card = UiBuilder.EnsurePanel(name, parent,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                pos, new Vector2(190f, 78f), color);

            UiBuilder.EnsureText($"{name}Title", card,
                name, TextAnchor.MiddleCenter, 16,
                new Vector2(0f, 12f), new Vector2(170f, 28f));

            UiBuilder.EnsureText($"{name}Cost", card,
                "cost ?", TextAnchor.MiddleCenter, 18,
                new Vector2(0f, -18f), new Vector2(120f, 24f));

            btn = card.gameObject.TryGetComponent<Button>(out var b) ? b : card.gameObject.AddComponent<Button>();
        }
    }
}
