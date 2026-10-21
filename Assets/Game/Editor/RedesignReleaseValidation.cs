using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;

namespace RocketRaja.Editor
{
    public static class RedesignReleaseValidation
    {
        [MenuItem("Rocket Raja/Redesign/Validate Build Policy")]
        public static void Run()
        {
            var results = new List<string>();
            new RedesignBuildValidation().OnPreprocessBuild(null);
            results.Add("PASS Ordinary builds validate content without requiring production acceptance or Development Build");
            var config = UnityEngine.Resources.Load<ProductionConfig>("ProductionConfig");
            bool shouldReject = RedesignEnvironment.Isolated || !config || !config.releaseAcceptanceComplete;
            bool rejected = false;
            try { RedesignBuildValidation.RequireProductionReady(); }
            catch (BuildFailedException) { rejected = true; }
            if (rejected != shouldReject) throw new InvalidOperationException("Production readiness guard did not match the configured release acceptance");
            results.Add("PASS Production readiness guard " + (rejected ? "blocks the unapproved production bundle" : "accepts configured release readiness"));
            Directory.CreateDirectory("Documentation/Implementation");
            File.WriteAllLines("Documentation/Implementation/Unity-Build-Policy.txt", results);
            UnityEngine.Debug.Log("REDESIGN_BUILD_POLICY_OK " + results.Count);
        }
    }
}
