using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathFinding))]
public class PathFindEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Draw Path"))
        {
            ((PathFinding)target).OnButtonClicked();
        }
    }
}
