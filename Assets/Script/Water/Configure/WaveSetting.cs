using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveSetting", menuName = "Custom/WaveSetting")]
public class WaveSetting : ScriptableObject
{
    [System.Serializable]
    public class WaveInput
    {
        public float amplitude;
        public float length;
        public float speed;
        [Range(0, 6.28f)]
        public float angle;
    }

    [HideInInspector]
    public bool isChanged = true;
    public List<WaveInput> input = new List<WaveInput>();   
    List<Vector4> waveData = new List<Vector4>();

    void OnValidate()
    {
        isChanged = true;
    }

    public Vector4[] GetWaveData()
    {
        waveData.Clear();
        foreach(var e  in input)
        {
            var wave = new Vector4();
            wave.x = e.amplitude;
            wave.y = e.length;
            wave.z = e.speed;
            wave.w = e.angle;
            waveData.Add(wave);
        }
        return waveData.ToArray();
    }
}