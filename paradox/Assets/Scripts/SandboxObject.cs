using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandboxObject : MonoBehaviour
{
    public Vector3 _sandboxPosition {get; set;}

    Sandbox sandbox;

    void Start()
    {
        sandbox = Sandbox.instance;
        _sandboxPosition = sandbox.GetSandboxPosition(transform.position);
    }

    void Update(){
        transform.position = sandbox.GetPosition(_sandboxPosition);
    }
}