using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ManualAnnotateModifier", menuName = "AnnotationModifier/Manual annotate")]
public sealed class ManualAnnotateModifier : AnnotationModifier
{
    [System.Flags]
    enum AnnotationPhase : sbyte //sbyte so that the everything function works in the inspector! (Super weird bug imo)
    { 
        PreAnnotate     = (1 << 0),
        PostAnnotate    = (1 << 1),
        PreExport       = (1 << 2),
        PostExport      = (1 << 3)
    }

    [SerializeField]
    AnnotationPhase createAnnotation;

    protected override void Start() {}

    private void CreateAnnotation() 
    {
        generator.DisableModifiers = true;
        generator.Annotate();
        generator.DisableModifiers = false;
    }

    public override void PreAnnotate()
    {
        if (createAnnotation.HasFlag(AnnotationPhase.PreAnnotate))
            CreateAnnotation();
    }

    public override void PostAnnotate()
    {
        if (createAnnotation.HasFlag(AnnotationPhase.PostAnnotate))
            CreateAnnotation();
    }
    public override void PreExport()
    {
        if (createAnnotation.HasFlag(AnnotationPhase.PreExport))
            CreateAnnotation();
    }
    public override void PostExport()
    {
        if (createAnnotation.HasFlag(AnnotationPhase.PostExport))
            CreateAnnotation();
    }
}