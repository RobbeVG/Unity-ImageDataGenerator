using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AnnotationCamera : MonoBehaviour
{
    //// TODO REMOVE
    // Could be removed.
    // Only tracks if it has rendered.

    private bool hasRendered = false;
    private Camera cameraComponent = null;

    #region Getters
    public Camera Component { get { return cameraComponent; } }
    public bool HasRendered { get { return hasRendered; } }

    #endregion Getters

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();
        if (!cameraComponent)
            Debug.LogWarning("Did not found on gameobject: " + name);

        cameraComponent.enabled = false; //Turn of the camera we will Render Manually
    }

    public void Render()
    {
        hasRendered = false;

        //Camera's are turned off so needs to be rendered manually
        cameraComponent.Render();
    }

    private void OnPostRender()
    {
        hasRendered = true;
    }
}
