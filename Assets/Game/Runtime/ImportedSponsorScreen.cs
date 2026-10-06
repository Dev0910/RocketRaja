using UnityEngine;
namespace RocketRaja
{
    // Keep the script GUID loadable while older saved scenes/prefabs are replaced.
    // Stale routes and UnityEvents cannot display an offer or invoke an ad provider.
    public sealed class ImportedSponsorScreen : MonoBehaviour
    {
        void OnEnable() => gameObject.SetActive(false);
        public void Bind(GameController owner) { }
        public void Open() => gameObject.SetActive(false);
        public void RequestVideo() { }
        public void Close() => gameObject.SetActive(false);
    }
}
