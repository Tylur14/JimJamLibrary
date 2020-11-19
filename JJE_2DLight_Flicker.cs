using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using Random = UnityEngine.Random;

/// <summary>
/// The Jim Jam Effects Library
/// WIP 2D Light Flicker Effect
/// Make a 2d light flicker to create atomospheric areas like a campfire!
///
/// Features:
/// |- Instant intensity change, WIP, looping
///
/// Note: Requires that your project is using 2D Lights, I think currently it's only working for Universal 2D? Should work the same for LWRP but with different refs
/// </summary>

public class JJE_2DLight_Flicker : MonoBehaviour
{
    [SerializeField] private Light2D targetLight;
    [SerializeField] private Vector2 flickerRateRange;
    [SerializeField] private Vector2 flickerIntensityRange;

    private void Awake()
    {
        targetLight = GetComponent<Light2D>();
        StartCoroutine(DoFlicker());
    }

    IEnumerator DoFlicker()
    {
        float waitTime = Random.Range(flickerRateRange.x, flickerRateRange.y);
        yield return new WaitForSeconds(waitTime);
        float flicker = Random.Range(flickerIntensityRange.x, flickerIntensityRange.y);
        targetLight.intensity = flicker;
        StartCoroutine(DoFlicker());
    }
}
