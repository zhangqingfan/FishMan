using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    static public PlayerController Instance { get; private set; }
    private Controller controller;

    private void Awake()
    {
        Instance = this;
        controller = new Controller();
    }

    private void OnEnable()
    {
        controller.Enable();
    }

    private void OnDisable()
    {
        controller.Disable();
    }

    public Vector2 GetMovement()
    {
        return controller.PC.Move.ReadValue<Vector2>(); 
    }

    //TODO...
    public bool IsHold()  
    {
        var b = controller.PC.RightClick.ReadValue<float>();
        return b > 0;
    }
}
