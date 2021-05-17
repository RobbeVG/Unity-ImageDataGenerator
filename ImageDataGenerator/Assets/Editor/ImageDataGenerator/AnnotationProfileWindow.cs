using UnityEngine;
using UnityEditor;
using UnityEditorInternal;


public class AnnotationProfileWindow : EditorWindow
{
    SerializedObject serializedObject = null;
    //SerializedProperty currentProperty = null;

    //Conditions properties
    bool editConditions = false;
    ReorderableList conditions;
    SerializedProperty annotationConditions;

    //Modifiers properties
    bool editModifiers = false;
    ReorderableList modifiers;
    SerializedProperty annotationModifiers;

    //Validations properties
    bool editValidations = false;
    ReorderableList validations;
    SerializedProperty annotationValidations;

    SerializedProperty outputProperty;

    Vector2 scrollPosition = Vector2.zero;

    Texture2D arrowTexture = null;

    public static void ShowWindow(SerializedObject intrestedObject) 
    {
        AnnotationProfileWindow window = GetWindow<AnnotationProfileWindow>("Annotation Profile Editor");
        window.serializedObject = intrestedObject;
        window.Initialize();
    }

    private void Initialize() 
    {
        if (serializedObject == null)
            return;

        SerializedReordebleList(out annotationConditions, out conditions, "conditions", "Editable verifiers");
        SerializedReordebleList(out annotationModifiers, out modifiers, "modifiers", "Editable modifiers");
        SerializedReordebleList(out annotationValidations, out validations, "validators", "Editable validations");

        outputProperty = serializedObject.FindProperty("output");

        arrowTexture = EditorGUIUtility.FindTexture("Assets/ImageDataGenerator/Textures/Editor/ArrowIcon.png");
        if (!arrowTexture)
            Debug.Log("No texture found");
    }

    private void OnGUI()
    {
        if (serializedObject == null || outputProperty.editable == false) 
        {
            Close();
            return;
        }
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(outputProperty);

        conditions.DoLayoutList();
        DrawReordebleListElementsToggleGroup(ref editConditions, "Edit conditions", annotationConditions);

        DrawArrow();

        modifiers.DoLayoutList();
        DrawReordebleListElementsToggleGroup(ref editModifiers, "Edit modifiers", annotationModifiers);

        DrawArrow();

        validations.DoLayoutList();
        DrawReordebleListElementsToggleGroup(ref editValidations, "Edit validations", annotationValidations);

        GUILayout.EndScrollView();

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
        currentList.drawElementCallback = delegate (Rect rect, int index, bool isActive, bool isFocused)
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

                Editor editor;
                //Checking if we need to create custom editor
                editor = Editor.CreateEditor(listElementProperty.objectReferenceValue);

                //Update the serializedObject
                editor.serializedObject.Update();

                //Draw header
                editor.DrawHeader();

                //Draw editor
                editor.OnInspectorGUI();

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


    // https://www.youtube.com/watch?v=c_3DXBrH-Is
    private void DrawProperties(SerializedProperty properties, bool drawChildren) 
    {
        string lastPropPath = string.Empty;
        foreach (SerializedProperty property in properties)
        {
            if (property.isArray && property.propertyType == SerializedPropertyType.Generic)
            {
                EditorGUILayout.BeginHorizontal();
                property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, property.displayName);
                EditorGUILayout.EndHorizontal();

                if (property.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    DrawProperties(property, drawChildren);
                    EditorGUI.indentLevel--;
                }
            }
            else 
            {
                if (!string.IsNullOrEmpty(lastPropPath) && property.propertyPath.Contains(lastPropPath)) 
                    continue;
                lastPropPath = property.propertyPath;
                EditorGUILayout.PropertyField(property, drawChildren);
            }
        }
    }
}
