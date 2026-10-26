using System;
namespace RocketRaja
{
    // Rewarded placements grant only after the provider confirms completion.
    public interface IRewardedAds : IDisposable
    {
        bool Ready { get; }
        bool PrivacyOptionsRequired { get; }
        void Initialize();
        void Tick();
        void Show(Action<bool, string> completed);
        void PrivacyOptions();
    }
    public sealed class DisabledRewardedAds : IRewardedAds
    {
        public bool Ready => false;
        public bool PrivacyOptionsRequired => false;
        public void Initialize() { }
        public void Tick() { }
        public void Dispose() { }
        public void PrivacyOptions() { }
        public void Show(Action<bool, string> completed) => completed?.Invoke(false, null);
    }
}
