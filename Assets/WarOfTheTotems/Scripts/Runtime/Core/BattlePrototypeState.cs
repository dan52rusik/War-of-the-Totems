using System;
using UnityEngine;

namespace WarOfTheTotems.Core
{
    [DisallowMultipleComponent]
    public sealed class BattlePrototypeState : MonoBehaviour
    {
        [Header("Base Health")]
        public int playerBaseHealth = 5;
        public int enemyBaseHealth = 500;

        [Header("Resources")]
        public int ancestralBone;
        public int primalSpark = 10;
        public int primalSparkMax = 10;
        public float primalSparkRegenPerSecond = 1f;

        [Header("Evolution")]
        public string evolvingLabel = "EVOLVING: STONE GUARD";
        [Range(0f, 1f)] public float evolutionProgress = 0.58f;
        public float evolutionDuration = 2.2f;

        [Header("Battle Flow")]
        public float enemyWaveInterval = 8f;
        public int enemyWaveSize = 3;

        [Header("Meta Progression")]
        public int startingUpgradeCoins = 5;
        public int upgradeCoins = 5;
        public int baseBearerBonusHealth;
        public int baseBearerHealthUpgradeCost = 5;
        public int baseBearerBonusDamage;
        public int baseBearerDamageUpgradeCost = 4;
        public int primalSparkBonus;
        public int primalSparkUpgradeCost = 4;
        public bool tutorialCompleted;
        public int highestUnlockedLevel = 1;

        [Header("Summon Costs")]
        public SummonOption[] summonOptions =
        {
            new("Base Bearer", 2, 0),
            new("Stone Totem Summon", 4, 1),
            new("Beast Totem Summon", 6, 2),
        };

        [Header("Levels")]
        public BattleLevelDefinition[] levels =
        {
            new("lvl_01", "Уровень 1", "Первый бой после обучения", 3, 5, 4, 2, 0f, 1, 999f, 5, 1, 4, 4, false, false),
            new("lvl_02", "Уровень 2", "Чуть крепче враг и башня", 3, 5, 6, 2, 0f, 1, 999f, 5, 1, 5, 4, false, false),
            new("lvl_03", "Уровень 3", "Нужно пережить более сильный удар", 4, 6, 8, 2, 0f, 1, 999f, 5, 1, 6, 5, false, false),
            new("lvl_04", "Уровень 4", "Башня врага крепче, награда выше", 4, 6, 10, 2, 0f, 1, 999f, 5, 1, 6, 5, false, false),
            new("lvl_05", "Уровень 5", "Первый настоящий барьер", 5, 7, 12, 2, 0f, 1, 999f, 5, 1, 7, 5, false, false),
            new("lvl_06", "Уровень 6", "Враги становятся живучее", 5, 7, 14, 3, 0f, 1, 999f, 5, 1, 8, 5, false, false),
            new("lvl_07", "Уровень 7", "Долгий размен на линии", 6, 8, 16, 3, 0f, 1, 999f, 5, 1, 8, 6, false, false),
            new("lvl_08", "Уровень 8", "Проверка прокачки здоровья", 6, 8, 18, 3, 0f, 1, 999f, 5, 1, 9, 6, false, false),
            new("lvl_09", "Уровень 9", "Башня держится дольше", 7, 9, 20, 3, 0f, 1, 999f, 5, 1, 10, 6, false, false),
            new("lvl_10", "Уровень 10", "Нужен заметный запас силы", 7, 9, 22, 3, 0f, 1, 999f, 5, 1, 10, 7, false, false),
            new("lvl_11", "Уровень 11", "Враг переживает больше ударов", 8, 10, 24, 3, 0f, 1, 999f, 5, 1, 11, 7, false, false),
            new("lvl_12", "Уровень 12", "Слабость без фарма уже накажет", 8, 10, 26, 3, 0f, 1, 999f, 5, 1, 12, 7, false, false),
            new("lvl_13", "Уровень 13", "Финишная серия начинается", 9, 11, 28, 4, 0f, 1, 999f, 5, 1, 12, 8, false, false),
            new("lvl_14", "Уровень 14", "Почти максимум для базового бойца", 9, 11, 30, 4, 0f, 1, 999f, 5, 1, 13, 8, false, false),
            new("lvl_15", "Уровень 15", "Финал первой линейки боев", 10, 12, 35, 4, 0f, 1, 999f, 5, 1, 14, 8, false, false),
        };
    }

    [Serializable]
    public struct SummonOption
    {
        public string label;
        public int sparkCost;
        public int boneCost;

        public SummonOption(string label, int sparkCost, int boneCost)
        {
            this.label = label;
            this.sparkCost = sparkCost;
            this.boneCost = boneCost;
        }
    }

    [Serializable]
    public struct BattleLevelDefinition
    {
        public string id;
        public string title;
        public string description;
        public int rewardCoins;
        public int playerBaseHealth;
        public int enemyBaseHealth;
        public int startingSpark;
        public float sparkRegenPerSecond;
        public int enemyWaveSize;
        public float enemyWaveInterval;
        public int playerBaseBearerHealth;
        public int playerBaseBearerDamage;
        public int enemyShadowHealth;
        public int enemyShadowDamage;
        public bool allowStoneTotem;
        public bool allowBeastTotem;

        public BattleLevelDefinition(
            string id,
            string title,
            string description,
            int rewardCoins,
            int playerBaseHealth,
            int enemyBaseHealth,
            int startingSpark,
            float sparkRegenPerSecond,
            int enemyWaveSize,
            float enemyWaveInterval,
            int playerBaseBearerHealth,
            int playerBaseBearerDamage,
            int enemyShadowHealth,
            int enemyShadowDamage,
            bool allowStoneTotem,
            bool allowBeastTotem)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.rewardCoins = rewardCoins;
            this.playerBaseHealth = playerBaseHealth;
            this.enemyBaseHealth = enemyBaseHealth;
            this.startingSpark = startingSpark;
            this.sparkRegenPerSecond = sparkRegenPerSecond;
            this.enemyWaveSize = enemyWaveSize;
            this.enemyWaveInterval = enemyWaveInterval;
            this.playerBaseBearerHealth = playerBaseBearerHealth;
            this.playerBaseBearerDamage = playerBaseBearerDamage;
            this.enemyShadowHealth = enemyShadowHealth;
            this.enemyShadowDamage = enemyShadowDamage;
            this.allowStoneTotem = allowStoneTotem;
            this.allowBeastTotem = allowBeastTotem;
        }
    }
}
