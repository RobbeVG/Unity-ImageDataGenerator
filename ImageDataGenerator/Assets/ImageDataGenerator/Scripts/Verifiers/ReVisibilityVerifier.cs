using System.Collections.Generic;
using UnityEngine;

////TODO DELETE ALLL THIS
[CreateAssetMenu(fileName = "ReVisibilityVerifier", menuName = "AnnotationSystem/Verifiers/ReVisibility")]

public class ReVisibilityVerifier : AnnotationVerifier
{
    [SerializeField]
    [Range(0.0f, 1.0f)]
    float minChangeVisibility = 0.3f;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    float maxChangeVisibility = 0.7f;


    public override bool Execute()
    {
        IReadOnlyDictionary<AnnotationObject, uint> pixelCounts = Generator.Segmentation.GetPixelCounts(); //REFERENCE
        Dictionary<AnnotationObject, uint> originalPixelCounts = new Dictionary<AnnotationObject, uint>();

        HashSet<AnnotationObject> originalObjects = new HashSet<AnnotationObject>();
        foreach (KeyValuePair<AnnotationObject, uint> item in pixelCounts)
        {
            originalObjects.Add(item.Key);
            originalPixelCounts.Add(item.Key, item.Value);
        }

        Generator.Segmentation.Camera.Render();
        Generator.Segmentation.Run();

        foreach (AnnotationObject annotationObject in originalObjects)
        {
            if (originalPixelCounts[annotationObject] == 0)
                continue;

            float visibilityPercentage = (float)pixelCounts[annotationObject] / (float)originalPixelCounts[annotationObject];

            Log(annotationObject.name + " is visible with blocked by: " + visibilityPercentage.ToString());

            if (minChangeVisibility < visibilityPercentage)
                if (maxChangeVisibility > visibilityPercentage)
                    return true;
        }
        return false;
    }
}
