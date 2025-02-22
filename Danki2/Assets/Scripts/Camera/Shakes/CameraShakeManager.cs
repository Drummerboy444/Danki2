﻿using System.Linq;
using UnityEngine;


public class CameraShakeManager
{
    private CameraShake[] _camShakes;

    public CameraShakeManager(int poolSize)
    {
        _camShakes = new CameraShake[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            _camShakes[i] = new CameraShake();
        }
    }

    public void ApplyShake(Transform transform)
    {
        transform.Translate(GetShakeVector(), Space.World);
        _camShakes.ToList().ForEach(cs => cs.TickDuration());
    }

    public void AddCameraShake(float strength, float duration, float interval = CameraShake.DefaultInterval)
    {
        if (TryGetExpiredCameraShake(out CameraShake cameraShake))
        {
            cameraShake.Set(strength, duration, interval);
        }
    }

    private Vector3 GetShakeVector()
    {
        float maxStrength = float.MinValue;
        CameraShake strongestShake = null;

        foreach (CameraShake cs in _camShakes)
        {
            if (cs.Duration > 0f && cs.Strength > maxStrength)
            {
                maxStrength = cs.Strength;
                strongestShake = cs;
            }
        }

        return strongestShake?.GetShakeVector() ?? Vector3.zero;
    }

    private bool TryGetExpiredCameraShake(out CameraShake expiredShake)
    {
        foreach (CameraShake cs in _camShakes)
        {
            if (cs.Duration <= 0f)
            {
                expiredShake = cs;
                return true;
            }
        }

        expiredShake = null;
        return false;
    }
}
