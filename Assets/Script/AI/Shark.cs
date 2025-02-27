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
        //var pursue = new SharkPursue(shark);
        //selector.nodeList.Add(pursue);
        selector.nodeList.Add(wander);
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
    Transform targetTrans;
    Vector3 originalLocalPos;
    public SharkWander(Shark shark) 
    { 
        this.shark = shark;
        originalLocalPos = shark.transform.localPosition;
        targetTrans = new GameObject("Shark Target").transform;
        targetTrans.SetParent(shark.transform.parent);
        targetTrans.transform.localPosition = originalLocalPos;
    }
    
    public override IEnumerator Exec()
    {
        var localTargetPos = Random.onUnitSphere * 50f + originalLocalPos;
        var worldTargetPos = targetTrans.parent.transform.TransformPoint(localTargetPos);
        worldTargetPos.y = Mathf.Clamp(worldTargetPos.y, WorldManager.fishHeight, 1.5f - Water.depth);
        localTargetPos = targetTrans.parent.transform.InverseTransformPoint(worldTargetPos);
        targetTrans.localPosition = localTargetPos;

        var wanderTime = Random.Range(2f, 10f);
        var passTime = 0f;

        while (true)
        {
            yield return null;
      
            if(passTime < wanderTime)
            {
                passTime += Time.deltaTime;
                shark.steerBehaviour.Arrive(targetTrans.position);
                continue;
            }
            
            break;
        }

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
                    //shark.steerBehaviour.Wander(false);
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
