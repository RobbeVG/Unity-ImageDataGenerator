using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TransformModifier", menuName = "AnnotationSystem/Modifiers/Transform")]

public class TransformModifier : AnnotationModifier
{
    enum TranslationSpace
    {
        WorldSpace,
        LocalSpace,
        CameraSpace,
        RelativeTowardsCamera
    }

    [Header("Translate Settings")]
    [SerializeField]
    TranslationSpace translationType = TranslationSpace.WorldSpace;

    [SerializeField]
    private Vector3 displacement = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void PreAnnotate()
    {
        foreach (AnnotationObject annotationObject in generator.ObjectManager.ModifiableAnnotatedObjects)
        {
            switch (translationType)
            {
                case TranslationSpace.WorldSpace:
                    annotationObject.transform.Translate(displacement, Space.World);
                    break;
                case TranslationSpace.LocalSpace:
                    annotationObject.transform.Translate(displacement, Space.Self);
                    break;
                case TranslationSpace.CameraSpace:
                    annotationObject.transform.Translate(displacement, generator.OutputCamera.transform);
                    break;
                case TranslationSpace.RelativeTowardsCamera:
                    GameObject lookAtGuy = new GameObject("LookAtGuy");
                    lookAtGuy.transform.position = generator.OutputCamera.transform.position;
                    lookAtGuy.transform.LookAt(annotationObject.Renderer.bounds.center);

                    annotationObject.transform.Translate(displacement, lookAtGuy.transform);
                    Destroy(lookAtGuy);
                    break;
                default:
                    break;
            }
            Log("Translated " + annotationObject.name + " with :" + displacement.ToString("R") + " in space: " + translationType.ToString()); ;
        }
    }
}
