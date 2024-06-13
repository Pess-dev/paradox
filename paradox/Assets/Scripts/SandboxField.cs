using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Events;

public class SandboxField : MonoBehaviour
{
    void OnTriggerEnter(Collider other){
        SandboxObject obj = other.GetComponentInParent<SandboxObject>();
        if (obj != null){
            obj.SetVisibility(true);
            if (!obj.isInSandbox)
                obj.EnterSandbox();
        }
    }

    void OnTriggerExit(Collider other){
        SandboxObject obj = other.GetComponentInParent<SandboxObject>();
        if (obj != null){
            obj.SetVisibility(false);
            //obj.ExitSandbox();
        }
    }
}