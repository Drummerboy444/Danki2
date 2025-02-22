﻿using System;
using UnityEngine;

[Serializable]
public class SerializableType : ISerializationCallbackReceiver
{
    [SerializeField]
    private string _serializedType;

    public Type Type { get; private set; }

    public SerializableType(Type type)
    {
        Type = type;
    }

    public void OnBeforeSerialize()
    {
        _serializedType = Type.AssemblyQualifiedName;
    }

    public void OnAfterDeserialize()
    {
        Type = Type.GetType(_serializedType);
    }
}
