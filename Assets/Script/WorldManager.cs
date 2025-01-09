using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class WorldManager : MonoBehaviour
{
    Dictionary<string, AsyncOperationHandle<GameObject>> addressHandles = new Dictionary<string, AsyncOperationHandle<GameObject>>();
    Dictionary<string, Queue<GameObject>> goPool = new Dictionary<string, Queue<GameObject>>();
    WorldManager _instance;
    public WorldManager Instance => _instance;
    public static readonly float height = -10f;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        
    }

    private void OnDestroy()
    {
        foreach(var item in addressHandles.Values)
        {
            Addressables.Release(item);
        }
    }
    public void CreateObject(string name, Vector3 position, float time = 5f)
    {
        if (goPool.ContainsKey(name) == false)
        {
            LoadFromAA(name, position, time);
            return;
        }

        if (goPool[name].Count == 0)
        {
            GameObject go = Instantiate(addressHandles[name].Result, position, Quaternion.identity);
            goPool[name].Enqueue(go);
        }

        var obj = goPool[name].Dequeue();
        obj.transform.position = position;
        StartCoroutine(ShowObject(name, obj, time));
    }
     
    IEnumerator ShowObject(string name, GameObject effect, float time)
    {
        effect.SetActive(true);
        yield return new WaitForSeconds(time);
        effect.SetActive(false);
        goPool[name].Enqueue(effect);
    }

    bool LoadFromAA(string name, Vector3 position, float time = 5f)
    {
        if (addressHandles.ContainsKey(name) == true)
            return false;

        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(name);
        handle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                if(addressHandles.ContainsKey(name) == false)
                {
                    addressHandles[name] = handle;
                    goPool[name] = new Queue<GameObject>();
                }
                else
                {
                    Debug.Log($"Repeated loading request {name}");
                    Addressables.Release(handle);
                }

                var obj = Instantiate(addressHandles[name].Result, position, Quaternion.identity);                
                StartCoroutine(ShowObject(name, obj, time));
            }
            else
            {
                Debug.LogError("Asset load failed!");
            }
        };
        return true;
    }
}
