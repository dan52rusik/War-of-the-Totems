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
        public float primalSparkRegenPerSecond = 1.35f;

        [Header("Evolution")]
        public string evolvingLabel = "EVOLVING: STONE GUARD";
        [Range(0f, 1f)] public float evolutionProgress = 0.58f;
        public float evolutionDuration = 2.2f;

        [Header("Battle Flow")]
        public float enemyWaveInterval = 8.5f;
        public int enemyWaveSize = 1;

        [Header("Summon Costs")]
        public SummonOption[] summonOptions =
        {
            new("Base Bearer", 2),
            new("Stone Totem Summon", 6),
            new("Beast Totem Summon", 10),
        };
    }

    [Serializable]
    public struct SummonOption
    {
        public string label;
        public int cost;

        public SummonOption(string label, int cost)
        {
            this.label = label;
            this.cost = cost;
        }
    }
}
