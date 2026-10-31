namespace RocketRaja.Editor
{
    /// <summary>Compatibility entry point for the retired Google-account scene-flow test.
    /// Provider-independent account tests remain in Tools/RedesignLab.
    /// </summary>
    public static class RedesignAccountFlowValidation
    {
        public static void Start() => LocalOnlyStartupValidation.StartPlayChecks();
    }
}
