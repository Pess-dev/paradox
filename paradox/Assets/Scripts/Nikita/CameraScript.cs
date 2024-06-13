using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {
    #region Singletones
    public static CameraScript cameraScript { get; private set; }

    private MovementScript movementScript;
    private InputManager inputManager;
    #endregion

    public Transform cameraMainTransform { get; private set; }

    private float cameraOffset;
    [SerializeField]
    private float cameraStandOffset = 0.75f;
    [SerializeField]
    private float cameraCrouchOffset = 0f;

    public float cameraAdditionalOffset = 0f;

    public float cameraSensitivityMultiplyer = 1f;
    private float cameraTransitionSpeed = 7f;
    public Vector3 cameraTransitionPosition = Vector3.zero;

    [HideInInspector]
    public bool rotatingEnabled = true;

    private void Awake() {
        if (cameraScript == null)
            cameraScript = this;

        cameraMainTransform = GetComponent<Transform>();
        transform.SetParent(null);
    }

    private void Start() {
        movementScript = MovementScript.movementScript;
        inputManager = InputManager.instance;

        movementScript.OnMovementStateChanged += ChangeMovementState;
    }

    private void Update() {
        if (!rotatingEnabled)
            return;

        #region RotationPart
        float XRotation = cameraMainTransform.localRotation.eulerAngles.x + inputManager.lookDelta.y * -1.2f * cameraSensitivityMultiplyer;
        if ((XRotation >= 75) & (XRotation < 200))
            XRotation = 75f;
        if ((XRotation <= 285) & (XRotation > 100))
            XRotation = 285f;
        cameraMainTransform.localRotation = Quaternion.Euler(XRotation, 
            cameraMainTransform.localRotation.eulerAngles.y + inputManager.lookDelta.x * 1.2f * cameraSensitivityMultiplyer, 0f);
        #endregion

        #region SmoothTransitionPart
        Vector3 cameraPos = new Vector3(movementScript.transform.position.x, 
            movementScript.transform.position.y + cameraOffset + cameraAdditionalOffset, movementScript.transform.position.z);
        if (cameraTransitionPosition != Vector3.zero) {
            cameraPos = cameraTransitionPosition;
        }
        cameraMainTransform.position = Vector3.Lerp(cameraMainTransform.position, cameraPos, Time.deltaTime * cameraTransitionSpeed);
        #endregion
    }

    void ChangeMovementState() {
        if ((int)movementScript.movementState == 1 || (int)movementScript.movementState == 3) {
            cameraOffset = cameraCrouchOffset;
        } else {
            cameraOffset = cameraStandOffset;
        }
    }
}