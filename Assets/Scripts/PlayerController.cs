using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]

public class PlayerMovementController : MonoBehaviour
{

    public static PlayerMovementController Current { get; private set; } = null;

    [SerializeField] private Camera headCamera;
    [SerializeField] private Transform _slot;
    [SerializeField, Range(0, 50)] private float speed = 10;
    [SerializeField, Range(0, 50)] private float sprintSpeed = 15;
    [SerializeField, Range(0, 2)] private float characterSense = 1f;

    private PickableItem _pickedItem;    

    public bool IsGrounded { get; private set; } = false;

    private CharacterController characterController;
    private PlayerInput input;
    
    private Vector3 movementDirection = Vector3.zero;
    private Vector2 lookDirection = Vector2.zero;
    private Vector3 localMovementAcelerationVector = Vector3.zero;

    private bool SprintState = false;
    
    private Vector3 velocity = Vector3.zero;
    private float contollerHitResetTimeout = 0;

    private Vector3 resultmovementDirection = Vector3.zero;

    public void StartControlling()
    {
        input.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void StopControlling()
    {
        input.enabled = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private RuntimeAnimatorController _defaultController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();
    
        StartControlling();
        Current = this;
    }
    private void FixedUpdate()
    {
        ResetCollisionData();
        CalculateVelocity(ref velocity);

        resultmovementDirection = velocity * 5f + CalculateMovementDirection();
    }
    private void LateUpdate()
    {
        var timescale = Time.deltaTime * 20f;

        transform.rotation = 
            Quaternion.Lerp(
                transform.rotation, 
                Quaternion.Euler(0, lookDirection.x, 0), 
                timescale);
        
        headCamera.transform.localRotation = 
            Quaternion.Lerp(
                headCamera.transform.localRotation, 
                Quaternion.Euler(-lookDirection.y, 0, 0), 
                timescale);

        characterController.Move(resultmovementDirection * Time.deltaTime);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (headCamera == null)
        {
            headCamera = GetComponentInChildren<Camera>();
        }
    }
#endif

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        contollerHitResetTimeout = 0.1f;

        IsGrounded = Vector3.Angle(hit.normal, Vector3.up) <= 35;


        var normalAngle = Quaternion.FromToRotation(hit.normal, Vector3.down);

        var deltaVelocity = normalAngle * velocity;
        deltaVelocity.y = Mathf.Min(0, deltaVelocity.y);


        if (IsGrounded)
        {
            deltaVelocity.x = 0;    
            deltaVelocity.z = 0;
        }

        velocity = Quaternion.Inverse(normalAngle) * deltaVelocity;
    }

    private void OnMove(InputValue inputValue)
    {
        var input = inputValue.Get<Vector2>();
        movementDirection = new Vector3(input.x, 0, input.y);
    }

    private void OnLook(InputValue inputValue)
    {
        lookDirection += inputValue.Get<Vector2>() * characterSense;

        lookDirection.y = Mathf.Clamp(lookDirection.y, -89, 89);
    }

    private void OnJump(InputValue inputValue)
    {
        if (inputValue.isPressed && IsGrounded)
        {
            velocity = Vector3.up * 4;
        }
    }

    private void OnSprint(InputValue inputValue)
    {
        SprintState = inputValue.isPressed;
    }

    private void OnInteraction(InputValue inputValue)
    {
        if (inputValue.isPressed)
        {
            if (_pickedItem)
            {
                DropItem(_pickedItem);
            }
            
            else
            {
                var ray = headCamera.ViewportPointToRay(Vector3.one * 0.5f);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 10f))
                {
                    var pickable = hit.transform.GetComponent<PickableItem>();

                    if (pickable)
                    {
                        PickItem(pickable);
                    }
                }
            }
        }
    }

    private void PickItem(PickableItem item)
    {
        _pickedItem = item;

        item.Rb.isKinematic = true;
        item.Rb.velocity = Vector3.zero;
        item.Rb.angularVelocity = Vector3.zero;

        item.transform.SetParent(_slot);

        item.transform.localPosition = Vector3.zero;
        item.transform.localEulerAngles = Vector3.zero;
    }

    private void DropItem(PickableItem item)
    {
        _pickedItem = null;

        item.transform.SetParent(null);

        item.Rb.isKinematic = false;

        item.Rb.AddForce(item.transform.forward * 16, ForceMode.VelocityChange);
    }

    private Vector3 CalculateMovementDirection()
    {
        localMovementAcelerationVector = Vector3.Lerp(localMovementAcelerationVector, transform.rotation * movementDirection * (SprintState ? sprintSpeed : speed), (IsGrounded ? 10 : 1) * Time.fixedDeltaTime);

        return localMovementAcelerationVector; 
    }
    private void CalculateVelocity(ref Vector3 velocity)
    {
        velocity = Vector3.Lerp(velocity, Physics.gravity, Time.fixedDeltaTime);
    }

    private void ResetCollisionData()
    {
        contollerHitResetTimeout -= Time.fixedDeltaTime;
        
        if (contollerHitResetTimeout < 0)
        {
            IsGrounded = false;
        }
    }
}
