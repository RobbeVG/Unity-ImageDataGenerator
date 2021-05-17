using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.UIElements;

[CustomEditor(typeof(AnnotationGenerator))]
public class AnnotationGeneratorEditor : Editor
{
    const float padding = 10;
    ReorderableList profiles;

    Texture2D arrowTexture = null;

    private void OnEnable()
    {
        SerializedReordebleList(out SerializedProperty annotationProfiles, out profiles, "profiles", "Profiles", draggable: false);

        arrowTexture = EditorGUIUtility.FindTexture("Assets/ImageDataGenerator/Textures/Editor/ArrowIcon.png");
        if (!arrowTexture)
            Debug.Log("No texture found");
    }

    public override void OnInspectorGUI()
    {
        DrawPropertiesExcluding(serializedObject, "profiles");

        EditorGUILayout.Space();

        profiles.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }

    private void SerializedReordebleList(out SerializedProperty property, out ReorderableList reorderableList, string propertyPath, string header, bool draggable = true, bool displayHeader = true, bool displayAddButton = true, bool displayRemoveButton = true) 
    {
        property = serializedObject.FindProperty(propertyPath);
        if (property == null)
        {
            string text = "No variable found in the AnnotationModifier, possible name change of variable?";
            EditorGUILayout.LabelField(text);
            Debug.LogError(text);
        }

        if (!property.isArray)
        {
            string text = "Variable modifiers in AnnotationModifier is not an array?";
            EditorGUILayout.LabelField(text);
            Debug.LogError(text);
        }

        ReorderableList currentList = new ReorderableList(serializedObject, property, draggable, displayHeader, displayAddButton, displayRemoveButton);
        currentList.drawHeaderCallback = delegate (Rect rect) { EditorGUI.LabelField(rect, header); };
        currentList.drawElementCallback = delegate(Rect rect, int index, bool isActive, bool isFocused) 
        {
            float locationNewField = rect.x;

            SerializedProperty listObject = currentList.serializedProperty.GetArrayElementAtIndex(index);
            //GUIContent content = new GUIContent(modifier.objectReferenceValue.name);
            GUIContent content = new GUIContent("");
            EditorGUILayout.BeginHorizontal();

            float propertyWidth = rect.width * (2.0f / 3.0f);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, propertyWidth, EditorGUIUtility.singleLineHeight), listObject, content); locationNewField += propertyWidth + padding;


            if (GUI.Button(new Rect(locationNewField, rect.y, rect.width - propertyWidth - padding, EditorGUIUtility.singleLineHeight), "Show editor")) 
            {
                Object listproperty = currentList.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue;

                SerializedObject serializedObject = new SerializedObject(listproperty);
                //SerializedObject temp = (SerializedObject)listproperty.;
                AnnotationProfileWindow.ShowWindow(serializedObject);
            }
            EditorGUILayout.EndHorizontal();
        };

        reorderableList = currentList;
    }
}
