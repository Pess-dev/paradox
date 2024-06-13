using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementScript : MonoBehaviour
{
    #region Singletones
    public static MovementScript movementScript { get; private set; }

    private CameraScript cameraScript;
    private InputManager inputManager;

    #endregion

    private CharacterController _characterController;

    public float movementVelosity { get; private set; }

    private float movementSpeed;
    public float movementSpeedMultiplier = 1f;

    #region PlayerAttributes
    [SerializeField]
    private float normalMovementSpeed = 2f;
    [SerializeField]
    private float crouchMovementSpeed = 1f;
    [SerializeField]
    private float sprintMovementSpeed = 3f;

    [SerializeField]
    private float standHeigh = 1.6f;
    [SerializeField]
    private float crouchHeigh = 0.4f;
    #endregion

    #region MovementState
    public enum MovementState {
        stillStand = 0,
        stillCrouch = 1,
        moveNormal = 2,
        moveCrouch = 3,
        moveSprint = 4
    }
    public MovementState movementState { get; private set; }
    public bool lockStand = false;
    public bool lockCrouch = false;

    public delegate void ChangedMovementState();
    public event ChangedMovementState OnMovementStateChanged;
    #endregion

    public float movementSensitivityMultiplyer = 1f;

    [SerializeField]
    private LayerMask floorMask;

    [HideInInspector]
    public bool movementEnabled = true;

    //  междусценье
    public void TeleportPlayer(Vector3 pos) {
        RaycastHit hit;
        Physics.Raycast(pos, Vector3.down, out hit, _characterController.height * 3f, floorMask);
        movementScript._characterController.enabled = false;
        print(hit.point);
        if (hit.point != Vector3.zero)
            movementScript.transform.position = hit.point + Vector3.up * _characterController.height * 0.5f;
        else
            movementScript.transform.position = pos;
        print(transform.position);
        movementScript._characterController.enabled = true;
    }

    private void Awake() {
        movementScript = this;
    }

    private void Start() {
        cameraScript = CameraScript.cameraScript;
        inputManager = InputManager.instance;

        OnMovementStateChanged += ChangeMovementState;

        _characterController = GetComponent<CharacterController>();
        movementState = MovementState.moveNormal;
    }

    private void LateUpdate() {
        if (!movementEnabled)
            return;


        CheckMovementState();

        Vector3 move = new Vector3(inputManager.moveInput.x * 0.625f, -2.5f,
            inputManager.moveInput.y * ((inputManager.moveInput.y < 0) ? 0.75f : 1)) * movementSpeed * movementSpeedMultiplier;
        move = Quaternion.Euler(0f, cameraScript.cameraMainTransform.rotation.eulerAngles.y, 0f) * move * Time.deltaTime * movementSensitivityMultiplyer;
        _characterController.Move(move);
        movementVelosity = new Vector2(_characterController.velocity.x, _characterController.velocity.z).sqrMagnitude / (movementSpeed * movementSpeed);
        movementVelosity = Mathf.Clamp01(movementVelosity);
    }

    private void ChangeMovementState() {
        switch (movementState) {
            case MovementState.stillStand:
                movementSpeed = normalMovementSpeed;
                ChangeCrouch(false);
                break;
            case MovementState.stillCrouch:
                movementSpeed = crouchMovementSpeed;
                ChangeCrouch(true);
                break;
            case MovementState.moveNormal:
                movementSpeed = normalMovementSpeed;
                ChangeCrouch(false);
                break;
            case MovementState.moveCrouch:
                movementSpeed = crouchMovementSpeed;
                ChangeCrouch(true);
                break;
            case MovementState.moveSprint:
                movementSpeed = sprintMovementSpeed;
                ChangeCrouch(false);
                break;
        }
    }

    #region MovementState
    private void CheckMovementState() {
        if (new Vector3(_characterController.velocity.x, 0, _characterController.velocity.z).sqrMagnitude <= 0.001f) {
            if (inputManager.crouch) {
                UpdateMovementState(MovementState.stillCrouch);
            } else {
                UpdateMovementState(MovementState.stillStand);
            }
            return;
        }
        if (inputManager.sprint) {
            UpdateMovementState(MovementState.moveSprint);
            return;
        }
        if (inputManager.crouch) {
            UpdateMovementState(MovementState.moveCrouch);
            return;
        }
        UpdateMovementState(MovementState.moveNormal);
    }

    private void UpdateMovementState(MovementState state) {
        if (lockStand && state == MovementState.stillCrouch) {
            state = MovementState.stillStand;
        } else if (lockStand && state == MovementState.moveCrouch) {
            state = MovementState.moveNormal;
        } else if (lockCrouch && state == MovementState.stillStand) {
            state = MovementState.stillCrouch;
        } else if (lockCrouch && state == MovementState.moveNormal) {
            state = MovementState.moveCrouch;
        } else if (lockCrouch && state == MovementState.moveSprint) {
            state = MovementState.moveCrouch;
        }

        if (movementState == state)
            return;

        movementState = state;
        if (OnMovementStateChanged != null)
            OnMovementStateChanged();
        return;
    }

    private void ChangeCrouch(bool shouldCrouch) {
        if (shouldCrouch) {
            _characterController.height = crouchHeigh;
            _characterController.center = new Vector3(0, (standHeigh - crouchHeigh) / -2, 0);
        } else {
            _characterController.height = standHeigh;
            _characterController.center = Vector3.zero;
        }
    }
    #endregion
}