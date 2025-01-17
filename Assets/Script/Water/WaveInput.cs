using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveInput
{
    public float amplitude;
    public float length;
    public float speed;
    public float angle;
}

[CreateAssetMenu(fileName = "WaveSetting", menuName = "Custom/WaveSetting")]
public class WaveSetting : ScriptableObject
{
    public List<WaveInput> input = new List<WaveInput>();
    [HideInInspector]
    public Vector4[] inputs = new Vector4[6];

    public void Update()
    {
        //TODO...
    }
}