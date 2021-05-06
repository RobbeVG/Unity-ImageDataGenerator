using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VisibilityVerifier", menuName = "AnnotationSystem/Verifiers/Visibility")]
public sealed class VisibilityVerifier : AnnotationVerifier
{
    #region Enum
    enum Type : byte
    {
        AllObjects,
        FirstObject
    }
    #endregion Enum

    #region Variables
    #region Inspector
    [Header("Setup Parameters")]
    [SerializeField]
    ComputeShader pixelCountComputeShader = null;

    [Header("Validate Parameters")]
    [Tooltip("Remapping the bounds of an object to screen coordinates")]
    [Range(0.0f, 1.0f)]
    [SerializeField]
    float minimumCoverPercentageOfBoundsOnScreen = 0.0f;
    [Tooltip("Percentage of pixels that cover the entire render texture")]
    [Range(0.0f, 1.0f)]
    [SerializeField]
    float minimumCoverPercentageOfPixelsOnTexture = 0.0f;

    [SerializeField]
    Type stopVisibilityCheckAfter = Type.FirstObject;
    #endregion Inspector
    #region Private

    //Compute shader variables
    ComputeBuffer pixelCountsVisibility = null;
    int threadGroupsX = 0;
    int threadGroupsY = 0;
    #endregion Private
    #endregion Variables

    protected override void Start()
    {
        //Check if set
        if (!pixelCountComputeShader)
            Debug.LogError("Please add pixel count Compute shader");

        if (generator.ObjectManager.AnnotatedObjects.Count == 0)
            Debug.LogError("Please select some annotation objects");

        //Calculate the 
        float width = generator.SegmentationCamera.Component.targetTexture.width;
        float height = generator.SegmentationCamera.Component.targetTexture.height;
        threadGroupsX = Mathf.CeilToInt(width / 16);
        threadGroupsY = Mathf.CeilToInt(height / 16);

        //Set compute buffers
        pixelCountsVisibility = new ComputeBuffer(threadGroupsX * threadGroupsY, sizeof(uint)); //number of threads inside the shader

        int kernelHandle = pixelCountComputeShader.FindKernel("CSMain");
        pixelCountComputeShader.SetBuffer(kernelHandle, "countsVisibility", pixelCountsVisibility);
    }

    public override bool Execute()
    {
        generator.ObjectManager.ModifiableAnnotatedObjects.Clear();

        ///SCREEN PROPORTION - FIRST PASS
        foreach (AnnotationObject annotationObject in generator.ObjectManager.RenderedObjects[generator.SegmentationCamera])
        {
            //Calculate rectange -> Big objects are often remapped on the whole screen... But most objects are discarded
            annotationObject.CalculateScreenBounds(generator.OutputCamera.Component);

            RectInt screenBound = annotationObject.ScreenBounds;
            float percentageOfScreen = (float)(screenBound.width * screenBound.height) / (float)(generator.OutputCamera.Component.pixelHeight * generator.OutputCamera.Component.pixelWidth);

            if (minimumCoverPercentageOfBoundsOnScreen > percentageOfScreen) //Check bounds on the screen percentage
                continue;

            //Ratio on the screen is big enough so we add the object to the list
            generator.ObjectManager.ModifiableAnnotatedObjects.Add(annotationObject);
        }
        Log("First Pass of PossibleAnnotationObjects: " + generator.ObjectManager.ModifiableAnnotatedObjects.Count.ToString());

        ///CHECK PIXEL PROPORTION ON TEXTURE - SECOND PASS
        //Setting up second pass
        HashSet<AnnotationObject> oldModifiableObjects = new HashSet<AnnotationObject>(generator.ObjectManager.ModifiableAnnotatedObjects);
        generator.ObjectManager.ModifiableAnnotatedObjects.Clear();
        //Total resolution
        uint resolutionTexture = (uint)generator.SegmentationCamera.Component.targetTexture.width * (uint)generator.SegmentationCamera.Component.targetTexture.height;
        //Set data in shader
        int kernelHandle = pixelCountComputeShader.FindKernel("CSMain");
        pixelCountComputeShader.SetTexture(kernelHandle, "visibilityTex", generator.SegmentationCamera.Component.targetTexture);
        foreach (AnnotationObject annotationObject in oldModifiableObjects)
        {
            pixelCountComputeShader.SetInt("threadGroupsX", threadGroupsX); //Array offset
            pixelCountComputeShader.SetFloats("colorID", annotationObject.IDColor.r, annotationObject.IDColor.g, annotationObject.IDColor.b, annotationObject.IDColor.a);
            pixelCountComputeShader.SetInts("startPosition", new int[2] { annotationObject.ScreenBounds.x, annotationObject.ScreenBounds.y });

            pixelCountComputeShader.Dispatch(kernelHandle, threadGroupsX, threadGroupsY, 1);
            uint[] countsVisibility = new uint[threadGroupsX * threadGroupsY]; // number of groups
            pixelCountsVisibility.GetData(countsVisibility);

            uint totalPixelsVisability = 0;
            foreach (uint count in countsVisibility)
                totalPixelsVisability += count;

            float visibilityPercentage = (float)totalPixelsVisability / (float)resolutionTexture;
            if (minimumCoverPercentageOfPixelsOnTexture < visibilityPercentage)
            {
                generator.ObjectManager.ModifiableAnnotatedObjects.Add(annotationObject); //Adding modifiable object
                if (stopVisibilityCheckAfter == Type.FirstObject) { break; }; //Continue execution?
            }
        }
        Log("Last Pass of PossibleAnnotationObjects: " + generator.ObjectManager.ModifiableAnnotatedObjects.Count.ToString());

        if (generator.ObjectManager.ModifiableAnnotatedObjects.Count == 0)
            return false;
        return true;
    }

    public override void Destroy()
    {
        pixelCountsVisibility.Release();
    }
}
