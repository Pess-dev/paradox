using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionScript : MonoBehaviour
{
    #region Singletones
    public static InteractionScript interactionScript { get; private set; }

    private InputManager inputManager;
    private UIScript uiScript;
    //private SceneManagerScript sceneManager;
    #endregion

    private Transform cameraTransform;

    public class GrabbedObj {
        public GrabbableParent grabbableScript;
        public Vector3 grabOffset;
        public float distance;
        public float absDistance;

        public bool IsGrabbing() {
            return (grabbableScript != null);
        }

        public void RemoveGrabbed() {
            grabOffset = Vector3.zero;
            distance = 0;
            if (grabbableScript != null)
                grabbableScript.OnEndGrab();
            grabbableScript = null;
        }

        public GrabbedObj() {
            grabbableScript = null;
            grabOffset = Vector3.zero;
            distance = 0;
        }
    }

    public class TakenObj {
        public GrabbableParent takeableScript;

        public bool IsHolding() {
            return (takeableScript != null);
        }

        public void TakenRBChange() {
            if(takeableScript != null) {
                //takeableScript.ObjRB.gameObject.layer = 8;
                //rb.useGravity = false;
                takeableScript.ObjRB.isKinematic = true;
                takeableScript.ObjRB.velocity = Vector3.zero;
                takeableScript.ObjRB.angularVelocity = Vector3.zero;
                takeableScript.OnStartTake();
            }
        }

        public void RemoveTaken() {
            if (takeableScript != null) {
                //takeableScript.ObjRB.gameObject.layer = 7;
                //rb.useGravity = true;
                takeableScript.ObjRB.isKinematic = false;
                takeableScript.ObjRB.velocity = Vector3.zero;
                takeableScript.ObjRB.angularVelocity = Vector3.zero;
                takeableScript.OnEndTake();
            }   
            takeableScript = null;
        }

        public TakenObj() {
            takeableScript = null;
        }
    }

    [SerializeField]
    private float grabForce = 100; //100
    [SerializeField]
    private float grabFriction = 3; //3
    [SerializeField]
    private float interactionRadius = 2;
    [SerializeField]
    private float interactionAngle = 30f;
    [SerializeField]
    private Vector3 takeOffset = Vector3.zero;

    [SerializeField]
    private LayerMask interactionLayerMask;
    [SerializeField]
    private LayerMask grabbedLayerMask;

    private RaycastHit raycastObjHit;
    private InteractableParent raycastInteractableScript = null;
    private GrabbableParent raycastGrabbableScript = null;

    public GrabbedObj grabbedObj { get; private set; }
    public TakenObj takenObj { get; private set; }

    private Coroutine smoothReleaseObjectCoroutine;

    private void Awake() {
        if (interactionScript == null)
            interactionScript = this;
    }

    private void Start() {
        cameraTransform = CameraScript.cameraScript.cameraMainTransform;
        inputManager = InputManager.instance;
        uiScript = UIScript.uiScript;
        grabbedObj = new GrabbedObj();
        takenObj = new TakenObj();
    }

    private void Update() {
        #region RayCast
        Collider[] colliders = Physics.OverlapSphere(cameraTransform.position, interactionRadius, interactionLayerMask);
        raycastInteractableScript = null;
        foreach (Collider item in colliders) {
            if (item.GetComponent<InteractableParent>() == null) 
                continue;
                
            float angle = Vector3.Angle(cameraTransform.forward, item.transform.position - cameraTransform.position);
            
            if (raycastInteractableScript == null)
                { if (angle <= interactionAngle && Vector3.Dot(cameraTransform.forward, item.transform.position - cameraTransform.position) > 0)
                    raycastInteractableScript = item.GetComponent<InteractableParent>();}
            else
            {
                float oldAngle = Vector3.Angle(cameraTransform.forward, raycastInteractableScript.transform.position - cameraTransform.position);
                if (angle <= interactionAngle && angle < oldAngle && Vector3.Dot(cameraTransform.forward, item.transform.position - cameraTransform.position) > 0)
                    raycastInteractableScript = item.GetComponent<InteractableParent>();
            }
        }
        
        // if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out raycastObjHit, interactionRadius, interactionLayerMask)) {
        //     raycastInteractableScript = raycastObjHit.transform.GetComponent<InteractableParent>();
        // } else {
        //     raycastInteractableScript = null;
        // }
        // if (raycastInteractableScript == null) {
        //     if (Physics.SphereCast(cameraTransform.position, 0.15f, cameraTransform.forward, out raycastObjHit, interactionRadius, interactionLayerMask)) {
        //         raycastInteractableScript = raycastObjHit.transform.GetComponent<InteractableParent>();
        //     } else {
        //         raycastInteractableScript = null;
        //     }
        // }

        if (grabbedObj.IsGrabbing()) {
            raycastInteractableScript = null;
        }

        if (raycastInteractableScript != null)
            uiScript.SetInteractImagePosition(raycastInteractableScript.transform.position);
        else
            uiScript.SetInteractImagePosition(Vector3.zero);
        #endregion

        #region Intaraction
        if (inputManager.interacted && raycastInteractableScript != null && !(takenObj.IsHolding() && takenObj.takeableScript == raycastInteractableScript)) {
            raycastInteractableScript.OnInteract();
        }else if (inputManager.interacted && raycastInteractableScript == null && grabbedObj.IsGrabbing()) {
            grabbedObj.grabbableScript.OnInteract();
        }
        #endregion

        #region GrabNewObj
        if (raycastInteractableScript != null) {
            if (!raycastInteractableScript.TryGetComponent(out raycastGrabbableScript))
                raycastGrabbableScript = null;
        } else {
            raycastGrabbableScript = null;
        }

        if (inputManager.grabbed && !grabbedObj.IsGrabbing() && raycastGrabbableScript != null) {
            grabbedObj.grabbableScript = raycastGrabbableScript;
            grabbedObj.grabOffset = raycastGrabbableScript.ObjRB.transform.InverseTransformPoint(raycastGrabbableScript.transform.position);
            grabbedObj.distance = Vector3.Magnitude(raycastGrabbableScript.transform.position - cameraTransform.position);
            grabbedObj.absDistance = grabbedObj.distance;
            grabbedObj.grabbableScript.OnStartGrab();
        }
        #endregion

        #region Take/ReleaseNewObj
        if (inputManager.taked) {
            if (takenObj.IsHolding()) {
                if (smoothReleaseObjectCoroutine == null)
                    smoothReleaseObjectCoroutine = StartCoroutine(SmoothReleaseObject());
            } else {
                if (grabbedObj.IsGrabbing() && grabbedObj.grabbableScript.CanTake) {
                    GrabbableParent tempInteractionScript = grabbedObj.grabbableScript;
                    grabbedObj.RemoveGrabbed();
                    takenObj.takeableScript = tempInteractionScript;
                    takenObj.TakenRBChange();
                }
            }
        }
        #endregion

        if (grabbedObj.IsGrabbing()) {
            grabbedObj.absDistance += inputManager.scroll * Time.deltaTime * 5f;
            grabbedObj.absDistance = Mathf.Clamp(grabbedObj.absDistance, 0.5f, 2f);
            grabbedObj.distance = Mathf.Lerp(grabbedObj.distance, grabbedObj.absDistance, Time.deltaTime * 7);
        }

        if (takenObj.IsHolding() && smoothReleaseObjectCoroutine == null) {
            Vector3 targetPos = CameraScript.cameraScript.transform.position;
            targetPos +=  CameraScript.cameraScript.transform.rotation * takeOffset;
            // targetPos -= CameraScript.cameraScript.transform.up * 0.3f;
            // targetPos -= CameraScript.cameraScript.transform.right * -0.5f;
            // targetPos -= CameraScript.cameraScript.transform.forward * -0.3f;
            targetPos += Quaternion.Euler(0f, CameraScript.cameraScript.cameraMainTransform.rotation.eulerAngles.y, 0f) * 
                takenObj.takeableScript.TakeOffset;

            takenObj.takeableScript.ObjRB.transform.position = Vector3.Lerp(takenObj.takeableScript.ObjRB.transform.position, targetPos, Time.deltaTime * 30f);
            takenObj.takeableScript.ObjRB.transform.rotation = Quaternion.Lerp(takenObj.takeableScript.ObjRB.transform.rotation,
                Quaternion.Euler(0f, CameraScript.cameraScript.cameraMainTransform.rotation.eulerAngles.y, 0f), Time.deltaTime * 7f);

            takenObj.takeableScript.ObjRB.Sleep();
        }
    }

    private IEnumerator SmoothReleaseObject() {
        Vector3 targetPos = CameraScript.cameraScript.transform.position;
        targetPos -= CameraScript.cameraScript.transform.up * 0.3f;
        Quaternion targetRot = takenObj.takeableScript.ObjRB.transform.rotation * 
            Quaternion.Euler(new Vector3((Random.value * 2 - 1) * 15f, (Random.value * 2 - 1) * 30f, (Random.value * 2 - 1) * 15f));
        takenObj.takeableScript.ObjRB.transform.position = targetPos;
        // while (Vector3.SqrMagnitude(targetPos - takenObj.takeableScript.ObjRB.transform.position) >= 0.01f) {
        //     takenObj.takeableScript.ObjRB.transform.position = Vector3.Lerp(takenObj.takeableScript.ObjRB.transform.position, targetPos, Time.deltaTime * 30f);
        //     takenObj.takeableScript.ObjRB.transform.rotation = Quaternion.Lerp(takenObj.takeableScript.ObjRB.transform.rotation, targetRot, Time.deltaTime * 7f);
        //     yield return new WaitForEndOfFrame();
        // }
        yield return new WaitForEndOfFrame();
        takenObj.RemoveTaken();
        smoothReleaseObjectCoroutine = null;
    }

    private void FixedUpdate() {
        #region GrabObjects
        if (!inputManager.grab && grabbedObj.IsGrabbing()) {
            grabbedObj.RemoveGrabbed();
        }

        if (grabbedObj.IsGrabbing()) {
            RaycastHit grabRaycastHit;
            Physics.Linecast(grabbedObj.grabbableScript.ObjRB.transform.TransformPoint(grabbedObj.grabOffset),
                cameraTransform.position + cameraTransform.forward * grabbedObj.distance, out grabRaycastHit, grabbedLayerMask);
            Vector3 targetPoint = (grabRaycastHit.point == Vector3.zero) ? 
            cameraTransform.position + cameraTransform.forward * grabbedObj.distance : grabRaycastHit.point;
            //Vector3 targetPoint = cameraTransform.position + cameraTransform.forward * grabbedObj.distance;
            if (Vector3.SqrMagnitude(grabbedObj.grabbableScript.ObjRB.transform.TransformPoint(grabbedObj.grabOffset) - 
                (cameraTransform.position + cameraTransform.forward * grabbedObj.distance)) <= 2f) {
                Vector3 direction = targetPoint - grabbedObj.grabbableScript.ObjRB.transform.TransformPoint(grabbedObj.grabOffset);
                Vector3 addVelocity = grabForce * direction / grabbedObj.grabbableScript.ObjRB.mass;
                Vector3 newVelocity = grabbedObj.grabbableScript.ObjRB.velocity + addVelocity;
                newVelocity /= grabFriction;
                addVelocity = Vector3.ClampMagnitude(newVelocity - grabbedObj.grabbableScript.ObjRB.velocity, addVelocity.magnitude);

                grabbedObj.grabbableScript.ObjRB.AddForceAtPosition(addVelocity * 25, grabbedObj.grabbableScript.ObjRB.transform.TransformPoint(grabbedObj.grabOffset), ForceMode.Force);
            } else {
                grabbedObj.RemoveGrabbed();
            }
        }
        #endregion
    }
}
