﻿using System;
using UnityEngine;

[Serializable]
public class CameraTransformLookup : SerializableEnumDictionary<Pole, Transform>
{
    public CameraTransformLookup(Transform defaultValue) : base(defaultValue) {}
    public CameraTransformLookup(Func<Transform> defaultValueProvider) : base(defaultValueProvider) {}
}

[Serializable]
public class PoleColorLookup : SerializableEnumDictionary<Pole, Color>
{
    public PoleColorLookup(Color defaultValue) : base(defaultValue) {}
    public PoleColorLookup(Func<Color> defaultValueProvider) : base(defaultValueProvider) {}
}

public class CameraVolume : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer meshRenderer = null;
    public MeshRenderer MeshRenderer { get => meshRenderer; set => meshRenderer = value; }

    [SerializeField]
    private MeshCollider meshCollider = null;
    public MeshCollider MeshCollider { get => meshCollider; set => meshCollider = value; }

    [SerializeField]
    private CameraTransformLookup cameraTransformLookup = new CameraTransformLookup(defaultValue: null);
    public CameraTransformLookup CameraTransformLookup => cameraTransformLookup;

    [SerializeField]
    private PoleColorLookup poleColorLookup = new PoleColorLookup(Color.white);
    public PoleColorLookup PoleColorLookup => poleColorLookup;

    [SerializeField]
    private bool overrideSmoothFactor = false;
    public bool OverrideSmoothFactor { get => overrideSmoothFactor; set => overrideSmoothFactor = value; }

    [SerializeField]
    private float smoothFactorOverride = 0;
    public float SmoothFactorOverride { get => smoothFactorOverride; set => smoothFactorOverride = value; }

    private Pole cameraOrientation;
    private Subscription enemiesClearedSubscription;

    private void OnDrawGizmos()
    {
        EnumUtils.ForEach<Pole>(p => GizmoUtils.DrawArrow(
            cameraTransformLookup[p].position,
            cameraTransformLookup[p].forward,
            poleColorLookup[p]
        ));
    }

    private void Start()
    {
        meshRenderer.enabled = false;

        cameraOrientation = PersistenceManager.Instance.SaveData.CurrentRoomNode.CameraOrientation;

        if (CombatRoomManager.Instance.InCombatRoom && !CombatRoomManager.Instance.EnemiesCleared)
        {
            meshCollider.enabled = false;
            enemiesClearedSubscription = CombatRoomManager.Instance.EnemiesClearedSubject.Subscribe(() => meshCollider.enabled = true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;

        CustomCamera.Instance.OverrideDesiredTransform(
            cameraTransformLookup[cameraOrientation],
            overrideSmoothFactor ? smoothFactorOverride : (float?) null
        );
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;

        CustomCamera.Instance.RemoveTransformOverride();
    }

    private void OnDestroy()
    {
        enemiesClearedSubscription?.Unsubscribe();
    }

    private bool IsPlayer(Collider other) => other.CompareTag(Tag.Player);
}
