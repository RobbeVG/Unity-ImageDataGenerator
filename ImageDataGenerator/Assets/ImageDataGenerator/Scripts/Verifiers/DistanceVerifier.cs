using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DistanceVerifier", menuName = "AnnotationSystem/Verifiers/Distance")]
public sealed class DistanceVerifier : AnnotationVerifier
{
    [SerializeField]
    private float distanceToObject = 12.0f;
    private float requiredSquaredDistance = 0.0f;

    protected override void Start() 
    {
        requiredSquaredDistance = distanceToObject * distanceToObject;
    }

    public override bool Execute()
    {
        bool verified = false;
        HashSet<AnnotationObject> copyOfEditable = new HashSet<AnnotationObject>(Generator.EditableObjects);
        foreach (AnnotationObject annotationObject in copyOfEditable)
        {
            bool result;
            float squareDistance = Vector3.SqrMagnitude(annotationObject.transform.position - Generator.OutputCamera.transform.position);
            if (squareDistance > requiredSquaredDistance)
            {
                result = false;
                Generator.EditableObjects.Remove(annotationObject);
            }
            else 
            {
                result = true;
                verified = true;
            }

            Log(annotationObject.name + " has an (squared) distance of: " + squareDistance.ToString() + " and requires a (squared) distance of: " + requiredSquaredDistance.ToString() + " = " + result.ToString());
        }
        return verified;
    }
}
