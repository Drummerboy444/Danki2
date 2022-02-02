﻿using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.VFX;

public class SwordEmpowermentVisual : MonoBehaviour
{
    [SerializeField] private DecalProjector decal = null;

    [SerializeField] private DecalProjector reverseDecal = null;

    [SerializeField] private TrailRenderer trail = null;

    [SerializeField] private VisualEffect vfx = null;

    [SerializeField, Range(0,1)] private float trailColourDamping = 0.6f;

    [SerializeField] private float vfxEmissiveIntensity = 5f;

    public void Activate(Color colour, Material decalMaterial)
    {
        decal.fadeFactor = 1;
        decal.material = decalMaterial;

        reverseDecal.fadeFactor = 1;
        reverseDecal.material = decalMaterial;

        trail.emitting = true;
        Color trailColour = Color.Lerp(colour, Color.white, trailColourDamping);
        trail.material.SetUnlitColour(trailColour);

        vfx.SetVector4("Colour", colour * vfxEmissiveIntensity);
        vfx.Play();
    }

    public void Reset()
    {
        decal.fadeFactor = 0;
        reverseDecal.fadeFactor = 0;
        trail.emitting = false;
        vfx.Stop();
    }
}
