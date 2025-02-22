﻿using System.Collections.Generic;
using UnityEngine;

public abstract class Emit : MonoBehaviour
{
    [SerializeField] private List<MeshRenderer> meshes = null;

    [SerializeField] private float intensity = 0;

    [SerializeField] private bool updateBaseColour = false;

    [SerializeField, Tooltip("Not required")] private Actor actor = null;

    protected abstract Color Colour { get; }

    private void Start()
    {
        Color colour = Colour * Mathf.Pow(2, intensity);

        if (actor == null)
        {
            meshes.ForEach(m => m.material.SetEmissiveColour(colour));
        }
        else
        {
            actor.EmissiveManager.SetBaseEmissiveColour(meshes, colour);
        }

        if (updateBaseColour)
        {
            meshes.ForEach(m => m.material.SetColour(VisualSettings.Instance.EnergyColour));
        }
    }
}
