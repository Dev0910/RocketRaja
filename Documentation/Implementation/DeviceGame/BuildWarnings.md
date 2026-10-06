# Development build warning review

The full native build before the Talent art correction succeeded with zero errors and 22 warnings. The current account scene-flow build succeeds with zero errors and 30 warnings; exact distinct messages are retained in `BuildWarnings.txt` and match the previous account-recovery build. The BuildReport messages were inspected in the Editor. The distinct categories are:

- Two optional MCP package assembly definitions have no runtime scripts; Unity omits those empty assemblies.
- Legacy cloud/smoke code uses Unity object-search overloads deprecated by this Editor version. These remain supported; the checks found their expected scene objects.
- Two legacy quality-harness comparisons produce CS0252 possible reference-comparison warnings. They are in preserved legacy smoke coverage, not the redesigned branch.
- The existing app icon is compressed. Unity warns that exported icon quality can be affected; this does not fail package creation or alignment checks.
- Eight additional ProBuilder Standard Vertex Color shader variants warn about vector truncation in Unity's shadow include on GLES3/Vulkan. They compile successfully; this report does not establish their visual behavior on the phone.

The exact count can differ between a full compilation and a cached build. Zero build errors is required for acceptance. These warnings are not represented as runtime errors or as evidence that the device test passed. The prior generated-Firebase dependency errors were investigated separately in `ResolverRecovery.md`; the successful build passed ordinary dependency resolution and generated POM/AAR validation.
