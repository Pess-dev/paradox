using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableSandboxObject : GrabbableParent
{
    public bool isGrabbed = false;
    public UnityEvent<bool> IsGrabbedChanged = new UnityEvent<bool>();
    override public void OnStartGrab(){
        isGrabbed = true;
        IsGrabbedChanged.Invoke(isGrabbed);
    }

    override public void OnEndGrab(){
        isGrabbed = false;
        IsGrabbedChanged.Invoke(isGrabbed);
    }
}
