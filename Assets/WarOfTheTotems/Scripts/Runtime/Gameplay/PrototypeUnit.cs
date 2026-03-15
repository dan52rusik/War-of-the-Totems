using UnityEngine;

namespace WarOfTheTotems.Gameplay
{
    public enum TeamSide
    {
        Player,
        Enemy,
    }

    public enum TotemType
    {
        None,
        Stone,
        Beast,
        Shadow,
    }

    public sealed class PrototypeUnit : MonoBehaviour
    {
        private PrototypeBattleController controller = null!;
        private Transform body = null!;
        private Transform weapon = null!;
        private TextMesh label = null!;
        private TextMesh healthValue = null!;
        private Transform healthFill = null!;

        private float maxHealth;
        private float currentHealth;
        private float moveSpeed;
        private float attackDamage;
        private float attackInterval;
        private float attackRange;
        private float engagementRange;
        private float attackCooldown;
        private float evolutionTimer;
        private float punchTimer;

        public TeamSide Team { get; private set; }
        public TotemType AssignedTotem { get; private set; }
        public bool IsDead => currentHealth <= 0f;
        public bool IsEvolving { get; private set; }
        public bool IsEvolved { get; private set; }
        public string CurrentLabel { get; private set; } = string.Empty;
        public float EvolutionProgress => controller.EvolutionDuration <= 0f ? 1f : Mathf.Clamp01(evolutionTimer / controller.EvolutionDuration);

        public void Initialize(PrototypeBattleController owner, TeamSide team, TotemType totem, bool evolved)
        {
            controller = owner;
            Team = team;
            AssignedTotem = totem;

            body = transform.Find("Visual");
            weapon = transform.Find("Visual/Weapon");
            label = transform.Find("NameLabel").GetComponent<TextMesh>();
            healthValue = transform.Find("HealthValue").GetComponent<TextMesh>();
            healthFill = transform.Find("HealthBar/Fill");

            IsEvolved = evolved;
            ApplyForm(evolved ? totem : TotemType.None);
        }

        public void Tick(float deltaTime)
        {
            if (IsDead)
            {
                return;
            }

            if (attackCooldown > 0f)
            {
                attackCooldown -= deltaTime;
            }

            if (punchTimer > 0f)
            {
                punchTimer -= deltaTime;
                var progress = punchTimer / 0.15f;
                if (progress < 0f) progress = 0f;
                var punchOffset = (Team == TeamSide.Player ? 1f : -1f) * 0.3f;
                body.localPosition = new Vector3(punchOffset * progress, 0f, 0f);
            }
            else
            {
                body.localPosition = Vector3.zero;
            }

            if (IsEvolving)
            {
                evolutionTimer += deltaTime;
                controller.SetEvolutionUI(true, $"EVOLVING: {GetEvolutionName()}", EvolutionProgress);
                if (evolutionTimer >= controller.EvolutionDuration)
                {
                    CompleteEvolution();
                }

                return;
            }

            if (!IsEvolved && Team == TeamSide.Player && AssignedTotem != TotemType.None && controller.IsInsideEvolutionZone(transform.position))
            {
                StartEvolution();
                return;
            }

            var enemy = controller.FindClosestEnemy(this, engagementRange);
            if (enemy != null)
            {
                TryEngageUnit(enemy, deltaTime);
                return;
            }

            if (controller.IsAtEnemyBase(this))
            {
                TryAttackBase();
                return;
            }

            Move(deltaTime);
        }

        public void ReceiveDamage(float damage)
        {
            if (IsDead)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - damage);
            RefreshHealthBar();

            if (currentHealth <= 0f)
            {
                controller.NotifyUnitKilled(this);
                Destroy(gameObject);
            }
        }

        private void Move(float deltaTime)
        {
            var direction = Team == TeamSide.Player ? Vector3.right : Vector3.left;
            transform.position += direction * (moveSpeed * deltaTime);
        }

        private void TryEngageUnit(PrototypeUnit enemy, float deltaTime)
        {
            var gap = Mathf.Abs(enemy.transform.position.x - transform.position.x);
            if (gap > attackRange)
            {
                Move(deltaTime);
                return;
            }

            if (attackCooldown > 0f)
            {
                return;
            }

            attackCooldown = attackInterval;
            punchTimer = 0.15f;
            enemy.ReceiveDamage(attackDamage);
        }

        private void TryAttackBase()
        {
            if (attackCooldown > 0f)
            {
                return;
            }

            attackCooldown = attackInterval;
            punchTimer = 0.15f;
            controller.DamageBase(Team == TeamSide.Player ? TeamSide.Enemy : TeamSide.Player, Mathf.CeilToInt(attackDamage));
        }

        private void StartEvolution()
        {
            IsEvolving = true;
            evolutionTimer = 0f;
            controller.SetEvolutionUI(true, $"EVOLVING: {GetEvolutionName()}", 0f);
        }

        private void CompleteEvolution()
        {
            IsEvolving = false;
            IsEvolved = true;
            controller.SetEvolutionUI(false, string.Empty, 0f);
            ApplyForm(AssignedTotem);
        }

        private void ApplyForm(TotemType form)
        {
            switch (form)
            {
                case TotemType.Stone:
                    CurrentLabel = "STONE GUARD";
                    maxHealth = 28f;
                    moveSpeed = 1.05f;
                    attackDamage = 4f;
                    attackInterval = 0.9f;
                    attackRange = 0.62f;
                    engagementRange = 5.0f;
                    SetVisual(new Color(0.58f, 0.62f, 0.66f), new Vector3(0.82f, 1.15f, 1f));
                    break;
                case TotemType.Beast:
                    CurrentLabel = "FANG BEAST";
                    maxHealth = 16f;
                    moveSpeed = 2.35f;
                    attackDamage = 3f;
                    attackInterval = 0.48f;
                    attackRange = 0.72f;
                    engagementRange = 5.0f;
                    SetVisual(new Color(0.80f, 0.47f, 0.26f), new Vector3(0.76f, 0.96f, 1f));
                    break;
                case TotemType.Shadow:
                    CurrentLabel = "SHADOW HUNTER";
                    maxHealth = 14f;
                    moveSpeed = 0.85f;
                    attackDamage = 3f;
                    attackInterval = 0.58f;
                    attackRange = 0.68f;
                    engagementRange = 5.0f;
                    SetVisual(new Color(0.28f, 0.16f, 0.38f), new Vector3(0.72f, 1.0f, 1f));
                    break;
                default:
                    CurrentLabel = "BASE BEARER";
                    maxHealth = 10f;
                    moveSpeed = 1.6f;
                    attackDamage = 1f;
                    attackInterval = 0.85f;
                    attackRange = 0.55f;
                    engagementRange = 5.0f;
                    SetVisual(new Color(0.92f, 0.82f, 0.60f), new Vector3(0.56f, 0.84f, 1f));
                    break;
            }

            currentHealth = Mathf.Min(currentHealth <= 0f ? maxHealth : currentHealth + (IsEvolved ? 4f : 0f), maxHealth);
            label.text = CurrentLabel;
            RefreshHealthBar();
        }

        private void SetVisual(Color color, Vector3 scale)
        {
            var renderers = body.GetComponentsInChildren<MeshRenderer>();
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer.name.Contains("Weapon"))
                {
                    renderer.sharedMaterial.color = color * 0.72f;
                }
                else if (renderer.name.Contains("Head"))
                {
                    renderer.sharedMaterial.color = Color.Lerp(color, Color.white, 0.18f);
                }
                else
                {
                    renderer.sharedMaterial.color = color;
                }
            }

            if (weapon != null && weapon.TryGetComponent<MeshRenderer>(out var weaponRenderer))
            {
                weaponRenderer.sharedMaterial.color = color * 0.75f;
            }

            body.localScale = scale;
            label.transform.localPosition = new Vector3(0f, scale.y * 1.18f, 0f);
            healthValue.transform.localPosition = new Vector3(0f, scale.y * 0.98f, 0f);
            var healthRoot = transform.Find("HealthBar");
            healthRoot.localPosition = new Vector3(0f, scale.y * 0.82f, 0f);
        }

        private void RefreshHealthBar()
        {
            if (healthFill == null)
            {
                return;
            }

            var fillScale = healthFill.localScale;
            fillScale.x = Mathf.Clamp01(currentHealth / maxHealth);
            healthFill.localScale = fillScale;
            healthValue.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
        }

        private string GetEvolutionName()
        {
            return AssignedTotem switch
            {
                TotemType.Stone => "STONE GUARD",
                TotemType.Beast => "FANG BEAST",
                TotemType.Shadow => "SHADOW HUNTER",
                _ => "BASE BEARER",
            };
        }
    }
}
