using UnityEngine;

[CreateAssetMenu(fileName = "TimerModifier", menuName = "AnnotationModifier/Time ")]
public sealed class TimerModifier : AnnotationModifier
{
    [SerializeField]
    private float captureInterval = 2.0f;

    private float elapsedTime = 0.0f;

    protected override void Start()
    {
        if (captureInterval <= 0)
            Debug.LogError("Use a positive non-zero value in the TimerModifier");
    }

    public override void PreAnnotate()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= captureInterval)
        {
            Time.timeScale = 0;
            //elapsedTime -= captureInterval; //-> Not resetting the time because other modifiers could stopExecution
        }
        else 
        {
            generator.QuitExcecution = true;
        }
    }

    public override void PostAnnotate()
    {
        //Reset time after annotate
        elapsedTime %= captureInterval;
    }
}