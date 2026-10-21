using UnityEngine;

namespace RocketRaja.Cloud
{
    /// <summary>Explicit release configuration, separate from billing and the legacy user collection.</summary>
    [CreateAssetMenu(menuName = "Rocket Raja/Redesign Cloud Configuration")]
    public sealed class RedesignCloudSettings : ScriptableObject
    {
        public bool enabledAfterReview;
        public string applicationId = "com.diwali.fireworkstycoon.dev";
        public string firebaseProjectId = "rocket-raja-4729c";
        public static RedesignCloudSettings Load() => Resources.Load<RedesignCloudSettings>("RedesignCloudSettings");
        public bool MatchesApplication => enabledAfterReview && applicationId == Application.identifier && !string.IsNullOrWhiteSpace(firebaseProjectId);
    }
}
