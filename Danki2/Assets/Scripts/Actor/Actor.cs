using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class Actor : MonoBehaviour
{
    [SerializeField] protected NavMeshAgent navmeshAgent = null;

    [SerializeField] private TrailRenderer trailRenderer = null;

    // Serialized properties
    [SerializeField] private Animator animator = null;
    public Animator Animator => animator;

    [SerializeField] private int maxHealth = 0;
    public int MaxHealth => MaxHealthPipes.Aggregate(maxHealth, (value, pipe) => pipe(value));

    [SerializeField]
    private AbilityAnimationListener abilityAnimationListener = null;
    public AbilityAnimationListener AbilityAnimationListener => abilityAnimationListener;

    [SerializeField] private Collider[] colliders = null;
    public Collider[] Colliders => colliders;

    [Header("Movement")]
    [SerializeField] private float speed = 0f;
    public float Speed => SpeedPipes.Aggregate(speed, (value, pipe) => pipe(value));

    [SerializeField] private float turnSpeed = 0f;
    public float TurnSpeed => turnSpeed;

    [SerializeField] public float RotationSpeedAcceleration = 1f;

    [SerializeField] private float strafeAngleLimit = 0f;
    public float StrafeAngleLimit => strafeAngleLimit;

    [SerializeField] private float facingAngleGrace = 0f;
    public float FacingAngleGrace => facingAngleGrace;

    [SerializeField] private float rotationSmoothing = 0f;
    public float RotationSmoothing => rotationSmoothing;

    [Header("Sockets")]
    [SerializeField] private Transform centre = null;
    public Transform CentreTransform => centre;
    public Vector3 Centre => centre.position;
    public float Height => Centre.y - transform.position.y;

    [SerializeField] private Transform abilitySource = null;
    public Transform AbilitySourceTransform => abilitySource;
    public Vector3 AbilitySource => abilitySource.position;

    [SerializeField] private Transform collisionTemplateSource = null;
    public Transform CollisionTemplateSourceTransform => collisionTemplateSource;
    public Vector3 CollisionTemplateSource => collisionTemplateSource.position;

    protected readonly Subject startSubject = new Subject();
    protected readonly Subject updateSubject = new Subject();
    protected readonly Subject lateUpdateSubject = new Subject();

    private Coroutine stopTrailCoroutine;

    private List<Func<int, int>> MaxHealthPipes = new List<Func<int, int>>();
    private List<Func<float, float>> SpeedPipes = new List<Func<float, float>>();

    // Services
    public HealthManager HealthManager { get; private set; }
    public EffectManager EffectManager { get; private set; }
    public EmissiveManager EmissiveManager { get; private set; }

    public bool Dead => HealthManager.Dead;
    public Subject<DeathData> DeathSubject = new Subject<DeathData>();
    public abstract ActorType Type { get; }
    protected abstract Tag Tag { get; }

    public bool IsPlayer => Tag == Tag.Player;

    protected virtual void Awake()
    {
        gameObject.SetTag(Tag);

        gameObject.SetLayerRecursively(Layer.Actors);

        ActorCache.Instance.Register(this);

        EffectManager = new EffectManager(this, updateSubject);
        HealthManager = new HealthManager(this, updateSubject);
        EmissiveManager = new EmissiveManager(this);
    }

    protected virtual void Start() => startSubject.Next();

    protected virtual void Update() => updateSubject.Next();

    protected virtual void LateUpdate() => lateUpdateSubject.Next();
        
    public bool Opposes(Actor target) => !CompareTag(target.tag);

    public void StartTrail(float duration)
    {
        trailRenderer.emitting = true;

        if (stopTrailCoroutine != null)
        {
            StopCoroutine(stopTrailCoroutine);
        }

        stopTrailCoroutine = this.WaitAndAct(duration, () => trailRenderer.emitting = false);
    }

    public bool IsCurrentAnimationState(string state)
    {
        return Animator.GetCurrentAnimatorStateInfo(0).IsName(state);
    }
    
    public void RegisterSpeedPipe(Func<float, float> pipe) => SpeedPipes.Add(pipe);

    public void RegisterMaxHealthPipe(Func<int, int> pipe) => MaxHealthPipes.Add(pipe);
}
