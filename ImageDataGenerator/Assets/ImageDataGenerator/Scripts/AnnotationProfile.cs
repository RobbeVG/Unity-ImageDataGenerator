using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "annotationProfile", menuName = "AnnotationSystem/Profile")]
public class AnnotationProfile : ScriptableObject
{
    [SerializeField]
    private List<AnnotationVerifier> conditions = new List<AnnotationVerifier>();
    [SerializeField]
    private List<AnnotationModifier> modifiers = new List<AnnotationModifier>();
    [SerializeField]
    private List<AnnotationVerifier> validators = new List<AnnotationVerifier>();

    List<AnnotationModule> annotationModules = new List<AnnotationModule>();

    [SerializeField]
    private GameObject cameraObject = null;

    [SerializeField]
    private AnnotationOutput output = null;

    public AnnotationCamera Camera { get; private set; }
    public AnnotationOutput Output { get { return output; } }

    private void OnEnable()
    {
        annotationModules = annotationModules.Concat(conditions).Concat(modifiers).Concat(validators).ToList();

        if (!output)
            Debug.LogError("No output in profile");
    }

    public void Initialize(AnnotationGenerator generator) 
    {
        if (cameraObject)
        {
            if (cameraObject.TryGetComponent(out Camera _) && cameraObject.TryGetComponent(out AnnotationCamera _)) 
            {
                GameObject newCamera = Instantiate(cameraObject, generator.transform);
                Camera = newCamera.GetComponent<AnnotationCamera>();
            }
            else
                Debug.LogError("Given camera does not match the requirements");
        }
        else
            Camera = generator.StandardCamera; 


        foreach (AnnotationModule module in annotationModules)
        {
            module.Initialize(generator);
        }
    }

    public bool Conditioning() 
    {
        bool verified = true;

        //Check verifying
        //Logger.Log("[VERIFYING]");
        foreach (AnnotationVerifier verifier in conditions)
        {
            bool result = verifier.Execute();
            verified &= result;

            if (!result) //Stop running verifiers once there is one negative
                break;
        }

        return verified;
    }

    /// <summary>
    /// Pre Annotate will execute before the actual annotation is taken.
    /// Use this function to create a change in the actual annotation image
    /// </summary>
    public virtual void PreAnnotate() 
    {
        foreach (AnnotationModule module in annotationModules)
        {
            module.PreAnnotate();
        }
    }
    /// <summary>
    /// This function will execute after the camera gets the instruction to render.
    /// Use this function to clean up the previous distortion. And set everything to normal
    /// 
    /// note: It is possible that the camera's are still rendering. Accessing them could lead to unintended behaviour.
    /// </summary>
    public virtual void PostAnnotate() 
    {
        foreach (AnnotationModule module in annotationModules)
        {
            module.PostAnnotate();
        }
    }
    /// <summary>
    /// This function gets called just before the exportation of the annotation.
    /// </summary>
    public virtual void PreExport() 
    {
        foreach (AnnotationModule module in annotationModules)
        {
            module.PreExport();
        }
    }

    /// <summary>
    /// This function gets called after the exportation of the annotation.
    /// </summary>
    public virtual void PostExport() 
    {
        foreach (AnnotationModule module in annotationModules)
        {
            module.PostExport();
        }
    }

    public bool Validation() 
    {
        bool validated = true;
        foreach (AnnotationVerifier verifier in validators)
        {
            bool result = verifier.Execute();
            validated &= result;

            if (!result) //Stop running verifiers once there is one negative
                break;
        }
        return validated;
    }

    private void OnDisable()
    {
        annotationModules.Clear();
    }
}
