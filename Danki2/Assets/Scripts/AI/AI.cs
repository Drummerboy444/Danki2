﻿public abstract class Ai : StateMachineMonoBehaviour
{
    protected abstract Actor Actor { get; }

    protected override void Start()
    {
        base.Start();

        Actor.DeathSubject.Subscribe(_ => OnDeath());
    }

    private void OnDeath()
    {
        StateMachineComponent.Exit();
        enabled = false;
    }
}
