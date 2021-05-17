using System.Collections.Generic;
using System.Security.Permissions;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AnnotationGenerator))]
public class AnnotationObjectManager : MonoBehaviour
{
    enum Type
    {
        Excluding,
        Including
    }

    [SerializeField]
    [Tooltip("Adding a parent object will allow all children to be visible.")]
    List<GameObject> selectedObjects = new List<GameObject>();
    [SerializeField]
    Type settingsSelected = Type.Including;

    static uint totalIds = 0;

    AnnotationGenerator generator = null;

#if UNITY_EDITOR //DEBUGGING PURPOSES
    [Header("==EDITOR ONLY==")]
    [SerializeField]
    int incrementID = 1;
#endif

    public HashSet<AnnotationObject> AnnotatedObjects { get; } = new HashSet<AnnotationObject>();

    // Start is called before the first frame update
    void Awake()
    {
        generator = GetComponent<AnnotationGenerator>();
        if (!generator)
            Debug.LogError("NO GENERATOR ?!?!?!?!?!?");

        switch (settingsSelected)
        {
            case Type.Excluding:
                AddAnnotationObjects(ExcludeObjects());
                break;
            case Type.Including:
                AddAnnotationObjects(IncludeObjects());
                break;
            default:
                break;
        }
        selectedObjects.Clear(); //Resets anyway when quits

        enabled = false;
    }

    private void OnDestroy() //Clean up previously placed visibility objects
    {
        foreach (AnnotationObject visibilityObject in AnnotatedObjects)
        {
            Destroy(visibilityObject);
        }
    }

    private void AddAnnotationObjects(HashSet<Renderer> renderers)
    {
        //Renderers are unique!
        foreach (Renderer renderer in renderers)
        {
            GameObject target = renderer.gameObject;
            if (!target.TryGetComponent(out AnnotationObject annotationObject)) //Check if added before by other objectManagers or manually
            {
                //If not added
                annotationObject = target.AddComponent<AnnotationObject>(); //Attach the script to objects of interest
                //Set render callback

                //Set ID
#if UNITY_EDITOR
                totalIds += (uint)incrementID;
                annotationObject.ID = totalIds;
#else
                annotationObject.ID = ++totalIds;
#endif
            }
            //Add callback to this manager
            annotationObject.renderCallBacks.Add(generator.OnSegmentationCamRender);

            //Add to annotated objects of this manager
            AnnotatedObjects.Add(annotationObject);
        }
        
    }

    private HashSet<Renderer> ExcludeObjects()
    {
        HashSet<Renderer> renderers = new HashSet<Renderer>(FindObjectsOfType<Renderer>());

        //Get rid of all objects that have a renderer
        foreach (GameObject gameObject in selectedObjects)
        {
            foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
            {
                renderers.Remove(renderer);
            }
        }

        return renderers;
    }

    private HashSet<Renderer> IncludeObjects() 
    {
        HashSet<Renderer> renderers = new HashSet<Renderer>();
        foreach (GameObject gameObject in selectedObjects)
        {
            foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
            {
                renderers.Add(renderer);
            }
        }
        return renderers;
    }
}
