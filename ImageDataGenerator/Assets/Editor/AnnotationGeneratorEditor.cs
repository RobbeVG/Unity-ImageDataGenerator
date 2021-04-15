using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(AnnotationGenerator))]
public class AnnotationGeneratorEditor : Editor
{
    //bool showSelector;
    bool editModifiers = false;

    ReorderableList modifiers;
    SerializedProperty annotationModifiers;

    Texture2D arrowTexture = null;

    private void OnEnable()
    {
        annotationModifiers = serializedObject.FindProperty("modifiers");
        if (annotationModifiers == null)
        {
            string text = "No variable found in the AnnotationModifier, possible name change of variable?";
            EditorGUILayout.LabelField(text);
            Debug.LogError(text);
        }

        if (!annotationModifiers.isArray)
        {
            string text = "Variable modifiers in AnnotationModifier is not an array?";
            EditorGUILayout.LabelField(text);
            Debug.LogError(text);
        }

        arrowTexture = EditorGUIUtility.FindTexture("Assets/ImageDataGenerator/Textures/Editor/ArrowIcon.png");
        if (!arrowTexture)
            Debug.Log("No texture foubd");

        modifiers = new ReorderableList(serializedObject, annotationModifiers, true, true, true, true);
        modifiers.drawHeaderCallback = DrawHeader;
        modifiers.drawElementCallback = DrawListItems;
    }

    public override void OnInspectorGUI()
    {
        DrawPropertiesExcluding(serializedObject, "modifiers");

        EditorGUILayout.Space();

        modifiers.DoLayoutList();

        EditorGUILayout.Space();

        editModifiers = EditorGUILayout.BeginToggleGroup("Edit Modifiers", editModifiers);
        if (editModifiers)
        {
            for (int index = 0; index < annotationModifiers.arraySize; index++)
            {
            //Getting current modifer property
            SerializedProperty modifier = annotationModifiers.GetArrayElementAtIndex(index);
            
                if (modifier.objectReferenceValue == null)
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
                editor = CreateEditor(modifier.objectReferenceValue);

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
                if (index != annotationModifiers.arraySize - 1)
                {
                    GUIStyle style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
                    GUIContent content = new GUIContent(arrowTexture);
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(content, style, GUILayout.ExpandWidth(true));
                    EditorGUILayout.Space();
                }

                editor.serializedObject.ApplyModifiedProperties();
            }
        }
        EditorGUILayout.EndToggleGroup();


        //bool hasSelector = ((AnnotationGenerator)target).gameObject.TryGetComponent(out AnnotationObjectSelector selector);
        //if (showSelector)
        //{
        //    if (!hasSelector)
        //        ((AnnotationGenerator)target).gameObject.AddComponent<AnnotationObjectSelector>();
        //}
        //else
        //{
        //    if (hasSelector)
        //        Destroy(selector);
        //}

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawHeader(Rect rect) 
    {
        EditorGUI.LabelField(rect, "Editable modifiers");
    }

    void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty modifier = modifiers.serializedProperty.GetArrayElementAtIndex(index);
        //GUIContent content = new GUIContent(modifier.objectReferenceValue.name);
        GUIContent content = new GUIContent("");
        EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), modifier, content);
    }
}
