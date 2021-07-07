using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JJU_TopDownController : MonoBehaviour
{    
    //[SerializeField] private Transform cam;
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed; // The current movement speed
    [SerializeField] private float smoothing;

    [Header("Header")]
    [SerializeField] private Transform arrow;
    
    private CharacterController _characterController;
    private Vector3 _targetPos;
    private Vector3 _vel = Vector3.zero;
    
    [HideInInspector]
    public Vector2 input;
    public virtual void Start()
    {
        _characterController = GetComponent<CharacterController>();
    }
    void Update()
    {
        // Gets movement input
        input.y = Input.GetAxisRaw("Vertical");
        input.x = Input.GetAxisRaw("Horizontal");

    }
    private void FixedUpdate()
    {
        Move(new Vector2(input.x, input.y)); // Move player

        if (input.y != 0 || input.x != 0)
        {
            Vector3 lookDir = _targetPos - arrow.localPosition;
            float angle = Mathf.Atan2(lookDir.x, lookDir.z) * Mathf.Rad2Deg;
            Vector3 rot = arrow.rotation.eulerAngles;
            rot.y = angle;
            arrow.rotation = Quaternion.Euler(rot);    
        }
        
        
        //Cursor.visible = false; // Make sure the player cursor is not visible during normal play
        //Cursor.lockState = CursorLockMode.Locked; // Make sure the player cursor cannot leave the window during normal play
    }
    
    private float ClampAngle (float angle, float min, float max)
    {
        angle = angle % 360;
        if ((angle >= -360F) && (angle <= 360F)) {
            if (angle < -360F) {
                angle += 360F;
            }
            if (angle > 360F) {
                angle -= 360F;
            }			
        }
        return Mathf.Clamp (angle, min, max);
    }

    public void Move(Vector2 moveDirection)
    {
        Vector2 newDir = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed); // Apply momentum
        Vector3 move = transform.right * newDir.x + transform.forward * newDir.y; // Set it as the facing direction of the player
        _targetPos = Vector3.SmoothDamp(_targetPos,move,ref _vel, Time.deltaTime * smoothing);
        _characterController.Move(_targetPos * Time.deltaTime); // Apply movement on player
    }
}
