using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(AnnotationExporter))]
public class AnnotationExporterEditor : Editor
{
    const float padding = 10;

    //Filename
    bool drawDateFormat = false;
    bool drawTimeFormat = false;

    private ReorderableList formatListEnums;
    private SerializedProperty formatTexts;

    SerializedProperty dateFlag = null;
    SerializedProperty timeFlag = null;

    //Output path
    bool custompath = false;
    SerializedProperty outputPath = null;

    private void OnEnable()
    {
        ///Filename
        //Get texts
        formatTexts = serializedObject.FindProperty("texts");

        //Get enums
        SerializedProperty formatsEnums = serializedObject.FindProperty("formats");

        dateFlag = serializedObject.FindProperty("dateFlag");
        timeFlag = serializedObject.FindProperty("timeFlag");

        if (formatTexts.arraySize != formatsEnums.arraySize)
            Debug.LogWarning("Editor:: Array sizes are not the same! Please check - FileNameModifierEditor");

        //make reoderableList
        formatListEnums = new ReorderableList(serializedObject, formatsEnums, true, true, true, true);

        formatTexts.arraySize = formatListEnums.count;

        formatListEnums.drawElementCallback = DrawListItems;
        formatListEnums.drawHeaderCallback = DrawHeader;
        formatListEnums.onReorderCallbackWithDetails = ReorderList;

        ///OutputPath
        outputPath = serializedObject.FindProperty("outputPath");
        if (outputPath == null)
            Debug.LogError("No outputPath found");

        //Check if output path was set! 
        if (outputPath.stringValue.Length > 0)
            custompath = true;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        //Clear dateDisplay and timeDisplay
        drawDateFormat = false;
        drawTimeFormat = false;
        formatTexts.arraySize = formatListEnums.count;

        DrawPropertiesExcluding(serializedObject, "outputPath", "formats", "texts", "timeFlag", "dateFlag");

        EditorGUILayout.Space();

        formatListEnums.DoLayoutList();

        if (drawDateFormat)
            EditorGUILayout.PropertyField(dateFlag);
        if (drawTimeFormat)
            EditorGUILayout.PropertyField(timeFlag);

        EditorGUILayout.Space();

        custompath = EditorGUILayout.BeginToggleGroup("Custom path", custompath);
        if (custompath)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(outputPath);
            if (GUILayout.Button("Open folder", GUILayout.Width(100))) 
            {
                outputPath.stringValue = EditorUtility.OpenFolderPanel("Select folder to put screen captures in", outputPath.stringValue, "");
            }
            EditorGUILayout.EndHorizontal();
        }
        else 
        {
            EditorGUILayout.LabelField("Currently saving annotations at : %AppData%\\LocalLow\\DAER\\DatasetGenerator");
            outputPath.stringValue = "";
        }

        EditorGUILayout.EndToggleGroup();

        serializedObject.ApplyModifiedProperties();
    }

    void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
    {
        //Get enum form serializedProperty (List)
        SerializedProperty formatEnum = formatListEnums.serializedProperty.GetArrayElementAtIndex(index);

        //Location tracker
        float locationNewField = rect.x;

        //Display enum
        EditorGUI.PropertyField(new Rect(locationNewField, rect.y, 100, EditorGUIUtility.singleLineHeight), formatEnum, GUIContent.none); locationNewField += 100 + padding;
        AnnotationExporter.Format formatValue = (AnnotationExporter.Format)formatEnum.enumValueIndex;

        SerializedProperty textPoperty = formatTexts.GetArrayElementAtIndex(index);

        //Display different object for different enums
        switch (formatValue)
        {
            case AnnotationExporter.Format.Text:
                EditorGUI.LabelField(new Rect(locationNewField, rect.y, 100, EditorGUIUtility.singleLineHeight), "Written text :"); locationNewField += 100 + padding;
                textPoperty.stringValue = EditorGUI.TextField(new Rect(locationNewField, rect.y, rect.width - locationNewField + padding, EditorGUIUtility.singleLineHeight), textPoperty.stringValue);
                break;

            case AnnotationExporter.Format.Date:
                drawDateFormat = true;
                DrawCharField(ref locationNewField, rect, textPoperty, "Date Delimiter :");
                break;

            case AnnotationExporter.Format.Time:
                drawTimeFormat = true;
                DrawCharField(ref locationNewField, rect, textPoperty, "Time Delimiter :");
                break;

            case AnnotationExporter.Format.SceneName:
                EditorGUI.LabelField(new Rect(locationNewField, rect.y, 100, EditorGUIUtility.singleLineHeight), "Current scene :"); locationNewField += 100 + padding;
                string sceneName = SceneManager.GetActiveScene().name;
                textPoperty.stringValue = sceneName;
                EditorGUI.LabelField(new Rect(locationNewField, rect.y, rect.width - locationNewField + padding, EditorGUIUtility.singleLineHeight), sceneName);
                break;
            default:
                break;
        }
    }

    void DrawCharField(ref float locationNewField, Rect rect, SerializedProperty textPoperty, string labelField)
    {
        EditorGUI.LabelField(new Rect(locationNewField, rect.y, 100, EditorGUIUtility.singleLineHeight), labelField); locationNewField += 100 + padding;
        string text = EditorGUI.TextField(new Rect(locationNewField, rect.y, 15, EditorGUIUtility.singleLineHeight), textPoperty.stringValue); locationNewField += 15 + padding;
        if (string.IsNullOrEmpty(text))
        {
            textPoperty.stringValue = text;
            EditorGUI.LabelField(new Rect(locationNewField, rect.y, 100, EditorGUIUtility.singleLineHeight), "= Nothing");
        }
        else
        {
            text = text.Substring(0, 1); //One character
            if (text == " ")
                EditorGUI.LabelField(new Rect(locationNewField, rect.y, 100, EditorGUIUtility.singleLineHeight), "= WhiteSpace");
            if (text == "/" || text == "\\" || text == "<" || text == ">" || text == ":" || text == "\"" || text == "|" || text == "?" || text == "*") // Forbidden characters
                text = ""; //No slashes

            textPoperty.stringValue = text;
        }
    }

    void DrawHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, "File format");
    }

    void ReorderList(ReorderableList list, int oldIndex, int newIndex)
    {
        SerializedProperty oldStringProperty = formatTexts.GetArrayElementAtIndex(oldIndex);
        SerializedProperty newStringProperty = formatTexts.GetArrayElementAtIndex(newIndex);
        string temp = oldStringProperty.stringValue;
        oldStringProperty.stringValue = newStringProperty.stringValue;
        newStringProperty.stringValue = temp;
    }
}
