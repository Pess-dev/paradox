using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Sandbox : MonoBehaviour
{
    [SerializeField]
    private CharacterController playerController;
    [SerializeField]
    private float playerSpeed = 0.1f;

    [SerializeField]
    private Transform center;
    [SerializeField]
    private Transform terrain;
    //private float terrainSpeed = 0.2f;

    [SerializeField]
    private float MoveDeadZoneDistance = 0.1f;
    [SerializeField]
    private float MoveMaxDistance = 5f;

    //private Vector3 _startSandboxPosition = Vector3.zero;
    
    //private Vector3 _lastPlayerPosition;

    [SerializeField]
    private bool sandboxMode = false;

    InputManager inputManager;
    public static Sandbox instance;


    void Awake(){
        if(instance == null){
            instance = this;
        }
        else{
            Destroy(gameObject);
        }
    }

    void Start(){
        inputManager = InputManager.instance;
       // _startSandboxPosition = terrain.position;
    }

    void FixedUpdate(){

        if (!sandboxMode)
            return;

        //print(inputManager.moveDirectionWorld+" "+inputManager.lookDelta+" "+inputManager.jump+" "+inputManager.interact);

        //_lastPlayerPosition = playerController.transform.position;


        //Перемещение игрока
        playerController.Move(inputManager.moveDirectionWorld * playerSpeed *Time.fixedDeltaTime);
        
        //Vector3 playerDelta = Vector3.ProjectOnPlane(playerObject._sandboxPosition, Vector3.up);
        
        //if (playerDelta.magnitude > MoveMaxDistance){
        //     playerObject._sandboxPosition -= playerDelta.normalized * (playerDelta.magnitude-MoveMaxDistance);
        //}


        //Перемещение карты
        Vector3 delta = Vector3.ProjectOnPlane(playerController.transform.position - center.position, Vector3.up);
        if(delta.magnitude > MoveDeadZoneDistance){
            terrain.position -= delta.normalized * (delta.magnitude-MoveDeadZoneDistance);
        }

        // Vector3 boundDelta = Vector3.ProjectOnPlane(terrain.position -center.position  , Vector3.up);
        // if (boundDelta.magnitude > Math.Abs(terrainSize-sandboxSize)){
        //     terrain.position = terrain.position - boundDelta.normalized *(boundDelta.magnitude-Math.Abs(terrainSize-sandboxSize));
        // }
        
    }

    public Vector3 GetPosition(Vector3 sandboxPosition){
        return  terrain.position + sandboxPosition;
    } 
    
    public Vector3 GetSandboxPosition(Vector3 position){
        return  position - terrain.position;
    }
}
