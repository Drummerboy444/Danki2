using UnityEngine;

public class ForestGolem : Enemy
{
    [SerializeField] private Ent entPrefab = null;

    [Header("Root")]
    [SerializeField] private ForestGolemRootIndicator rootIndicatorPrefab = null;
    [SerializeField] private ForestGolemRoot rootPrefab = null;
    [SerializeField] private float rootDelay = 0;
    [SerializeField] private float rootRange = 0;
    [SerializeField] private int rootDamage = 0;
    [SerializeField] private float rootDamageInterval = 0;
    [SerializeField] private int rootDamageRepetitions = 0;

    [Header("Boulder")]
    [SerializeField] private ForestGolemBoulder boulderPrefab = null;
    [SerializeField] private float boulderRange = 0;
    [SerializeField] private int boulderDamage = 0;
    [SerializeField] private float boulderSpikeInterval = 0;
    [SerializeField] private float boulderSpikeInitialDelay = 0;
    [SerializeField] private float boulderSpikeRange = 0;
    [SerializeField] private int boulderSpikeDamage = 0;

    [Header("Stomp")]
    [SerializeField] private float stompPositionOffset = 0;
    [SerializeField] private float stompRadius = 0;
    [SerializeField] private int stompDamage = 0;
    [SerializeField] private float stompKnockbackDuration = 0;
    [SerializeField] private float stompKnockbackSpeed = 0;
    [SerializeField] private float stompEffectScaleFactor = 0;

    public Subject EntSpawnedSubject { get; } = new Subject();
    public Subject BoulderThrowSubject { get; } = new Subject();
    public Subject StompSubject { get; } = new Subject();

    public override ActorType Type => ActorType.ForestGolem;

    public void FireRoot(Vector3 position)
    {
        Instantiate(rootIndicatorPrefab, position, Quaternion.identity);

        this.WaitAndAct(rootDelay, () =>
        {
            Instantiate(rootPrefab, position, Quaternion.identity);

            this.ActOnInterval(
                rootDamageInterval,
                _ => HandleRootDamage(position),
                numRepetitions: rootDamageRepetitions
            );
        });
    }

    public void SpawnEnt(Vector3 position)
    {
        Instantiate(entPrefab, position, Quaternion.identity);
        EntSpawnedSubject.Next();
    }

    private void HandleRootDamage(Vector3 position)
    {
        Player player = ActorCache.Instance.Player;
        if (Vector3.Distance(player.transform.position, position) > rootRange) return;

        player.HealthManager.ReceiveDamage(rootDamage, this);
        CustomCamera.Instance.AddShake(ShakeIntensity.Low);
    }

    public void ThrowBoulder(Vector3 targetPosition)
    {
        Player player = ActorCache.Instance.Player;
        ForestGolemBoulder.Create(
            boulderPrefab,
            AbilitySource,
            targetPosition,
            () =>
            {
                if (Vector3.Distance(player.transform.position, targetPosition) > boulderRange) return;

                player.HealthManager.ReceiveDamage(boulderDamage, this);
                CustomCamera.Instance.AddShake(ShakeIntensity.High);
            },
            () =>
            {
                HandleBoulderSpike(targetPosition, player);
            },
            boulderSpikeInterval,
            boulderSpikeInitialDelay
        );
        
        BoulderThrowSubject.Next();
    }

    private void HandleBoulderSpike(Vector3 position, Player player)
    {
        if (Vector3.Distance(player.transform.position, position) < boulderSpikeRange)
        {
            player.HealthManager.ReceiveDamage(boulderSpikeDamage, this);
            CustomCamera.Instance.AddShake(ShakeIntensity.Medium);
        }
    }

    public void Stomp()
    {
        Vector3 targetPosition = CollisionTemplateSource + transform.forward * stompPositionOffset;
        
        SmashObject.Create(targetPosition, stompEffectScaleFactor);

        Player player = ActorCache.Instance.Player;

        if (Vector3.Distance(player.transform.position, targetPosition) <= stompRadius)
        {
            player.HealthManager.ReceiveDamage(stompDamage, this);

            Vector3 knockbackDirection = player.transform.position - transform.position;
            player.MovementManager.LockMovement(
                stompKnockbackDuration,
                stompKnockbackSpeed,
                knockbackDirection,
                knockbackDirection
            );

            CustomCamera.Instance.AddShake(ShakeIntensity.Medium);
        }
        
        StompSubject.Next();
    }
}
