using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayField : MonoBehaviour
{
    void OnTriggerEnter(Collider other){
        Camera obj = other.GetComponentInParent<Camera>();
        if (obj != null){
            Sandbox.instance.isReady = true;
        }
    }
    
    void OnTriggerExit(Collider other){
        Camera obj = other.GetComponentInParent<Camera>();
        if (obj != null){
            Sandbox.instance.isReady = false;
        }
    }
}
