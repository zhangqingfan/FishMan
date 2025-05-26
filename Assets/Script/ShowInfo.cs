using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowInfo : MonoBehaviour
{
    GUIStyle guiStyle;
    public AudioSource audioSource;
    float sliderValue = 0.5f;

    private void Start()
    {
        guiStyle = new GUIStyle();
        audioSource.Play();
    }

    private void OnGUI()
    {
        guiStyle.fontSize = 30;
        guiStyle.normal.textColor = Color.white;
        GUILayout.BeginArea(new Rect(10, 10, 600, 190));
        GUI.Box(new Rect(0, 0, 455, 190), "");
        GUILayout.Label("MOVE:WSAD", guiStyle);
        GUILayout.Label("SHOOT:LEFT BUTTON", guiStyle);
        GUILayout.Label("ROTATE VIEW:RIGHT BUTTON", guiStyle);
        GUILayout.Label("ADJUST VIEW:MIDDLE BUTTON", guiStyle);
        GUILayout.Label("VOLUME:", guiStyle);
        sliderValue = GUILayout.HorizontalSlider(sliderValue, 0.0f, 1.0f,  GUILayout.Width(300), GUILayout.Height(30));
        GUILayout.EndArea();
    }

    private void Update()
    {
        audioSource.volume = sliderValue;
    }
}