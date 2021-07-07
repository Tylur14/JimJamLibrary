using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// JimJam Utility by Tyler Sims --- Twitter @HipToBeeSquare
/// 
/// This is a simple camera system that can work in a multitude of scenarios.
/// It takes a transform to place itself at and follow, and then takes a transform
/// to look at and track.
/// 
/// </summary>
public class JJU_BasicCamera : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] [Range(0.01f,25f)] float smoothing; // How quick the camera will move, the lower the faster
    [SerializeField] Transform moveTarget;  // Where the camera tries to place itself
    [SerializeField] Transform lookTarget;  // Where the camera tries to look at
    private Vector3 vel = Vector3.zero;     // Default value required for the SmoothDamp function in Move()

    private void Update()
    {
        // ~ These are just the default testing keybinds
        // BASIC KEYBINDS, REMOVE AS NEEDED
        if (Input.GetKeyDown(KeyCode.E))
            FlipShoulder();
        /*if (Input.GetKeyDown(KeyCode.D))
            FlipShoulder(-1);
        if (Input.GetKeyDown(KeyCode.A))
            FlipShoulder(1);*/
    }

    private void FixedUpdate()
    {
        Move();     // Apply to move
        Rotate();   // Attempt to rotate
    }

    private void Move()
    {
        if (moveTarget == null)                 // Make sure the place you want the camera be exists
            return;
        transform.position = Vector3.SmoothDamp // Smoothly move to that position
            (transform.position,       
            moveTarget.position, ref vel,
            smoothing * Time.deltaTime);
    }

    private void Rotate()
    {
        if (lookTarget == null)             // Make sure the thing you want the camera to look at exists
            return;
        this.transform.LookAt(lookTarget);  // Look at target (built-in Unity function)
    }

    public void FlipShoulder(int dir = 0) // ~ This isn't really a fully thought out function, it just flips the camera to the opposite side for third person setups
    {
        if (moveTarget == null)             // Make sure the place you want the camera be exists
            return;
        //  0  - Flip to opposite side
        //  1  - Flip to right side
        // -1 - Flip to left side
        var pos = moveTarget.localPosition; // Get the current position of the "MoveToTarget" object
        if (dir == 0)                       // If 0
            pos.x = -pos.x;                 // Flip to the opposite side of the current position
        else if (dir == 1)                  // If 1
            pos.x = Mathf.Abs(pos.x);       // Flip to the "right shoulder" position (the positive horizontal local position)
        else if (dir == -1)                 // If -1
            pos.x = -Mathf.Abs(pos.x);      // Flip to the "left shoulder" position (the negative horizontal local position)
        moveTarget.localPosition = pos;     // Apply position change
    }
}
