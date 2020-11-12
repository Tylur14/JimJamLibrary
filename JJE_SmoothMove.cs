using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JJE_SmoothMove : MonoBehaviour
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
    [Range(0.1f,30.0f)]
    [SerializeField] float speed = 10.0f;
    
    [Header("Effect Settings")]
    public Vector3 startValue;
    public Vector3 endValue;
    public bool isLocal;
    
    [Header("Interaction Settings")]
    [Range(0.1f,3.0f)]
    [SerializeField] float intervalTime = 1f;
    
    // Private Vector variables
    private Vector3 _target;
    private Vector3 _valueDir;
    private Vector3 _vel = Vector3.zero;

    private void Awake()
    {
        _target = endValue;
    }

    private void Update()
    {
        // for testing purposes only!
        if(Input.GetKeyDown(KeyCode.Alpha1))
            StartMoveLoop();
        if(Input.GetKeyDown(KeyCode.Alpha2))
            OneShotToStart();
        if(Input.GetKeyDown(KeyCode.Alpha3))
            OneShotToEnd();
        if(Input.GetKeyDown(KeyCode.Alpha0))
            Stop();
    }

    public void StartMoveLoop()
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
                    Move();
                    springTimer -= Time.deltaTime;
                    yield return null;
                }
            }
            _target = _target == startValue ? endValue : startValue;
            StartMoveLoop();
        }
    }

    public void OneShotToStart()
    {
        StopAllCoroutines();
        _target = startValue;
        StartCoroutine(ActivateOnce());
    }
    
    public void OneShotToEnd()
    {
        StopAllCoroutines();
        _target = endValue;
        StartCoroutine(ActivateOnce());
    }

    public void Stop()
    {
        StopAllCoroutines();
        StartCoroutine(ActivateOnce());
    }

    IEnumerator ActivateOnce()
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
                Move();
                
                print("doing one shot");
                yield return null;
            } 
            i--;
        }
    }

    void Move()
    {
        _valueDir = Vector3.SmoothDamp(_valueDir,_target,ref _vel, speed * Time.deltaTime);
        
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
}
