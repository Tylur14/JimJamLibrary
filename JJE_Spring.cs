using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// The Jim Jam Effects Library
/// Spring Effect
/// Move, scale, and rotate (kinda) with a springy twang!
///
/// Features:
/// |- Smooth performant transform manipulations
/// |- One shot
/// </summary>

public class JJE_Spring : MonoBehaviour
{
    enum SpringOptions // Determine single transform Vector to affect
    {
        Mover,
        Scalar,
        Rotator_WIP
    }
    
    enum TransformType // By default it's used on transform, but GUI affects rectTransform
    {
        WorldSpace,
        GUI
    }

    [Header("Spring Settings")]
    [SerializeField] private SpringOptions effectType = SpringOptions.Mover;
    [SerializeField] private TransformType transformType = TransformType.WorldSpace;
    [Range(0.1f,1.0f)]
    [SerializeField] float springAmount = 0.15f;
    [Range(0.1f,30.0f)]
    [SerializeField] float springSpeed = 10.0f;
    
    [Header("Effect Settings")]
    public Vector3 startValue;
    public Vector3 endValue;
    public bool isLocal;
    
    [Header("Interaction Settings")]
    [Range(0.1f,3.0f)]
    [SerializeField] float intervalTime = 1f;
    
    // Private Vector variables
    private Vector3 _target;
    private Vector3 _value;
    private Vector3 _valueDir;
    

    private void Awake()
    {
        _valueDir = isLocal ? transform.localPosition : transform.position;
        //_target = endValue;
    }

    #if UNITY_EDITOR
    private void Update()
    {
        // for testing purposes only!
        if(Input.GetKeyDown(KeyCode.Alpha1))
            StartLoop();
        if(Input.GetKeyDown(KeyCode.Alpha2))
            OneShotToStart();
        if(Input.GetKeyDown(KeyCode.Alpha3))
            OneShotToEnd();
        if(Input.GetKeyDown(KeyCode.Alpha0))
            StopSpring();
    }
    #endif
    
    public void StartLoop()
    {
        StopAllCoroutines();
        StartCoroutine(DoLoop());
        IEnumerator DoLoop()
        {
            float springTimer = intervalTime;
            while (springTimer > 0)
            {
                //https://answers.unity.com/questions/1111308/unity-coroutine-movement-over-time-is-not-consiste.html
                // ^- Not 100% sure what it does but it makes it work
                float elapsedTime = 0;
                float ratio = elapsedTime / intervalTime;
                while(ratio < 1f)
                {
                    elapsedTime += Time.deltaTime;
                    ratio = elapsedTime / intervalTime;
                    Spring();
                    springTimer -= Time.deltaTime;
                    yield return null;
                }
            }
            _target = _target == startValue ? endValue : startValue;
            StartLoop();
        }
    }

    public void OneShotToStart()
    {
        StopAllCoroutines();
        _target = startValue;
        StartCoroutine(SpringOnce());
    }
    
    public void OneShotToEnd()
    {
        StopAllCoroutines();
        _target = endValue;
        StartCoroutine(SpringOnce());
    }

    public void StopSpring()
    {
        StopAllCoroutines();
        StartCoroutine(SpringOnce());
    }

    IEnumerator SpringOnce()
    {
        int i = Mathf.RoundToInt(intervalTime);
        while (i > 0)
        {
            float elapsedTime = 0;
            float ratio = elapsedTime / 1;
            while(ratio < 1f)
            {
                elapsedTime += Time.deltaTime;
                ratio = elapsedTime / 1;
                Spring();
                
                print("doing one shot");
                yield return null;
            } 
            i--;
        }
    }

    void Spring()
    {
        _value = Vector3.Lerp(_value, (_target - _valueDir) * springAmount, springSpeed * Time.deltaTime);
        _valueDir += _value;
        
        switch (effectType)
        {
            case SpringOptions.Mover:
                ApplyMove(_valueDir);
                break;
            case SpringOptions.Scalar:
                ApplyScale(_valueDir);
                break;
            case SpringOptions.Rotator_WIP:
                ApplyRotate(_valueDir);
                break;
        }
    }

    void ApplyScale(Vector3 incValue)
    {
        switch (transformType)
        {
            case TransformType.WorldSpace:
                this.transform.localScale = incValue;
                break;
            case TransformType.GUI:
                RectTransform rt = GetComponent<RectTransform>();
                rt.sizeDelta = incValue;
                break;
        }
        
    }

    void ApplyMove(Vector3 incValue)
    {
        switch (transformType)
        {
            case TransformType.WorldSpace:
                if (!isLocal)
                    this.transform.position = incValue;
                else this.transform.localPosition = incValue;
                break;
            case TransformType.GUI:
                RectTransform rt = GetComponent<RectTransform>();
                rt.anchoredPosition = incValue;
                break;
        }
        
    }

    // Need to find smoother solution
    void ApplyRotate(Vector3 incValue)
    {
        switch (transformType)
        {
            case TransformType.WorldSpace:
                this.transform.rotation = Quaternion.Euler(incValue);
                break;
            case TransformType.GUI:
                RectTransform rt = GetComponent<RectTransform>();
                rt.rotation = Quaternion.Euler(incValue);
                break;
        }
        
    }
    
    // Possible functions for interacting with the spring
        // One shot -- go to specific state and stay there
        // There and back -- go to specific state and return after
        // Loop -- go to specific state, return after, repeat
        // Loop for -- go to specific state, return after, repeat until looped x amount of times
}
