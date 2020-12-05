using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
    public enum FlickerEffects { SnapFlicker, PulseFlicker}

    [Header("General Settings")]
    [SerializeField] private FlickerEffects flickerType;
    [SerializeField] private float duration = 0.15f;
    
    [Header("Snap Settings")]
    [Range(0.05f,5f)]
    [SerializeField] private float snapSpeed = 1f;
    [Range(0.05f,5f)]
    [SerializeField] private float retractSpeed = 1f;
    [SerializeField] private float snapFlicker;
    
    [Header("Pulse Settings")]
    [Range(0.05f,20f)]
    [SerializeField] private float pulseSpeed;
    [SerializeField] private Vector2 flickerRateRange;
    [SerializeField] private Vector2 flickerIntensityRange;

    [Header("Events")]
    [SerializeField] private UnityEvent snapEvents;
    //[SerializeField] private UnityEvent retractEvents;
    
    [Header("Debugging")]
    [SerializeField] private float elapsedTime;
    [SerializeField] private float ratio;
    
    private Light2D _targetLight;
    private void Awake()
    {
        _targetLight = GetComponent<Light2D>();
        if(flickerType == FlickerEffects.PulseFlicker)
            StartCoroutine(FlickerLoop());
        else if(flickerType == FlickerEffects.SnapFlicker)
            StartCoroutine(SnapLoop());
    }

    private IEnumerator FlickerLoop()
    {
        var flicker = Random.Range(flickerIntensityRange.x, flickerIntensityRange.y);

        float e = 0;
        float r = e / duration;
        while (r < 1f)
        {
            elapsedTime = e;
            ratio = r;
            e += Time.deltaTime;
            r = e / duration;
            _targetLight.intensity = Mathf.Lerp(_targetLight.intensity, flicker, ratio * Time.deltaTime * pulseSpeed);
            yield return null;
        }

        StartCoroutine(FlickerLoop());
    }

    private IEnumerator SnapLoop()
    {
        snapFlicker *= -1;
        var flicker = snapFlicker;
        if (flicker < 0)
            flicker = flickerIntensityRange.x;
        
        float e = 0;
        float r = e / duration;
        while(r < 1f)
        {
            elapsedTime = e;
            ratio = r;
            e += Time.deltaTime;
            r = e / duration;     
            _targetLight.intensity = Mathf.Lerp(_targetLight.intensity,flicker,ratio * Time.deltaTime * snapSpeed);
            yield return null;
        }
        snapEvents.Invoke();
        bool lightCheck = _targetLight.intensity > flickerIntensityRange.x; 
        while (lightCheck)
        {
            _targetLight.intensity = Mathf.Lerp(_targetLight.intensity,flickerIntensityRange.x,Time.deltaTime * retractSpeed);
            if (_targetLight.intensity - 0.05f <= flickerIntensityRange.x)
                lightCheck = false;
            yield return null;
        }
        float waitTime = Random.Range(flickerRateRange.x, flickerRateRange.y);
        yield return new WaitForSeconds(waitTime);
        StartCoroutine(SnapLoop());
    }
}
