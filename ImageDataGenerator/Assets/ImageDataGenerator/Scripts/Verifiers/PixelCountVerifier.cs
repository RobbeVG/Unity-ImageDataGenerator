using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PixelCountVerifier", menuName = "AnnotationSystem/Verifiers/PixelCount")]
public sealed class PixelCountVerifier : AnnotationVerifier
{
    #region Variables
    #region Inspector
    [Tooltip("Percentage of pixels that cover the entire render texture")]
    [Range(0.0f, 1.0f)]
    [SerializeField]
    float coverPercentage = 0.0f;

    [SerializeField]
    bool StopAfterOne = false;
    #endregion Inspector

    //#region Private
    ////Compute shader variables
    //ComputeBuffer pixelCountsVisibility = null;
    //int threadGroupsX = 0;
    //int threadGroupsY = 0;
    //#endregion Private

    //Dictionary<AnnotationObject, uint> ObjectsPixelCount = new Dictionary<AnnotationObject, uint>();
    #endregion Variables

    //protected override void Start()
    //{
    //    //Check if set
    //    if (!pixelCountComputeShader)
    //        Debug.LogError("Please add pixel count Compute shader");

    //    if (Generator.ObjectManager.AnnotatedObjects.Count == 0)
    //        Debug.LogError("Please select some annotation objects");

    //    //Calculate the 
    //    float width = Generator.Segmentation.Camera.Component.targetTexture.width;
    //    float height = Generator.Segmentation.Camera.Component.targetTexture.height;
    //    threadGroupsX = Mathf.CeilToInt(width / 16);
    //    threadGroupsY = Mathf.CeilToInt(height / 16);

    //    //Set compute buffers
    //    pixelCountsVisibility = new ComputeBuffer(threadGroupsX * threadGroupsY, sizeof(uint)); //number of threads inside the shader

    //    int kernelHandle = pixelCountComputeShader.FindKernel("CSMain");
    //    pixelCountComputeShader.SetBuffer(kernelHandle, "countsVisibility", pixelCountsVisibility);
    //}

    public override bool Execute()
    {
        HashSet<AnnotationObject> copyOfEditable = new HashSet<AnnotationObject>(Generator.EditableObjects);
        foreach (AnnotationObject annotationObject in copyOfEditable)
        {
            float screenCoverPercentage = (float)Generator.Segmentation.GetPixelCount(annotationObject) / (float)Generator.Segmentation.SegmentationResolution;
            if (screenCoverPercentage < coverPercentage)
                Generator.EditableObjects.Remove(annotationObject);
        }

        Log("There are " + Generator.EditableObjects.Count.ToString() + "editable Annotation Objects.");

        if (Generator.EditableObjects.Count == 0)
            return false;
        else if (StopAfterOne) 
            Generator.EditableObjects = new HashSet<AnnotationObject>() { Generator.EditableObjects.ElementAt(Random.Range(0, Generator.EditableObjects.Count)) }; //Creating editable objest

        return true;
    }

    //public void CountPixels(HashSet<AnnotationObject> annotationObjects) 
    //{
    //    //Set data in shader
    //    int kernelHandle = pixelCountComputeShader.FindKernel("CSMain");
    //    pixelCountComputeShader.SetTexture(kernelHandle, "visibilityTex", Generator.Segmentation.Camera.Component.targetTexture);
    //    foreach (AnnotationObject annotationObject in annotationObjects)
    //    {
    //        pixelCountComputeShader.SetInt("threadGroupsX", threadGroupsX); //Array offset
    //        pixelCountComputeShader.SetFloats("colorID", annotationObject.IDColor.r, annotationObject.IDColor.g, annotationObject.IDColor.b, annotationObject.IDColor.a);
    //        pixelCountComputeShader.SetInts("startPosition", new int[2] { annotationObject.ScreenBounds.x, annotationObject.ScreenBounds.y });

    //        pixelCountComputeShader.Dispatch(kernelHandle, threadGroupsX, threadGroupsY, 1);
    //        uint[] countsVisibility = new uint[threadGroupsX * threadGroupsY]; // number of groups
    //        pixelCountsVisibility.GetData(countsVisibility);

    //        uint totalPixelsVisability = 0;
    //        foreach (uint count in countsVisibility)
    //            totalPixelsVisability += count;

    //        ObjectsPixelCount[annotationObject] = totalPixelsVisability;
    //    }
    //}

    //public override void Destroy()
    //{
    //    pixelCountsVisibility.Release();
    //}
}
