using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[DisallowMultipleComponent]
[RequireComponent(typeof(Renderer))]
public class AnnotationObject : MonoBehaviour
{
    public delegate void OnWillRenderCallBack(AnnotationCamera camera, AnnotationObject annotationObject);

    #region Inspector ReadOnly
    /*[ReadOnly]*/ [SerializeField]
    Color idColor = Color.black;
    //[ReadOnly] [SerializeField]
    //RenderTexture renderTexture;
    #endregion Inspector ReadOnly

    private new Renderer renderer = null; //Same name (new)
    private uint id = 0;

    List<AnnotationCamera> previousRendered = new List<AnnotationCamera>();
    #region Getters and Setters
    public uint ID { get { return id; } set { id = value; SetIDColor(); } }
    public Color IDColor { get { return idColor; } }
    //public RenderTexture RenderTexture { get { return renderTexture; } set { renderTexture = value; } }
    public Renderer Renderer { get { return renderer; } }
    public List<OnWillRenderCallBack> renderCallBacks { get; } = new List<OnWillRenderCallBack>();
    #endregion

    //Screen bounds -> Manually calculate with CalculateCameraBounds
    RectInt screenBounds = new RectInt();
    public RectInt ScreenBounds { get { return screenBounds; } }


    //Vector3[] boundCoordinates = new Vector3[8];

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
        if (!renderer)
            Debug.LogError("No Renderer found on visibility object");
    }

    private void Update()
    {
        previousRendered.Clear();
    }

    private void OnWillRenderObject()
    {
        AnnotationCamera annotationCamera = Camera.current.GetComponent<AnnotationCamera>();
        if (annotationCamera) //Potentially not an Annoatation Camera
            foreach (OnWillRenderCallBack callback in renderCallBacks) //Multiple object managers
            {
                callback(annotationCamera, this);
            }
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawWireCube(renderer.bounds.center, renderer.bounds.extents * 2);

        //Mesh mesh = new Mesh();
        //mesh.colors = new Color[8] { Color.white, Color.white, Color.white, Color.white, Color.white, Color.white, Color.white, Color.white };
        //mesh.vertices = boundCoordinates;
        //for (int i = 0; i < boundCoordinates.Length; i++)
        //{
        //    Gizmos.color = Color.Lerp(Color.white, Color.black, i / (float)(boundCoordinates.Length - 1));
        //    Gizmos.DrawSphere(boundCoordinates[i], 0.33f);
        //}
    }

    private void SetIDColor() 
    {
        int temp = unchecked((int)id);
        MaterialPropertyBlock block = new MaterialPropertyBlock();

        uint r = (id & 0xFF);
        uint g = ((id >> 8) & 0xFF);
        uint b = ((id >> 16) & 0xFF);
        uint a = ((id >> 24) & 0xFF);

        idColor.r = r / 255.0f;
        idColor.g = g / 255.0f;
        idColor.b = b / 255.0f;
        idColor.a = a / 255.0f;

        block.SetColor("_ID", idColor);
        renderer.SetPropertyBlock(block);
    }

    /// <summary>
    /// Calculates the boundaries of the object in the camera's view.
    /// The bottom-left of the screen is (0,0); the right-top is (pixelWidth,pixelHeight). The z position is in world units from the camera.
    /// </summary>
    /// <param name="camera"></param>
    public void CalculateScreenBounds(Camera camera)
    {
        Vector3 cen = renderer.bounds.center;
        Vector3 ext = renderer.bounds.extents;

        //boundCoordinates = new Vector3[8]
        //{
        //    new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z),
        //    new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z),
        //    new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z),
        //    new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z),
        //    new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z),
        //    new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z),
        //    new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z),
        //    new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z)
        //};

        Vector2[] screenPoints = new Vector2[8]
        {
            camera.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z)),
            camera.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z)),
            camera.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z)),
            camera.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z)),
            camera.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z)),
            camera.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z)),
            camera.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z)),
            camera.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z))
        };

        //Vector3[] coordinates = new Vector3[8] {
        //    camera.WorldToViewportPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z), Camera.MonoOrStereoscopicEye.Mono),
        //    camera.WorldToViewportPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z), Camera.MonoOrStereoscopicEye.Mono),
        //    camera.WorldToViewportPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z), Camera.MonoOrStereoscopicEye.Mono),
        //    camera.WorldToViewportPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z), Camera.MonoOrStereoscopicEye.Mono),
        //    camera.WorldToViewportPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z), Camera.MonoOrStereoscopicEye.Mono),
        //    camera.WorldToViewportPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z), Camera.MonoOrStereoscopicEye.Mono),
        //    camera.WorldToViewportPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z), Camera.MonoOrStereoscopicEye.Mono),
        //    camera.WorldToViewportPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z), Camera.MonoOrStereoscopicEye.Mono)
        //};

        //Ordering:[0] = Left, [1] = Right, [2] = Down, [3] = Up, [4] = Near, [5] = Far

        //Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        ////planes[0].



        //for (int i = 0; i < coordinates.Length; i++)
        //{
        //    Vector3 coord = coordinates[i];
        //    coord.x = Mathf.Clamp(coord.x, 0.0f, 1.0f);
        //    coord.y = Mathf.Clamp(coord.y, 0.0f, 1.0f);


        //    boundCoordinates[i] = camera.ViewportToWorldPoint(coord, Camera.MonoOrStereoscopicEye.Mono);
        //    //boundCoordinates[i].z = coord.z;
        //}

        Vector2 min = screenPoints[0];
        Vector2 max = screenPoints[0];

        for (int i = 1; i < screenPoints.Length; i++)
        {
            ref Vector2 currentPoint = ref screenPoints[i];

            min.x = Mathf.Min(min.x, currentPoint.x);
            min.y = Mathf.Min(min.y, currentPoint.y);
            max.x = Mathf.Max(max.x, currentPoint.x);
            max.y = Mathf.Max(max.y, currentPoint.y);



            //min = Vector2.Min(min, currentPoint);
            //max = Vector2.Max(max, currentPoint);
        }

        screenBounds = new RectInt(Mathf.RoundToInt(min.x), Mathf.RoundToInt(min.y), Mathf.RoundToInt(max.x - min.x), Mathf.RoundToInt(max.y - min.y));
        screenBounds.ClampToBounds(new RectInt(0, 0, camera.pixelWidth, camera.pixelHeight));
    }
}
