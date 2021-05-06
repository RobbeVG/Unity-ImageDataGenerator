using UnityEngine;

public abstract class AnnotationVerifier : ScriptableObject
{
    protected AnnotationGenerator generator = null;
    public AnnotationGenerator Generator { get { return generator; } set { generator = value; } }

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
    /// This function will execute after all verifiers are updated.
    /// Returning false will terminate the annotation.
    /// Returning true will execute the next verifier.
    /// </summary>
    public abstract bool Execute();

    /// <summary>
    /// Post Update function for verifier in case necesarry.
    /// Post Update is called after the annotation is taken.
    /// </summary>
    public virtual void PostAnnotate() { }

    /// <summary>
    /// This function gets called when either the gameGenerator is destroyed or when you manually delete a verifier.
    /// </summary>
    public virtual void Destroy() { }
}
