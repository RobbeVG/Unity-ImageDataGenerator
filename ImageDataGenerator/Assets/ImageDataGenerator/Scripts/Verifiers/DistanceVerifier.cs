
using UnityEngine;

[CreateAssetMenu(fileName = "DistanceVerifier", menuName = "AnnotationSystem/Verifiers/Distance")]
public sealed class DistanceVerifier : AnnotationVerifier
{
    [SerializeField]
    private float distanceToObject = 12.0f;

    public override bool Execute()
    {
        return true;
    }

    public override void PostAnnotate()
    {
    }
}
