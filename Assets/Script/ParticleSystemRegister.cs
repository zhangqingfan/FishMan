using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemRegister : MonoBehaviour
{
    ParticleSystem[] psArray;

    private void OnEnable()
    {
        if(psArray != null)
        {
            for(int i = 0; i < psArray.Length; i++) 
            {
                Water.Instance.AddParticleSystem(psArray[i]);
            }
        }
    }

    private void OnDisable()
    {
        if (psArray != null)
        {
            for (int i = 0; i < psArray.Length; i++)
            {
                Water.Instance.RemoveParticleSystem(psArray[i]);
            }
        }
    }

    void Start()
    {
        psArray = transform.GetComponentsInChildren<ParticleSystem>();
        if (psArray != null)
        {
            for (int i = 0; i < psArray.Length; i++)
            {
                Water.Instance.AddParticleSystem(psArray[i]);
            }
        }

    }
}
