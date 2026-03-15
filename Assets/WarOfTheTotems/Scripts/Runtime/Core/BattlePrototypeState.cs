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

        [Header("Summon Costs")]
        public SummonOption[] summonOptions =
        {
            new("Base Bearer", 2, 0),
            new("Stone Totem Summon", 4, 1),
            new("Beast Totem Summon", 6, 2),
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
}
