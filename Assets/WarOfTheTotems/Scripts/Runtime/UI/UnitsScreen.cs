using System;
using UnityEngine;
using UnityEngine.UI;
using WarOfTheTotems.Systems;

namespace WarOfTheTotems.UI
{
    /// <summary>
    /// Экран прокачки юнитов между боями.
    /// </summary>
    public sealed class UnitsScreen
    {
        public event Action? OnUpgradeHealthRequested;
        public event Action? OnUpgradeDamageRequested;
        public event Action? OnUpgradeSparkRequested;
        public event Action? OnBattleRequested;
        public event Action? OnResetRequested;

        private RectTransform root = null!;
        private Text? balanceText;
        private Text? hpValueText;
        private Text? hpCostText;
        private Text? dmgValueText;
        private Text? dmgCostText;
        private Text? manaValueText;
        private Text? manaCostText;
        private Button? hpBtn;
        private Button? dmgBtn;
        private Button? manaBtn;

        // ----------------------------------------------------------------
        // Построение UI
        // ----------------------------------------------------------------

        public RectTransform Build(Transform canvasRoot)
        {
            root = UiBuilder.EnsurePanel("UnitsOverlay", canvasRoot,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0.05f, 0.07f, 0.10f, 0.92f));
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.sizeDelta = Vector2.zero;

            var card = UiBuilder.EnsurePanel("UnitsCard", root,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 35f), new Vector2(920f, 620f),
                new Color(0.90f, 0.86f, 0.74f, 0.98f));

            var accent = UiBuilder.EnsurePanel("UnitsAccent", card,
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                Vector2.zero, new Vector2(0f, 6f),
                new Color(0.82f, 0.48f, 0.14f));
            accent.pivot = new Vector2(0.5f, 1f);
            accent.sizeDelta = new Vector2(0f, 6f);
            accent.anchoredPosition = Vector2.zero;

            UiBuilder.EnsureText("UnitsTitle", card,
                "Улучшения Базы", TextAnchor.MiddleCenter, 38,
                new Vector2(-60f, 246f), new Vector2(420f, 54f),
                new Color(0.20f, 0.16f, 0.12f));

            balanceText = UiBuilder.EnsureText("UnitsBalance", card,
                "COINS: 0", TextAnchor.MiddleRight, 26,
                new Vector2(345f, 246f), new Vector2(180f, 40f),
                new Color(0.82f, 0.48f, 0.14f));

            UiBuilder.EnsureText("UnitsSubtitle", card,
                "Укрепи бойца перед следующим боем", TextAnchor.MiddleCenter, 20,
                new Vector2(0f, 194f), new Vector2(700f, 32f),
                new Color(0.39f, 0.34f, 0.28f));

            // Карточки юнитов
            var selCard = UiBuilder.EnsurePanel("UnitsSelectedCard", card,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-250f, 84f), new Vector2(210f, 150f),
                new Color(0.23f, 0.29f, 0.21f, 0.96f));
            UiBuilder.EnsureText("UnitsSelectedName", selCard,
                "Пещерный\nЧеловек", TextAnchor.MiddleCenter, 26,
                new Vector2(0f, 16f), new Vector2(185f, 72f),
                new Color(0.96f, 0.94f, 0.86f));
            UiBuilder.EnsureText("UnitsSelectedStatus", selCard,
                "АКТИВЕН", TextAnchor.MiddleCenter, 17,
                new Vector2(0f, -48f), new Vector2(140f, 24f),
                new Color(0.62f, 0.90f, 0.52f));

            var lock1 = UiBuilder.EnsurePanel("UnitsLockedCard1", card,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 84f), new Vector2(210f, 150f),
                new Color(0.40f, 0.39f, 0.35f, 0.92f));
            UiBuilder.EnsureText("UnitsLocked1", lock1,
                "Тотем Камня\n(Закрыто)", TextAnchor.MiddleCenter, 20,
                Vector2.zero, new Vector2(185f, 64f),
                new Color(0.84f, 0.82f, 0.78f));

            var lock2 = UiBuilder.EnsurePanel("UnitsLockedCard2", card,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(250f, 84f), new Vector2(210f, 150f),
                new Color(0.38f, 0.31f, 0.28f, 0.92f));
            UiBuilder.EnsureText("UnitsLocked2", lock2,
                "Тотем Зверя\n(Закрыто)", TextAnchor.MiddleCenter, 20,
                Vector2.zero, new Vector2(185f, 64f),
                new Color(0.84f, 0.82f, 0.78f));

            // Карточка характеристик
            BuildStatsCard(card);

            // Кнопка «В бой»
            var battleBtn = UiBuilder.EnsureButton("UnitsBattleButton", card,
                "В  БОЙ", new Vector2(0f, -280f), new Vector2(280f, 60f),
                new Color(0.24f, 0.38f, 0.60f));
            battleBtn.onClick.AddListener(() => OnBattleRequested?.Invoke());

            // Кнопка RESET
            var resetBtn = UiBuilder.EnsureButton("ResetProgressButton", card,
                "RESET", new Vector2(0f, -340f), new Vector2(200f, 46f),
                new Color(0.50f, 0.24f, 0.18f));
            resetBtn.onClick.AddListener(() => OnResetRequested?.Invoke());

            return root;
        }

        private void BuildStatsCard(RectTransform card)
        {
            // Col layout inside statsCard (560×270):
            // Label(MiddleRight) x=-95,w=155 | Value x=18,w=50 | Cost: x=88,w=68 | CostV x=138,w=46 | Btn x=218,w=100
            var stats = UiBuilder.EnsurePanel("UnitsStatsCard", card,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -108f), new Vector2(560f, 270f),
                new Color(0.96f, 0.92f, 0.82f));

            UiBuilder.EnsureText("UnitsStatsTitle", stats,
                "Характеристики", TextAnchor.MiddleCenter, 24,
                new Vector2(0f, 106f), new Vector2(320f, 32f),
                new Color(0.26f, 0.20f, 0.14f));

            // HP row y=40
            UiBuilder.EnsureText("UnitsHpLabel", stats, "Здоровье:", TextAnchor.MiddleRight, 20, new Vector2(-95f, 40f), new Vector2(155f, 30f), new Color(0.20f, 0.18f, 0.16f));
            hpValueText = UiBuilder.EnsureText("UnitsHpValue", stats, "5", TextAnchor.MiddleLeft, 22, new Vector2(18f, 40f), new Vector2(50f, 30f), new Color(0.16f, 0.58f, 0.26f));
            UiBuilder.EnsureText("UnitsHpCostLabel", stats, "Цена:", TextAnchor.MiddleRight, 19, new Vector2(88f, 40f), new Vector2(68f, 26f), new Color(0.32f, 0.29f, 0.24f));
            hpCostText = UiBuilder.EnsureText("UnitsHpCost", stats, "5", TextAnchor.MiddleLeft, 19, new Vector2(138f, 40f), new Vector2(46f, 26f), new Color(0.82f, 0.48f, 0.14f));
            hpBtn = UiBuilder.EnsureButton("UnitsHpUpgradeButton", stats, "HP +", new Vector2(218f, 40f), new Vector2(100f, 36f), new Color(0.72f, 0.40f, 0.12f));
            UiBuilder.SetRect(stats.Find("UnitsHpUpgradeButton") as RectTransform, new Vector2(218f, 40f), new Vector2(100f, 36f));
            hpBtn.onClick.AddListener(() => OnUpgradeHealthRequested?.Invoke());

            // DMG row y=-12
            UiBuilder.EnsureText("UnitsDmgLabel", stats, "Урон:", TextAnchor.MiddleRight, 20, new Vector2(-95f, -12f), new Vector2(155f, 30f), new Color(0.20f, 0.18f, 0.16f));
            dmgValueText = UiBuilder.EnsureText("UnitsDmgValue", stats, "1", TextAnchor.MiddleLeft, 22, new Vector2(18f, -12f), new Vector2(50f, 30f), new Color(0.72f, 0.33f, 0.18f));
            UiBuilder.EnsureText("UnitsDmgCostLabel", stats, "Цена:", TextAnchor.MiddleRight, 19, new Vector2(88f, -12f), new Vector2(68f, 26f), new Color(0.32f, 0.29f, 0.24f));
            dmgCostText = UiBuilder.EnsureText("UnitsDmgCost", stats, "4", TextAnchor.MiddleLeft, 19, new Vector2(138f, -12f), new Vector2(46f, 26f), new Color(0.82f, 0.48f, 0.14f));
            dmgBtn = UiBuilder.EnsureButton("UnitsDmgUpgradeButton", stats, "ATK +", new Vector2(218f, -12f), new Vector2(100f, 36f), new Color(0.73f, 0.42f, 0.16f));
            dmgBtn.onClick.AddListener(() => OnUpgradeDamageRequested?.Invoke());

            // MANA row y=-64
            UiBuilder.EnsureText("UnitsManaLabel", stats, "Мана:", TextAnchor.MiddleRight, 20, new Vector2(-95f, -64f), new Vector2(155f, 30f), new Color(0.20f, 0.18f, 0.16f));
            manaValueText = UiBuilder.EnsureText("UnitsManaValue", stats, "2", TextAnchor.MiddleLeft, 22, new Vector2(18f, -64f), new Vector2(50f, 30f), new Color(0.18f, 0.40f, 0.70f));
            UiBuilder.EnsureText("UnitsManaCostLabel", stats, "Цена:", TextAnchor.MiddleRight, 19, new Vector2(88f, -64f), new Vector2(68f, 26f), new Color(0.32f, 0.29f, 0.24f));
            manaCostText = UiBuilder.EnsureText("UnitsManaCost", stats, "4", TextAnchor.MiddleLeft, 19, new Vector2(138f, -64f), new Vector2(46f, 26f), new Color(0.82f, 0.48f, 0.14f));
            manaBtn = UiBuilder.EnsureButton("UnitsManaUpgradeButton", stats, "MANA +", new Vector2(218f, -64f), new Vector2(100f, 36f), new Color(0.29f, 0.50f, 0.70f));
            manaBtn.onClick.AddListener(() => OnUpgradeSparkRequested?.Invoke());
        }

        // ----------------------------------------------------------------
        // Обновление данных
        // ----------------------------------------------------------------

        public void Refresh(ProgressionSystem prog, int startingSpark)
        {
            if (balanceText  != null) balanceText.text  = $"COINS: {prog.Coins}";
            if (hpValueText  != null) hpValueText.text  = $"{5 + prog.BonusHealth}";
            if (hpCostText   != null) hpCostText.text   = $"{prog.HealthUpgradeCost}";
            if (dmgValueText != null) dmgValueText.text = $"{1 + prog.BonusDamage}";
            if (dmgCostText  != null) dmgCostText.text  = $"{prog.DamageUpgradeCost}";
            if (manaValueText!= null) manaValueText.text= $"{startingSpark + prog.SparkBonus}";
            if (manaCostText != null) manaCostText.text = $"{prog.SparkUpgradeCost}";

            if (hpBtn   != null) hpBtn.interactable   = prog.CanUpgradeHealth;
            if (dmgBtn  != null) dmgBtn.interactable  = prog.CanUpgradeDamage;
            if (manaBtn != null) manaBtn.interactable = prog.CanUpgradeSpark;
        }
    }
}
