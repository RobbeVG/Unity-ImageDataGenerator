using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MaterialModifier", menuName = "AnnotationModifier/Material")]
public sealed class MaterialModifier : AnnotationModifier
{
    delegate Material CreateMaterial(Material mat);

    enum Type : byte
    {
        stretchedTexture,
        missingMaterial,
        lowResolutionTexture,
        ZFightingMaterial
    }

    [SerializeField]
    Type materlialBugType = Type.missingMaterial;

    [Header("Z-Fighting parameters")]
    [SerializeField]
    Shader offsetShader = null;
    [SerializeField]
    [Tooltip("The factor parameter scales with the slope of the object in screen space which means you’ll see the offset affected when viewing the object from an angle.")]
    float factorOffset = 0;
    [SerializeField]
    [Tooltip("The units parameter scales with the minimum resolvable depth buffer value meaning as the depth buffer becomes less precise the value will increase preventing z-fighting.")]
    float unitOffset = 0;

    [Header("Modify Textures")]
    [SerializeField]
    Vector2 scale = new Vector2(50.0f, 1);
    [SerializeField]
    Vector2 lowRes = new Vector2(200.0f, 200.0f);

    Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

    protected override void Start()
    {
        if (materlialBugType == Type.ZFightingMaterial)
            if (!offsetShader)
                Debug.LogError("No offset Shader for the ZFighting material");
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
            case Type.ZFightingMaterial:
                function = CreateZFightingMaterial;
                break;
            default:
                function = (Material mat) => { return mat; };
                break;
        }

        foreach (AnnotationObject annotationObject in generator.ObjectManager.ModifiableAnnotatedObjects)
        {
            Log("Changing material to " + materlialBugType.ToString() + " on: " + annotationObject.gameObject.name);
            Renderer renderer = annotationObject.GetComponent<Renderer>();
            if (renderer)
            {
                originalMaterials.Add(renderer, renderer.materials);
                Material[] newMaterials = new Material[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    newMaterials[i] = function(renderer.materials[i]);
                    //render.materials[i] = function(renderer.materials[i]); Does not work because the array is copied
                }
                renderer.materials = newMaterials;
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

    private Material CreateZFightingMaterial(Material mat)
    {
        Material temp = new Material(mat);

        temp.shader = offsetShader;
        temp.SetFloat("_FactorOffset", factorOffset);
        temp.SetFloat("_UnitOffset", unitOffset);

        return temp;
    }
}