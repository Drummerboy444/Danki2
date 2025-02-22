﻿using System.Collections.Generic;
using UnityEngine;

public class RuneSelectionMenu : MonoBehaviour
{
    [SerializeField] private RunePanel nextRunePanel = null;
    [SerializeField] private Transform currentRunePanels = null;
    [SerializeField] private ClickableRunePanel clickableRunePanelPrefab = null;

    private readonly List<ClickableRunePanel> clickableRunePanels = new List<ClickableRunePanel>();
    private readonly List<Subscription> subscriptions = new List<Subscription>();

    private Rune nextRune;
    
    public Subject SkipClickedSubject { get; } = new Subject();
    public Subject RuneSelectedSubject { get; } = new Subject();

    private void Start()
    {
        nextRune = ActorCache.Instance.Player.RuneManager.GetNextRune();
        nextRunePanel.Initialise(nextRune);
    }

    private void OnEnable()
    {
        RuneRoomManager.Instance.ViewRunes();
        InitialisePanels();
    }

    private void OnDisable()
    {
        clickableRunePanels.ForEach(p => Destroy(p.gameObject));
        clickableRunePanels.Clear();
        subscriptions.ForEach(s => s.Unsubscribe());
        subscriptions.Clear();
    }
    
    public void Skip() => SkipClickedSubject.Next();

    private void InitialisePanels()
    {
        ActorCache.Instance.Player.RuneManager.RuneSockets.ForEach(runeSocket =>
        {
            ClickableRunePanel clickableRunePanel = ClickableRunePanel.Create(
                clickableRunePanelPrefab,
                currentRunePanels,
                runeSocket
            );

            clickableRunePanels.Add(clickableRunePanel);
            subscriptions.Add(clickableRunePanel.OnClickSubject.Subscribe(() =>
            {
                RuneRoomManager.Instance.SelectRune(runeSocket, nextRune);
                RuneSelectedSubject.Next();
            }));
        });
    }
}
