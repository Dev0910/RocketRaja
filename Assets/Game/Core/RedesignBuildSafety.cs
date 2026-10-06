// REDESIGN_LAB is only for the standalone .NET harness. Do not allow a custom
// Unity build to expose fixture-reset code by injecting this symbol into a release.
#if UNITY_5_3_OR_NEWER && REDESIGN_LAB && !UNITY_EDITOR && !DEVELOPMENT_BUILD
#error REDESIGN_LAB is not permitted in a non-development Unity player.
#endif
