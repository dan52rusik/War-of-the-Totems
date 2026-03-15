using System;
using UnityEngine;
using WarOfTheTotems.Core.State;

namespace WarOfTheTotems.Systems
{
    /// <summary>
    /// Управляет ресурсами текущего боя: Primal Spark (регенерация + трата)
    /// и Ancestral Bone (накопление за убийства врагов).
    /// Не знает о юнитах, UI или уровнях — только математика ресурсов.
    /// </summary>
    public sealed class ResourceSystem
    {
        public event Action<int, int>? OnSparkChanged;   // (current, max)
        public event Action<int>?      OnBoneChanged;    // (current)

        private BattleSessionState session = null!;
        private float sparkAccumulator;

        public void Bind(BattleSessionState battleSession)
        {
            session          = battleSession;
            sparkAccumulator = 0f;
        }

        // ----------------------------------------------------------------
        // Update — вызывается из GameController.Update() только во время боя
        // ----------------------------------------------------------------

        public void Tick(float deltaTime, float regenPerSecond)
        {
            if (session.PrimalSpark >= session.PrimalSparkMax)
            {
                sparkAccumulator = 0f;
                return;
            }

            sparkAccumulator += regenPerSecond * deltaTime;
            if (sparkAccumulator >= 1f)
            {
                var gained = Mathf.FloorToInt(sparkAccumulator);
                sparkAccumulator -= gained;
                session.GainSpark(gained);
                OnSparkChanged?.Invoke(session.PrimalSpark, session.PrimalSparkMax);
            }
        }

        // ----------------------------------------------------------------
        // Трата (вызывается при спавне юнита)
        // ----------------------------------------------------------------

        /// <returns>true, если ресурсов хватает и они потрачены.</returns>
        public bool TrySpend(int spark, int bone)
        {
            if (session.PrimalSpark < spark || session.AncestralBone < bone)
                return false;

            var sparkOk = spark == 0 || session.TrySpendSpark(spark);
            var boneOk  = bone  == 0 || session.TrySpendBone(bone);

            if (sparkOk && boneOk)
            {
                OnSparkChanged?.Invoke(session.PrimalSpark, session.PrimalSparkMax);
                OnBoneChanged?.Invoke(session.AncestralBone);
                return true;
            }

            return false;
        }

        /// <summary>Добавить Bone (после убийства вражеского юнита).</summary>
        public void GainBone(int amount)
        {
            session.GainBone(amount);
            OnBoneChanged?.Invoke(session.AncestralBone);
        }

        // ----------------------------------------------------------------
        // Чтение (без изменений)
        // ----------------------------------------------------------------

        public int  Spark    => session.PrimalSpark;
        public int  SparkMax => session.PrimalSparkMax;
        public int  Bone     => session.AncestralBone;

        public bool CanAfford(int spark, int bone)
            => session.PrimalSpark >= spark && session.AncestralBone >= bone;
    }
}
