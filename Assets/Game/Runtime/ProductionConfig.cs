using UnityEngine;
namespace RocketRaja
{
    [CreateAssetMenu(menuName = "Rocket Raja/Production Configuration")]
    public sealed class ProductionConfig : ScriptableObject
    {
        public string applicationId = "com.diwali.fireworkstycoon";
        public string privacyUrl = "";
        public bool releaseAcceptanceComplete;
    }
}
