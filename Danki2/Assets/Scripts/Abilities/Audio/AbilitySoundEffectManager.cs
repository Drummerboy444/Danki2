﻿using FMOD.Studio;
using UnityEngine;

public class AbilitySoundEffectManager : MonoBehaviour
{
    [SerializeField]
    private Player player = null;

    private void Start()
    {
        player.AbilityAnimationListener.StartSubject
            .Subscribe(PlayStartEvent);

        player.AbilityAnimationListener.SwingSubject
            .Subscribe(PlaySwingEvent);

        player.AbilityAnimationListener.ImpactSubject
            .Subscribe(PlayImpactEvent);
    }

    private void PlayStartEvent()
    {
        string startEvent = GetAbilityFmodEvents().FmodStartEventRef;
        PlayAbilityEvent(startEvent);
    }

    private void PlaySwingEvent()
    {
        string swingEvent = GetAbilityFmodEvents().FmodSwingEventRef;
        PlayAbilityEvent(swingEvent);
    }

    private void PlayImpactEvent()
    {
        string impactEvent = GetAbilityFmodEvents().FmodImpactEventRef;
        PlayAbilityEvent(impactEvent);
    }

    private AbilityFmodEvents GetAbilityFmodEvents() =>
        AbilityTypeLookup.Instance.GetAbilityFmodEvents(player.CurrentCast.Ability.Type);

    private void PlayAbilityEvent(string fmodEvent)
    {
        if (string.IsNullOrEmpty(fmodEvent)) return;

        EventInstance eventInstance = FmodUtils.CreatePositionedInstance(fmodEvent, player.AbilitySource);
        eventInstance.start();
        eventInstance.release();
    }
}