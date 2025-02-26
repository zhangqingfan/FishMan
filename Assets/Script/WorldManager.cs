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
    public static WorldManager Instance;
    public static readonly float fishHeight = -1f;
    public static readonly float seaHeight = 0f;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        foreach(var item in addressHandles.Values)
        {
            Addressables.Release(item);
        }
    }

    public GameObject CreateObject(string name, Vector3 position, float time = 5f)
    {
        if (goPool.ContainsKey(name) == false)
        {
            LoadFromAA(name, position, time);
            return null;
        }

        if (goPool[name].Count == 0)
        {
            Debug.Log("wrong!222");
            GameObject go = Instantiate(addressHandles[name].Result, position, Quaternion.identity);
            goPool[name].Enqueue(go);
        }

        var obj = goPool[name].Dequeue();
        obj.transform.position = position;

        if(time >= 0)
        {
            Debug.Log("StartCoroutine");
            StartCoroutine(ShowObject(name, obj, time));
        }

        return obj;
    }

    IEnumerator ShowObject(string name, GameObject gameObject, float time)
    {
        if (time >= 0f)
        {
            gameObject.SetActive(true);
            yield return new WaitForSeconds(time);
        }
        Debug.Log("abc");
        ReleaseObject(name, gameObject);
    }

    public void ReleaseObject(string name, GameObject gameObject)
    {
        if (goPool.ContainsKey(name) == false)
        {
            Debug.Log("Memory pool does not contain " + name);
            return;
        }

        Debug.Log("release!" + gameObject);
        gameObject.SetActive(false);
        goPool[name].Enqueue(gameObject);
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
