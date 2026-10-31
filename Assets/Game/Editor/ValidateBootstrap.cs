namespace RocketRaja.EditorTools
{
    /// <summary>Compatibility entry points validate the shipped local-only startup.</summary>
    public static class ValidateBootstrap
    {
        public static readonly string[] States = { "loading", "recovery", "legacy" };
        public static void CaptureAll() => RocketRaja.Editor.LocalOnlyStartupValidation.CaptureAll();
        public static void StartSmoke() => RocketRaja.Editor.LocalOnlyStartupValidation.StartPlayChecks();
    }
}
