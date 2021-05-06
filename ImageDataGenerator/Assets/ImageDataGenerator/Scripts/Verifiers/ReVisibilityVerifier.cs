using System.Collections.Generic;
using UnityEngine;

//TODO DELETE ALLL THIS
[CreateAssetMenu(fileName = "ReVisibilityVerifier", menuName = "AnnotationSystem/Verifiers/ReVisibility")]

public class ReVisibilityVerifier : AnnotationVerifier
{
    [SerializeField]
    VisibilityVerifier visibility = null;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    float minBlockage = 0.3f;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    float maxBlockage = 0.7f;

    protected override void Start()
    {

        if (!visibility)
            Debug.LogError("Visibility not passed");
        if (visibility.Generator == null)
        {
            Debug.LogError("Visibility not initialized");
        }
    }

    public override bool Execute()
    {
        Dictionary<AnnotationObject, uint> originalValues = new Dictionary<AnnotationObject, uint>(visibility.ObjectsPixelCount);
        HashSet<AnnotationObject> objects = new HashSet<AnnotationObject>(originalValues.Keys);

        visibility.Generator.SegmentationCamera.Render();
        visibility.CountPixels(objects);

        foreach (AnnotationObject bject in objects)
        {
            float amountOfBlockage = 1.0f - ((float)visibility.ObjectsPixelCount[bject] / (float)originalValues[bject]);
            if (minBlockage < amountOfBlockage)
                if (maxBlockage > amountOfBlockage)
                    return true;

            Debug.LogWarning("Still visible of original object " + amountOfBlockage);
        }

        return false;
    }
}
