using BehaviourTree;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SharkAI : BehaviourTree.Node
{
    Selector selector;
    public SharkAI(Shark shark)
    {
        selector = new Selector();
        var wander = new SharkWander(shark);
        var pursue = new SharkPursue(shark);
        selector.nodeList.Add(pursue);
        selector.nodeList.Add(wander);

        //not necessary;
        nodeList.Add(selector);
    }

    public void Update()
    {
        if (selector.result == ExecResult.Success || selector.result == ExecResult.Failure)
        {
            mono.StartCoroutine(selector.Exec());
        }
    }
}

public class SharkWander : BehaviourTree.Node
{
    Shark shark;
    public SharkWander(Shark shark) { this.shark = shark; }

    public override IEnumerator Exec()
    {
        shark.steerBehaviour.Wander(true);
        result = ExecResult.Success;
        yield break;
    }
}

public class SharkPursue: BehaviourTree.Node
{
    Shark shark;
    
    public SharkPursue(Shark shark) { this.shark = shark; }
    
    public override IEnumerator Exec()
    {
        Transform target = null;
        
        while (true)
        {
            yield return null;

            if(target != null && Vector3.Distance(target.position, shark.gameObject.transform.position) <= 15f)
            {
                shark.steerBehaviour.Seek(target.position);
                continue;
            }
                
            var actors = ActorManager.Instance.GetActorInRange<Actor>(shark.gameObject.transform.position, 20f);
            foreach (var actor in actors) 
            {
                if (actor != shark)
                {
                    shark.steerBehaviour.Wander(false);
                    continue;
                }
            }

            break;
        }
        result = ExecResult.Failure;
    }
}

public class Shark : Actor
{
    Animator animator;
    SharkAI sharkAI;
    void Start()
    {
        steerBehaviour = GetComponent<SteerBehaviour>();
        animator = GetComponent<Animator>();
        sharkAI = new SharkAI(this);
    }

    void Update()
    {
        sharkAI.Update();
    }
}
