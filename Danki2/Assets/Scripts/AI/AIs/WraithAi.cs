﻿using UnityEngine;

public class WraithAi : Ai
{
    [SerializeField] private Wraith wraith = null;

    [Header("General")]
    [SerializeField] private float aggroRange = 0;
    [SerializeField] private float rangedAttackStateRange = 0;
    [SerializeField] private float rangedAttackStateTolerance = 0;
    
    [Header("Ranged Attacks")]
    [SerializeField] private float minRangedAttacksDuration = 0;
    [SerializeField] private float spineDelay = 0;
    [SerializeField] private float guidedOrbDelay = 0;
    
    [Header("Melee Attacks")]
    [SerializeField] private float swipeRotationSmoothingOverride = 9;
    [SerializeField] private float swipeRange = 0;
    [SerializeField] private float maxSwipeAngle = 0;
    [SerializeField] private float swipeDelay = 0;
    [SerializeField] private float engageDistance = 0;
    [SerializeField] private float disengageDistance = 0;
    
    [Header("Blink")]
    [SerializeField] private float forcedBlinkMinTime = 0;
    [SerializeField] private float forcedBlinkMaxTime = 0;
    [SerializeField] private int meleeAttacksBeforeBlinking = 0;
    [SerializeField] private float blinkDelay = 0;
    [SerializeField] private float postBlinkAttackDelay = 0;
    [SerializeField] private float minBlinkDistance = 0;
    [SerializeField] private float maxBlinkDistance = 0;
    
    protected override Actor Actor => wraith;
    
    protected override IStateMachineComponent BuildStateMachineComponent()
    {
        Player player = ActorCache.Instance.Player;

        IStateMachineComponent rangedAttackStateMachine = new StateMachine<RangedAttackState>(RangedAttackState.ChooseAttack)
            .WithComponent(RangedAttackState.TelegraphSpine, new TelegraphAttackAndWatch(wraith, player, Color.green))
            .WithComponent(RangedAttackState.Spine, new WraithCastSpine(wraith, player))
            .WithComponent(RangedAttackState.TelegraphGuidedOrb, new TelegraphAttackAndWatch(wraith, player, Color.blue))
            .WithComponent(RangedAttackState.GuidedOrb, new WraithCastGuidedOrb(wraith, player))
            .WithDecisionState(RangedAttackState.ChooseAttack, new CyclicalDecider<RangedAttackState>(
                RangedAttackState.TelegraphSpine,
                RangedAttackState.TelegraphSpine,
                RangedAttackState.TelegraphGuidedOrb
            ))
            .WithTransition(RangedAttackState.TelegraphSpine, RangedAttackState.Spine, new TimeElapsed(spineDelay))
            .WithTransition(RangedAttackState.Spine, RangedAttackState.ChooseAttack)
            .WithTransition(RangedAttackState.TelegraphGuidedOrb, RangedAttackState.GuidedOrb, new TimeElapsed(guidedOrbDelay))
            .WithTransition(RangedAttackState.GuidedOrb, RangedAttackState.ChooseAttack);

        IStateMachineComponent meleeAttackStateMachine = new StateMachine<MeleeAttackState>(MeleeAttackState.WatchTarget)
            .WithComponent(MeleeAttackState.WatchTarget, new WatchTarget(wraith, player, swipeRotationSmoothingOverride))
            .WithComponent(MeleeAttackState.Telegraph, new TelegraphAttack(wraith, Color.yellow, true))
            .WithComponent(MeleeAttackState.Swipe, new WraithCastSwipe(wraith))
            .WithTransition(MeleeAttackState.WatchTarget, MeleeAttackState.Telegraph, new Facing(wraith.MovementManager, player, maxSwipeAngle))
            .WithTransition(MeleeAttackState.Telegraph, MeleeAttackState.Swipe, new TimeElapsed(swipeDelay))
            .WithTransition(MeleeAttackState.Swipe, MeleeAttackState.WatchTarget);

        IStateMachineComponent blinkStateMachine = new StateMachine<BlinkState>(BlinkState.Telegraph)
            .WithComponent(BlinkState.Telegraph, new WraithTelegraphBlink(wraith))
            .WithComponent(BlinkState.Blink, new WraithCastBlink(wraith, player, minBlinkDistance, maxBlinkDistance))
            .WithComponent(BlinkState.PostBlinkAttack, new WraithCastRapidSpine(wraith, player))
            .WithComponent(BlinkState.PostBlinkPause, new WatchTarget(wraith, player))
            .WithTransition(BlinkState.Telegraph, BlinkState.Blink, new TimeElapsed(blinkDelay)) // TODO: Make blink castable through stun and knockback
            .WithTransition(BlinkState.Blink, BlinkState.PostBlinkAttack)
            .WithTransition(BlinkState.PostBlinkAttack, BlinkState.PostBlinkPause);

        return new StateMachine<State>(State.Idle)
            .WithComponent(State.Advance, new MoveTowards(wraith, player))
            .WithComponent(State.RangedAttacks, rangedAttackStateMachine)
            .WithComponent(State.MeleeEngage, new MoveTowards(wraith, player))
            .WithComponent(State.MeleeAttacks, meleeAttackStateMachine)
            .WithComponent(State.Blink, blinkStateMachine)
            .WithTransition(State.Idle, State.Advance, new DistanceLessThan(wraith, player, aggroRange) | new TakesDamage(wraith))
            .WithTransition(
                State.Advance,
                State.RangedAttacks,
                new DistanceLessThan(wraith, player, rangedAttackStateRange) & new HasLineOfSight(wraith.AbilitySourceTransform, player.CentreTransform)
            )
            .WithTransition(
                State.RangedAttacks,
                State.Advance,
                !new IsTelegraphing(wraith) &
                new TimeElapsed(minRangedAttacksDuration) &
                    (
                        new DistanceGreaterThan(wraith, player, rangedAttackStateRange + rangedAttackStateTolerance) |
                        !new HasLineOfSight(wraith.AbilitySourceTransform, player.CentreTransform)
                    )
            )
            .WithTransition(State.RangedAttacks, State.MeleeEngage, !new IsTelegraphing(wraith) & new TimeElapsed(minRangedAttacksDuration) & new DistanceLessThan(player, wraith, engageDistance))
            .WithTransition(State.MeleeEngage, State.MeleeAttacks, new DistanceLessThan(player, wraith, swipeRange))
            .WithTransition(State.MeleeEngage, State.RangedAttacks, new DistanceGreaterThan(player, wraith, disengageDistance))
            .WithTransition(State.RangedAttacks, State.Blink, !new IsTelegraphing(wraith) & new RandomTimeElapsed(forcedBlinkMinTime, forcedBlinkMaxTime))
            .WithTransition(State.MeleeAttacks, State.Blink, !new IsTelegraphing(wraith) & new SubjectEmittedTimes(wraith.SwipeSubject, meleeAttacksBeforeBlinking))
            .WithTransition(State.Blink, State.Advance, new TimeElapsed(blinkDelay + postBlinkAttackDelay));
    }

    private enum State
    {
        Idle,
        Advance,
        RangedAttacks,
        MeleeAttacks,
        Blink,
        MeleeEngage
    }

    private enum RangedAttackState
    {
        ChooseAttack,
        TelegraphSpine,
        Spine,
        TelegraphGuidedOrb,
        GuidedOrb
    }

    private enum MeleeAttackState
    {
        WatchTarget,
        Telegraph,
        Swipe
    }

    private enum BlinkState
    {
        Telegraph,
        Blink,
        PostBlinkAttack,
        PostBlinkPause
    }
}
