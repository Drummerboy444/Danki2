﻿public class PurgeHandler : StackingEffectHandler
{
    private const int Damage = 50;
    
    public PurgeHandler(Actor actor, EffectManager effectManager)
        : base(actor, effectManager, StackingEffect.Purge) {}

    protected override void HandleEffectAdded()
    {
        int stacks = effectManager.GetStacks(StackingEffect.Purge);
        int maxStacks = effectManager.GetMaxStacks(StackingEffect.Purge);

        if (stacks >= maxStacks)
        {
            effectManager.RemoveStackingEffect(StackingEffect.Purge);
            actor.HealthManager.ReceiveDamage(Damage, ActorCache.Instance.Player);
        }
    }
}
