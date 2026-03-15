using System;
using UnityEngine;
using UnityEngine.UI;
using WarOfTheTotems.Core.Data;
using WarOfTheTotems.Systems;

namespace WarOfTheTotems.UI
{
    /// <summary>
    /// Экран выбора уровня перед боем.
    /// </summary>
    public sealed class LevelSelectScreen
    {
        public event Action?      OnStartRequested;
        public event Action?      OnNextRequested;
        public event Action?      OnPrevRequested;

        private RectTransform root = null!;
        private Text? titleText;
        private Text? descText;
        private Text? levelNameText;
        private Text? enemyHpText;
        private Text? rewardText;
        private Text? statusText;
        private Button? startBtn;
        private Button? prevBtn;
        private Button? nextBtn;

        // ----------------------------------------------------------------
        // Построение UI
        // ----------------------------------------------------------------

        public RectTransform Build(Transform canvasRoot)
        {
            root = UiBuilder.EnsurePanel("LevelsOverlay", canvasRoot,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0.05f, 0.07f, 0.10f, 0.92f));
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.sizeDelta = Vector2.zero;

            var card = UiBuilder.EnsurePanel("LevelsCard", root,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 35f), new Vector2(920f, 560f),
                new Color(0.11f, 0.12f, 0.16f, 0.98f));

            var accent = UiBuilder.EnsurePanel("LevelsAccent", card,
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                Vector2.zero, new Vector2(0f, 6f),
                new Color(0.84f, 0.46f, 0.12f, 1f));
            accent.pivot = new Vector2(0.5f, 1f);
            accent.sizeDelta = new Vector2(0f, 6f);
            accent.anchoredPosition = Vector2.zero;

            titleText = UiBuilder.EnsureText("LevelsTitle", card,
                "ВЫБОР УРОВНЯ", TextAnchor.MiddleCenter, 38,
                new Vector2(0f, 220f), new Vector2(500f, 50f),
                new Color(0.95f, 0.92f, 0.78f));

            descText = UiBuilder.EnsureText("LevelsDesc", card,
                "", TextAnchor.MiddleCenter, 20,
                new Vector2(0f, 178f), new Vector2(700f, 32f),
                new Color(0.52f, 0.57f, 0.65f));

            // Кнопки навигации
            prevBtn = UiBuilder.EnsureButton("LevelPrevButton", card,
                "<", new Vector2(-330f, 10f), new Vector2(70f, 70f),
                new Color(0.20f, 0.23f, 0.30f));
            prevBtn.onClick.AddListener(() => OnPrevRequested?.Invoke());

            nextBtn = UiBuilder.EnsureButton("LevelNextButton", card,
                ">", new Vector2(330f, 10f), new Vector2(70f, 70f),
                new Color(0.20f, 0.23f, 0.30f));
            nextBtn.onClick.AddListener(() => OnNextRequested?.Invoke());

            // Карточка уровня
            var levelCard = UiBuilder.EnsurePanel("Level01Card", card,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 10f), new Vector2(360f, 230f),
                new Color(0.19f, 0.22f, 0.18f));

            levelNameText = UiBuilder.EnsureText("Level01Title", levelCard,
                "УРОВЕНЬ 1", TextAnchor.MiddleCenter, 30,
                new Vector2(0f, 74f), new Vector2(320f, 40f),
                new Color(0.90f, 0.86f, 0.68f));

            enemyHpText = UiBuilder.EnsureText("Level01Enemy", levelCard,
                "", TextAnchor.MiddleCenter, 20,
                new Vector2(0f, 22f), new Vector2(320f, 30f),
                new Color(0.72f, 0.50f, 0.50f));

            rewardText = UiBuilder.EnsureText("Level01Reward", levelCard,
                "", TextAnchor.MiddleCenter, 19,
                new Vector2(0f, -14f), new Vector2(320f, 28f),
                new Color(0.80f, 0.72f, 0.42f));

            statusText = UiBuilder.EnsureText("Level01Status", levelCard,
                "ОТКРЫТ", TextAnchor.MiddleCenter, 18,
                new Vector2(0f, -60f), new Vector2(220f, 26f),
                new Color(0.52f, 0.90f, 0.62f));

            startBtn = UiBuilder.EnsureButton("Level01StartButton", card,
                "В  БОЙ", new Vector2(0f, -196f), new Vector2(320f, 72f),
                new Color(0.82f, 0.40f, 0.10f));
            var sl = card.Find("Level01StartButton/Level01StartButtonLabel")?.GetComponent<Text>();
            if (sl != null) sl.fontSize = 30;
            startBtn.onClick.AddListener(() => OnStartRequested?.Invoke());

            return root;
        }

        // ----------------------------------------------------------------
        // Обновление данных
        // ----------------------------------------------------------------

        public void Refresh(LevelDefinition level, int index, bool unlocked, bool canPrev, bool canNext)
        {
            if (titleText    != null) titleText.text     = "ВЫБОР УРОВНЯ";
            if (descText     != null) descText.text      = level.description;
            if (levelNameText!= null) levelNameText.text = level.title.ToUpperInvariant();
            if (enemyHpText  != null) enemyHpText.text   = $"Башня врага: {level.enemyBaseHealth} HP";
            if (rewardText   != null) rewardText.text    = unlocked
                ? $"Награда: {level.rewardCoins} монет"
                : $"Откроется после уровня {Mathf.Max(1, index)}";
            if (statusText   != null)
            {
                statusText.text  = unlocked ? "ОТКРЫТ" : "ЗАКРЫТ";
                statusText.color = unlocked
                    ? new Color(0.52f, 0.90f, 0.62f)
                    : new Color(0.90f, 0.48f, 0.48f);
            }

            if (startBtn != null) startBtn.interactable = unlocked;
            if (prevBtn  != null) prevBtn.interactable  = canPrev;
            if (nextBtn  != null) nextBtn.interactable  = canNext;
        }
    }
}
