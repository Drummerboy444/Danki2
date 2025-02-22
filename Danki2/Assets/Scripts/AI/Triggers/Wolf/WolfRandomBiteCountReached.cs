﻿using System.Collections.Generic;
using UnityEngine;

public class WolfRandomBiteCountReached : StateMachineTrigger
{
    private readonly Wolf wolf;
    private readonly int minAttacks;
    private readonly int maxAttacks;

    private int attacks;
    private int requiredAttacks;
    private readonly List<Subscription> attackSubscriptions = new List<Subscription>();

    public WolfRandomBiteCountReached(Wolf wolf, int minAttacks, int maxAttacks)
    {
        this.wolf = wolf;
        this.minAttacks = minAttacks;
        this.maxAttacks = maxAttacks;
    }

    public override void Activate()
    {
        attacks = 0;
        requiredAttacks = Random.Range(minAttacks, maxAttacks + 1);
        attackSubscriptions.Add(wolf.OnBite.Subscribe(() => attacks++));
    }

    public override void Deactivate()
    {
        attackSubscriptions.ForEach(s => s.Unsubscribe());
        attackSubscriptions.Clear();
    }

    public override bool Triggers()
    {
        return attacks >= requiredAttacks;
    }
}
