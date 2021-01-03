using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Jim Jam Effects - Compass Animator
/// Get the OG DOOM aesthetic with this compass animator!
/// Gives a movable and easily controllable billboard entity that can either be a single sprite or animated.
/// Recommended to use with either the GreenScreen sprite creation tool.
/// https://github.com/Tylur14/JimJamLibrary -- JimJam Library, has GreenScreen included
/// 
/// Features:
/// |- Create 8-directional animations
/// </summary>
public class JJE_CompassAnimator : MonoBehaviour
{
    [SerializeField] private string animationName;
    [Header("Animation Settings")]
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameRate = 0.15f;

    [Header("Animation Status")]
    [SerializeField] private float degrees;
    [SerializeField] private int frameIndex;
    [SerializeField] int directionalOffset = 0;   // S,SE,E,NE,N,NW,W,SW -- Current facing direction
    
    [Header("Facing Direction")]
    public Vector2 facingDir;
    
    private float _timer;
    private Transform _player;
    private SpriteRenderer _spriteRenderer;
    
    private void Awake()
    {
        // Assumption - there is a object tagged "Player" in the scene
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // Requirement - there should be a sprite renderer on this object
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        transform.LookAt(_player);
        
        FindFacingDirection();
        _spriteRenderer.sprite = frames[(frameIndex * 8) + directionalOffset];
        
        if (_timer > 0)
            _timer -= Time.deltaTime;
        else Animate();
    }
    
    void Animate()
    {
        _timer = frameRate;
        frameIndex++;
        if (frameIndex > frames.Length / 8 - 1)
            frameIndex = 0;
    }
    
    void FindFacingDirection()
    {
        // Gets a vector that points from the player's position to the target's.
        var heading = transform.position - _player.transform.position;
        degrees = ((Mathf.Atan2(heading.z, heading.x) / Mathf.PI) * 180f);
        if (degrees < 0) 
            degrees += 360f;

        FindOffset();
    }
    void FindOffset()
    {
        // Gets current facing direction offset
        if (degrees >= 22.5f && degrees < 67.5f)        // South-East
            directionalOffset = 1;
        else if (degrees >= 67.5f && degrees < 112.5f)  // East
            directionalOffset = 2;
        else if (degrees >= 112.5f && degrees < 157.5f) // North-East
            directionalOffset = 3;
        else if (degrees >= 157.5f && degrees < 202.5f) // North
            directionalOffset = 4;
        else if (degrees >= 202.5f && degrees < 247.5f) // North-West
            directionalOffset = 5;
        else if (degrees >= 247.5f && degrees < 292.5f) // West
            directionalOffset = 6;
        else if (degrees >= 292.5f && degrees < 337.5f) // South-West
            directionalOffset = 7;
        else directionalOffset = 0;                     // South
            
        if (facingDir == new Vector2(1, -1))       // South-East
            directionalOffset += 7;
        else if (facingDir == new Vector2(1, 0))   // East
            directionalOffset += 6;
        else if (facingDir == new Vector2(1, 1))    // North-East
            directionalOffset += 5;
        else if (facingDir == new Vector2(0, 1))    // North
            directionalOffset += 4;
        else if (facingDir == new Vector2(-1, 1))  // North-West
            directionalOffset += 3;
        else if (facingDir == new Vector2(-1, 0))   // West
            directionalOffset += 2;
        else if (facingDir == new Vector2(-1, -1))   // South-West
            directionalOffset += 1;
        else if (facingDir == new Vector2(0, -1))    // South
            directionalOffset += 0;

        if (directionalOffset > 7) // replace 7 with direction
        {
            int i = directionalOffset - 7;
            directionalOffset = -1;
            directionalOffset += i;
        }
    }

}

