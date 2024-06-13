using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Sandbox : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField]
    private Transform center;
    
    public Transform terrain;


    [SerializeField]
    private float targetDeadZoneDistance = 2.5f;

    public bool sandboxMode = false;   
    public bool isReady = false;

    public static Sandbox instance;

    public Vector3 moveDelta {get; private set;} = Vector3.zero;

    void Awake(){
        if(instance == null){
            instance = this;
        }
        else{
            Destroy(gameObject);
        }
    }

    void Start(){
    }

    void Update(){
        if (isReady && InputManager.instance.jumped)
            sandboxMode = !sandboxMode;

        if (!isReady)
            sandboxMode = false;

        if (!sandboxMode)
        {
            MovementScript.movementScript.movementEnabled = true;
            CameraScript.cameraScript.rotatingEnabled = true;
            InteractionScript.interactionScript.isInteractionEnabled = true;
            CameraShakeScript.cameraShakeScript.enabled = true;
            return;
        }
        else{
            MovementScript.movementScript.movementEnabled = false;
            CameraScript.cameraScript.rotatingEnabled = false;
            InteractionScript.interactionScript.isInteractionEnabled = false;
            CameraShakeScript.cameraShakeScript.enabled = false;
        }
        
    }
    void FixedUpdate(){
        if (!sandboxMode)
            return;
        Vector3 delta = Vector3.ProjectOnPlane(target.position - center.position, Vector3.up);
        if(delta.magnitude > targetDeadZoneDistance){
            moveDelta = delta.normalized * (delta.magnitude-targetDeadZoneDistance);
            terrain.position -= moveDelta;
        }
    }

}
