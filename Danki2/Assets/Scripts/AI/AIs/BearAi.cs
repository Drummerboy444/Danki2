﻿using UnityEngine;

public class BearAi : Ai
{
    [SerializeField] private Bear bear = null;

    [Header("General")]
    [SerializeField] private float aggroDistance = 0;
    [SerializeField, Tooltip("Value must be less than maxAttackRange.")] private float minAdvanceRange = 0;
    [SerializeField, Tooltip("Value must be greater than minAdvanceRange.")] private float maxAttackRange = 0;
    [SerializeField] private float maxChargeRange = 0;
    [SerializeField] private float chargeMinInterval = 0;
    [SerializeField] private float chargeMaxInterval = 0;
    [SerializeField] private float chargeRecoveryTime = 0;
    [SerializeField] private float minIdleSoundTimer = 0;
    [SerializeField] private float maxIdleSoundTimer = 0;

    [Header("Attack")]
    [SerializeField] private float abilityInterval = 0;
    [SerializeField] private float maxAttackAngle = 0;

    protected override Actor Actor => bear;

    protected override IStateMachineComponent BuildStateMachineComponent()
    {
        Player player = ActorCache.Instance.Player;

        IStateMachineComponent advanceStateMachine = new StateMachine<AdvanceState>(AdvanceState.Walk)
            .WithComponent(AdvanceState.Walk, new WalkTowards(bear, player))
            .WithComponent(AdvanceState.Run, new MoveTowards(bear, player))
            .WithTransition(AdvanceState.Walk, AdvanceState.Run, new TakesNonTickDamage(bear));

        IStateMachineComponent attackStateMachine = new StateMachine<AttackState>(AttackState.ChooseAbility)
            .WithComponent(AttackState.WatchTarget, new WatchTarget(bear, player))
            .WithComponent(AttackState.TelegraphSwipe, new BearTelegraphSwipe(bear))
            .WithComponent(AttackState.TelegraphMaul, new BearTelegraphMaul(bear))
            .WithComponent(AttackState.TelegraphCleave, new BearTelegraphCleave(bear))
            .WithComponent(AttackState.Swipe, new BearSwipe(bear))
            .WithComponent(AttackState.Maul, new BearMaul(bear))
            .WithComponent(AttackState.Cleave, new BearCleave(bear))
            .WithTransition(AttackState.WatchTarget, AttackState.ChooseAbility, new TimeElapsed(abilityInterval) & new Facing(bear, player, maxAttackAngle))
            .WithTransition(AttackState.TelegraphSwipe, AttackState.Swipe, new AbilityImpact(bear))
            .WithTransition(AttackState.TelegraphMaul, AttackState.Maul, new AbilityFinish(bear))
            .WithTransition(AttackState.TelegraphCleave, AttackState.Cleave, new AbilityImpact(bear))
            .WithTransition(AttackState.Swipe, AttackState.WatchTarget, new AbilityFinish(bear))
            .WithTransition(AttackState.Maul, AttackState.WatchTarget)
            .WithTransition(AttackState.Cleave, AttackState.WatchTarget, new AbilityFinish(bear))
            .WithDecisionState(AttackState.ChooseAbility, new RandomDecider<AttackState>(
                AttackState.TelegraphSwipe,
                AttackState.TelegraphMaul,
                AttackState.TelegraphCleave
            ));

        return new StateMachine<State>(State.Idle)
            .WithComponent(State.Advance, advanceStateMachine)
            .WithComponent(State.Attack, attackStateMachine)
            .WithComponent(State.TelegraphCharge, new BearTelegraphCharge(bear))
            .WithComponent(State.Charge, new BearChannelCharge(bear, player))
            .WithComponent(State.Watch, new WatchTarget(bear, player))
            .WithComponent(State.Idle, new BearIdle(bear, minIdleSoundTimer, maxIdleSoundTimer))
            .WithTransition(State.Idle, State.Advance, new DistanceLessThan(bear, player, aggroDistance) | new TakesDamage(bear))
            .WithTransition(State.Advance, State.Attack, new DistanceLessThan(bear, player, minAdvanceRange) & new Facing(bear, player, maxAttackAngle))
            .WithTransition(State.Attack, State.Advance, new DistanceGreaterThan(bear, player, maxAttackRange) & !new IsTelegraphing(bear) & !new SubjectEmitted(bear.CleaveSubject))
            .WithTransition(State.Attack, State.TelegraphCharge, new SubjectEmitted(bear.CleaveSubject))
            .WithTransition(
                State.Advance,
                State.TelegraphCharge,
                new DistanceLessThan(bear, player, maxChargeRange) & new RandomTimeElapsed(chargeMinInterval, chargeMaxInterval) & new HasLineOfSight(bear.CentreTransform, player.CentreTransform)
            )
            .WithTransition(State.TelegraphCharge, State.Charge, new AbilityFinish(bear))
            .WithTransition(State.Charge, State.Watch, new SubjectEmitted(bear.ChargeSubject))
            .WithTransition(State.Watch, State.Advance, new TimeElapsed(chargeRecoveryTime));
    }

    private enum State
    {
        Idle,
        Advance,
        TelegraphCharge,
        Charge,
        Attack,
        Watch
    }

    private enum AdvanceState
    {
        Walk,
        Run
    }

    private enum AttackState
    {
        WatchTarget,
        ChooseAbility,
        TelegraphSwipe,
        Swipe,
        TelegraphMaul,
        Maul,
        TelegraphCleave,
        Cleave
    }
}
