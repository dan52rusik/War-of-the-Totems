using UnityEngine;

namespace WarOfTheTotems.Units
{
    /// <summary>
    /// Все вычисленные параметры одного экземпляра юнита.
    /// Передаётся в Unit.Initialize() вместо хардкода внутри ApplyForm().
    /// </summary>
    public struct UnitStats
    {
        public string displayName;

        // Боевые характеристики
        public float maxHealth;
        public float moveSpeed;
        public float attackDamage;
        public float attackInterval;
        public float attackRange;
        public float engagementRange;

        // Визуальные параметры
        public Color bodyColor;
        public Vector3 bodyScale;

        public static UnitStats BaseBearing(float bonusHealth, float bonusDamage) => new()
        {
            displayName     = "BASE BEARER",
            maxHealth       = 5f + bonusHealth,
            moveSpeed       = 1.6f,
            attackDamage    = 1f + bonusDamage,
            attackInterval  = 0.85f,
            attackRange     = 0.55f,
            engagementRange = 5.0f,
            bodyColor       = new Color(0.92f, 0.82f, 0.60f),
            bodyScale       = new Vector3(0.56f, 0.84f, 1f),
        };

        public static UnitStats StoneGuard() => new()
        {
            displayName     = "STONE GUARD",
            maxHealth       = 28f,
            moveSpeed       = 1.05f,
            attackDamage    = 4f,
            attackInterval  = 0.90f,
            attackRange     = 0.62f,
            engagementRange = 5.0f,
            bodyColor       = new Color(0.58f, 0.62f, 0.66f),
            bodyScale       = new Vector3(0.82f, 1.15f, 1f),
        };

        public static UnitStats FangBeast() => new()
        {
            displayName     = "FANG BEAST",
            maxHealth       = 16f,
            moveSpeed       = 2.35f,
            attackDamage    = 3f,
            attackInterval  = 0.48f,
            attackRange     = 0.72f,
            engagementRange = 5.0f,
            bodyColor       = new Color(0.80f, 0.47f, 0.26f),
            bodyScale       = new Vector3(0.76f, 0.96f, 1f),
        };

        public static UnitStats ShadowHunter(float health, float damage) => new()
        {
            displayName     = "SHADOW HUNTER",
            maxHealth       = health,
            moveSpeed       = 0.85f,
            attackDamage    = damage,
            attackInterval  = 0.58f,
            attackRange     = 0.68f,
            engagementRange = 5.0f,
            bodyColor       = new Color(0.28f, 0.16f, 0.38f),
            bodyScale       = new Vector3(0.72f, 1.00f, 1f),
        };
    }
}
