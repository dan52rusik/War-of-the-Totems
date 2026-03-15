using UnityEngine;
using WarOfTheTotems.Core;
using WarOfTheTotems.Core.Data;
using WarOfTheTotems.Core.State;
using WarOfTheTotems.Systems;

namespace WarOfTheTotems.Units
{
    /// <summary>
    /// Создаёт и конфигурирует юнитов.
    /// Содержит всю логику "какие stats выдать для данного типа юнита".
    /// Заменяет switch-case из PrototypeUnit.ApplyForm().
    /// </summary>
    public static class UnitFactory
    {
        // ----------------------------------------------------------------
        // Создание юнита
        // ----------------------------------------------------------------

        /// <summary>
        /// Создаёт юнита из prefab-объекта (переданного шаблона).
        /// </summary>
        public static Unit Create(
            GameObject template,
            Transform parent,
            Vector3 position,
            TeamSide team,
            TotemType totem,
            IBattleCallbacks callbacks,
            BattleSessionState session,
            ProgressionSystem progression,
            bool startEvolved = false)
        {
            var go   = Object.Instantiate(template, position, Quaternion.identity, parent);
            var unit = go.GetComponent<Unit>();

            if (unit == null)
                unit = go.AddComponent<Unit>();

            var stats = BuildStats(team, totem, session, progression);
            unit.Initialize(callbacks, team, totem, stats, startEvolved);
            return unit;
        }

        // ----------------------------------------------------------------
        // Построение параметров
        // ----------------------------------------------------------------

        public static UnitStats BuildStats(
            TeamSide team,
            TotemType totem,
            BattleSessionState session,
            ProgressionSystem progression)
        {
            return totem switch
            {
                TotemType.Stone  => UnitStats.StoneGuard(),
                TotemType.Beast  => UnitStats.FangBeast(),
                TotemType.Shadow => BuildShadowStats(session),
                _                => BuildBaseBearerStats(team, session, progression),
            };
        }

        // ----------------------------------------------------------------
        // Вспомогательные методы
        // ----------------------------------------------------------------

        private static UnitStats BuildBaseBearerStats(
            TeamSide team,
            BattleSessionState session,
            ProgressionSystem progression)
        {
            // Если в уровне заданы конкретные HP/DMG — используем их,
            // иначе берём базу + бонус из прокачки
            var level  = session.ActiveLevel;
            var health = level.playerBaseBearerHealth > 0
                ? (float)(level.playerBaseBearerHealth + progression.BonusHealth)
                : 5f + progression.BonusHealth;

            var damage = level.playerBaseBearerDamage > 0
                ? (float)(level.playerBaseBearerDamage + progression.BonusDamage)
                : 1f + progression.BonusDamage;

            return UnitStats.BaseBearing(
                bonusHealth: health - 5f,   // UnitStats.BaseBearing прибавляет к базе 5
                bonusDamage: damage - 1f);  // UnitStats.BaseBearing прибавляет к базе 1
        }

        private static UnitStats BuildShadowStats(BattleSessionState session)
        {
            var level  = session.ActiveLevel;
            var health = level.enemyShadowHealth > 0 ? (float)level.enemyShadowHealth : 14f;
            var damage = level.enemyShadowDamage > 0 ? (float)level.enemyShadowDamage : 3f;
            return UnitStats.ShadowHunter(health, damage);
        }
    }
}
