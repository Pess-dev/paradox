using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class GrabbableParent : InteractableParent {
    protected Rigidbody objRB;
    //protected Renderer objRenderer;
    [SerializeField]
    protected bool canTake = false;
    public bool canGrab = true;
    [SerializeField]
    protected Vector3 takeOffset = Vector3.zero;

    public Rigidbody ObjRB {
        get { return objRB; }
    }
    public bool CanTake {
        get {
            return canTake;
        }
    }
    public Vector3 TakeOffset {
        get {
            return takeOffset;
        }
    }

    virtual protected void Awake() {
        //base.Awake();
        objRB = GetComponent<Rigidbody>();
        if (objRB == null) {
            objRB = gameObject.AddComponent<Rigidbody>();
        }
        //objRenderer = GetComponent<Renderer>();
    }

    virtual public void OnStartGrab() {
        //print("StartGrab");
    }

    virtual public void OnEndGrab() {
        //print("EndGrab");
    }

    virtual public void OnStartTake() {
        //print("StartTake");
        //objRenderer.sortingOrder = 100000;
    }

    virtual public void OnEndTake() {
        //print("EndTake");
        //objRenderer.sortingOrder = 0;
    }
}
