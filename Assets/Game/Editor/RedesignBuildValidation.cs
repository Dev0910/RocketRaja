using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace RocketRaja.Editor
{
    public sealed class RedesignBuildValidation : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1000;
        public void OnPreprocessBuild(BuildReport report)
        {
            // Ordinary Build Profiles/BuildPipeline builds validate content, but do not
            // require store rollout approval. The production command checks that separately.
            RedesignAuthoring.Validate();
        }

        internal static void RequireProductionReady()
        {
            var config = UnityEngine.Resources.Load<ProductionConfig>("ProductionConfig");
            if (RedesignEnvironment.Isolated || !config || !config.releaseAcceptanceComplete)
                throw new BuildFailedException("Classic release acceptance is incomplete. Finish local-save, advertising and store-installed validation before creating a production bundle.");
        }
    }
}
