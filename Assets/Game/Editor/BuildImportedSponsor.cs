namespace RocketRaja.EditorTools
{
    // Older authoring calls fail explicitly instead of regenerating a retired screen.
    public static class BuildImportedSponsor
    {
        public static void Build() => throw new System.InvalidOperationException("Sponsor ads are retired in the redesign.");
        public static void Integrate() => throw new System.InvalidOperationException("Sponsor ads are retired in the redesign.");
    }
}
