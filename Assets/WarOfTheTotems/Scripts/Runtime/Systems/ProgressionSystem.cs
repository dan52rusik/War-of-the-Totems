using System;
using WarOfTheTotems.Core.State;

namespace WarOfTheTotems.Systems
{
    /// <summary>
    /// Управляет мета-прогрессией: апгрейды, монеты, сохранение.
    /// Не зависит от UI — при изменениях стреляет событиями.
    /// </summary>
    public sealed class ProgressionSystem
    {
        // ----------------------------------------------------------------
        // События
        // ----------------------------------------------------------------

        /// <summary>Любое изменение мета-прогрессии (монеты, апгрейд и т.п.).</summary>
        public event Action? OnChanged;

        // ----------------------------------------------------------------
        // Зависимости
        // ----------------------------------------------------------------

        private MetaProgressionState meta = null!;
        private int maxLevels;

        public void Bind(MetaProgressionState metaState, int totalLevelCount)
        {
            meta      = metaState;
            maxLevels = totalLevelCount;
        }

        // ----------------------------------------------------------------
        // Инициализация
        // ----------------------------------------------------------------

        public void Load()
        {
            meta.Load(maxLevels);
            OnChanged?.Invoke();
        }

        // ----------------------------------------------------------------
        // Апгрейды
        // ----------------------------------------------------------------

        public bool TryUpgradeHealth()
        {
            if (!meta.TryUpgradeHealth()) return false;
            OnChanged?.Invoke();
            return true;
        }

        public bool TryUpgradeDamage()
        {
            if (!meta.TryUpgradeDamage()) return false;
            OnChanged?.Invoke();
            return true;
        }

        public bool TryUpgradeSpark()
        {
            if (!meta.TryUpgradeSpark()) return false;
            OnChanged?.Invoke();
            return true;
        }

        // ----------------------------------------------------------------
        // Монеты и уровни
        // ----------------------------------------------------------------

        /// <summary>Зачислить монеты за победу в бою.</summary>
        public void AwardCoins(int amount)
        {
            meta.AddCoins(amount);
            meta.Save();
            OnChanged?.Invoke();
        }

        /// <summary>Пометить обучение пройденным.</summary>
        public void MarkTutorialComplete()
        {
            meta.CompleteTutorial();
            OnChanged?.Invoke();
        }

        /// <summary>Разблокировать уровень с индексом index (0-based).</summary>
        public void UnlockLevel(int zeroBasedIndex)
        {
            meta.UnlockLevel(zeroBasedIndex, maxLevels);
            OnChanged?.Invoke();
        }

        // ----------------------------------------------------------------
        // Сброс
        // ----------------------------------------------------------------

        public void ResetAll()
        {
            meta.Reset(maxLevels);
            OnChanged?.Invoke();
        }

        // ----------------------------------------------------------------
        // Удобный доступ к данным
        // ----------------------------------------------------------------

        public int  Coins                => meta.Coins;
        public int  BonusHealth          => meta.BaseBearerBonusHealth;
        public int  BonusDamage          => meta.BaseBearerBonusDamage;
        public int  SparkBonus           => meta.PrimalSparkBonus;
        public int  HealthUpgradeCost    => meta.HealthUpgradeCost;
        public int  DamageUpgradeCost    => meta.DamageUpgradeCost;
        public int  SparkUpgradeCost     => meta.SparkUpgradeCost;
        public bool TutorialCompleted    => meta.TutorialCompleted;
        public bool IsLevelUnlocked(int i) => meta.IsLevelUnlocked(i);

        public bool CanUpgradeHealth  => meta.Coins >= meta.HealthUpgradeCost;
        public bool CanUpgradeDamage  => meta.Coins >= meta.DamageUpgradeCost;
        public bool CanUpgradeSpark   => meta.Coins >= meta.SparkUpgradeCost;
    }
}
