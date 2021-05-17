using System.Collections.Generic;
using UnityEngine;

public abstract class AnnotationModule : ScriptableObject
{
    protected AnnotationGenerator Generator { get; private set; }
    protected void Log(string message) { Generator.Logger.Log(LogType.Log, GetType().Name + " >> " + message); }

    /// <summary>
    /// Start will execute when the annotation generator property is already initialized.
    /// </summary>
    public void Initialize(AnnotationGenerator generator)
    {
        if (!generator)
            Debug.LogError("There was no reference to the annotation generator");
        Generator = generator;
        Start();
        Log("Initialized");
    }

    /// <summary>
    /// Start will execute when the annotation generator property is already initialized.
    /// </summary>
    protected abstract void Start();
    /// <summary>
    /// Pre Annotate will execute before the actual annotation is taken.
    /// Use this function to create a change in the actual annotation image
    /// </summary>
    public abstract void PreAnnotate();
    /// <summary>
    /// This function will execute after the camera gets the instruction to render.
    /// Use this function to clean up the previous distortion. And set everything to normal
    /// 
    /// note: It is possible that the camera's are still rendering. Accessing them could lead to unintended behaviour.
    /// </summary>
    public abstract void PostAnnotate();
    /// <summary>
    /// This function gets called just before the exportation of the annotation.
    /// </summary>
    public abstract void PreExport();
    /// <summary>
    /// This function gets called after the exportation of the annotation.
    /// </summary>
    public abstract void PostExport();
    /// <summary>
    /// This function gets called when either the gameGenerator is destroyed or when you manually delete a modifier.
    /// </summary>
    public abstract void Destroy();
}
