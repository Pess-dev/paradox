using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SandboxObject : MonoBehaviour
{
    public Vector3 _sandboxPosition {get; set;}

    //Sandbox sandbox;

    public float gravitySpeed = -2f;

    private CharacterController _controller;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        // sandbox = Sandbox.instance;
        // _sandboxPosition = sandbox.GetSandboxPosition(transform.position);
    }

    void Update(){
        _controller.Move(new Vector3(0, gravitySpeed*Time.deltaTime, 0));
        //transform.position = sandbox.GetPosition(_sandboxPosition);
    }
}