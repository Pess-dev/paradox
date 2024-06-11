using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    public static InputManager instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Vector3 moveDirection { get; private set; } = Vector3.zero;
    public Vector2 lookDelta { get; private set; } = Vector2.zero;

    public bool jump { get; private set; } = false;

    public bool interact { get; private set; } = false;

    void OnMove(InputValue value){
        Vector2 dir = value.Get<Vector2>();
        moveDirection = CameraToWorld(new Vector3(dir.x, 0, dir.y));
    }
    void OnLook(InputValue value){
        lookDelta = value.Get<Vector2>();
    }
    void OnJump(InputValue value){
        jump = value.isPressed;
    }
    void OnInteract(InputValue value){
        interact = value.isPressed;
    }

    Vector3 CameraToWorld(Vector3 direction){
        Vector3 planeDir = Vector3.ProjectOnPlane(direction, Vector3.up).normalized*direction.magnitude;
        planeDir = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0) * planeDir;
        return planeDir;
    }
}
