﻿using FMODUnity;
using UnityEngine;

public class CustomCamera : Singleton<CustomCamera>
{
    [Header("Camera settings")]

    [SerializeField]
    private new Camera camera = null; // new is needed because camera is hiding the obselete camera property

    [SerializeField]
    private StudioListener listener = null;

    [SerializeField, Range(0, 50)] private float height = 10;
    [SerializeField, Range(10, 80)] private float angle = 40;
    [SerializeField, Range(0f, 1f)] private float mouseFollowFactor = 0f;
    [SerializeField, Range(0f, 1f)] private float smoothFactor = 0.1f;

    [Header("Shake levels")]
    [SerializeField, Range(0, 50)] private float smallShakeStrength = 10;
    [SerializeField, Range(0, 1)] private float smallShakeDuration = 10;
    [SerializeField, Range(0, 50)] private float mediumShakeStrength = 10;
    [SerializeField, Range(0, 1)] private float mediumShakeDuration = 10;
    [SerializeField, Range(0, 50)] private float bigShakeStrength = 10;
    [SerializeField, Range(0, 1)] private float bigShakeDuration = 10;

    private GameObject target;

    private Vector3 desiredPosition = Vector3.zero;
    private Quaternion desiredRotation = Quaternion.identity;
    private Quaternion defaultRotation;

    private readonly CameraShakeManager shakeManager = new CameraShakeManager(4);

    private Pole orientation;

    private Transform transformOverride = null;
    private float? smoothFactorOverride = null;

    private float HorizontalMouseOffset => Input.mousePosition.x / Screen.width - 0.5f;
    private float VerticalMouseOffset => Input.mousePosition.y / Screen.height - 0.5f;

    private float SmoothFactor => smoothFactorOverride ?? smoothFactor;

    protected override void Awake()
    {
        base.Awake();
        orientation = PersistenceManager.Instance.SaveData.CurrentRoomNode.CameraOrientation;
    }

    private void Start()
    {
        target = ActorCache.Instance.Player.gameObject;

        FollowTarget(true);
        defaultRotation = Quaternion.Euler(new Vector3(angle, OrientationUtils.GetYRotation(orientation), 0));
        gameObject.transform.rotation = defaultRotation;

        listener.attenuationObject = target;
    }

    private void Update()
    {
        FollowTarget(false);
        shakeManager.ApplyShake(transform);
    }

    public void AddShake(ShakeIntensity intensity)
    {
        switch (intensity)
        {
            case ShakeIntensity.Low:
                shakeManager.AddCameraShake(smallShakeStrength, smallShakeDuration);
                break;
            case ShakeIntensity.Medium:
                shakeManager.AddCameraShake(mediumShakeStrength, mediumShakeDuration);
                break;
            case ShakeIntensity.High:
                shakeManager.AddCameraShake(bigShakeStrength, bigShakeDuration);
                break;
        }
    }

    public void OverrideDesiredTransform(Transform transformOverride, float? smoothFactorOverride = null)
    {
        this.transformOverride = transformOverride;
        this.smoothFactorOverride = smoothFactorOverride;
    }

    public void RemoveTransformOverride()
    {
        transformOverride = null;
        smoothFactorOverride = null;
    }

    public bool PointInViewport(Vector3 point)
    {
        Vector3 viewPortPoint = camera.WorldToViewportPoint(point);

        return viewPortPoint.x > 0
            && viewPortPoint.x < 1
            && viewPortPoint.y > 0
            && viewPortPoint.y < 1
            && viewPortPoint.z > 0;
    }

    public Vector3 WorldToViewportPoint(Vector3 point) => camera.WorldToViewportPoint(point);

    private void FollowTarget(bool snap)
    {
        SetDesiredPosition();
        SetDesiredRotation();

        if (snap)
        {
            transform.position = desiredPosition;
            transform.rotation = desiredRotation;
            return;
        }

        float lerpAmount = MathUtils.GetDeltaTimeLerpAmount(SmoothFactor);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, lerpAmount);
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, lerpAmount);
    }

    private void SetDesiredPosition()
    {
        if (transformOverride)
        {
            desiredPosition = transformOverride.position;
            return;
        }

        float distanceFromFloorIntersect = height / (Mathf.Tan(Mathf.Deg2Rad * angle));
        Vector3 targetPosition = target.transform.position;

        float xOffset = 0;
        float zOffset = 0;
        float mouseXOffset = 0;
        float mouseZOffset = 0;

        switch (orientation)
        {
            case Pole.North:
                xOffset = 0;
                zOffset = -distanceFromFloorIntersect;
                mouseXOffset = HorizontalMouseOffset;
                mouseZOffset = VerticalMouseOffset;
                break;
            case Pole.East:
                xOffset = -distanceFromFloorIntersect;
                zOffset = 0;
                mouseXOffset = VerticalMouseOffset;
                mouseZOffset = -HorizontalMouseOffset;
                break;
            case Pole.South:
                xOffset = 0;
                zOffset = distanceFromFloorIntersect;
                mouseXOffset = -HorizontalMouseOffset;
                mouseZOffset = -VerticalMouseOffset;
                break;
            case Pole.West:
                xOffset = distanceFromFloorIntersect;
                zOffset = 0;
                mouseXOffset = -VerticalMouseOffset;
                mouseZOffset = HorizontalMouseOffset;
                break;
        }

        desiredPosition.Set(
            targetPosition.x + xOffset + (mouseXOffset * mouseFollowFactor),
            targetPosition.y + height,
            targetPosition.z +zOffset + (mouseZOffset * mouseFollowFactor)
        );
    }

    private void SetDesiredRotation()
    {
        desiredRotation = transformOverride
            ? transformOverride.rotation
            : defaultRotation;
    }
}
