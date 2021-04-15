using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AnnotationGeneratorLogHandler : ILogHandler
{
    private FileStream m_FileStream;
    private StreamWriter m_StreamWriter;

    public AnnotationGeneratorLogHandler(string name)
    {
        Directory.CreateDirectory(Application.persistentDataPath + "/Logs");
        string filePath = Application.persistentDataPath + "/Logs/" + name + "_AnnotationGeneratorLogs.txt";

        m_FileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        m_FileStream.SetLength(0);
        m_FileStream.Flush();
        m_StreamWriter = new StreamWriter(m_FileStream);
    }

    public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
    {
        string output = "[" + DateTime.Now + "]\t";

        output += string.Format(format, args);
        m_StreamWriter.WriteLine(output);
        m_StreamWriter.Flush();
    }

    public void LogException(Exception exception, UnityEngine.Object context)
    {
        m_StreamWriter.WriteLine(exception.Message);
        m_StreamWriter.Flush();
        Debug.unityLogger.LogException(exception, context);
    }
}

/// <summary>
/// AnnotationGenerator class is responsible for generating annotations
/// </summary>
[RequireComponent(typeof(AnnotationExporter))]
[RequireComponent(typeof(AnnotationObjectManager))]
[DisallowMultipleComponent]
public class AnnotationGenerator : MonoBehaviour
{
    #region Variables
    [Header("Annotation Settings")]
    [SerializeField]
    RenderTexture outputTextureTemplate = null;
    [SerializeField]
    private int timeScale = 1;
    [SerializeField]
    private List<AnnotationModifier> modifiers = new List<AnnotationModifier>();

    //private Variable
    private List<AnnotationCamera> annotationCameras = new List<AnnotationCamera>();
    
    private AnnotationCamera outputCamera = null;
    private AnnotationExporter annotationExporter = null;
    private AnnotationObjectManager annotationObjectManager = null;
    private Logger annotationLogger = null;

    //TODO : USE ENUM FOR THESE THINGS (FLAGS)
    //public Variables
    public bool QuitExcecution { get; set; } = false;
    public bool DisableModifiers { get; set; } = false; //Use with caution! Modifiers will not be called anymore!
    public bool ResetTimeScale { get; set; } = true; // Disable reset time if a modifier needs more time than one update frame

    bool exportFinished = true;
    #endregion Variables

    #region Access functions
    public AnnotationExporter Exporter { get { return annotationExporter; } }
    public AnnotationObjectManager ObjectManager { get { return annotationObjectManager; } }
    public AnnotationCamera OutputCamera { get { return outputCamera; } set { if (value) outputCamera = value; } }
    public Logger Logger { get { return annotationLogger; } }
    

    ///// <summary>
    ///// Get the annotation camera (by force)
    ///// </summary>
    ///// <param name="type">Type of annotation camera</param>
    ///// <returns>The annotation camera</returns>
    //public AnnotationCamera GetAnnotationCamera(AnnotationCamera.CameraType type) { return annotationCameras[type]; }
    ///// <summary>
    ///// Get the annotation camera
    ///// </summary>
    ///// <param name="type">Type of the annotation camera</param>
    ///// <param name="value">The annotation camera</param>
    ///// <returns>True if the annotation camera was found, false otherwise</returns>
    //public bool TryAnnotationCamera(AnnotationCamera.CameraType type, out AnnotationCamera value) { return annotationCameras.TryGetValue(type, out value); }

    /// <summary>
    /// Connecting a camera to a generator. The camera object will be a child of the generator.
    /// </summary>
    /// <param name="prototype">The camera which is attached to a gameObject</param>
    /// <returns>The annotationCamera component if succesfull, otherwise it will return null</returns>
    /// 
    public AnnotationCamera InstantiateNewCamera(in GameObject prototype, bool autoRender = true)
    {
        GameObject connectedCamera = Instantiate(prototype, transform); //-> New instantiated camera 
        connectedCamera.transform.position = outputCamera.transform.position;
        connectedCamera.transform.rotation = outputCamera.transform.rotation;

        if (connectedCamera.TryGetComponent(out AnnotationCamera annotationCamera))
        {
            if (autoRender)
                annotationCameras.Add(annotationCamera);
            return annotationCamera;
        }
        
        //else
        Destroy(connectedCamera);
        return null;
    }
    /// <summary>
    /// Adds modifier to the list and will set it's generator property. Also will initialize the modifier.
    /// </summary>
    /// <param name="modifier">Modifier that needs to be added</param>
    public void AddModifier(AnnotationModifier modifier) 
    {
        modifier.Generator = this;
        modifier.Initialize();
        modifiers.Add(modifier);
    }
    /// <summary>
    /// Removes modifier to the list. Also will destroy the modifier.
    /// </summary>
    /// <param name="modifier">Modifier that needs to be removed</param>
    public void RemoveModifier(AnnotationModifier modifier) 
    {
        modifier.Destroy();
        modifiers.Remove(modifier);
    }
    #endregion Access functions

    private void Awake()
    {
        outputCamera = GetComponentInChildren<AnnotationCamera>();
        if (!OutputCamera)
            Debug.LogError("No annotation OutputCamera detected... Please add annotation camera as child of the annotation generator");
        annotationCameras.Add(outputCamera);
        
        if (!outputTextureTemplate)
            Debug.LogError("No outputTextureTemplate found... Please add a Render Texture as template for the annotation generator");
            
        // Getting the annotation exporter script
        annotationExporter = GetComponent<AnnotationExporter>();
        if (!annotationExporter)
            Debug.LogError("No annotation exporter found for the annotation generator");
        // Getting the annotation exporter script
        annotationObjectManager = GetComponent<AnnotationObjectManager>();
        if (!annotationObjectManager)
            Debug.LogError("No annotation object manager found for the annotation generator");

        annotationLogger = new Logger(new AnnotationGeneratorLogHandler(gameObject.name));
    }

    private void Start()
    {
        Logger.Log("Start Annotation Generator");
        Time.timeScale = timeScale;

        //Needs to be init here because child object not fully initialized
        outputCamera.Component.targetTexture = new RenderTexture(outputTextureTemplate);

        if (modifiers != null)
            foreach (AnnotationModifier modifier in modifiers)
            {
                modifier.Generator = this;
                modifier.Initialize();
            }
    }

    private void Update()
    {
        Time.timeScale = 0;

        if (exportFinished)
            Annotate();

        //HOW DOES THIS WORK WITH OTHER GENERATORS?
        if (ResetTimeScale) //Reset the time scale 
            Time.timeScale = timeScale;
        else
            ResetTimeScale = true; //For the next frame
    }

    private void OnDestroy()
    {
        foreach (AnnotationModifier modifier in modifiers)
        {
            modifier.Destroy();
        }
        modifiers.Clear();
    }

    /// <summary>
    /// Calling the PreAnnotate function first of all modifiers.
    /// Then, renders the outputCamera attached to this object and all others.
    /// Calling the PostAnnotate function afterwards.
    /// Lastly starts Coroutine to export annotation.
    /// </summary>
    public void Annotate() 
    {
        Logger.Log("[PRE-ANNOTATE]");
        if (!DisableModifiers)
            foreach (AnnotationModifier modifier in modifiers)
            {
                modifier.PreAnnotate();
                if (QuitExcecution) { QuitExcecution = false; return; } 
                if (DisableModifiers) { break; }
            }

        Logger.Log("[ANNOTATE]");
        foreach (AnnotationCamera camera in annotationCameras)
        {
            camera.Render();
        }

        Logger.Log("[POST-ANNOTATE]");
        if (!DisableModifiers)
            foreach (AnnotationModifier modifier in modifiers)
            {
                modifier.PostAnnotate();
                if (QuitExcecution) { QuitExcecution = false; return; }
                if (DisableModifiers) { break; }
            }

        StartCoroutine(AwaitExport());
        exportFinished = false;
    }
    /// <summary>
    /// Is a Coroutine that awaits the end of a frame. And checks if every camera was done rendering.
    /// If this is true, it will initiate the annotation export
    /// </summary>
    /// <returns></returns>
    private IEnumerator AwaitExport() 
    {
        yield return new WaitForEndOfFrame();
        //Will go over all cameras

        foreach (AnnotationCamera camera in annotationCameras)
        {
            while (!camera.HasRendered) 
            {
                yield return null; 
            } //Will wait until camera has rendered
        }

        //ALL Cameras rendered

        Export();
    }
    /// <summary>
    /// Calling the PreExport function first of all modifiers.
    /// Then, exports given output camera to the annotation exporter.
    /// Calling the PostExport function afterwards.
    /// </summary>
    private void Export() 
    {
        Logger.Log("[PRE-EXPORT]");
        if (!DisableModifiers)
            foreach (AnnotationModifier modifier in modifiers)
            {
                modifier.PreExport();
                if (QuitExcecution) { QuitExcecution = false; return; }
                if (DisableModifiers) { break; }
            }
        
        Logger.Log("[EXPORT]");
        annotationExporter.Export(outputCamera);
        
        Logger.Log("[POST-EXPORT]");
        if (!DisableModifiers)
            foreach (AnnotationModifier distorter in modifiers)
            {
                distorter.PostExport();
                if (QuitExcecution) { QuitExcecution = false; return; }
                if (DisableModifiers) { break; }
            }

        exportFinished = true;
    }
}
