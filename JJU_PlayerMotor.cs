using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// JimJam Utility by Tyler Sims --- Twitter @HipToBeeSquare
/// 
/// This is a player motor used for 3D movement using the built-in
/// CharacterController provided by Unity. It (in theory) would be
/// modified or replaced with a permenant system after prototyping
/// but could also be used for a full game if needed.
/// 
/// Biggest Issues:
///  ! Does not handle sliding down slopes
///  ! Moving platforms do not taxi the player
///  ! Falling as a result of walking off a ledge seems MUCH faster than after a jump
/// 
/// Features:
/// [Rotation]---------------------------------------------------------
///     * Handles rotating the CharacterController
/// [Movement]---------------------------------------------------------
///     * Handles moving the CharacterController
///         - 3D movement
///         - Jumping
///     * Shifts movement speed between running or walking
///     * Handles applying gravity to the CharacterController
/// [General]-----------------------------------------------------------
///     * Can output a value to determine if the character is in motion or not  - ! This may need to be modified or reworked entirely
///     
/// How to use: (This is intended for the Player character, other characters may require a custom solution)
/// Requirements: CharacterController (Unity default class), JJU_PlayerMotor, JJU_BasicCamera (only for part 2)
/// [Part 1 - Just the input & motor]
/// Quickest way to get started is to simply:
///     * Add a capsule to the scene
///     * Add this (JJU_PlayerInput)
///     
/// [Part 2 - The camera]
/// To get it to function with the JJU_BasicCamera:
///     * Add a "MoveToTarget" child of this object (where you want your camera to be)
///     * Add a "LookAtTarget" child of this object (where you want your camera to look at)
///     * Add "JJU_BasicCamera" to your main camera object
/// </summary>
public class JJU_PlayerMotor : MonoBehaviour
{
    // -----------  -----------  -----------
    // ----------- START VARIABLES ---------
    // -----------  -----------  -----------
    [Header("Movement")]
    [Range(0.01f,50.0f)]
    public float gravity = 20.0f; // How many gravitys are you on my dude?

    private float moveSpeed;                       // The current movement speed
    [SerializeField] private float walkSpeed = 4f; // How fast the player should go  when 'walking'
    [SerializeField] private float runSpeed  = 7f; // How fast the player should go when 'running'

    
    public float rotSpeed;      // How quickly the character rotates
    public Vector3 moveOutput;  // Currently not used but can be for checking if the player is moving. Not entirely accurate and need better method

    [HideInInspector]
    public float jumpVel;                            // The velocity the player is jumping with
    private Vector3 vel = Vector3.zero;              // Default value used for SmoothDamp
    private CharacterController characterController; // The component we apply movement to
    // -----------  -----------  -----------
    // ----------- END VARIABLES -----------
    // -----------  -----------  -----------

    public void Awake()
    {
        characterController = GetComponent<CharacterController>(); // Get reference to the CharacterController (this is what we apply the movement to)
    }

    public void ToggleRun(bool state)
    {
        moveSpeed = state ? walkSpeed : runSpeed;   // Set running speed
    }
    
    
    // Apply the sum movement
    public void Move(Vector2 moveDirection)
    {
        Vector3 newDir = new Vector3(moveDirection.x * moveSpeed * Time.deltaTime,0f,
            moveDirection.y * moveSpeed * Time.deltaTime);                              // Apply momentum
        Vector3 move = transform.right * newDir.x + transform.forward * newDir.z;       // Set it as the facing direction of the player
        move.y = 0f;                                                                    // Zero out the vertical, otherwise it can REALLY mess up jumps
        move.y -= gravity + -jumpVel * transform.forward.sqrMagnitude;                  // Apply gravity
        moveOutput = move;
        characterController.Move(move * Time.deltaTime);                                // Apply movement on player
    }

    // Apply the sum rotation
    public void Rotate(Vector2 lookInput)
    {
        Quaternion xQuaternion = Quaternion.AngleAxis(lookInput.x, Vector3.up);     // Get horizontal rotation
        Quaternion yQuaternion = Quaternion.AngleAxis(lookInput.y, Vector3.left);   // Get vertical rotation
        Quaternion bigQ = xQuaternion * yQuaternion;                                // Combine rotations
        transform.localRotation = Quaternion.Lerp(transform.localRotation, bigQ,    // Apply smooth rotation
            rotSpeed * Time.deltaTime);
    }
}
