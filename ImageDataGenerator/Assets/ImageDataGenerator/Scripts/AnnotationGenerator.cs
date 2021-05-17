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
[RequireComponent(typeof(AnnotationSegmentation))]
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
    private float timeBetweenAnnotations = 0.1f;

    [SerializeField]
    private List<AnnotationProfile> profiles = new List<AnnotationProfile>();

    //private Variable
    private float currentTimeBetweenAnnotations = 0.0f;

    //public Variables
    private HashSet<AnnotationObject> RenderedObjects = new HashSet<AnnotationObject>();
    public IReadOnlyCollection<AnnotationObject> GetRenderedObjects() { return RenderedObjects; }
    public HashSet<AnnotationObject> EditableObjects { get; set; }

    //TODO REMOVE
    public bool StopAnnotation { get; set; } = true;

    bool segmentationDone = true;
    bool exportFinished = true;
    #endregion Variables

    #region Access Variables
    public AnnotationCamera OutputCamera { get; private set; }
    public AnnotationSegmentation Segmentation { get; private set; }
    public AnnotationExporter Exporter { get; private set; }

    public Logger Logger { get; private set; }
    #endregion Access Variables

    //#region Access Functions
    ///// <summary>
    ///// Adds modifier to the list and will set it's generator property. Also will initialize the modifier.
    ///// </summary>
    ///// <param name="modifier">Modifier that needs to be added</param>
    //public void AddModifier(AnnotationModifier modifier) 
    //{
    //    modifier.Generator = this;
    //    modifier.Initialize();
    //    modifiers.Add(modifier);
    //}
    ///// <summary>
    ///// Removes modifier to the list. Also will destroy the modifier.
    ///// </summary>
    ///// <param name="modifier">Modifier that needs to be removed</param>
    //public void RemoveModifier(AnnotationModifier modifier) 
    //{
    //    modifier.Destroy();
    //    modifiers.Remove(modifier);
    //}
    ///// <summary>
    ///// Adds verifier to the list and will set it's generator property. Also will initialize the verifier.
    ///// </summary>
    ///// <param name="verifier">Verifier that needs to be added</param>
    //public void AddVerifier(AnnotationVerifier verifier) 
    //{
    //    verifier.Generator = this;
    //    verifier.Initialize();
    //    verifiers.Add(verifier);
    //}
    ///// <summary>
    ///// Removes verifier to the list. Also will destroy the verifier.
    ///// </summary>
    ///// <param name="verifier">Verifier that needs to be removed</param>
    //public void RemoveVerifier(AnnotationVerifier verifier) 
    //{
    //    verifier.Destroy();
    //    verifiers.Remove(verifier);
    //}
    //#endregion Access functions

    private void Awake()
    {
        OutputCamera = GetComponentInChildren<AnnotationCamera>();
        if (!OutputCamera)
            Debug.LogError("No annotation OutputCamera detected... Please add annotation camera as child of the annotation generator");
        
        if (!outputTextureTemplate)
            Debug.LogError("No outputTextureTemplate found... Please add a Render Texture as template for the annotation generator");

        Segmentation = GetComponent<AnnotationSegmentation>(); //Not necessary but could be important for some situations

        // Getting the annotation exporter script
        Exporter = new AnnotationExporter();
        
        // Getting the annotation exporter script
        // ObjectManager = GetComponent<AnnotationObjectManager>();
        // if (!ObjectManager)
        //    Debug.LogError("No annotation object manager found for the annotation generator");

        Logger = new Logger(new AnnotationGeneratorLogHandler(gameObject.name));
    }

    private void Start()
    {
        Logger.Log("Start Annotation Generator");
        Time.timeScale = timeScale;

        //Needs to be init here because child object not fully initialized
        OutputCamera.Component.targetTexture = new RenderTexture(outputTextureTemplate);
        //Segmentation
        Segmentation.Initialize(this);

        foreach (AnnotationProfile profile in profiles)
        {
            profile.Initialize(this);
        }
    }

    private void Update()
    {
        Time.timeScale = 0;
        currentTimeBetweenAnnotations += Time.deltaTime; //Add current delta time -> Could be higher than capture

        if (exportFinished && currentTimeBetweenAnnotations >= timeBetweenAnnotations) 
        {
            if (segmentationDone) 
            {
                Segmentation.Camera.Render();
                segmentationDone = false;
            }

            //Check if segmentation camera is still rendering
            if (Segmentation.Camera.FinishedRender)
                segmentationDone = true;
            else
                return;

            //Segmentation rendered all objects visible
            EditableObjects = new HashSet<AnnotationObject>(RenderedObjects); //COPY

            //Count pixels or optional export
            Segmentation.Run();
            
            //Annotations for each profile
            foreach (AnnotationProfile profile in profiles)
            {
                Logger.Log("[CONDITIONING]");
                if (profile.Conditioning()) 
                {
                    Logger.Log("[PRE-ANNOTATE]");
                    profile.PreAnnotate();

                    Logger.Log("[ANNOTATE]");
                    OutputCamera.Render();

                    Logger.Log("[VALIDATE]");
                    bool validated = profile.Validation();

                    Logger.Log("[POST-ANNOTATE]");
                    profile.PostAnnotate();

                    if (validated) 
                    {
                        //Logger.Log("POST-CONDITIONING");

                        StartCoroutine(AwaitExport());
                        exportFinished = false;

                        currentTimeBetweenAnnotations = 0.0f;
                    }
                }
            }
        }

        EditableObjects = null;
        Time.timeScale = timeScale;
    }

    private void OnDestroy()
    {
        //foreach (AnnotationModifier modifier in modifiers)
        //{
        //    modifier.Destroy();
        //}
        //foreach (AnnotationVerifier verifier in verifiers)
        //{
        //    verifier.Destroy();
        //}
    }

    /// <summary>
    /// Calling the PreAnnotate function first of all modifiers.
    /// Then, renders the outputCamera attached to this object and all others.
    /// Calling the PostAnnotate function afterwards.
    /// Lastly starts Coroutine to export annotation.
    /// </summary>
    //public void Annotate() 
    //{
    //    Logger.Log("[PRE-ANNOTATE]");
    //    foreach (AnnotationModifier modifier in modifiers)
    //    {
    //        modifier.PreAnnotate();
    //    }

    //    Logger.Log("[ANNOTATE]");
    //    OutputCamera.Render();

    //    validated = true;
    //    if (validators.Count != 0)
    //    {
    //        //Check verifying
    //        Logger.Log("[VALIDATING]");
    //        foreach (AnnotationVerifier verifier in validators)
    //        {
    //            bool result = verifier.Execute();
    //            validated &= result;

    //            if (!result) //Stop running verifiers once there is one negative
    //                break;
    //        }
    //    }

    //    Logger.Log("[POST-ANNOTATE]");
    //    foreach (AnnotationModifier modifier in modifiers)
    //    {
    //        modifier.PostAnnotate();
    //    }

    //    if (validated)
    //    {
    //        StartCoroutine(AwaitExport());
    //        exportFinished = false;
    //    }
        
    //}
    /// <summary>
    /// Is a Coroutine that awaits the end of a frame. And checks if every camera was done rendering.
    /// If this is true, it will initiate the annotation export
    /// </summary>
    /// <returns></returns>
    private IEnumerator AwaitExport() 
    {
        yield return new WaitForEndOfFrame();
        
        while (!OutputCamera.FinishedRender) 
        {
            yield return null; 
        } //Will wait until camera has rendered
        
        Export();
    }
    /// <summary>
    /// Calling the PreExport function first of all modifiers.
    /// Then, exports given output camera to the annotation exporter.
    /// Calling the PostExport function afterwards.
    /// </summary>
    private void Export() 
    {
        foreach (AnnotationProfile profile in profiles)
        {
            Logger.Log("[PRE-EXPORT]");
            profile.PreExport();

            Logger.Log("[EXPORT]");
            Exporter.Export(OutputCamera, profile.Output);
            
            Logger.Log("[POST-EXPORT]");
            profile.PostExport();
        }
        exportFinished = true;
    }

    public void AddSegmentationCamRenderCallback(AnnotationObject annotationObject) 
    {
        annotationObject.renderCallBacks.Add(OnSegmentationCamRender);
    }

    public void OnSegmentationCamRender(AnnotationCamera camera, AnnotationObject annotationObject)
    {
        if (camera != Segmentation.Camera)
            return;

        RenderedObjects.Add(annotationObject);
    }
}
