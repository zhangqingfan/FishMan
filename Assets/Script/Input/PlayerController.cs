using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    static public PlayerController Instance { get; private set; }
    Controller controller;
    public bool isHold = false;

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

    private void Start()
    {
        StartCoroutine(MonitorRightClick());
    }

    public Vector2 GetMovement()
    {
        return controller.PC.Move.ReadValue<Vector2>(); 
    }

    IEnumerator MonitorRightClick()
    {
        int holdFrameCount = 0;

        while (true)
        {
            yield return null;

            if (controller.PC.RightClick.ReadValue<float>() > 0) 
            {
                if(holdFrameCount++ > 10)
                    isHold = true;
                continue;
            }

            isHold = false;
            holdFrameCount = 0;
        }
    } 
}
