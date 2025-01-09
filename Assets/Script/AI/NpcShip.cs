using BehaviourTree;
using System.Collections;
using UnityEngine;

public class ShipAI : BehaviourTree.Node
{
    NpcShip ship;
    Selector selector;
    public ShipAI(NpcShip ship) 
    { 
        this.ship = ship;
        selector = new Selector();
        var greet = new Greeting(ship);
        var wander = new ShipWander(ship);
        selector.nodeList.Add(greet);
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

public class Greeting : Node
{
    Transform playerShip;
    NpcShip npcShip;
    Transform helloPanelTrans;

    public Greeting(NpcShip npcShip)
    {
        playerShip = GameObject.Find("Ship").transform;
        this.npcShip = npcShip;
    }
    public override IEnumerator Exec()
    {
        while(true)
        {
            yield return null;

            var dis = (playerShip.position - npcShip.transform.position).magnitude;
            //Debug.Log(dis);
            if(dis > 12f)
            {
                result = ExecResult.Failure;
                yield break;
            }

           
        }
    }
}

public class ShipWander : Node
{
    NpcShip npcShip;
    public ShipWander(NpcShip npcShip)
    {
        this.npcShip = npcShip;
    }

    public override IEnumerator Exec()
    {
        Debug.Log("wandering");
        result = ExecResult.Success;
        yield break;
    }
}

public class NpcShip : Actor
{
    ShipAI shipAI;

    void Start()
    {
        steerBehaviour = GetComponent<SteerBehaviour>();
        shipAI = new ShipAI(this);
    }

    void Update()
    {
        shipAI.Update();
    }
}