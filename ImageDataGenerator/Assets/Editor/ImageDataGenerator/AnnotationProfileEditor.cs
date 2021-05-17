using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NUnit.Framework;

[CustomEditor(typeof(AnnotationProfile))]
public class AnnotationProfileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Show editor"))
        {
           AnnotationProfileWindow.ShowWindow(serializedObject);
        }
    }
}
