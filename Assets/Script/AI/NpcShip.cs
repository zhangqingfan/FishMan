using BehaviourTree;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
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
    Camera renderCamera;
    float length = 18f;

    public Greeting(NpcShip npcShip)
    {
        playerShip = GameObject.Find("Ship").transform;
        this.npcShip = npcShip;
        renderCamera = Camera.main;
    }
    public override IEnumerator Exec()
    {
        var dis = (playerShip.position - npcShip.transform.position).magnitude;
        if (dis > length)
        {
            result = ExecResult.Failure;
            npcShip.greetPanel.localScale = Vector3.zero;
            yield break;
        }

        npcShip.greetPanel.DOScale(new Vector3(1, 1, 1), 0.1f);
        npcShip.steerBehaviour.Stop();

        while (true)
        {
            yield return null;
            npcShip.greetPanel.rotation = renderCamera.transform.rotation;

            dis = (playerShip.position - npcShip.transform.position).magnitude;
            if(dis > length)
            {
                npcShip.greetPanel.DOScale(Vector3.zero, 0.1f);
                result = ExecResult.Failure;
                yield break;
            }
        }
    }
}

public class ShipWander : Node
{
    NpcShip npcShip;
    List<Vector3> wayPoints = new List<Vector3> ();
    int curIndex = 0;

    public ShipWander(NpcShip npcShip)
    {
        this.npcShip = npcShip;
    }

    WayPoint FindRandomWayPoint()
    {
        int index = Random.Range(0, PathFinding.instance.wayPoints.Length);
        return PathFinding.instance.wayPoints.Length == 0 ? null : PathFinding.instance.wayPoints[index];
    }

    public override IEnumerator Exec()
    {
        yield return null;

        if (curIndex >= wayPoints.Count)
        {
            curIndex = 0;
            var endPoint = FindRandomWayPoint();
            if (endPoint == null)
            {
                result = ExecResult.Failure;
                yield break;
            }

            wayPoints = PathFinding.instance.FindPath(npcShip.gameObject.transform.position, endPoint.transform.position);
            foreach (var p in wayPoints)
                Debug.Log(p);
        }

        npcShip.steerBehaviour.Arrive(wayPoints[curIndex]);

        var dis = Vector3.Distance(npcShip.gameObject.transform.position, wayPoints[curIndex]);
        if(dis < 1.0f)
            curIndex++;
        
        result = ExecResult.Success;
        yield break;
    }
}

public class NpcShip : Actor
{
    ShipAI shipAI;
    public Transform greetPanel;

    void Start()
    {
        steerBehaviour = GetComponent<SteerBehaviour>();
        shipAI = new ShipAI(this);
    }

    void Update()
    {
        shipAI.Update();
        Debug.Log(transform.position);
    }
}