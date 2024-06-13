using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(InteractableSandboxObject))]
public class SandboxObject : MonoBehaviour
{
    public float gravitySpeed = -0.5f;
   
    public bool isInField = false;
    public bool isInSandbox = false;

    [SerializeField]
    private int sandboxLayer = 7;
    [SerializeField]
    private int originalLayer = 9;

    public CharacterController controller{get; private set;}
    Sandbox _sandbox;

    Rigidbody _rb;
    InteractableSandboxObject _interactableSandboxObject;

    private float rotateLerp = 20f;

    void Start()
    {
        _sandbox = Sandbox.instance;
        controller = GetComponent<CharacterController>();
        _rb = GetComponent<Rigidbody>();
        _interactableSandboxObject = GetComponent<InteractableSandboxObject>();
        _interactableSandboxObject.IsGrabbedChanged.AddListener(Grabbed);
        ExitSandbox();
        _rb.interpolation = RigidbodyInterpolation.None;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void FixedUpdate(){
        if (!isInSandbox)
            return;
        controller.Move(new Vector3(0, gravitySpeed*Time.fixedDeltaTime, 0));
        //rotate to match with Y
        transform.rotation *= Quaternion.FromToRotation(transform.up,Vector3.Slerp(transform.up, Vector3.up, Time.deltaTime * rotateLerp));
    }

    public void SetVisibility(bool isVisible){
        this.isInField = isVisible;
        
        if (isInSandbox) 
            _interactableSandboxObject.isInteractable = isVisible;
    }

    public void EnterSandbox(){
        if (!CanEnterSandbox()) 
            return;
        isInSandbox = true;

        transform.SetParent(_sandbox.terrain.transform);
        SetGameLayerRecursive(gameObject, sandboxLayer);
        _rb.isKinematic = true;
        controller.enabled = true;
        MeshRenderer mr = GetComponentInChildren<MeshRenderer>();
        if (mr != null){
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        
        // Collider collider = GetComponentInChildren<Collider>();
        // if (collider != null){
        //     collider.isTrigger = true;
        // }
        

        SetVisibility(true);
    }
    public void ExitSandbox(){
        isInSandbox = false;
        _rb.isKinematic = false;
        controller.enabled = false;
        SetGameLayerRecursive(gameObject, originalLayer);
        MeshRenderer mr = GetComponentInChildren<MeshRenderer>();
        if (mr != null){
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
        transform.SetParent(null);

        // Collider collider = GetComponentInChildren<Collider>();
        // if (collider != null){
        //     collider.isTrigger = false;
        // }
        
        _interactableSandboxObject.isInteractable = true;
    }
    public bool CanEnterSandbox(){
        return !_interactableSandboxObject.isGrabbed;
    }
    public void Grabbed(bool isGrabbed){
       // print("Grabbed: " + isGrabbed+" sandbox: "+isInSandbox+" field: "+isInField);
        if (isGrabbed){
            SetVisibility(false);
            ExitSandbox();
        }
        if (!isGrabbed&&isInField){
            EnterSandbox();
        }
    }
    private void SetGameLayerRecursive(GameObject _go, int _layer)
        {
            _go.layer = _layer;
            foreach (Transform child in _go.transform)
            {
                child.gameObject.layer = _layer;
 
                Transform _HasChildren = child.GetComponentInChildren<Transform>();
                if (_HasChildren != null)
                    SetGameLayerRecursive(child.gameObject, _layer);
             
            }
        }
}