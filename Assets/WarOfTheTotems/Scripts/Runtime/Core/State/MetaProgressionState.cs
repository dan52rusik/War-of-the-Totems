using UnityEngine;

namespace WarOfTheTotems.Core.State
{
    /// <summary>
    /// Только мета-прогрессия: монеты, апгрейды, разблокировки.
    /// Хранится между боями, сохраняется в PlayerPrefs.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MetaProgressionState : MonoBehaviour
    {
        private const string KeyCoins        = "WoT.Coins";
        private const string KeyBonusHp      = "WoT.BonusHp";
        private const string KeyHpCost       = "WoT.HpCost";
        private const string KeyBonusDmg     = "WoT.BonusDmg";
        private const string KeyDmgCost      = "WoT.DmgCost";
        private const string KeySparkBonus   = "WoT.SparkBonus";
        private const string KeySparkCost    = "WoT.SparkCost";
        private const string KeyTutorial     = "WoT.TutorialDone";
        private const string KeyHighestLevel = "WoT.HighestLevel";

        [Header("Starting Values (Inspector)")]
        [SerializeField] private int startingCoins = 5;

        // --- Текущие значения (runtime) ---
        public int Coins                    { get; private set; }
        public int BaseBearerBonusHealth    { get; private set; }
        public int HealthUpgradeCost        { get; private set; } = 5;
        public int BaseBearerBonusDamage    { get; private set; }
        public int DamageUpgradeCost        { get; private set; } = 4;
        public int PrimalSparkBonus         { get; private set; }
        public int SparkUpgradeCost         { get; private set; } = 4;
        public bool TutorialCompleted       { get; private set; }
        public int HighestUnlockedLevel     { get; private set; } = 1;

        // ----------------------------------------------------------------
        // Save / Load
        // ----------------------------------------------------------------

        public void Load(int maxLevels)
        {
            Coins                 = PlayerPrefs.GetInt(KeyCoins,        startingCoins);
            BaseBearerBonusHealth = PlayerPrefs.GetInt(KeyBonusHp,      0);
            HealthUpgradeCost     = PlayerPrefs.GetInt(KeyHpCost,       5);
            BaseBearerBonusDamage = PlayerPrefs.GetInt(KeyBonusDmg,     0);
            DamageUpgradeCost     = PlayerPrefs.GetInt(KeyDmgCost,      4);
            PrimalSparkBonus      = PlayerPrefs.GetInt(KeySparkBonus,   0);
            SparkUpgradeCost      = PlayerPrefs.GetInt(KeySparkCost,    4);
            TutorialCompleted     = PlayerPrefs.GetInt(KeyTutorial,     0) == 1;
            HighestUnlockedLevel  = Mathf.Clamp(
                PlayerPrefs.GetInt(KeyHighestLevel, 1), 1, Mathf.Max(1, maxLevels));
        }

        public void Save()
        {
            PlayerPrefs.SetInt(KeyCoins,        Coins);
            PlayerPrefs.SetInt(KeyBonusHp,      BaseBearerBonusHealth);
            PlayerPrefs.SetInt(KeyHpCost,       HealthUpgradeCost);
            PlayerPrefs.SetInt(KeyBonusDmg,     BaseBearerBonusDamage);
            PlayerPrefs.SetInt(KeyDmgCost,      DamageUpgradeCost);
            PlayerPrefs.SetInt(KeySparkBonus,   PrimalSparkBonus);
            PlayerPrefs.SetInt(KeySparkCost,    SparkUpgradeCost);
            PlayerPrefs.SetInt(KeyTutorial,     TutorialCompleted ? 1 : 0);
            PlayerPrefs.SetInt(KeyHighestLevel, HighestUnlockedLevel);
            PlayerPrefs.Save();
        }

        // ----------------------------------------------------------------
        // Мутации
        // ----------------------------------------------------------------

        public bool TryUpgradeHealth()
        {
            if (Coins < HealthUpgradeCost) return false;
            Coins -= HealthUpgradeCost;
            BaseBearerBonusHealth += 4;
            HealthUpgradeCost     += 3;
            Save();
            return true;
        }

        public bool TryUpgradeDamage()
        {
            if (Coins < DamageUpgradeCost) return false;
            Coins -= DamageUpgradeCost;
            BaseBearerBonusDamage += 1;
            DamageUpgradeCost     += 3;
            Save();
            return true;
        }

        public bool TryUpgradeSpark()
        {
            if (Coins < SparkUpgradeCost) return false;
            Coins -= SparkUpgradeCost;
            PrimalSparkBonus  += 1;
            SparkUpgradeCost  += 3;
            Save();
            return true;
        }

        public void AddCoins(int amount)
        {
            Coins += Mathf.Max(0, amount);
        }

        public void CompleteTutorial()
        {
            if (TutorialCompleted) return;
            TutorialCompleted    = true;
            HighestUnlockedLevel = Mathf.Max(HighestUnlockedLevel, 1);
            Save();
        }

        public void UnlockLevel(int index, int totalLevels)
        {
            HighestUnlockedLevel = Mathf.Min(totalLevels, Mathf.Max(HighestUnlockedLevel, index + 1));
            Save();
        }

        public void Reset(int maxLevels)
        {
            PlayerPrefs.DeleteKey(KeyCoins);
            PlayerPrefs.DeleteKey(KeyBonusHp);
            PlayerPrefs.DeleteKey(KeyHpCost);
            PlayerPrefs.DeleteKey(KeyBonusDmg);
            PlayerPrefs.DeleteKey(KeyDmgCost);
            PlayerPrefs.DeleteKey(KeySparkBonus);
            PlayerPrefs.DeleteKey(KeySparkCost);
            PlayerPrefs.DeleteKey(KeyTutorial);
            PlayerPrefs.DeleteKey(KeyHighestLevel);
            PlayerPrefs.Save();
            Load(maxLevels);
        }

        public bool IsLevelUnlocked(int zeroBasedIndex)
            => zeroBasedIndex <= Mathf.Max(0, HighestUnlockedLevel - 1);
    }
}
