using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class WorldManager : MonoBehaviour
{
    Dictionary<string, AsyncOperationHandle<GameObject>> addressHandles = new Dictionary<string, AsyncOperationHandle<GameObject>>();
    Dictionary<string, Queue<GameObject>> effectPool = new Dictionary<string, Queue<GameObject>>();

    private void Start()
    {
        CreateEffect("Splash", this.transform.position, 10f);
    }

    private void OnDestroy()
    {
        foreach(var item in addressHandles.Values)
        {
            Addressables.Release(item);
        }
    }
    public void CreateEffect(string name, Vector3 position, float time = 5f)
    {
        if (effectPool.ContainsKey(name) == false)
        {
            if(LoadFromAA(name, position, time) == false)
            {
                Debug.Log("Already Loaded!");
            }   
            return;
        }

        if (effectPool[name].Count == 0)
        {
            GameObject go = Instantiate(addressHandles[name].Result, position, Quaternion.identity);
            effectPool[name].Enqueue(go);
        }

        var obj = effectPool[name].Dequeue();
        obj.transform.position = position;
        StartCoroutine(ShowEffect(name, obj, time));
    }
     
    IEnumerator ShowEffect(string name, GameObject effect, float time)
    {
        effect.SetActive(true);
        yield return new WaitForSeconds(time);
        effect.SetActive(false);
        effectPool[name].Enqueue(effect);
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
                addressHandles[name] = handle;
                var obj = Instantiate(addressHandles[name].Result, position, Quaternion.identity);
                effectPool[name] = new Queue<GameObject>();
                StartCoroutine(ShowEffect(name, obj, time));
            }
            else
            {
                Debug.LogError("Asset load failed!");
            }
        };
        return true;
    }
}
