using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(SandboxObject))]
public class PlayerFigure : MonoBehaviour
{
    [SerializeField]
    private float playerSpeed = 3f;
    [HideInInspector]
    public SandboxObject playerObj;
    void Start(){
        playerObj = GetComponent<SandboxObject>();
    }

    void FixedUpdate(){
        if (!Sandbox.instance.sandboxMode)
            return;
        playerObj.controller.Move(InputManager.instance.moveDirectionWorld * playerSpeed * Time.fixedDeltaTime);
    }
}
