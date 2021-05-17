public abstract class AnnotationVerifier : AnnotationModule
{
    /// <summary>
    /// This function will execute after all verifiers are updated.
    /// Returning false will terminate the annotation.
    /// Returning true will execute the next verifier.
    /// </summary>
    public abstract bool Execute();

    protected override void Start() { }
    public override void PreAnnotate() { }
    public override void PostAnnotate() { }
    public override void PreExport() { }
    public override void PostExport() { }
    public override void Destroy() { }
}
