﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 
/// </summary>
public class JJU_PVGamesAnimController : MonoBehaviour
{
    // Animation state -- which animation sheet should currently be displayed
    [Header("Animation State")]
    [Range(0,3)]
    [SerializeField] private int animationState;
    private int _stateIndex;
    
    // Animation Sheets (Preferable to only use one or the other)
    
    [Header("PVGames JimJam Compass Animator Sheets")]
    [Tooltip("Used with the JJE_PVGamesAnimator")]
    [SerializeField] private JJE_PVGamesAnimator[] PVGamesAnimationSheets; //0:Dead, 1:IDLE, 2:MOVE, 3:ATTACK
    
    // Target settings, what the animated object will be looking at
    [Header("Target Settings")]
    [SerializeField] private bool autoTarget;
    [SerializeField] private Transform target;

    private Vector2 _facingDirection;
    private void Awake()
    {
        if (!target)
        {
            _facingDirection.x = Random.Range(-1,2);
            _facingDirection.y = Random.Range(-1,2);
        }
        SetAnimation(true);
    }

    private void Update()
    {
        if (PVGamesAnimationSheets[_stateIndex].enabled)
        {
            SetAnimation();
            if(autoTarget && target)
                GetFacingDirection(target.position);
        }
        
    }

    void SetAnimation(bool bypass = false)
    {
        if (_stateIndex != animationState || bypass)
        {
            foreach (var sheet in PVGamesAnimationSheets)
            {
                sheet.facingDir = _facingDirection;
                sheet.gameObject.SetActive(false);
            }
            _stateIndex = animationState;
            PVGamesAnimationSheets[_stateIndex].gameObject.SetActive(true);
        }
    }
    
    void GetFacingDirection(Vector3 tar)
    {
        var dir = (tar - transform.position).normalized;
        
        dir.x = GetMixRoundedFloat(dir.x)*-1;
        dir.z = GetMixRoundedFloat(dir.z); 
        
        _facingDirection.x = dir.x;
        _facingDirection.y = dir.z;
        PVGamesAnimationSheets[_stateIndex].facingDir = _facingDirection;
    }

    int GetMixRoundedFloat(float i)
    {
        if (i > -0.45f && i < 0.45f)
            return 0;
        else if (i >= 0.46f)
            return 1;
        else if (i <= -0.46f)
            return -1;
        else return 0;
    }
}
