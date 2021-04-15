using UnityEngine;

public abstract class AnnotationModifier : ScriptableObject
{
    protected AnnotationGenerator generator = null;
    public AnnotationGenerator Generator { set { generator = value; } }

    protected void Log(string message) { generator.Logger.Log(LogType.Log, GetType().Name + " >> " + message); }

    /// <summary>
    /// Initialize is called by the annotation generator just before the first frame.
    /// It will execute the Start() function next.
    /// </summary>
    public void Initialize() 
    {
        if (!generator)
            Debug.LogError("There was no reference to the annotation generator");
        Log("Initialized");
        Start();
    }
    /// <summary>
    /// Start will execute when the annotation generator property is already initialized.
    /// </summary>
    protected virtual void Start() { }
    /// <summary>
    /// Pre Annotate will execute before the actual annotation is taken.
    /// Use this function to create a change in the actual annotation image
    /// </summary>
    public virtual void PreAnnotate() { }
    /// <summary>
    /// This function will execute after the camera gets the instruction to render.
    /// Use this function to clean up the previous distortion. And set everything to normal
    /// 
    /// note: It is possible that the camera's are still rendering. Accessing them could lead to unintended behaviour.
    /// </summary>
    public virtual void PostAnnotate() { }
    /// <summary>
    /// This function gets called just before the exportation of the annotation.
    /// </summary>
    public virtual void PreExport() { }
    /// <summary>
    /// This function gets called after the exportation of the annotation.
    /// </summary>
    public virtual void PostExport() { }
    /// <summary>
    /// This function gets called when either the gameGenerator is destroyed or when you manually delete a modifier.
    /// </summary>
    public virtual void Destroy() { }
}

