using System;
using UnityEngine;

[CreateAssetMenu(fileName = "QuitVerifier", menuName = "AnnotationSystem/Verifiers/Quit")]
public class QuitVerifier : AnnotationVerifier
{
    [SerializeField]
    [Tooltip("Amount of annotations until quit")]
    int amountOfAnnotations = 1000;

    int currentAmountOfAnnotations;

    protected override void Start()
    {
        currentAmountOfAnnotations = 0;
    }

    public override bool Execute()
    {
        if (currentAmountOfAnnotations >= amountOfAnnotations) 
        {
#if UNITY_EDITOR
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            Log("Stopping application");
            return false;
        }
        return true;
    }

    public override void PostAnnotate()
    {
        currentAmountOfAnnotations++;
    }
}
