using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CameraModifier", menuName = "AnnotationSystem/Modifiers/Camera")]
public sealed class CameraModifier : AnnotationModifier
{
    enum CameraProperties 
    {
        Aliasing,
        Overdraw
    }


    [SerializeField]
    private RenderTexture aliasedRenderTexture = null;
    private RenderTexture originalRenderTexture = null;

    [SerializeField]
    CameraProperties properties = CameraProperties.Aliasing;

    [SerializeField]
    Shader replacementShader = null;
    [SerializeField]
    Color overdrawColor = Color.white;

    Camera outputCameraComponent = null;

    protected override void Start()
    {
        if (!aliasedRenderTexture)
            Debug.LogError("No aliased renderTexture was given");

        outputCameraComponent = Generator.OutputCamera.Component;
        if (!outputCameraComponent)
            Debug.LogError("Was not able to receive scene camera component from the cameras of AnnotationGenerator");

        switch (properties)
        {
            case CameraProperties.Aliasing:
                break;
            case CameraProperties.Overdraw:
                outputCameraComponent.clearFlags = CameraClearFlags.SolidColor;
                outputCameraComponent.backgroundColor = Color.black;
                outputCameraComponent.SetReplacementShader(replacementShader, "");
                Shader.SetGlobalColor("_OverDrawColor", overdrawColor);
                break;
            default:
                break;
        }
    }

    //Creating MSAA on the targetTexture
    public override void PreAnnotate()
    {
        switch (properties)
        {
            case CameraProperties.Aliasing:
                outputCameraComponent.allowMSAA = false;
                originalRenderTexture = outputCameraComponent.targetTexture;
                outputCameraComponent.targetTexture = aliasedRenderTexture;
                break;
            case CameraProperties.Overdraw:
                break;
            default:
                break;
        }
    }

    //Swapping it again so that other generators have the correct texture
    public override void PostAnnotate()
    {
        switch (properties)
        {
            case CameraProperties.Aliasing:
                outputCameraComponent.allowMSAA = true;
                outputCameraComponent.targetTexture = originalRenderTexture;
                break;
            case CameraProperties.Overdraw:
                break;
            default:
                break;
        }
    }

    //Swapping textures for export
    public override void PreExport()
    {
        switch (properties)
        {
            case CameraProperties.Aliasing:
                originalRenderTexture = outputCameraComponent.targetTexture;
                outputCameraComponent.targetTexture = aliasedRenderTexture;
                break;
            case CameraProperties.Overdraw:
                break;
            default:
                break;
        }
    }

    public override void PostExport()
    {
        switch (properties)
        {
            case CameraProperties.Aliasing:
                outputCameraComponent.targetTexture = originalRenderTexture;
                break;
            case CameraProperties.Overdraw:
                break;
            default:
                break;
        }

    }
} 