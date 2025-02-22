﻿using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class Bear : Enemy
{
    private const string SwipeAnimationName = "Swipe_OneShot";
    private const string MaulAnimationName = "Bite_OneShot";
    private const string MaulWindupAnimationName = "BiteWindup_OneShot";
    private const string CleaveAnimationName = "Smash_OneShot";

    [Header("FMOD Events"), EventRef, SerializeField]
    private string roarEvent = null;

    [EventRef, SerializeField]
    private string idleEvent = null;

    [SerializeField] private Transform cleaveOrigin = null;

    [Header("Swipe")]
    [SerializeField] private int swipeDamage = 0;
    [SerializeField] private float swipePauseDuration = 0;
    [SerializeField] private float swipeDamageRange = 0;

    [Header("Charge")]
    [SerializeField] private int chargeDamage = 0;
    [SerializeField] private float chargeSpeed = 0;
    [SerializeField] private float chargeEffectInterval = 0;
    [SerializeField] private int chargeEffectRepetitions = 0;
    [SerializeField] private float chargeRotationRate = 0;
    [SerializeField] private float chargeDamageRange = 0;
    [SerializeField] private float chargePauseDuration = 0;
    [SerializeField] private float chargeKnockBackDuration = 0;
    [SerializeField] private float chargeKnockBackSpeed = 0;

    [Header("Maul")]
    [SerializeField] private int maulDamage = 0;
    [SerializeField] private int maulBiteCount = 0;
    [SerializeField] private float maulBiteInterval = 0;
    [SerializeField] private float maulBiteRange = 0;
    [SerializeField] private float maulSlowDuration = 0;
    
    [Header("Cleave")]
    [SerializeField] private int cleaveDamage = 0;
    [SerializeField] private float cleaveRange = 0;
    [SerializeField] private float cleavePauseDuration = 0;
    [SerializeField] private float cleaveKnockBackDuration = 0;
    [SerializeField] private float cleaveKnockBackSpeed = 0;

    private Actor chargeTarget = null;
    private Vector3 chargeDirection;
    private bool charging = false;
    
    public override ActorType Type => ActorType.Bear;

    public Subject ChargeSubject { get; } = new Subject();
    public Subject CleaveSubject { get; } = new Subject();

    protected override void Start()
    {
        base.Start();

        HealthManager.ModifiedDamageSubject.Subscribe(_ => OnTakeDamage());
    }

    protected override void Update()
    {
        base.Update();

        if (charging) ContinueCharge();
    }

    public void PlaySwipeAnimation() => Animator.Play(SwipeAnimationName);

    public void PlayCleaveAnimation() => Animator.Play(CleaveAnimationName);

    public void PlayBiteWindupAnimation() => Animator.Play(MaulWindupAnimationName);

    public void Swipe()
    {
        Vector3 forward = transform.forward;
        MovementManager.LookAt(transform.position + forward);

        Quaternion castRotation = Quaternion.LookRotation(transform.forward);

        AbilityUtils.TemplateCollision(
            this,
            CollisionTemplateShape.Wedge90,
            swipeDamageRange,
            CollisionTemplateSource,
            castRotation,
            playerCallback: player =>
            {
                player.HealthManager.ReceiveDamage(swipeDamage, this);
                CustomCamera.Instance.AddShake(ShakeIntensity.Medium);
            }
        );

        SwipeObject.Create(AbilitySource, castRotation);

        MovementManager.Pause(swipePauseDuration);
    }

    public void Charge(Actor target)
    {
        chargeTarget = target;
        chargeDirection = target.transform.position - transform.position;
        charging = true;
        this.ActOnInterval(chargeEffectInterval, ChargeEffect, chargeEffectInterval, chargeEffectRepetitions);
    }

    private void ContinueCharge()
    {
        Vector3 desiredDirection = chargeTarget.transform.position - transform.position;
        MovementManager.SetMovementTargetPoint(transform.position + desiredDirection);
    }

    private void ChargeEffect(int index)
    {
        if (index == chargeEffectRepetitions - 1)
        {
            MovementManager.Pause(chargePauseDuration);
            charging = false;
            ChargeSubject.Next();
        }
        
        Quaternion castRotation = Quaternion.LookRotation(transform.forward);
        
        AbilityUtils.TemplateCollision(
            this,
            CollisionTemplateShape.Wedge90,
            chargeDamageRange,
            CollisionTemplateSource,
            castRotation,
            playerCallback: player =>
            {
                ChargeKnockBack(player);
                player.HealthManager.ReceiveDamage(chargeDamage, this);
                CustomCamera.Instance.AddShake(ShakeIntensity.Medium);
            }
        );

        SwipeObject.Create(AbilitySource, castRotation);
        BeginBiteAnim();
    }

    private void ChargeKnockBack(Player player)
    {
        Vector3 knockBackDirection = player.transform.position - transform.position;
        Vector3 knockBackFaceDirection = player.transform.forward;

        player.MovementManager.LockMovement(
            chargeKnockBackDuration,
            chargeKnockBackSpeed,
            knockBackDirection
        );
    }

    public void Maul()
    {
        Vector3 floorTargetPosition = transform.position + transform.forward;
        MovementManager.LookAt(floorTargetPosition);

        MovementManager.Pause(maulBiteInterval * maulBiteCount);

        MaulObject maulObject = MaulObject.Create(AbilitySource);
        this.ActOnInterval(
            maulBiteInterval,
            index => HandleMaulBite(index, maulObject),
            initialInterval: 0,
            numRepetitions: maulBiteCount
        );
    }

    private void HandleMaulBite(int index, MaulObject maulObject)
    {
        Vector3 forward = transform.forward;
        Vector3 horizontalDirection = Vector3.Cross(forward, Vector3.up).normalized;
        int directionMultiplier = index % 2 == 1 ? 1 : -1;
        Vector3 randomisedCastDirection = forward.normalized + 0.25f * directionMultiplier * horizontalDirection;

        Quaternion castRotation = AbilityUtils.GetMeleeCastRotation(randomisedCastDirection);

        maulObject.Bite(castRotation);
        BeginBiteAnim();

        AbilityUtils.TemplateCollision(
            this,
            CollisionTemplateShape.Wedge45,
            maulBiteRange,
            CollisionTemplateSource,
            castRotation,
            playerCallback: player =>
            {
                player.HealthManager.ReceiveDamage(maulDamage, this);
                player.EffectManager.AddActiveEffect(ActiveEffect.Slow, maulSlowDuration);
                CustomCamera.Instance.AddShake(ShakeIntensity.Medium);
            }
        );
    }

    private void BeginBiteAnim()
    {
        if (!IsCurrentAnimationState(MaulAnimationName))
        {
            Animator.Play(MaulAnimationName);
        }
    }

    public void Cleave()
    {
        Vector3 forward = transform.forward;
        Vector3 castDirection = forward;
        Quaternion castRotation = AbilityUtils.GetMeleeCastRotation(castDirection);

        SmashObject.Create(cleaveOrigin.position, cleaveRange/2);

        AbilityUtils.TemplateCollision(
            this,
            CollisionTemplateShape.Cylinder,
            cleaveRange,
            cleaveOrigin.position,
            castRotation,
            playerCallback: player =>
            {
                player.HealthManager.ReceiveDamage(cleaveDamage, this);
                MaulKnockBack(player);
                CustomCamera.Instance.AddShake(ShakeIntensity.High);
            }
        );

        MovementManager.LookAt(transform.position + forward);

        CleaveSubject.Next();

        MovementManager.Pause(cleavePauseDuration);
    }

    public void PlayIdleSound()
    {
        EventInstance fmodEvent = FmodUtils.CreatePositionedInstance(idleEvent, transform.position);
        fmodEvent.start();
        fmodEvent.release();
    }

    private void MaulKnockBack(Player player)
    {
        Vector3 knockBackDirection = player.transform.position - transform.position;
        Vector3 knockBackFaceDirection = player.transform.forward;

        player.MovementManager.LockMovement(
            cleaveKnockBackDuration,
            cleaveKnockBackSpeed,
            knockBackDirection
        );
    }

    private void OnTakeDamage()
    {
        Animator.Play(CommonAnimStrings.RandomFlinch);
        Roar();
    }

    private void Roar()
    {
        EventInstance fmodEvent = FmodUtils.CreatePositionedInstance(roarEvent, transform.position);
        fmodEvent.start();
        fmodEvent.release();
    }
}
