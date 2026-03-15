using System;
using UnityEngine;
using WarOfTheTotems.Core.Data;
using WarOfTheTotems.Core.State;

namespace WarOfTheTotems.Systems
{
    /// <summary>
    /// Управляет прогрессией текущего боя:
    /// волнами врагов, уроном базам, победой/поражением.
    /// Не знает о UI и не создаёт юнитов напрямую — вместо этого
    /// генерирует события, на которые подписывается GameController.
    /// </summary>
    public sealed class BattleSystem
    {
        // ----------------------------------------------------------------
        // События
        // ----------------------------------------------------------------

        /// <summary>Запросить спавн вражеского юнита.</summary>
        public event Action? OnEnemySpawnRequested;

        /// <summary>Бой завершён. true = игрок победил.</summary>
        public event Action<bool>? OnBattleEnded;

        /// <summary>Нанесён урон базе игрока.</summary>
        public event Action<int, int>? OnPlayerBaseDamaged;  // (current, max)

        /// <summary>Нанесён урон базе врага.</summary>
        public event Action<int, int>? OnEnemyBaseDamaged;   // (current, max)

        // ----------------------------------------------------------------
        // Состояние
        // ----------------------------------------------------------------

        private BattleSessionState session = null!;
        private float waveTimer;
        private bool  running;
        private bool  ended;

        public LevelDefinition ActiveLevel { get; private set; }

        // ----------------------------------------------------------------
        // Инициализация
        // ----------------------------------------------------------------

        public void StartBattle(BattleSessionState battleSession, LevelDefinition level)
        {
            session         = battleSession;
            ActiveLevel     = level;
            waveTimer       = 0f;
            running         = true;
            ended           = false;

            // Сразу спавним первую волну
            RequestEnemyWave();
        }

        public void Stop()
        {
            running = false;
        }

        // ----------------------------------------------------------------
        // Tick — вызывается из GameController.Update()
        // ----------------------------------------------------------------

        public void Tick(float deltaTime)
        {
            if (!running || ended) return;

            waveTimer += deltaTime;
            if (waveTimer >= ActiveLevel.enemyWaveInterval)
            {
                waveTimer = 0f;
                RequestEnemyWave();
            }
        }

        // ----------------------------------------------------------------
        // Повреждение баз (вызывается из Unit через GameController)
        // ----------------------------------------------------------------

        public void DamagePlayerBase(int amount)
        {
            if (ended) return;
            session.DamagePlayerBase(amount);
            OnPlayerBaseDamaged?.Invoke(session.PlayerBaseHealth, session.PlayerBaseMaxHealth);

            if (session.IsPlayerBaseDead)
                EndBattle(playerWon: false);
        }

        public void DamageEnemyBase(int amount)
        {
            if (ended) return;
            session.DamageEnemyBase(amount);
            OnEnemyBaseDamaged?.Invoke(session.EnemyBaseHealth, session.EnemyBaseMaxHealth);

            if (session.IsEnemyBaseDead)
                EndBattle(playerWon: true);
        }

        // ----------------------------------------------------------------
        // Вспомогательные запросы состояния
        // ----------------------------------------------------------------

        public bool IsPlayerBaseAt(float normalizedX, float worldX, float baseX)
            => session.IsPlayerBaseDead is false; // заглушка, логика в GameController

        // ----------------------------------------------------------------
        // Приватные методы
        // ----------------------------------------------------------------

        private void RequestEnemyWave()
        {
            for (var i = 0; i < Mathf.Max(1, ActiveLevel.enemyWaveSize); i++)
                OnEnemySpawnRequested?.Invoke();
        }

        private void EndBattle(bool playerWon)
        {
            ended   = true;
            running = false;
            OnBattleEnded?.Invoke(playerWon);
        }
    }
}
