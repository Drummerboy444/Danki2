﻿using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }
    public static bool Exists => Instance != null;

    protected virtual bool DestroyOnLoad => true;

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            if (!DestroyOnLoad) DontDestroyOnLoad();
        }
        else
        {
            Debug.LogError($"Tried to instantiate more than one instance of the singleton: {typeof(T).Name}");
            Destroy(gameObject);
        }
    }

    public void DontDestroyOnLoad()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    public void Destroy()
    {
        Instance = null;
        Destroy(gameObject);
    }
}
