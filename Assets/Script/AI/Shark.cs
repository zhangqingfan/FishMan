using BehaviourTree;
using UnityEngine;

public class SharkAI : Node
{
    public SharkAI(MonoBehaviour mono) : base(mono)
    {

    }
}



public class Shark : MonoBehaviour
{
    Animator animator;
    SteerBehaviour steerBehaviour;
    SharkAI sharkAI;

    void Start()
    {
        steerBehaviour = GetComponent<SteerBehaviour>();
        animator = GetComponent<Animator>();
        sharkAI = new SharkAI(this);

        //TODO....
        animator.SetBool("Attack", false);


    }

    void Update()
    {
        
    }
}
