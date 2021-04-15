using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DuplicateModifier", menuName = "AnnotationModifier/Duplicate")]
public sealed class DuplicateModifier : AnnotationModifier
{
    HashSet<AnnotationObject> objects = new HashSet<AnnotationObject>();
    HashSet<AnnotationObject> originalModifiableObjects = null;

    [SerializeField]
    bool swapModifiableObjects = true;

    public override void PreAnnotate()
    {
        foreach (AnnotationObject annotationObject in generator.ObjectManager.ModifiableAnnotatedObjects)
        {
            Log("Creating a copy of : " + annotationObject.gameObject.name);
            objects.Add(Instantiate(annotationObject));
        }
        if (swapModifiableObjects) 
        {
            originalModifiableObjects = generator.ObjectManager.ModifiableAnnotatedObjects;
            generator.ObjectManager.ModifiableAnnotatedObjects = objects;
        }
    }

    public override void PostAnnotate()
    {
        foreach (AnnotationObject annotationObject in objects)
        {
            Log("Destroying copy of : " + annotationObject.gameObject.name);
            Destroy(annotationObject.gameObject);
        }
        
        if (swapModifiableObjects)
            generator.ObjectManager.ModifiableAnnotatedObjects = originalModifiableObjects;
        
        objects.Clear();
    }
}
