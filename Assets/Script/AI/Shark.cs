using BehaviourTree;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SharkAI : BehaviourTree.Node
{
    Selector selector;
    public SharkAI(SteerBehaviour steerBehaviour)
    {
        var selector = new Selector();
       
        var wander = new SharkWander(steerBehaviour);
        var pursue = new SharkPursue(steerBehaviour);
        selector.nodeList.Add(pursue);
        selector.nodeList.Add(wander);

        nodeList.Add(selector);
    }

    public void Update()
    {
        if (selector.result == ExecResult.InProcess)
            return;

        selector.Exec();
    }
}

public class SharkWander : BehaviourTree.Node
{
    SteerBehaviour steerBehaviour;
    public SharkWander(SteerBehaviour steer) { steerBehaviour = steer; }

    public override IEnumerator Exec()
    {
        steerBehaviour.Wander(true);
        result = ExecResult.Success;
        yield break;
    }
}

public class SharkPursue: BehaviourTree.Node
{
    SteerBehaviour steerBehaviour;
    public SharkPursue(SteerBehaviour steer) {  steerBehaviour = steer; }   

    public override IEnumerator Exec()
    {
        while(true) 
        {
            yield return null;

            //TODO...



            steerBehaviour.Wander(false);

            break;
        }

        result = ExecResult.Success;
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
        sharkAI = new SharkAI(steerBehaviour);
    }

    void Update()
    {
        //sharkAI.Update();
    }
}
