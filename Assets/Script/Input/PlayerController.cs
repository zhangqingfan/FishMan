using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    static public PlayerController Instance { get; private set; }
    Controller controller;

    [HideInInspector]
    public bool isHold = false;

    [HideInInspector]
    public bool showRing = false;

    [HideInInspector]
    public bool fire = false;

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
        StartCoroutine(MonitorLeftClick());
    }

    public Vector2 GetMovement()
    {
        return controller.PC.Move.ReadValue<Vector2>(); 
    }

    public Vector2 GetMouseOffset()
    {
        return controller.PC.MouseOffset.ReadValue<Vector2>();
    }

    public Vector2 GetMouseScroll() 
    {
        return controller.PC.MouseScroll.ReadValue<Vector2>();
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
                {
                    isHold = true;
                }
                continue;
            }

            isHold = false;
            holdFrameCount = 0;
        }
    }

    public IEnumerator MonitorLeftClick()
    {
        while (true)
        {
            yield return null;

            if (controller.PC.RightClick.ReadValue<float>() > 0)
            {
                showRing = false;
                continue;
            }

            if (controller.PC.LeftClick.ReadValue<float>() > 0)
            {
                if(showRing == false)
                {
                    showRing = true;
                    continue;
                }

                fire = true;
            }
        }
    }
}
