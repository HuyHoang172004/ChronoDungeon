using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(Health), typeof(EnemyKnight))]
public sealed class EliteEnemy : MonoBehaviour, ITimeLoopResettable
{
    [SerializeField, Min(1f)] private float eliteHealth = 220f;
    [SerializeField, Min(1f)] private float rewardHeal = 35f;
    [SerializeField] private EliteRewardPickup rewardPrefab;
    [SerializeField] private SpriteRenderer aura;
    private Health health;
    private EnemyKnight knight;
    private EliteRewardPickup reward;
    private bool rewardSpawned;

    public float EliteHealth => eliteHealth;
    public EliteRewardPickup Reward => reward;

    private void Awake()
    {
        health = GetComponent<Health>();
        knight = GetComponent<EnemyKnight>();
        health.SetMaximumHealth(eliteHealth, true);
        health.Changed += OnHealthChanged;
        if (aura != null) aura.color = new Color(1f, .7f, .12f, .22f);
    }

    private void OnEnable()
    {
        if (health != null && !health.IsDead) rewardSpawned = false;
        if (aura != null) aura.enabled = true;
    }

    private void OnDisable()
    {
        if (aura != null) aura.enabled = false;
    }

    private void OnHealthChanged(Health changed)
    {
        if (!changed.IsDead || rewardSpawned || rewardPrefab == null) return;
        rewardSpawned = true;
        if (reward == null)
        {
            reward = Instantiate(rewardPrefab, transform.parent);
            reward.name = "Temporal Elite Reward";
        }
        reward.transform.position = transform.position;
        reward.SetHealAmount(rewardHeal);
        reward.ResetReward();
    }

    public void CaptureInitialState() { }
    public void ResetToInitialState()
    {
        rewardSpawned = false;
        if (reward != null) reward.gameObject.SetActive(false);
        if (aura != null) aura.enabled = true;
    }

    private void OnDestroy()
    {
        if (health != null) health.Changed -= OnHealthChanged;
    }
}
