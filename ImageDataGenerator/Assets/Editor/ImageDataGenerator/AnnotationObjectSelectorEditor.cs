using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnnotationObjectManager))]
public class AnnotationObjectSelectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Will only be used when a certain modifier needs accessing to these objects.");
        EditorGUILayout.LabelField("(e.g. The VisibilityModifier, MaterialModifier)");
        base.OnInspectorGUI();
    }
}
