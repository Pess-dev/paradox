using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventHandlerScript : MonoBehaviour
{
    #region Singletones
    public static EventHandlerScript eventHandler { get; private set; }

    private MovementScript movementScript;
    private CameraScript cameraScript;
    private CameraShakeScript cameraShakeScript;
    private InputScript inputScript;
    private SubtitlesScript subtitlesScript;
    private UIScript uiScript;
    private InteractionScript interactionScript;
    #endregion

    private void Awake() {
        eventHandler = this;
    }

    private void Start() {
        movementScript = MovementScript.movementScript;
        cameraScript = CameraScript.cameraScript;
        cameraShakeScript = CameraShakeScript.cameraShakeScript;
        inputScript = InputScript.inputScript;
        subtitlesScript = SubtitlesScript.subtitlesScript;
        uiScript = UIScript.uiScript;
        interactionScript = InteractionScript.interactionScript;

        Cursor.lockState = CursorLockMode.Locked;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        movementScript.OnMovementStateChanged += UpdateForce;
        StartCoroutine(HideCheck());
    }

    //Kinda
    #region MainFunctions

    public void StartDialogue(List<SubtitlesLine> lines, Vector3 lookAtTargetPosition, float lookAtSpeed, Vector3 cameraPosition) {
        NewDialogue(lines);
        if (lookAtTargetPosition != Vector3.zero)
            LookAt(lookAtTargetPosition, lookAtSpeed, -1f, true);
        if (cameraPosition != Vector3.zero)
            SetCameraTransitionPosition(cameraPosition);
        OnEndSubtitlesLines += EndDialogue;
    }

    public void StartDialogue(List<SubtitlesLine> lines, Transform lookAtTargetTransform, float lookAtSpeed, Vector3 cameraPosition) {
        NewDialogue(lines);
        LookAt(lookAtTargetTransform, lookAtSpeed, -1f, true);
        if (cameraPosition != Vector3.zero)
            SetCameraTransitionPosition(cameraPosition);
        OnEndSubtitlesLines += EndDialogue;
    }

    public void EndDialogue() {
        OnEndSubtitlesLines -= EndDialogue;
        UnlockCamera();
        RemoveCameraTransitionPosition();
    }

    public void TeleportPlayer(Vector3 pos) {
        movementScript.TeleportPlayer(pos);
        forceList.Clear();
        UpdateForce();
    }

    #endregion

    //Semi done
    #region ForceRegion

    public bool isHiden { get; private set; } = false;

    private List<ForceClass> forceList = new List<ForceClass>();

    public void AddForce(ForceClass force) {
        forceList.Add(force);
        UpdateForce();
    }

    public void RemoveForce(ForceClass force) {
        forceList.Remove(force);
        UpdateForce();
    }

    private void UpdateForce() {
        bool isPlayerCrouching = false;
        if ((int)movementScript.movementState == 1 || (int)movementScript.movementState == 3)
            isPlayerCrouching = true;
        float minSpeedMultiplyer = 99f;
        float maxHeighOffset = 99f;
        bool shouldBeHiden = false;
        bool shouldLockCrouch = false;
        bool shouldLockStand = false;

        foreach (ForceClass item in forceList) {
            if ((!item.shouldCrouch && !item.shouldStand) || (isPlayerCrouching && item.shouldCrouch) || (!isPlayerCrouching && item.shouldStand)) {
                minSpeedMultiplyer = Mathf.Min(minSpeedMultiplyer, item.speedMultiplyer);
                maxHeighOffset = Mathf.Min(maxHeighOffset, item.heighOffset);
            }
            if (item.hiden) {
                shouldBeHiden = true;
            }
            if (isPlayerCrouching && item.lockCrouch) {
                shouldLockCrouch = true;
            }
            if (!isPlayerCrouching && item.lockStand) {
                shouldLockStand = true;
            }
        }

        minSpeedMultiplyer = (minSpeedMultiplyer == 99f) ? 1f : minSpeedMultiplyer;
        maxHeighOffset = (maxHeighOffset == 99f) ? 0f : maxHeighOffset;

        movementScript.movementSpeedMultiplier = minSpeedMultiplyer;
        cameraScript.cameraAdditionalOffset = maxHeighOffset;
        isHiden = shouldBeHiden;
        movementScript.lockCrouch = shouldLockCrouch;
        movementScript.lockStand = shouldLockStand;
    }

    #endregion

    //No (REWORK)
    #region Hide

    private Coroutine SmoothHideCoroutine = null;

    private IEnumerator HideCheck() {
        bool prevHiding = false;
        while (true) {
            yield return new WaitForEndOfFrame();
            if (prevHiding == isHiden)
                continue;
            prevHiding = isHiden;
            uiScript.SetHideVineteState(prevHiding);
        }
    }

    #endregion

    //Semi done
    #region MovementLock
    private Coroutine unlockMovementCoroutine;

    public void LockMovement() {
        if (unlockMovementCoroutine != null) {
            StopCoroutine(unlockMovementCoroutine);
        }
        LockMovementRelatedInput();
        movementScript.movementSensitivityMultiplyer = 0f;
    }

    public void LockMovement(float time) {
        if (unlockMovementCoroutine != null) {
            StopCoroutine(unlockMovementCoroutine);
        }
        LockMovementRelatedInput();
        movementScript.movementSensitivityMultiplyer = 0f;
        unlockMovementCoroutine = StartCoroutine(DelayedUnlockMovement(time));
    }

    private IEnumerator DelayedUnlockMovement(float time) {
        yield return new WaitForSeconds(time);
        unlockMovementCoroutine = null;
        UnlockMovement();
        yield break;
    }

    public void UnlockMovement() {
        if (unlockMovementCoroutine != null) {
            StopCoroutine(unlockMovementCoroutine);
        }
        UnlockMovementRelatedInput();
        unlockMovementCoroutine = StartCoroutine(SmoothUnlockMovement());
    }

    private IEnumerator SmoothUnlockMovement() {
        while (movementScript.movementSensitivityMultiplyer <= 0.99f) {
            movementScript.movementSensitivityMultiplyer = Mathf.Lerp(movementScript.movementSensitivityMultiplyer, 1f, Time.deltaTime * 2.5f);
            yield return new WaitForEndOfFrame();
        }
        movementScript.movementSensitivityMultiplyer = 1f;
        unlockMovementCoroutine = null;
        yield break;
    }

    #endregion

    //Semi done
    #region CameraFocus

    private Coroutine focusCameraCoroutine;

    private Vector3 cameraPreviousLookAt = Vector3.zero;

    public void LockCamera() {
        if (focusCameraCoroutine != null) {
            StopCoroutine(focusCameraCoroutine);
        }
        LockItemRelatedInput();
        LockCameraRelatedInput();
        uiScript.SetInteractImageVisibility(false);
        cameraScript.cameraSensitivityMultiplyer = 0f;
    }

    public void LockCamera(float time) {
        if (focusCameraCoroutine != null) {
            StopCoroutine(focusCameraCoroutine);
        }
        LockItemRelatedInput();
        LockCameraRelatedInput();
        uiScript.SetInteractImageVisibility(false);
        cameraScript.cameraSensitivityMultiplyer = 0f;
        focusCameraCoroutine = StartCoroutine(DelayedUnlockCamera(time));
    }

    private IEnumerator DelayedUnlockCamera(float time) {
        yield return new WaitForSeconds(time);
        focusCameraCoroutine = null;
        UnlockCamera();
        yield break;
    }

    public void UnlockCamera() {
        if (focusCameraCoroutine != null) {
            StopCoroutine(focusCameraCoroutine);
        }
        if (cameraPreviousLookAt == Vector3.zero) {
            UnlockItemRelatedInput();
            UnlockCameraRelatedInput();
            uiScript.SetInteractImageVisibility(true);
            focusCameraCoroutine = StartCoroutine(SmoothUnlockCamera());
            return;
        } else {
            focusCameraCoroutine = StartCoroutine(SmoothLookAtPrevious());
            return;
        }
    }

    private IEnumerator SmoothUnlockCamera() {
        while (cameraScript.cameraSensitivityMultiplyer <= 0.99f) {
            cameraScript.cameraSensitivityMultiplyer = Mathf.Lerp(cameraScript.cameraSensitivityMultiplyer, 1f, Time.deltaTime * 2.5f);
            yield return new WaitForEndOfFrame();
        }
        cameraScript.cameraSensitivityMultiplyer = 1f;
        focusCameraCoroutine = null;
        yield break;
    }

    private IEnumerator SmoothLookAtPrevious() {
        float angle = 90f;
        while (angle > 1.5f) {
            Vector3 targetDirection = cameraPreviousLookAt - cameraScript.cameraMainTransform.transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            cameraScript.cameraMainTransform.rotation = Quaternion.Lerp(cameraScript.cameraMainTransform.rotation, targetRotation, Time.deltaTime * 10f);
            angle = Vector3.Angle(cameraScript.cameraMainTransform.forward, targetDirection);
            yield return new WaitForEndOfFrame();
        }
        cameraPreviousLookAt = Vector3.zero;
        focusCameraCoroutine = null;
        UnlockCamera();
        yield break;
    }

    public void LookAt(Vector3 targetPosition, float speed, float timeToWait, bool returnBack) {
        LockCamera();
        if (focusCameraCoroutine != null) {
            StopCoroutine(focusCameraCoroutine);
        }
        if (returnBack && cameraPreviousLookAt == Vector3.zero) {
            cameraPreviousLookAt = cameraScript.cameraMainTransform.transform.position + cameraScript.cameraMainTransform.transform.forward * 5f;
        }
        focusCameraCoroutine = StartCoroutine(SmoothTransitionCamera(targetPosition, speed, timeToWait));
    }
    public void LookAt(Vector3 targetPosition, float speed) {
        LockCamera();
        if (focusCameraCoroutine != null) {
            StopCoroutine(focusCameraCoroutine);
        }
        focusCameraCoroutine = StartCoroutine(SmoothTransitionCamera(targetPosition, speed, 0f));
    }
    private IEnumerator SmoothTransitionCamera(Vector3 targetPosition, float speed, float timeToWait) {
        if (timeToWait < 0f) {
            while (true) {
                Vector3 targetDirection = targetPosition - cameraScript.cameraMainTransform.position;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                cameraScript.cameraMainTransform.rotation = Quaternion.Lerp(cameraScript.cameraMainTransform.rotation, targetRotation, Time.deltaTime * speed);
                yield return new WaitForEndOfFrame();
            }
        }
        if (timeToWait == 0f) {
            float angle = 90f;
            while (angle > 1f) {
                Vector3 targetDirection = targetPosition - cameraScript.cameraMainTransform.position;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                cameraScript.cameraMainTransform.rotation = Quaternion.Lerp(cameraScript.cameraMainTransform.rotation, targetRotation, Time.deltaTime * speed);
                angle = Vector3.Angle(cameraScript.cameraMainTransform.forward, targetDirection);
                yield return new WaitForEndOfFrame();
            }
        } else {
            while (timeToWait > 0) {
                timeToWait -= Time.deltaTime;
                Vector3 targetDirection = targetPosition - cameraScript.cameraMainTransform.position;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                cameraScript.cameraMainTransform.rotation = Quaternion.Lerp(cameraScript.cameraMainTransform.rotation, targetRotation, Time.deltaTime * speed);
                yield return new WaitForEndOfFrame();
            }
        }
        focusCameraCoroutine = null;
        UnlockCamera();
        yield break;
    }

    public void LookAt(Transform targetTransform, float speed, float timeToWait, bool returnBack) {
        LockCamera();
        if (focusCameraCoroutine != null) {
            StopCoroutine(focusCameraCoroutine);
        }
        if (returnBack && cameraPreviousLookAt == Vector3.zero) {
            cameraPreviousLookAt = cameraScript.cameraMainTransform.transform.position + cameraScript.cameraMainTransform.transform.forward * 5f;
        }
        focusCameraCoroutine = StartCoroutine(SmoothTransitionCamera(targetTransform, speed, timeToWait));
    }
    public void LookAt(Transform targetTransform, float speed) {
        LockCamera();
        if (focusCameraCoroutine != null) {
            StopCoroutine(focusCameraCoroutine);
        }
        focusCameraCoroutine = StartCoroutine(SmoothTransitionCamera(targetTransform, speed, 0f));
    }
    private IEnumerator SmoothTransitionCamera(Transform targetTransform, float speed, float timeToWait) {
        if (timeToWait < 0f) {
            while (true) {
                Vector3 targetDirection = targetTransform.position - cameraScript.cameraMainTransform.position;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                cameraScript.cameraMainTransform.rotation = Quaternion.Lerp(cameraScript.cameraMainTransform.rotation, targetRotation, Time.deltaTime * speed);
                yield return new WaitForEndOfFrame();
            }
        }
        if (timeToWait == 0f) {
            float angle = 90f;
            while (angle > 1f) {
                Vector3 targetDirection = targetTransform.position - cameraScript.cameraMainTransform.position;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                cameraScript.cameraMainTransform.rotation = Quaternion.Lerp(cameraScript.cameraMainTransform.rotation, targetRotation, Time.deltaTime * speed);
                angle = Vector3.Angle(cameraScript.cameraMainTransform.forward, targetDirection);
                yield return new WaitForEndOfFrame();
            }
        } else {
            while (timeToWait > 0) {
                timeToWait -= Time.deltaTime;
                Vector3 targetDirection = targetTransform.position - cameraScript.cameraMainTransform.position;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                cameraScript.cameraMainTransform.rotation = Quaternion.Lerp(cameraScript.cameraMainTransform.rotation, targetRotation, Time.deltaTime * speed);
                yield return new WaitForEndOfFrame();
            }
        }
        focusCameraCoroutine = null;
        UnlockCamera();
        yield break;
    }

    #endregion

    //Kinda
    #region CameraTransition

    private Coroutine delayedUnlockCameraPositionCoroutine;

    public void SetCameraTransitionPosition(Vector3 position) {
        if (delayedUnlockCameraPositionCoroutine != null) {
            StopCoroutine(delayedUnlockCameraPositionCoroutine);
        }
        LockMovement();
        cameraScript.cameraTransitionPosition = position;
    }

    public void SetCameraTransitionPosition(Vector3 position, float time) {
        if (delayedUnlockCameraPositionCoroutine != null) {
            StopCoroutine(delayedUnlockCameraPositionCoroutine);
        }
        LockMovement();
        cameraScript.cameraTransitionPosition = position;
        StartCoroutine(DelayedUnlockCameraPosition(time));
    }

    private IEnumerator DelayedUnlockCameraPosition(float time) {
        yield return new WaitForSeconds(time);
        RemoveCameraTransitionPosition();
    }

    public void RemoveCameraTransitionPosition() {
        if (delayedUnlockCameraPositionCoroutine != null) {
            StopCoroutine(delayedUnlockCameraPositionCoroutine);
        }
        UnlockMovement();
        cameraScript.cameraTransitionPosition = Vector3.zero;
    }

    #endregion

    //Kinda
    #region InputOptions

    public void LockAllInput() {
        LockCamera();
        LockMovement();
    }

    public void UnlockAllInput() {
        UnlockCamera();
        UnlockMovement();
    }

    public void LockItemRelatedInput() {
        inputScript.lockItemRelatedButtons = true;
    }

    public void UnlockItemRelatedInput() {
        inputScript.lockItemRelatedButtons = false;
    }

    public void LockMovementRelatedInput() {
        inputScript.lockMovementRelatedButtons = true;
    }

    public void UnlockMovementRelatedInput() {
        inputScript.lockMovementRelatedButtons = false;
    }

    public void LockCameraRelatedInput() {
        inputScript.lockCameraRelatedButtons = true;
    }

    public void UnlockCameraRelatedInput() {
        inputScript.lockCameraRelatedButtons = false;
    }

    #endregion

    //Kinda
    #region Subtitles

    private Coroutine dialogueLinesChangeCoroutine;

    private List<SubtitlesLine> subtitlesLines;
    private int curLine;
    private bool waitToContinueDialogue = false;
    private bool skipDialogueLine = false;

    public delegate void ChangedSubtitlesLines(string tags);
    public event ChangedSubtitlesLines OnChangedSubtitlesLines;

    public delegate void EndSubtitlesLines();
    public event EndSubtitlesLines OnEndSubtitlesLines;

    public void SubtitlesNewLine(SubtitlesLine line, int priority, float duration) {
        subtitlesScript.NewLine(line, priority, duration);
    }

    public void ClearLine() {
        subtitlesScript.ClearLine();
    }

    public void NewDialogue(List<SubtitlesLine> lines) {
        if (dialogueLinesChangeCoroutine != null) {
            StopCoroutine(dialogueLinesChangeCoroutine);
            if (OnEndSubtitlesLines != null) {
                OnEndSubtitlesLines();
            }
        }
        subtitlesLines = lines;
        curLine = 0;
        dialogueLinesChangeCoroutine = StartCoroutine(DialogueLinesChange());
    }

    private string[] GetTags() {
        return subtitlesLines[curLine].tags.Split('_');
    }

    private bool GotTag(string[] tags, string tag) {
        foreach (string item in tags) {
            if (tag == item)
                return true;
        }
        return false;
    }

    public void ContinueDialogue() {
        waitToContinueDialogue = false;
    }

    public void SkipLine() {
        skipDialogueLine = true;
    }

    private IEnumerator DialogueLinesChange() {
        SubtitlesNewLine(subtitlesLines[curLine], 99, 0f);
        if (OnChangedSubtitlesLines != null) {
            OnChangedSubtitlesLines(subtitlesLines[curLine].tags);
        }
        string[] lineTags = GetTags();
        yield return new WaitForEndOfFrame();
        while (curLine < subtitlesLines.Count) {
            if (inputScript.nextLineButtonPressed || skipDialogueLine) {
                if (subtitlesScript.IsPrintingLine()) {
                    if (!GotTag(lineTags, "cantSpeedUp"))
                        subtitlesScript.SpeedUpLine(5f);
                } else {
                    if (!waitToContinueDialogue) {
                        skipDialogueLine = false;
                        curLine++;
                        if (curLine >= subtitlesLines.Count)
                            break;
                        SubtitlesNewLine(subtitlesLines[curLine], 99, 0f);
                        lineTags = GetTags();
                        if (OnChangedSubtitlesLines != null) {
                            OnChangedSubtitlesLines(subtitlesLines[curLine].tags);
                        }
                        if (GotTag(lineTags, "waitToContinue"))
                            waitToContinueDialogue = true;
                        if (GotTag(lineTags, "skipLine"))
                            skipDialogueLine = true;
                    }
                }
            }
            yield return new WaitForEndOfFrame();
        }

        ClearLine();
        subtitlesLines = new List<SubtitlesLine>();
        curLine = 0;
        if (OnEndSubtitlesLines != null) {
            OnEndSubtitlesLines();
        }
        dialogueLinesChangeCoroutine = null;
        yield break;
    }

    #endregion

    //Nothing
    #region CheckVisibility



    #endregion

    //Not so much
    #region ItemOptions

    public void ForcedRelease() {
        interactionScript.grabbedObj.RemoveGrabbed();
    }

    public void ForcedRelease(InteractableParent script) {
        if (script != interactionScript.grabbedObj.grabbableScript)
            return;
        interactionScript.grabbedObj.RemoveGrabbed();
    }

    public InteractableParent GetInteractable() {
        return interactionScript.takenObj.takeableScript;
    }

    public bool GotTagInHands(string tag) {
        if (!interactionScript.takenObj.IsHolding())
            return false;
        return GetInteractable().GotObjTag(tag);
    }

    #endregion

}

[System.Serializable]
public class ForceClass {
    public float speedMultiplyer;
    public float heighOffset;
    public bool hiden;
    public bool shouldCrouch;
    public bool shouldStand;
    public bool lockCrouch;
    public bool lockStand;

    public ForceClass(float speedMultiplyer) {
        this.speedMultiplyer = speedMultiplyer;
        this.heighOffset = 0f;
        this.hiden = false;
        this.shouldCrouch = false;
        this.shouldStand = false;
        this.lockCrouch = false;
        this.lockStand = false;
    }

    public ForceClass(float speedMultiplyer, float heighOffset) {
        this.speedMultiplyer = speedMultiplyer;
        this.heighOffset = heighOffset;
        this.hiden = false;
        this.shouldCrouch = false;
        this.shouldStand = false;
        this.lockCrouch = false;
        this.lockStand = false;
    }

    public ForceClass(float speedMultiplyer, float heighOffset, bool hiden, bool shouldCrouch, bool shouldStand, bool lockCrouch, bool lockStand) {
        this.speedMultiplyer = speedMultiplyer;
        this.heighOffset = heighOffset;
        this.hiden = hiden;
        this.shouldCrouch = shouldCrouch;
        this.shouldStand = shouldStand;
        this.lockCrouch = lockCrouch;
        this.lockStand = lockStand;
    }
}

[System.Serializable]
public class SubtitlesLine {
    public string line;
    public string tags;
    public float lineSpeed;
    public Color lineColor;

    public SubtitlesLine(string line) {
        this.line = line;
        this.tags = "";
        this.lineSpeed = 12f;
        this.lineColor = Color.white;
    }

    public SubtitlesLine(string line, float lineSpeed) {
        this.line = line;
        this.tags = "";
        this.lineSpeed = lineSpeed;
        this.lineColor = Color.white;
    }

    public SubtitlesLine(string line, float lineSpeed, Color lineColor) {
        this.line = line;
        this.tags = "";
        this.lineSpeed = lineSpeed;
        this.lineColor = lineColor;
    }

    public SubtitlesLine(string line, string tags, float lineSpeed, Color lineColor) {
        this.line = line;
        this.tags = tags;
        this.lineSpeed = lineSpeed;
        this.lineColor = lineColor;
    }
}