using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;

namespace RocketRaja.Editor
{
    public static class RedesignReleaseValidation
    {
        [MenuItem("Rocket Raja/Redesign/Validate Release Isolation")]
        public static void Run()
        {
            if (!RedesignEnvironment.Isolated) throw new InvalidOperationException("This check is for the current isolated prototype, not release acceptance.");
            var results = new List<string>();
            foreach (var mode in new[] { BuildOptions.None, BuildOptions.CompressWithLz4, BuildOptions.AutoRunPlayer })
            {
                bool rejected = false;
                try { RedesignBuildValidation.RequireEligibleBuild(mode); }
                catch (BuildFailedException) { rejected = true; }
                if (!rejected) throw new InvalidOperationException("Isolated release mode was accepted: " + mode);
                results.Add("PASS Isolated non-development build is rejected: " + mode);
            }
            RedesignBuildValidation.RequireEligibleBuild(BuildOptions.Development | BuildOptions.AllowDebugging);
            results.Add("PASS Explicit development diagnostics remain allowed by the isolation guard");
            Directory.CreateDirectory("Documentation/Implementation");
            File.WriteAllLines("Documentation/Implementation/Unity-Release-Isolation.txt", results);
            UnityEngine.Debug.Log("REDESIGN_RELEASE_ISOLATION_OK " + results.Count);
        }
    }
}
