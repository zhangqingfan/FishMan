using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowInfo : MonoBehaviour
{
    GUIStyle guiStyle;

    private void Start()
    {
        guiStyle = new GUIStyle();
    }

    private void OnGUI()
    {
        guiStyle.fontSize = 30;
        guiStyle.normal.textColor = Color.white;
        GUILayout.BeginArea(new Rect(10, 10, 600, 100));
        GUI.Box(new Rect(0, 0, 440, 100), "");
        GUILayout.Label("MOVE:WSAD", guiStyle);
        GUILayout.Label("SHOOT:LEFT BUTTON", guiStyle);
        GUILayout.Label("ROTATE VIEW:RIGHT BUTTON", guiStyle);
        GUILayout.EndArea();
    }
}