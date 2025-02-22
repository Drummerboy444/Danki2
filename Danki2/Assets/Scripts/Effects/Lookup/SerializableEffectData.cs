﻿using System;
using UnityEngine;

[Serializable]
public abstract class SerializableEffectData
{
    [SerializeField] private string displayName = "";
    [SerializeField] private string tooltip = "";
    [SerializeField] private Sprite sprite = null;

    public string DisplayName { get => displayName; set => displayName = value; }
    public string Tooltip { get => tooltip; set => tooltip = value; }
    public Sprite Sprite { get => sprite; set => sprite = value; }
}

[Serializable]
public class SerializableActiveEffectData : SerializableEffectData {}

[Serializable]
public class SerializablePassiveEffectData : SerializableEffectData {}

[Serializable]
public class SerializableStackingEffectData : SerializableEffectData
{
    [SerializeField] private bool hasMaxStackSize = false;
    [SerializeField] private int maxStackSize = 0;
    [SerializeField] private float duration = 0;

    public bool HasMaxStackSize { get => hasMaxStackSize; set => hasMaxStackSize = value; }
    public int MaxStackSize { get => maxStackSize; set => maxStackSize = value; }
    public float Duration { get => duration; set => duration = value; }
}
