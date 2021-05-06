using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CameraModifier", menuName = "AnnotationSystem/Modifiers/Camera")]
public sealed class CameraModifier : AnnotationModifier
{
    [SerializeField]
    private RenderTexture aliasedRenderTexture = null;
    private RenderTexture originalRenderTexture = null;

    Camera outputCameraComponent = null;

    protected override void Start()
    {
        if (!aliasedRenderTexture)
            Debug.LogError("No aliased renderTexture was given");

        outputCameraComponent = generator.OutputCamera.Component;
        if (!outputCameraComponent)
            Debug.LogError("Was not able to receive scene camera component from the cameras of AnnotationGenerator");
    }

    //Creating MSAA on the targetTexture
    public override void PreAnnotate()
    {
        outputCameraComponent.allowMSAA = false;
        originalRenderTexture = outputCameraComponent.targetTexture;
        outputCameraComponent.targetTexture = aliasedRenderTexture; 
    }

    //Swapping it again so that other generators have the correct texture
    public override void PostAnnotate()
    {
        outputCameraComponent.allowMSAA = true;
        outputCameraComponent.targetTexture = originalRenderTexture;
    }


    //Swapping textures for export
    public override void PreExport()
    {
        originalRenderTexture = outputCameraComponent.targetTexture;
        outputCameraComponent.targetTexture = aliasedRenderTexture;
    }

    public override void PostExport()
    {
        outputCameraComponent.targetTexture = originalRenderTexture;
    }
} 