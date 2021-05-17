using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[CreateAssetMenu(fileName = "DuplicateModifier", menuName = "AnnotationSystem/Modifiers/Duplicate")]
public sealed class DuplicateModifier : AnnotationModifier
{
    //enum TranslationSpace 
    //{
    //    WorldSpace,
    //    LocalSpace,
    //    CameraSpace,
    //    RelativeTowardsCamera
    //}

    [SerializeField]
    bool swapModifiableObjects = true;

    [SerializeField]
    bool clearColorID = true;

    [SerializeField]
    bool destroyDuplicate = true;

    HashSet<AnnotationObject> objects;
    HashSet<AnnotationObject> originalModifiableObjects;

    protected override void Start()
    {
        objects = new HashSet<AnnotationObject>();
        originalModifiableObjects = null;
    }

    public override void PreAnnotate()
    {
        foreach (AnnotationObject annotationObject in Generator.EditableObjects)
        {
            Log("Creating a copy of : " + annotationObject.gameObject.name);

            AnnotationObject dupedObject = Instantiate(annotationObject, annotationObject.transform.position, annotationObject.transform.rotation);

            if (clearColorID)
                dupedObject.ID = 0;

            //if (randomizeColor) 
            //{
            //    Color randomColor = Random.ColorHSV();

            //    int amountMaterials = dupedObject.Renderer.materials.Length;
            //    Material[] materials = new Material[amountMaterials];

            //    //Copy 
            //    for (int i = 0; i < amountMaterials; i++)
            //    {
            //        materials[i] = new Material(dupedObject.Renderer.materials[i]);
            //        materials[i].color = randomColor;
            //    }

            //    Log("Randomized duplicate materials with color: " + randomColor.ToString());
            //    dupedObject.Renderer.materials = materials;
            //}

            //if (translateDuplicate) 
            //{
            //    switch (translationType)
            //    {
            //        case TranslationSpace.WorldSpace:
            //            dupedObject.transform.Translate(translationVector, Space.World);
            //            break;
            //        case TranslationSpace.LocalSpace:
            //            dupedObject.transform.Translate(translationVector, Space.Self);
            //            break;
            //        case TranslationSpace.CameraSpace:
            //            dupedObject.transform.Translate(translationVector, generator.OutputCamera.transform);
            //            break;
            //        case TranslationSpace.RelativeTowardsCamera:
            //            GameObject lookAtGuy = new GameObject("LookAtGuy");
            //            lookAtGuy.transform.position = generator.OutputCamera.transform.position;
            //            lookAtGuy.transform.LookAt(dupedObject.Renderer.bounds.center);

            //            dupedObject.transform.Translate(translationVector, lookAtGuy.transform);
            //            Destroy(lookAtGuy);
            //            break;
            //        default:
            //            break;
            //    }
            //    Log("Translated duplicate object with :" + translationVector.ToString() + " in space: " + translationType.ToString());
            //}

            objects.Add(dupedObject);
        }
        if (swapModifiableObjects) 
        {
            originalModifiableObjects = Generator.EditableObjects;
            Generator.EditableObjects = objects;
        }
    }

    public override void PostAnnotate()
    {
        if (destroyDuplicate)
            foreach (AnnotationObject annotationObject in objects)
            {
                Log("Destroying copy of : " + annotationObject.gameObject.name);
                Destroy(annotationObject.gameObject);
            }

        if (swapModifiableObjects)
            Generator.EditableObjects = originalModifiableObjects;
        
        objects.Clear();
    }
}
