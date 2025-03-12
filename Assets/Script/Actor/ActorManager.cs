using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SteerBehaviour))]
public class Actor : MonoBehaviour
{
    [HideInInspector]
    public SteerBehaviour steerBehaviour;

    private void OnEnable()
    {
        ActorManager.Instance.AddActor(this);
    }

    private void OnDisable()
    {
        ActorManager.Instance.RemoveActor(this);
    }

    void Start()
    {
        steerBehaviour = GetComponent<SteerBehaviour>();
    }
}

public class ActorManager
{
    List<Actor> actorList = new List<Actor>();
    static ActorManager _instance = new ActorManager();

    public static ActorManager Instance => _instance;

    public void AddActor(Actor actor)
    {
        RemoveActor(actor);
        actorList.Add(actor);
    }

    public void RemoveActor(Actor actor)
    {
        foreach (var item in actorList)
        {
            if (item == actor)
            {
                actorList.Remove(item);
                return;
            }   
        }
    }

    public List<T> GetActorInRange<T>(Vector3 centerPoint, float radius) where T : Actor
    {
        var list = new List<T>();
        foreach (var item in actorList)
        {
            if(Vector3.Distance(centerPoint, item.transform.position) <= radius && item is T)
                list.Add(item as T);
        }
        return list;
    }
}
