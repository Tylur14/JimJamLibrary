using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// MAJOR BUG FOUND: (FIXED) - Timing in editor is out of sync with build
///     |- There is still a serious timing issue between in-editor & in-build;
///     |- From last test, in-editor was quick and snappy but in-build was slow and annoying.
///     |- SOLUTION: it appears that Vector3.SmoothDamp was the main issue. Replaced with Lerp.
/// </summary>

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

    enum StartingState
    {
        None,
        Looping,
        ToStart,
        ToEnd
    }
    
    [Header("Spring Settings")]
    [SerializeField] private SpringOptions effectType = SpringOptions.Mover;
    [SerializeField] private TransformType transformType = TransformType.WorldSpace;
    [SerializeField] private StartingState startState = StartingState.None;
    [Range(0.1f,400.0f)]
    [SerializeField] float speed = 10.0f;
    
    [Header("Effect Settings")]
    public Vector3 startValue;
    public Vector3 endValue;
    public bool isLocal;
    
    [Header("Interaction Settings")]
    [Range(0.1f,15.0f)]
    [SerializeField] float intervalTime = 1f;
    
    // Private Vector variables
    private Vector3 _target;
    private Vector3 _valueDir;
    private Vector3 _vel = Vector3.zero;

    private void Awake()
    {
        _valueDir = startValue;
        _target = endValue;

        switch (startState)
        {
            case StartingState.None:
                Stop();
                break;
            case StartingState.Looping:
                StartMoveLoop();
                break;
            case StartingState.ToStart:
                OneShotToStart();
                break;
            case StartingState.ToEnd:
                OneShotToEnd();
                break;
        }
    }

    public void StartMoveLoop()
    {
        //StopAllCoroutines();
        StartCoroutine(DoLoop());
        IEnumerator DoLoop()
        {
            //https://answers.unity.com/questions/1111308/unity-coroutine-movement-over-time-is-not-consiste.html
            float elapsedTime = 0;
            float ratio = elapsedTime / intervalTime;
            while(ratio < 1f)
            {
                elapsedTime += Time.deltaTime;
                ratio = elapsedTime / intervalTime;
                Move();
                yield return null;
            }
            _target = _target == startValue ? endValue : startValue;
            StartMoveLoop();
        }
    }

    public void OneShotToOpposite()
    {
        StopAllCoroutines();
        _target = _target == startValue ? endValue : startValue;
        StartCoroutine(ActivateOnce());
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
            while(ratio < intervalTime)
            {
                elapsedTime += Time.deltaTime;
                ratio = elapsedTime / 1;
                Move();
                yield return null;
            } 
            i--;
        }
    }

    void Move()
    {
        //_valueDir = Vector3.SmoothDamp(_valueDir,_target,ref _vel, speed * Time.deltaTime);
        _valueDir = Vector3.Lerp(_valueDir,_target, speed * Time.deltaTime);
        
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
#if UNITY_EDITOR
[CustomEditor(typeof(JJE_SmoothMove))]
public class SME : Editor
{
    public override void OnInspectorGUI()
    {
        
        JJE_SmoothMove instance = (JJE_SmoothMove)target;
        DrawDefaultInspector();
        
        //    ===========================================
        
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Loop",GUILayout.Width(60),GUILayout.Height(30)))
        {
            instance.StartMoveLoop();
        }
        if (GUILayout.Button("ToStart",GUILayout.Width(60),GUILayout.Height(30)))
        {
            instance.OneShotToStart();
        }
        if (GUILayout.Button("ToEnd",GUILayout.Width(60),GUILayout.Height(30)))
        {
            instance.OneShotToEnd();
        }
        if (GUILayout.Button("Stop",GUILayout.Width(60),GUILayout.Height(30)))
        {
            instance.Stop();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
    
}
#endif