using UnityEngine;
using WarOfTheTotems.Core;
using WarOfTheTotems.Core.Data;
using WarOfTheTotems.Systems;
using WarOfTheTotems.Units;

namespace WarOfTheTotems.Units
{
    /// <summary>
    /// Интерфейс обратных вызовов от юнита к игровой системе.
    /// Разрывает прямую зависимость Unit → GameController.
    /// </summary>
    public interface IBattleCallbacks
    {
        float EvolutionDuration { get; }

        void SetEvolutionUI(bool active, string label, float progress);
        bool IsInsideEvolutionZone(Vector3 worldPos);
        bool IsAtEnemyBase(Unit unit);
        Unit? FindClosestEnemy(Unit self, float maxRange);
        void NotifyUnitKilled(Unit unit);
        void NotifyUnitEvolved(Unit unit);
        void DamageBase(TeamSide targetBase, int amount);
    }
}
