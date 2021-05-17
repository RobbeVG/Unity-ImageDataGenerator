using System.Collections.Generic;
using UnityEngine;

public class AnnotationSegmentation : MonoBehaviour
{
    AnnotationGenerator generator = null;

    public AnnotationCamera Camera { get; private set; }

    private Dictionary<AnnotationObject, uint> pixelCount = new Dictionary<AnnotationObject, uint>();

    [SerializeField]
    private Shader segmentationShader = null;
    [SerializeField]
    private ComputeShader pixelCountComputeShader = null;

    [SerializeField]
    private bool countPixels = true;

    [SerializeField]
    AnnotationOutput exportSegmentationOutput = null;

    ComputeBuffer pixelCountsVisibility = null;
    int threadGroupsX = 0;
    int threadGroupsY = 0;

    public uint GetPixelCount(AnnotationObject annotationObject) => pixelCount.ContainsKey(annotationObject) ? pixelCount[annotationObject] : 0;

    public uint SegmentationResolution { get { return (uint)Camera.Component.targetTexture.width * (uint)Camera.Component.targetTexture.height; } }

    private void OnValidate()
    {
        if (!segmentationShader) 
        {
            Shader foundSegmentationShader = Shader.Find("Custom/ObjectIDSegmentation");
            segmentationShader = foundSegmentationShader;
        }

        if (!pixelCountComputeShader) 
        {
            ComputeShader[] compShaders = Resources.FindObjectsOfTypeAll<ComputeShader>();
            for (int i = 0; i < compShaders.Length; i++)
            {
                if (compShaders[i].name == "CountComputeShader")
                {
                    pixelCountComputeShader = compShaders[i];
                    return;
                }
            }
            Debug.LogWarning("Compute shader to count pixels has not been found");
        }

    }

    private void Awake()
    {
        if (!segmentationShader)
            Debug.LogError("No segmentation shader for segmentation generation");

        if (countPixels && !pixelCountComputeShader) 
            Debug.LogError("No computeshader for pixel counting");
    }

    public void Initialize(AnnotationGenerator generator)
    {
        this.generator = generator;

        AnnotationCamera outputCamera = generator.OutputCamera;
        //Instantiate segmentation camera
        GameObject segmentationGameObject = Instantiate(outputCamera.gameObject, gameObject.transform);
        segmentationGameObject.name = "SegmentationCam";
        segmentationGameObject.transform.position = outputCamera.transform.position;
        segmentationGameObject.transform.rotation = outputCamera.transform.rotation;

        Camera = segmentationGameObject.GetComponent<AnnotationCamera>();
        Camera.Component.clearFlags = CameraClearFlags.SolidColor;
        Camera.Component.backgroundColor = Color.clear;
        Camera.Component.renderingPath = RenderingPath.Forward;
        Camera.Component.allowMSAA = false;

        RenderTextureDescriptor descriptor = outputCamera.Component.targetTexture.descriptor; //Dimensions

        descriptor.bindMS = false;
        descriptor.msaaSamples = 1;
        Camera.Component.targetTexture = new RenderTexture(descriptor); //Create new texture  using dimensions of output
        Camera.Component.targetTexture.antiAliasing = 1;

        Camera.Component.SetReplacementShader(segmentationShader, "");


        //Instantiate shader settings

        float width = Camera.Component.targetTexture.width;
        float height = Camera.Component.targetTexture.height;
        threadGroupsX = Mathf.CeilToInt(width / 16);
        threadGroupsY = Mathf.CeilToInt(height / 16);

        pixelCountsVisibility = new ComputeBuffer(threadGroupsX * threadGroupsY, sizeof(uint)); //number of threads inside the shader

        int kernelHandle = pixelCountComputeShader.FindKernel("CSMain");
        pixelCountComputeShader.SetBuffer(kernelHandle, "countsVisibility", pixelCountsVisibility);
    }

    public void Run() 
    {
        // VISIBILITY
        if (countPixels) 
        {
            int kernelHandle = pixelCountComputeShader.FindKernel("CSMain");
            pixelCountComputeShader.SetTexture(kernelHandle, "visibilityTex", Camera.Component.targetTexture);

            //TODO Shader needs to count pixels for dynamic amount of id's at the same time! MASSIVE IMPROVEMENT -> ATM it's rerun for each object's id

            foreach (AnnotationObject annotationObject in generator.GetRenderedObjects())
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

                pixelCount[annotationObject] = totalPixelsVisability;
            }
        }

        if (exportSegmentationOutput)
            generator.Exporter.Export(Camera, exportSegmentationOutput);
    }

    private void OnDestroy()
    {
        pixelCountsVisibility.Release();
    }
}
