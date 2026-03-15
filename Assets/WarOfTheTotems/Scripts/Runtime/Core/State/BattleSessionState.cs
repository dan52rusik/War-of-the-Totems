using UnityEngine;
using WarOfTheTotems.Core.Data;

namespace WarOfTheTotems.Core.State
{
    /// <summary>
    /// Только состояние текущего активного боя.
    /// Сбрасывается при каждом старте нового боя.
    /// Не сохраняется между сессиями.
    /// </summary>
    public sealed class BattleSessionState
    {
        // Здоровье баз
        public int PlayerBaseHealth  { get; private set; }
        public int PlayerBaseMaxHealth { get; private set; }
        public int EnemyBaseHealth   { get; private set; }
        public int EnemyBaseMaxHealth  { get; private set; }

        // Ресурсы
        public int  PrimalSpark    { get; private set; }
        public int  PrimalSparkMax { get; private set; }
        public int  AncestralBone  { get; private set; }

        // Активный уровень
        public LevelDefinition ActiveLevel { get; private set; }

        // ----------------------------------------------------------------
        // Инициализация нового боя
        // ----------------------------------------------------------------

        public void BeginBattle(LevelDefinition level, int sparkBonus)
        {
            ActiveLevel         = level;
            PlayerBaseHealth    = level.playerBaseHealth;
            PlayerBaseMaxHealth = Mathf.Max(1, level.playerBaseHealth);
            EnemyBaseHealth     = level.enemyBaseHealth;
            EnemyBaseMaxHealth  = Mathf.Max(1, level.enemyBaseHealth);
            PrimalSparkMax      = Mathf.Max(1, level.startingSpark + sparkBonus);
            PrimalSpark         = PrimalSparkMax;
            AncestralBone       = 0;
        }

        // ----------------------------------------------------------------
        // Ресурсы
        // ----------------------------------------------------------------

        public bool TrySpendSpark(int amount)
        {
            if (PrimalSpark < amount) return false;
            PrimalSpark -= amount;
            return true;
        }

        public bool TrySpendBone(int amount)
        {
            if (AncestralBone < amount) return false;
            AncestralBone -= amount;
            return true;
        }

        /// <summary>Добавить spark (регенерация). Не превышает макс.</summary>
        public void GainSpark(int amount)
            => PrimalSpark = Mathf.Min(PrimalSparkMax, PrimalSpark + amount);

        /// <summary>Добавить bone (за убийство врага).</summary>
        public void GainBone(int amount)
            => AncestralBone += Mathf.Max(0, amount);

        // ----------------------------------------------------------------
        // Урон базам
        // ----------------------------------------------------------------

        public void DamagePlayerBase(int amount)
            => PlayerBaseHealth = Mathf.Max(0, PlayerBaseHealth - amount);

        public void DamageEnemyBase(int amount)
            => EnemyBaseHealth = Mathf.Max(0, EnemyBaseHealth - amount);

        // ----------------------------------------------------------------
        // Запросы
        // ----------------------------------------------------------------

        public bool IsPlayerBaseDead  => PlayerBaseHealth <= 0;
        public bool IsEnemyBaseDead   => EnemyBaseHealth  <= 0;

        public float PlayerBaseRatio  => PlayerBaseMaxHealth <= 0 ? 0f
            : Mathf.Clamp01((float)PlayerBaseHealth / PlayerBaseMaxHealth);

        public float EnemyBaseRatio   => EnemyBaseMaxHealth  <= 0 ? 0f
            : Mathf.Clamp01((float)EnemyBaseHealth  / EnemyBaseMaxHealth);
    }
}
