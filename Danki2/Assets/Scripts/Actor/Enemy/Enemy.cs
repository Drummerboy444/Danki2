﻿using UnityEngine;

public abstract class Enemy : Actor
{
    public const float DamageInterruptTime = 0.4f;

    public EnemyMovementManager MovementManager { get; private set; }

    public BehaviourSubject<bool> PlayerTargeted { get; } = new BehaviourSubject<bool>(false);

    public Color? CurrentTelegraph { get; private set; } = null;

    protected override Tag Tag => Tag.Enemy;

    protected override void Awake()
    {
        base.Awake();

        MovementManager = new EnemyMovementManager(this, updateSubject, navmeshAgent);

        SetUpInterruption();
    }

    public void StartTelegraph(Color colour) => CurrentTelegraph = colour;

    public void StopTelegraph() => CurrentTelegraph = null;

    private void SetUpInterruption()
    {
        if (Animator == null) return;

        HealthManager.ModifiedDamageSubject.Subscribe(_ =>
        {
            Animator.SetFloat("AbilityPauseFactor", 0);
            this.WaitAndAct(DamageInterruptTime, () => Animator.SetFloat("AbilityPauseFactor", 1));
        });
    }
}
