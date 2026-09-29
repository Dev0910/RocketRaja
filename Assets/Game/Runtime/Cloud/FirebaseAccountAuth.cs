using System;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using RocketRaja.Core;

namespace RocketRaja.Cloud
{
    public sealed class FirebaseAccountAuth : IAccountAuth
    {
        readonly IGoogleSignInProvider google;
        FirebaseAuth auth;
        public event Action Changed;
        public FirebaseAccountAuth(IGoogleSignInProvider google) { this.google = google; }
        public AccountProfile Current
        {
            get
            {
                var user = auth?.CurrentUser;
                if (user == null || user.IsAnonymous || !user.ProviderData.Any(p => p.ProviderId == "google.com")) return null;
                return new AccountProfile { uid = user.UserId, displayName = user.DisplayName ?? "", email = user.Email ?? "", photoUrl = user.PhotoUrl?.ToString() ?? "" };
            }
        }
        public async Task InitializeAsync()
        {
            if (auth != null) return;
            if (!google.Supported) throw new NotSupportedException("Google login is available in the Android build.");
            var result = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (result != DependencyStatus.Available) throw new InvalidOperationException("Google services are unavailable on this device.");
            Firebase.Analytics.FirebaseAnalytics.SetAnalyticsCollectionEnabled(false);
            Firebase.Crashlytics.Crashlytics.IsCrashlyticsCollectionEnabled = false;
            auth = FirebaseAuth.DefaultInstance;
            auth.StateChanged += OnChanged;
        }
        void OnChanged(object sender, EventArgs args) => Changed?.Invoke();
        public async Task<AccountProfile> SignInAsync()
        {
            await InitializeAsync();
            string token = await google.SignInAsync();
            using (var credential = GoogleAuthProvider.GetCredential(token, null))
                await auth.SignInWithCredentialAsync(credential);
            return Current ?? throw new InvalidOperationException("Google authentication did not finish.");
        }
        public async Task SignOutAsync()
        {
            auth?.SignOut();
            await google.SignOutAsync();
        }
        public void Dispose() { if (auth != null) auth.StateChanged -= OnChanged; }
    }
}
