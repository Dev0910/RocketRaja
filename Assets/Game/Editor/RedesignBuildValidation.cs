using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace RocketRaja.Editor
{
    public sealed class RedesignBuildValidation : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1000;
        public void OnPreprocessBuild(BuildReport report)
        {
            RequireEligibleBuild(report.summary.options);
            RedesignAuthoring.Validate();
        }

        internal static void RequireEligibleBuild(BuildOptions options)
        {
            // Applies to Build Profiles/BuildPipeline as well as the custom production menu.
            var config = UnityEngine.Resources.Load<ProductionConfig>("ProductionConfig");
            if ((options & BuildOptions.Development) == 0 && (RedesignEnvironment.Isolated || !config || !config.releaseAcceptanceComplete))
                throw new BuildFailedException("Classic release acceptance is incomplete. Finish account/billing configuration and store-installed validation before creating a release player.");
        }
    }
}
