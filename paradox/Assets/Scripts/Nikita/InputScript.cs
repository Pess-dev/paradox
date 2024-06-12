using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputScript : MonoBehaviour
{
    public static InputScript inputScript { get; private set; }

    public Vector2 movementVector { get; private set; } = Vector2.zero;
    public Vector2 movementVectorNormalized { get; private set; } = Vector2.zero;
    public Vector2 mouseVector { get; private set; } = Vector2.zero;
    public Vector2 smoothMouseVector { get; private set; } = Vector2.zero;

    public bool crouchPressed { get; private set; }
    public bool sprintPressed { get; private set; }

    public bool interactButtonPressed { get; private set; }
    public bool grabButtonPressed { get; private set; }
    public bool grabButtonPressing { get; private set; }
    public bool takeButtonPressed { get; private set; }
    public bool nextLineButtonPressed { get; private set; }

    public float mouseScroll { get; private set; }

    public bool lockItemRelatedButtons = false;
    public bool lockMovementRelatedButtons = false;
    public bool lockCameraRelatedButtons = false;

    private float deltaLerp = 0f;

    private void Awake() {
        if (inputScript == null)
            inputScript = this;
    }

    private void Start() {

    }

    private void Update() {
        movementVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        mouseVector = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        mouseScroll = Input.mouseScrollDelta.y;

        crouchPressed = Input.GetButton("Crouch");
        sprintPressed = Input.GetButton("Sprint");

        interactButtonPressed = Input.GetButtonDown("Use");
        grabButtonPressed = Input.GetButtonDown("Grab");
        grabButtonPressing = Input.GetButton("Grab");
        takeButtonPressed = Input.GetButtonDown("Take");

        if (lockItemRelatedButtons) {
            mouseScroll = 0f;
            interactButtonPressed = false;
            grabButtonPressed = false;
            grabButtonPressing = false;
            takeButtonPressed = false;
        }
        if (lockMovementRelatedButtons) {
            movementVector = Vector2.zero;
            crouchPressed = false;
            sprintPressed = false;
        }
        if (lockCameraRelatedButtons) {
            mouseVector = Vector2.zero;
        }

        movementVectorNormalized = movementVector.normalized;
        deltaLerp = Mathf.Lerp(deltaLerp, Time.deltaTime, 0.1f);
        smoothMouseVector = Vector2.Lerp(smoothMouseVector, mouseVector, deltaLerp * 12f);
        //smoothMouseVector = Vector2.Lerp(smoothMouseVector, mouseVector, 0.07f);

        nextLineButtonPressed = Input.GetButtonDown("Grab");// || Input.GetButtonDown("Take") || Input.GetButtonDown("Use");
    }
}
