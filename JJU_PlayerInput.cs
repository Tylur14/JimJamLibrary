using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// JimJam Utility by Tyler Sims --- Twitter @HipToBeeSquare
/// 
/// This is the input manager that pairs with the PlayerMotor class, 
/// it could be fitted for a new system but at that point you're basically just
/// making a whole new thing anyways.
/// 
/// Realistically you'd use this for quick prototyping but you could theoretically make
/// and entire game with it!
/// 
/// Features:
/// [Rotation]---------------------------------------------------------
///     * Handles mouse sensitivity for looking
///     * Clamps rotations with specified values
///     * Clamps angles to prevent from leaving the preferred 0 to 360 
/// [Movement]---------------------------------------------------------
///     * Handles jumping power
///     * Handles movement input
///         - 3D movement
///         - Jumping
///     * Handles running toggle
/// [General]-----------------------------------------------------------
///     * Keeps cursor hidden and locked into game window  - ! This may need to be modified or reworked entirely
///     
/// How to use:
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

[RequireComponent(typeof(JJU_PlayerMotor))]
[RequireComponent(typeof(CharacterController))]
public class JJU_PlayerInput : MonoBehaviour
{

    [Header("Rotation")]
    [SerializeField] private float sensitivityX = 5F;   // Determines how quickly the player rotates their look view horizontally
    [SerializeField] private float sensitivityY = 5F;   // Determines how quickly the player rotates their look view vertically
    [SerializeField]
    Vector2 rangeX = new Vector2(-360f, 360f);          // The boundaries for how far the player can turn horizontally, typically for 3D games you want a full
                                                        // range of motion, but there are some cases (see Five Nights at Freddie's) where you want a limited
                                                        // range of motion.
    [SerializeField]
    Vector2 rangeY = new Vector2(-60f, 60f);            // The boundaries for how far the player can turn vertically, typically you want this capped at a small range
                                                        // since after about 90 degrees the player could look upside down entirely

    [SerializeField] float jumpForce;                   // Controls how quickly the player can jump, note that I said quickly and not how high since this pairs with the
                                                        // _motor's gravity value that ultimately determines how high the player can jump

    private JJU_PlayerMotor _motor;                     // The core motor that uses the info passed from this class to apply movement and rotation to the player character
    private CharacterController _characterController;   // ~ May not be needed since the only thing it does is check for ground, which could be done elsewhere

    Vector2 input;                                      // The keyboard input values for movement
    float rotX = 0F;                                    // Horizontal mouse movement for looking
    float rotY = 0F;                                    // Vertical mouse movement for looking

    private void Awake()
    {
        _motor = GetComponent<JJU_PlayerMotor>();                       // Get reference to the PlayerMotor, this class isn't useful without it
        _characterController = GetComponent<CharacterController>();     // Get reference to the CharacerController, used only for checking ifGrounded currently
    }

    // Get inputs and check if player is running or not
    private void Update()
    {
        // Handle jumping
        if (Input.GetKeyDown(KeyCode.Space) && _characterController.isGrounded)                                 // ! - May need input mapping modified
            _motor.jumpVel = jumpForce;

        _motor.jumpVel = _motor.jumpVel > 0 ? _motor.jumpVel -= Time.deltaTime * _motor.gravity : 0;

        // Gets movement input
        input.y = Input.GetAxis("Vertical");                 input.x = Input.GetAxis("Horizontal");             // ! - May need input mapping modified

        // Gets Look Input
        rotY += Input.GetAxis("Mouse Y") * sensitivityY;     rotX += Input.GetAxis("Mouse X") * sensitivityX;   // ! - May need input mapping modified
        rotY = ClampAngle(rotY, rangeY.x, rangeY.y);         rotX = ClampAngle(rotX, rangeX.x, rangeX.y);

        // Check if the player is running
        _motor.ToggleRun(Input.GetButtonDown("Fire1") || Input.GetAxisRaw("Fire1") != 0 ? false : true);        // ! - May need input mapping modified
    }

    // Apply inputs
    private void FixedUpdate()
    {
        _motor.Move(new Vector2(input.x, input.y));    // Move player
        _motor.Rotate(new Vector2(rotX, rotY));        // Rotate player

        Cursor.visible = false;                   // Make sure the player cursor is not visible during normal play
        Cursor.lockState = CursorLockMode.Locked; // Make sure the player cursor cannot leave the window during normal play
    }

    private float ClampAngle(float angle, float min, float max)
    {
        // Honestly don't remember what is going on here, it IS important tho
        angle = angle % 360;
        if ((angle >= -360F) && (angle <= 360F))
        {
            if (angle < -360F)
            {
                angle += 360F;
            }
            if (angle > 360F)
            {
                angle -= 360F;
            }
        }
        return Mathf.Clamp(angle, min, max);
    }
}
