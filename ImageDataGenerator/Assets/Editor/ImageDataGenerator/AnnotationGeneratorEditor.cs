using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(AnnotationGenerator))]
public class AnnotationGeneratorEditor : Editor
{
    //Modifiers properties
    bool editModifiers = false;
    ReorderableList modifiers;
    SerializedProperty annotationModifiers;

    //Verifiers properties
    bool editVerifiers = false;
    ReorderableList verifiers;
    SerializedProperty annotationVerifiers;

    Texture2D arrowTexture = null;

    private void OnEnable()
    {
        SerializedReordebleList(out annotationModifiers, out modifiers, "modifiers", "Editable modifiers");
        SerializedReordebleList(out annotationVerifiers, out verifiers, "verifiers", "Editable verifiers");

        arrowTexture = EditorGUIUtility.FindTexture("Assets/ImageDataGenerator/Textures/Editor/ArrowIcon.png");
        if (!arrowTexture)
            Debug.Log("No texture foubd");
    }

    public override void OnInspectorGUI()
    {
        DrawPropertiesExcluding(serializedObject, "modifiers", "verifiers");

        EditorGUILayout.Space(10.0f);

        verifiers.DoLayoutList();
        DrawReordebleListElementsToggleGroup(ref editVerifiers, "Edit verifiers", annotationVerifiers);

        DrawArrow();

        modifiers.DoLayoutList();
        DrawReordebleListElementsToggleGroup(ref editModifiers, "Edit modifiers", annotationModifiers);

        serializedObject.ApplyModifiedProperties();
    }

    private void SerializedReordebleList(out SerializedProperty property, out ReorderableList reorderableList, string propertyPath, string header) 
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

        ReorderableList currentList = new ReorderableList(serializedObject, property, true, true, true, true);
        currentList.drawHeaderCallback = delegate (Rect rect) { EditorGUI.LabelField(rect, header); };
        currentList.drawElementCallback = delegate(Rect rect, int index, bool isActive, bool isFocused) 
        {
            SerializedProperty listObject = currentList.serializedProperty.GetArrayElementAtIndex(index);
            //GUIContent content = new GUIContent(modifier.objectReferenceValue.name);
            GUIContent content = new GUIContent("");
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), listObject, content);
        };

        reorderableList = currentList;
    }

    private void DrawReordebleListElementsToggleGroup(ref bool toggle, string toggleName, SerializedProperty property) 
    {
        toggle = EditorGUILayout.BeginToggleGroup(toggleName, toggle);
        if (toggle)
        {
            for (int index = 0; index < property.arraySize; index++)
            {
                //Getting current modifer property
                SerializedProperty listElementProperty = property.GetArrayElementAtIndex(index);

                if (listElementProperty.objectReferenceValue == null)
                    continue;

                //if (modifier.objectReferenceValue.GetType() == typeof(VisibilityModifier) || modifier.objectReferenceValue.GetType() == typeof(MaterialEditor))
                //    showSelector = true;
                //else
                //    showSelector = false;

                bool customEditor = false;

                Editor editor;
                //Checking if we need to create custom editor
                //if (modifier.objectReferenceValue.GetType() == typeof(FileNameModifier))
                //{
                //    editor = CreateEditor(modifier.objectReferenceValue, typeof(FileNameModifierEditor));
                //    customEditor = true;
                //}
                //else
                editor = CreateEditor(listElementProperty.objectReferenceValue);

                //Update the serializedObject
                editor.serializedObject.Update();

                //Draw header
                editor.DrawHeader();

                //Draw editor
                if (customEditor)
                {
                    EditorGUILayout.LabelField("Open the serialized object to reorder the elements in the list");
                    editor.OnInspectorGUI();
                }
                else
                    DrawPropertiesExcluding(editor.serializedObject, "m_Script");


                //Draw arrow texture in middle if not last object
                if (index != property.arraySize - 1)
                    DrawArrow();

                editor.serializedObject.ApplyModifiedProperties();
            }
        }
        EditorGUILayout.EndToggleGroup();
    }

    private void DrawArrow() 
    {
        GUIStyle style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
        GUIContent content = new GUIContent(arrowTexture);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(content, style, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();
    }
}
