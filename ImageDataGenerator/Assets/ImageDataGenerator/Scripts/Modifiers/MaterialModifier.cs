using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MaterialModifier", menuName = "AnnotationSystem/Modifiers/Material")]
public sealed class MaterialModifier : AnnotationModifier
{
    delegate Material CreateMaterial(Material mat);

    enum Type : byte
    {
        stretchedTexture,
        missingMaterial,
        lowResolutionTexture,
        RandomColor
    }

    [SerializeField]
    Type materlialBugType = Type.missingMaterial;

    [Header("Modify Textures")]
    [SerializeField]
    Vector2 scale = new Vector2(50.0f, 1);
    [SerializeField]
    Vector2 lowRes = new Vector2(200.0f, 200.0f);


    Color randomColor = Color.clear;
    Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

    protected override void Start()
    {
    }

    public override void PreAnnotate()
    {
        //Debug.Log("Amount of potential Annotations Material objects " + generator.ObjectManager.ModifiableAnnotatedObjects.Count.ToString());
        CreateMaterial function;

        switch (materlialBugType)
        {
            case Type.stretchedTexture:
                function = CreateStretchedMaterial;
                break;
            case Type.missingMaterial:
                function = CreateMissingMaterial;
                break;
            case Type.lowResolutionTexture:
                function = CreateLowResMaterial;
                break;
            case Type.RandomColor:
                function = CreateRandomColorMaterial;
                randomColor = Random.ColorHSV();
                break;
            default:
                function = (Material mat) => { return mat; };
                break;
        }

        foreach (AnnotationObject annotationObject in Generator.EditableObjects)
        {
            Log("Changing material to " + materlialBugType.ToString() + " on: " + annotationObject.gameObject.name  + " " + randomColor.ToString() );
            Renderer renderer = annotationObject.Renderer;
            if (renderer)
            { 
                originalMaterials.Add(renderer, renderer.materials);

                Material[] newMaterials = new Material[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    newMaterials[i] = function(renderer.materials[i]);
                    //render.materials[i] = function(renderer.materials[i]); Does not work because the array is copied
                }
                annotationObject.Renderer.materials = newMaterials;
            }
        }
    }

    public override void PostAnnotate()
    {
        foreach (KeyValuePair<Renderer, Material[]> keyValuePair in originalMaterials) 
        {
            Log("Changing material to original material on: " + keyValuePair.Key.gameObject.name);

            keyValuePair.Key.materials = keyValuePair.Value;
        }

        originalMaterials.Clear();
    }

    private Material CreateStretchedMaterial(Material mat)
    {
        Material temp = new Material(mat);
        temp.mainTextureScale = scale;
        return temp;
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)] Compiler will probably inline
    private Material CreateMissingMaterial(Material mat)
    {
        return null;
    }

    private Material CreateLowResMaterial(Material mat)
    {
        Material temp = new Material(mat);
        temp.mainTextureScale = lowRes;
        return temp;
    }

    private Material CreateRandomColorMaterial(Material mat)
    {
        Material temp = new Material(mat);
        temp.color = Random.ColorHSV();
        return temp;
    }
}