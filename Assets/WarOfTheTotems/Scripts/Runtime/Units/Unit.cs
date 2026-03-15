using UnityEngine;
using WarOfTheTotems.Core;

namespace WarOfTheTotems.Units
{
    /// <summary>
    /// Игровой юнит. Принимает UnitStats извне (через UnitFactory),
    /// не содержит хардкода характеристик.
    /// Общается с игровой системой через IBattleCallbacks.
    /// </summary>
    public sealed class Unit : MonoBehaviour
    {
        // ----------------------------------------------------------------
        // Приватные ссылки на визуал
        // ----------------------------------------------------------------
        private Transform  body        = null!;
        private Transform  weapon      = null!;
        private TextMesh   nameLabel   = null!;
        private TextMesh   healthValue = null!;
        private Transform  healthFill  = null!;

        // ----------------------------------------------------------------
        // Параметры (выдаются фабрикой)
        // ----------------------------------------------------------------
        private UnitStats       stats;
        private IBattleCallbacks callbacks = null!;

        // ----------------------------------------------------------------
        // Runtime-состояние
        // ----------------------------------------------------------------
        private float currentHealth;
        private float attackCooldown;
        private float evolutionTimer;
        private float punchTimer;

        // ----------------------------------------------------------------
        // Публичные свойства
        // ----------------------------------------------------------------
        public TeamSide  Team           { get; private set; }
        public TotemType AssignedTotem  { get; private set; }
        public bool      IsDead         => currentHealth <= 0f;
        public bool      IsEvolving     { get; private set; }
        public bool      IsEvolved      { get; private set; }
        public string    CurrentLabel   => stats.displayName;

        public float EvolutionProgress
            => callbacks.EvolutionDuration <= 0f
                ? 1f
                : Mathf.Clamp01(evolutionTimer / callbacks.EvolutionDuration);

        // ----------------------------------------------------------------
        // Инициализация
        // ----------------------------------------------------------------

        public void Initialize(IBattleCallbacks battleCallbacks, TeamSide team, TotemType totem, UnitStats unitStats, bool startEvolved = false)
        {
            callbacks      = battleCallbacks;
            Team           = team;
            AssignedTotem  = totem;
            stats          = unitStats;
            IsEvolved      = startEvolved;

            // Найти дочерние трансформы
            body        = transform.Find("Visual");
            weapon      = transform.Find("Visual/Weapon");
            nameLabel   = transform.Find("NameLabel").GetComponent<TextMesh>();
            healthValue = transform.Find("HealthValue").GetComponent<TextMesh>();
            healthFill  = transform.Find("HealthBar/Fill");

            // Применить параметры
            currentHealth = stats.maxHealth;
            ApplyVisual();
        }

        // ----------------------------------------------------------------
        // Тик (вызывается из GameController.Update через UnitRegistry)
        // ----------------------------------------------------------------

        public void Tick(float deltaTime)
        {
            if (IsDead) return;

            // Cooldown'ы
            if (attackCooldown > 0f) attackCooldown -= deltaTime;
            TickPunch(deltaTime);

            // Эволюция
            if (IsEvolving)
            {
                evolutionTimer += deltaTime;
                callbacks.SetEvolutionUI(true, $"EVOLVING: {CurrentLabel}", EvolutionProgress);
                if (evolutionTimer >= callbacks.EvolutionDuration)
                    CompleteEvolution();
                return;
            }

            // Начать эволюцию если в зоне
            if (!IsEvolved && Team == TeamSide.Player && AssignedTotem != TotemType.None
                && callbacks.IsInsideEvolutionZone(transform.position))
            {
                StartEvolution();
                return;
            }

            // Поиск врага
            var enemy = callbacks.FindClosestEnemy(this, stats.engagementRange);
            if (enemy != null)
            {
                TryEngageUnit(enemy, deltaTime);
                return;
            }

            // Атака базы
            if (callbacks.IsAtEnemyBase(this))
            {
                TryAttackBase();
                return;
            }

            Move(deltaTime);
        }

        // ----------------------------------------------------------------
        // Получение урона
        // ----------------------------------------------------------------

        public void ReceiveDamage(float damage)
        {
            if (IsDead) return;
            currentHealth = Mathf.Max(0f, currentHealth - damage);
            RefreshHealthBar();

            if (currentHealth <= 0f)
            {
                callbacks.NotifyUnitKilled(this);
                Destroy(gameObject);
            }
        }

        // ----------------------------------------------------------------
        // Приватные методы — движение и бой
        // ----------------------------------------------------------------

        private void Move(float deltaTime)
        {
            var dir = Team == TeamSide.Player ? Vector3.right : Vector3.left;
            transform.position += dir * (stats.moveSpeed * deltaTime);
        }

        private void TryEngageUnit(Unit enemy, float deltaTime)
        {
            var gap = Mathf.Abs(enemy.transform.position.x - transform.position.x);
            if (gap > stats.attackRange) { Move(deltaTime); return; }
            if (attackCooldown > 0f) return;

            attackCooldown = stats.attackInterval;
            punchTimer     = 0.15f;
            enemy.ReceiveDamage(stats.attackDamage);
        }

        private void TryAttackBase()
        {
            if (attackCooldown > 0f) return;
            attackCooldown = stats.attackInterval;
            punchTimer     = 0.15f;
            callbacks.DamageBase(
                Team == TeamSide.Player ? TeamSide.Enemy : TeamSide.Player,
                Mathf.CeilToInt(stats.attackDamage));
        }

        // ----------------------------------------------------------------
        // Эволюция
        // ----------------------------------------------------------------

        private void StartEvolution()
        {
            IsEvolving     = true;
            evolutionTimer = 0f;
            callbacks.SetEvolutionUI(true, $"EVOLVING: {CurrentLabel}", 0f);
        }

        private void CompleteEvolution()
        {
            IsEvolving = false;
            IsEvolved  = true;
            callbacks.SetEvolutionUI(false, string.Empty, 0f);
            callbacks.NotifyUnitEvolved(this);
        }

        /// <summary>Обновить параметры (например, после эволюции).</summary>
        public void ReinitializeStats(UnitStats newStats)
        {
            var hpRatio   = stats.maxHealth > 0f ? currentHealth / stats.maxHealth : 1f;
            stats         = newStats;
            currentHealth = Mathf.Min(stats.maxHealth * hpRatio + (IsEvolved ? 4f : 0f), stats.maxHealth);
            ApplyVisual();
        }

        // ----------------------------------------------------------------
        // Визуал
        // ----------------------------------------------------------------

        private void ApplyVisual()
        {
            var renderers = body.GetComponentsInChildren<MeshRenderer>();
            foreach (var r in renderers)
            {
                if (r.name.Contains("Weapon"))
                    r.material.color = stats.bodyColor * 0.72f;
                else if (r.name.Contains("Head"))
                    r.material.color = Color.Lerp(stats.bodyColor, Color.white, 0.18f);
                else
                    r.material.color = stats.bodyColor;
            }

            if (weapon != null && weapon.TryGetComponent<MeshRenderer>(out var wr))
                wr.material.color = stats.bodyColor * 0.75f;

            body.localScale = stats.bodyScale;

            nameLabel.text  = stats.displayName;
            nameLabel.transform.localPosition  = new Vector3(0f, stats.bodyScale.y * 1.18f, 0f);
            healthValue.transform.localPosition = new Vector3(0f, stats.bodyScale.y * 0.98f, 0f);

            var healthRoot = transform.Find("HealthBar");
            if (healthRoot != null)
                healthRoot.localPosition = new Vector3(0f, stats.bodyScale.y * 0.82f, 0f);

            RefreshHealthBar();
        }

        private void TickPunch(float deltaTime)
        {
            if (punchTimer <= 0f) { body.localPosition = Vector3.zero; return; }
            punchTimer -= deltaTime;
            var progress    = Mathf.Max(0f, punchTimer / 0.15f);
            var punchOffset = (Team == TeamSide.Player ? 1f : -1f) * 0.3f;
            body.localPosition = new Vector3(punchOffset * progress, 0f, 0f);
        }

        private void RefreshHealthBar()
        {
            if (healthFill == null) return;
            var s     = healthFill.localScale;
            s.x       = Mathf.Clamp01(currentHealth / stats.maxHealth);
            healthFill.localScale = s;
            healthValue.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(stats.maxHealth)}";
        }
    }
}
