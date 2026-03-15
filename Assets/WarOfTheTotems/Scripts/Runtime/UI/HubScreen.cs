using System;
using UnityEngine;
using UnityEngine.UI;
using WarOfTheTotems.Core;
using WarOfTheTotems.Systems;

namespace WarOfTheTotems.UI
{
    /// <summary>
    /// Экран Хаба — базовый лагерь между боями.
    /// Показывает статус двух баз и кнопку "В бой".
    /// </summary>
    public sealed class HubScreen
    {
        public event Action? OnBattleRequested;

        private RectTransform root = null!;
        private Text? roadText;
        private Text? playerBaseLabel;
        private Text? enemyBaseLabel;

        // ----------------------------------------------------------------
        // Построение UI
        // ----------------------------------------------------------------

        public RectTransform Build(Transform canvasRoot)
        {
            // Тёмный backdrop на весь экран
            root = UiBuilder.EnsurePanel("HubOverlay", canvasRoot,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0.05f, 0.07f, 0.10f, 0.92f));
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.sizeDelta = Vector2.zero;

            // Центральная карточка
            var card = UiBuilder.EnsurePanel("HubCard", root,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 35f), new Vector2(880f, 540f),
                new Color(0.11f, 0.13f, 0.17f, 0.98f));

            // Акцент-полоса сверху
            var accent = UiBuilder.EnsurePanel("HubAccent", card,
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                Vector2.zero, new Vector2(0f, 6f),
                new Color(0.84f, 0.46f, 0.12f, 1f));
            accent.pivot = new Vector2(0.5f, 1f);
            accent.sizeDelta = new Vector2(0f, 6f);
            accent.anchoredPosition = Vector2.zero;

            // Заголовок
            UiBuilder.EnsureText("HubTitle", card,
                "ЛАГЕРЬ", TextAnchor.MiddleCenter, 42,
                new Vector2(0f, 210f), new Vector2(500f, 58f),
                new Color(0.95f, 0.92f, 0.78f));

            UiBuilder.EnsureText("HubSubtitle", card,
                "Готовься к следующей тренировке", TextAnchor.MiddleCenter, 20,
                new Vector2(0f, 166f), new Vector2(600f, 30f),
                new Color(0.56f, 0.60f, 0.68f));

            // Разделитель
            UiBuilder.EnsurePanel("HubDivider", card,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 130f), new Vector2(700f, 2f),
                new Color(0.28f, 0.32f, 0.40f));

            // Панели "Твоя база" / "База врага"
            var playerPanel = UiBuilder.EnsurePanel("HubPlayerBase", card,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-210f, 36f), new Vector2(320f, 200f),
                new Color(0.15f, 0.22f, 0.16f, 0.96f));

            UiBuilder.EnsureText("HubPlayerTitle", playerPanel,
                "ТВОЯ БАЗА", TextAnchor.MiddleCenter, 22,
                new Vector2(0f, 68f), new Vector2(280f, 30f),
                new Color(0.52f, 0.90f, 0.62f));

            playerBaseLabel = UiBuilder.EnsureText("HubPlayerStat", playerPanel,
                "HP: 5", TextAnchor.MiddleCenter, 32,
                new Vector2(0f, 10f), new Vector2(280f, 44f),
                new Color(0.55f, 0.95f, 0.70f));

            roadText = UiBuilder.EnsureText("HubRoadText", playerPanel,
                "Защити любой ценой", TextAnchor.MiddleCenter, 17,
                new Vector2(0f, -44f), new Vector2(290f, 48f),
                new Color(0.45f, 0.56f, 0.48f));

            var enemyPanel = UiBuilder.EnsurePanel("HubEnemyBase", card,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(210f, 36f), new Vector2(320f, 200f),
                new Color(0.22f, 0.14f, 0.14f, 0.96f));

            UiBuilder.EnsureText("HubEnemyTitle", enemyPanel,
                "БАЗА ВРАГА", TextAnchor.MiddleCenter, 22,
                new Vector2(0f, 68f), new Vector2(280f, 30f),
                new Color(0.90f, 0.48f, 0.48f));

            enemyBaseLabel = UiBuilder.EnsureText("HubEnemyStat", enemyPanel,
                "HP: ?", TextAnchor.MiddleCenter, 32,
                new Vector2(0f, 10f), new Vector2(280f, 44f),
                new Color(0.95f, 0.58f, 0.58f));

            UiBuilder.EnsureText("HubEnemyHint", enemyPanel,
                "Уничтожь её!", TextAnchor.MiddleCenter, 17,
                new Vector2(0f, -44f), new Vector2(290f, 48f),
                new Color(0.55f, 0.36f, 0.36f));

            // Кнопка «В БОЙ»
            var battleBtn = UiBuilder.EnsureButton("HubBattleButton", card,
                "В  БОЙ", new Vector2(0f, -192f), new Vector2(340f, 76f),
                new Color(0.82f, 0.40f, 0.10f));
            var lblT = card.Find("HubBattleButton/HubBattleButtonLabel")?.GetComponent<Text>();
            if (lblT != null) { lblT.fontSize = 34; }

            battleBtn.onClick.AddListener(() => OnBattleRequested?.Invoke());

            return root;
        }

        // ----------------------------------------------------------------
        // Обновление данных
        // ----------------------------------------------------------------

        public void Refresh(int playerHp, int enemyHp)
        {
            if (playerBaseLabel != null) playerBaseLabel.text = $"HP: {playerHp}";
            if (enemyBaseLabel  != null) enemyBaseLabel.text  = $"HP: {enemyHp}";
        }
    }
}
