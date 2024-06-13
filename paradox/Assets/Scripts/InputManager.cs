using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    public static InputManager instance = null;

    public float lookSensitivity = 1f;

    private void Awake()
    {
        if (instance == null){
            instance = this;
        }
        else{
            Destroy(gameObject);
        }
    }

    public Vector3 moveDirectionWorld { get; private set; } = Vector3.zero;
    public Vector2 moveInput { get; private set; } = Vector3.zero;
    public Vector2 lookDelta { get; private set; } = Vector2.zero;
    public float scroll { get; private set; } = 0;

    public bool jumped { get; private set; } = false;

    public bool sprint { get; private set; } = false;
    public bool crouch { get; private set; } = false;
    public bool interacted { get; private set; } = false;
    public bool taked { get; private set; } = false;
    public bool grabbed { get; private set; } = false;
    public bool grab { get; private set; } = false;

    void Start(){
        Cursor.lockState = CursorLockMode.Locked;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

    void LateUpdate(){
        taked = false;
        interacted = false;
        grabbed = false;
        jumped = false;
    }

    void OnMove(InputValue value){
        moveInput = value.Get<Vector2>();
        moveDirectionWorld = CameraToWorld(new Vector3(moveInput.x, 0, moveInput.y));
    }
    void OnLook(InputValue value){
        lookDelta = value.Get<Vector2>() * lookSensitivity;
    }
    void OnJump(InputValue value){
        jumped = value.isPressed;
    }
    
    void OnRun(InputValue value){
        sprint = value.isPressed;
    }
    void OnCrouch(InputValue value){
        crouch = value.isPressed;
    }
    void OnTake(InputValue value){
        taked = value.isPressed;
    }
    void OnScroll(InputValue value){
        scroll = value.Get<float>();
    }
    void OnGrab(InputValue value){
        grab = value.isPressed;
        grabbed = grab;
    }
    void OnInteract(InputValue value){
        interacted = value.isPressed;
    }
    
    Vector3 CameraToWorld(Vector3 direction){
        Vector3 planeDir = Vector3.ProjectOnPlane(direction, Vector3.up).normalized*direction.magnitude;
        planeDir = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0) * planeDir;
        return planeDir;
    }
}
