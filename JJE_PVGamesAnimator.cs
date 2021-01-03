using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Jim Jam Effects - PVGames Animator
/// Get the Diablo aesthetic with this PVGames compass animator!
/// Gives a movable and easily controllable billboard entity that can either be a single sprite or animated.
/// Recommended to use with PVGames assets... because that's specifically what this is made for
/// https://pvgames.itch.io/ -- PVGames Itch Page
/// 
/// Features:
/// |- Create 8-directional animations
/// |- Quickly use PVGames Sprites in Unity without any extra sorting or configuration
/// </summary>
public class JJE_PVGamesAnimator : MonoBehaviour
{
    [SerializeField] private string animationName;
    [Header("Animation Settings")]
    
    [SerializeField] private float frameRate = 0.15f;
    [SerializeField] private Sprite[] unsortedFrames;

    [Header("Animation Status")]
    [SerializeField] private float degrees;
    [SerializeField] private int frameIndex;
    [SerializeField] int directionalOffset = 0;   // S,SE,E,NE,N,NW,W,SW -- Current facing direction
    
    [Header("Facing Direction")]
    public Vector2 facingDir;

    // PVGames Variables -- may move to separate class, could also be funneled into a single variable
    /*[SerializeField]*/ private Sprite[] frames;
    /*[SerializeField]*/ private Sprite[] _framesSouth;
    /*[SerializeField]*/ private Sprite[] _framesSouthWest;
    /*[SerializeField]*/ private Sprite[] _framesWest;
    /*[SerializeField]*/ private Sprite[] _framesNorthWest;
    /*[SerializeField]*/ private Sprite[] _framesNorth;
    /*[SerializeField]*/ private Sprite[] _framesNorthEast;
    /*[SerializeField]*/ private Sprite[] _framesEast;
    /*[SerializeField]*/ private Sprite[] _framesSouthEast;
    
    private float _timer;
    private Transform _player;
    private SpriteRenderer _spriteRenderer;
    
    private void Awake()
    {
        SortPVGamesSprites();
        
        // Assumption - there is a object tagged "Player" in the scene
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // Requirement - there should be a sprite renderer on this object
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
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
            if (facingDir == new Vector2(1, -1))       // South-East
                directionalOffset = 7;
            else if (facingDir == new Vector2(1, 0))   // East
                directionalOffset = 6;
            else if (facingDir == new Vector2(1, 1))    // North-East
                directionalOffset = 5;
            else if (facingDir == new Vector2(0, 1))    // North
                directionalOffset = 4;
            else if (facingDir == new Vector2(-1, 1))  // North-West
                directionalOffset = 3;
            else if (facingDir == new Vector2(-1, 0))   // West
                directionalOffset = 2;
            else if (facingDir == new Vector2(-1, -1))   // South-West
                directionalOffset = 1;
            else if (facingDir == new Vector2(0, -1))    // South
                directionalOffset = 0;
    }

    // Used with PVGames Assets to sort them into a usable order
    void SortPVGamesSprites()
    {
        frames = new Sprite[unsortedFrames.Length];
        int splitIndex = unsortedFrames.Length / 8;

        for(int i = 0; i < unsortedFrames.Length; i+=splitIndex)
        {
            var buffer = new Sprite[splitIndex];
            Array.Copy(unsortedFrames, i, buffer, 0, splitIndex);
            // process array
            switch (i/splitIndex) // *Direction* - *Order*
            {
                case 0: // South - 1
                    _framesSouth = buffer;
                    break;
                case 1: // West - 7
                    _framesWest = buffer;
                    break;
                case 2: // East - 3
                    _framesEast = buffer;
                    break;
                case 3: // North - 5
                    _framesNorth = buffer;
                    break;
                case 4: // South East - 8
                    _framesSouthEast = buffer;
                    break;
                case 5: // North East - 6
                    _framesNorthEast = buffer;
                    break;
                case 6: // South West - 2
                    _framesSouthWest = buffer;
                    break;
                case 7: // North West - 4
                    _framesNorthWest = buffer;
                    break;
                    
            }
        } 
        
        Sprite[] newArray0 = CombineSpriteArray(_framesSouth, _framesSouthWest); // 1 & 2
        Sprite[] newArray1 = CombineSpriteArray(_framesEast, _framesNorthWest); // 3 & 4
        Sprite[] newArray2 = CombineSpriteArray(_framesNorth, _framesNorthEast); // 5 & 6
        Sprite[] newArray3 = CombineSpriteArray(_framesWest, _framesSouthEast); // 7 & 8
        Sprite[] newArray4 = CombineSpriteArray(newArray0, newArray1); // (1+2) & (3&4)
        Sprite[] newArray5 = CombineSpriteArray(newArray2, newArray3); // (5+6) & (7+8)
        Sprite[] mergedSemiSortedArray = CombineSpriteArray(newArray4, newArray5); // ((1+2)+(3+4)) & ((5+6)+(7+8)) -- now sorted into correct directions

        // Sort into CompassAnimator compatible array
        int split = 0;
        int dir = 0;
        for (int j = 0; j < mergedSemiSortedArray.Length; j ++)
        {
            if (split >= splitIndex)
            {
                split = 0;
                dir++;
            }   
            if (dir >= 8)
                dir = 0;
            int accessor = dir + (split * 8);
            frames[accessor] = mergedSemiSortedArray[j];
            
            // DEBUG
            //print("Frame: " + j + ", Direction: " + dir +", Split: " + split + ", Accessed Item: " +(dir + (split * 8)));
            split++;
        }
    }

    // Take two arrays and merge them -- currently only good for sprite arrays
    private Sprite[] CombineSpriteArray(Sprite[] a1, Sprite[] a2)
    {
        Sprite[] newArray = new Sprite[a1.Length + a2.Length];
        Array.Copy(a1, newArray, a1.Length);
        Array.Copy(a2, 0, newArray, a1.Length, a2.Length);
        return newArray;
    }
}

