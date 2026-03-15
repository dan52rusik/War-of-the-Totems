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
        public int ancestralBone = 0;
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
        public int baseBearerBonusHealth = 0;
        public int baseBearerHealthUpgradeCost = 5;
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
            new("lvl_01", "Уровень 1", "Первый бой после обучения", 3, 5, 4, 2, 0f, 1, 999f, 5, 5, 4, 4, false, false),
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
