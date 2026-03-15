using System;
using UnityEngine;

namespace WarOfTheTotems.Core.Data
{
    /// <summary>
    /// Конфигурация одного игрового уровня.
    /// Используется как в редакторе (Inspector), так и в рантайме.
    /// </summary>
    [Serializable]
    public struct LevelDefinition
    {
        [Tooltip("Уникальный строковый ID уровня (напр. 'lvl_01', 'tutorial')")]
        public string id;

        [Tooltip("Отображаемое название")]
        public string title;

        [Tooltip("Короткое описание для UI")]
        public string description;

        [Header("Награда")]
        [Tooltip("Монеты за победу")]
        public int rewardCoins;

        [Header("Базы")]
        public int playerBaseHealth;
        public int enemyBaseHealth;

        [Header("Ресурсы")]
        public int   startingSpark;
        public float sparkRegenPerSecond;

        [Header("Волны врагов")]
        public int   enemyWaveSize;
        public float enemyWaveInterval;

        [Header("Параметры юнитов")]
        [Tooltip("0 = использовать дефолтные значения юнита")]
        public int playerBaseBearerHealth;
        [Tooltip("0 = использовать дефолтные значения юнита")]
        public int playerBaseBearerDamage;
        public int enemyShadowHealth;
        public int enemyShadowDamage;

        [Header("Разблокированные юниты")]
        public bool allowStoneTotem;
        public bool allowBeastTotem;

        // ----------------------------------------------------------------
        // Конструктор (для создания данных через код)
        // ----------------------------------------------------------------
        public LevelDefinition(
            string id, string title, string description,
            int rewardCoins,
            int playerBaseHealth, int enemyBaseHealth,
            int startingSpark, float sparkRegenPerSecond,
            int enemyWaveSize, float enemyWaveInterval,
            int playerBaseBearerHealth, int playerBaseBearerDamage,
            int enemyShadowHealth, int enemyShadowDamage,
            bool allowStoneTotem, bool allowBeastTotem)
        {
            this.id                     = id;
            this.title                  = title;
            this.description            = description;
            this.rewardCoins            = rewardCoins;
            this.playerBaseHealth       = playerBaseHealth;
            this.enemyBaseHealth        = enemyBaseHealth;
            this.startingSpark          = startingSpark;
            this.sparkRegenPerSecond    = sparkRegenPerSecond;
            this.enemyWaveSize          = enemyWaveSize;
            this.enemyWaveInterval      = enemyWaveInterval;
            this.playerBaseBearerHealth = playerBaseBearerHealth;
            this.playerBaseBearerDamage = playerBaseBearerDamage;
            this.enemyShadowHealth      = enemyShadowHealth;
            this.enemyShadowDamage      = enemyShadowDamage;
            this.allowStoneTotem        = allowStoneTotem;
            this.allowBeastTotem        = allowBeastTotem;
        }

        // ----------------------------------------------------------------
        // Фабрика — обучающий уровень
        // ----------------------------------------------------------------
        public static LevelDefinition Tutorial() => new(
            id:                     "tutorial",
            title:                  "Обучение",
            description:            "Показательный бой, чтобы понять механику",
            rewardCoins:            0,
            playerBaseHealth:       5,
            enemyBaseHealth:        5,
            startingSpark:          2,
            sparkRegenPerSecond:    0f,
            enemyWaveSize:          1,
            enemyWaveInterval:      999f,
            playerBaseBearerHealth: 14,
            playerBaseBearerDamage: 1,
            enemyShadowHealth:      8,
            enemyShadowDamage:      1,
            allowStoneTotem:        false,
            allowBeastTotem:        false);
    }
}
