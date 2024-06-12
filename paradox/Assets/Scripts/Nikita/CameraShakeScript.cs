using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShakeScript : MonoBehaviour
{
    #region Singletones
    public static CameraShakeScript cameraShakeScript { get; private set; }

    private MovementScript movementScript;
    private InputManager inputManager;
    #endregion

    private Transform cameraShakeTransform;

    #region ShakeVariables
    private float shakeTime = 0;

    private float shakeSpeed = 5;
    private float shakeXrange = 0.05f;
    private float shakeYrange = 0.12f;

    private float shakeStrength = 0;
    #endregion

    #region ShakeHelpVariables
    [SerializeField]
    private float walkShakeSpeed = 5;
    [SerializeField]
    private float walkShakeXrange = 0.05f;
    [SerializeField]
    private float walkShakeYrange = 0.05f;

    [SerializeField]
    private float sprintShakeSpeed = 7;
    [SerializeField]
    private float sprintShakeXrange = 0.1f;
    [SerializeField]
    private float sprintShakeYrange = 0.07f;

    [SerializeField]
    private float crouchShakeSpeed = 3;
    [SerializeField]
    private float crouchShakeXrange = 0.05f;
    [SerializeField]
    private float crouchShakeYrange = 0.07f;

    [SerializeField]
    private float stillShakeSpeed = 1;
    [SerializeField]
    private float stillShakeXrange = 0.1f;
    [SerializeField]
    private float stillShakeYrange = 0.07f;
    #endregion

    private float rotationShake = 0f;

    private void Awake() {
        cameraShakeScript = this;
        cameraShakeTransform = transform;
    }

    private void Start() {
        movementScript = MovementScript.movementScript;
        inputManager = InputManager.instance;

        //movementScript.OnMovementStateChanged += ChangeMovementState;
    }

    private void Update() {
        ChangeMovementState();
        rotationShake = Mathf.Lerp(rotationShake, inputManager.lookDelta.x, Time.deltaTime * 7f);
        /*rotationShake = Mathf.Lerp(rotationShake,
            (Mathf.Abs(inputScript.smoothMouseVector.x) > Mathf.Abs(inputScript.movementVectorNormalized.x * 0.1f)) ? 
            inputScript.smoothMouseVector.x : inputScript.movementVectorNormalized.x * 0.1f,
            Time.deltaTime * 7f);*/
        rotationShake = Mathf.Clamp(rotationShake, -0.75f, 0.75f);
        shakeTime += Time.deltaTime * shakeSpeed;
        shakeTime %= Mathf.PI * 2;
        float shakeSin = Mathf.Sin(shakeTime * 2);
        float shakeCos = Mathf.Cos(shakeTime);

        shakeStrength = Mathf.Lerp(shakeStrength, movementScript.movementVelosity, Time.deltaTime * 7 * ((movementScript.movementVelosity + 1.5f) / 2));
        shakeStrength = Mathf.Clamp(shakeStrength, 0.1f, 1f);

        Vector3 shakePosition = new Vector3(shakeCos * shakeXrange, shakeSin * shakeYrange, 0) * shakeStrength;
        cameraShakeTransform.localPosition = shakePosition;
        cameraShakeTransform.localRotation = Quaternion.Euler(shakeSin * shakeYrange * -7f, 0f, shakeCos * shakeXrange * -7f + rotationShake * -5f);
    }

    void ChangeMovementState() {
        float shakeInterpolateSpeed = 2f;
        if ((int)movementScript.movementState <= 1) {
            shakeSpeed = Mathf.Lerp(shakeSpeed, stillShakeSpeed, Time.deltaTime * shakeInterpolateSpeed);
            shakeXrange = Mathf.Lerp(shakeXrange, stillShakeXrange, Time.deltaTime * shakeInterpolateSpeed);
            shakeYrange = Mathf.Lerp(shakeYrange, stillShakeYrange, Time.deltaTime * shakeInterpolateSpeed);
            return;
        }
        if ((int)movementScript.movementState == 2) {
            shakeSpeed = Mathf.Lerp(shakeSpeed, walkShakeSpeed, Time.deltaTime * shakeInterpolateSpeed);
            shakeXrange = Mathf.Lerp(shakeXrange, walkShakeXrange, Time.deltaTime * shakeInterpolateSpeed);
            shakeYrange = Mathf.Lerp(shakeYrange, walkShakeYrange, Time.deltaTime * shakeInterpolateSpeed);
            return;
        }
        if ((int)movementScript.movementState == 3) {
            shakeSpeed = Mathf.Lerp(shakeSpeed, crouchShakeSpeed, Time.deltaTime * shakeInterpolateSpeed);
            shakeXrange = Mathf.Lerp(shakeXrange, crouchShakeXrange, Time.deltaTime * shakeInterpolateSpeed);
            shakeYrange = Mathf.Lerp(shakeYrange, crouchShakeYrange, Time.deltaTime * shakeInterpolateSpeed);
            return;
        }
        if ((int)movementScript.movementState == 4) {
            shakeSpeed = Mathf.Lerp(shakeSpeed, sprintShakeSpeed, Time.deltaTime * shakeInterpolateSpeed);
            shakeXrange = Mathf.Lerp(shakeXrange, sprintShakeXrange, Time.deltaTime * shakeInterpolateSpeed);
            shakeYrange = Mathf.Lerp(shakeYrange, sprintShakeYrange, Time.deltaTime * shakeInterpolateSpeed);
            return;
        }
    }
}
