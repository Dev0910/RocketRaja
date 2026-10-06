using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.AppCheck;
using Firebase.Auth;
using RocketRaja.Core;
using UnityEngine.Networking;

namespace RocketRaja.Cloud
{
    /// <summary>Explicitly configured HTTPS connection. Construct only after Firebase initialization
    /// and App Check Play Integrity provider setup; construction never enables a store or service.</summary>
    public sealed class FirebasePremiumConnection : IPremiumHttpConnection
    {
        readonly Uri endpoint;
        readonly Func<PremiumIdentity> current;
        public bool Configured => endpoint != null;
        public FirebasePremiumConnection(string endpointUrl, Func<PremiumIdentity> current)
        {
            this.current = current ?? throw new ArgumentNullException(nameof(current));
            if (string.IsNullOrEmpty(endpointUrl)) return;
            if (!Uri.TryCreate(endpointUrl, UriKind.Absolute, out var value) || value.Scheme != "https" || value.IsLoopback ||
                value.UserInfo != "" || value.Query != "" || value.Fragment != "") throw new ArgumentException("Premium endpoint must be an explicit HTTPS service URL.", nameof(endpointUrl));
            endpoint = new Uri(value.AbsoluteUri.TrimEnd('/') + "/");
        }
        FirebaseUser RequireOwner(PremiumIdentity owner)
        {
            if (owner == null || !owner.Same(current())) throw new PremiumServiceException("ownership-lost");
            var user = FirebaseAuth.DefaultInstance.CurrentUser;
            if (user == null || user.IsAnonymous || user.UserId != owner.Uid || !user.ProviderData.Any(p => p.ProviderId == "google.com"))
                throw new PremiumServiceException("authentication-required");
            return user;
        }
        public async Task<PremiumHttpResponse> PostAsync(PremiumIdentity owner, string route, string json)
        {
            if (!Configured) throw new PremiumServiceException("unconfigured");
            if (route != "activate" && route != "wallet" && route != "fulfill" && route != "draw") throw new ArgumentException("Unknown premium route");
            byte[] body = Encoding.UTF8.GetBytes(json);
            if (body.Length > 16384) throw new PremiumServiceException("invalid-request");
            string idToken;
            AppCheckToken appToken;
            try
            {
                idToken = await RequireOwner(owner).TokenAsync(false);
                RequireOwner(owner);
                appToken = await FirebaseAppCheck.DefaultInstance.GetAppCheckTokenAsync(false);
            }
            catch (PremiumServiceException) { throw; }
            catch { throw new PremiumServiceException("authentication-required"); }
            RequireOwner(owner);
            if (string.IsNullOrEmpty(idToken) || string.IsNullOrEmpty(appToken.Token)) throw new PremiumServiceException("authentication-required");
            using (var request = new UnityWebRequest(new Uri(endpoint, route), "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(body);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", "Bearer " + idToken);
                request.SetRequestHeader("X-Firebase-AppCheck", appToken.Token);
                request.timeout = 20;
                request.redirectLimit = 0; // Never forward account tokens to a redirected origin.
                var completion = new TaskCompletionSource<bool>();
                var operation = request.SendWebRequest();
                operation.completed += _ => completion.TrySetResult(true);
                if (operation.isDone) completion.TrySetResult(true);
                await completion.Task;
                RequireOwner(owner);
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.DataProcessingError)
                    throw new PremiumServiceException("service-unavailable");
                if (request.downloadedBytes > 262144) throw new PremiumServiceException("invalid-response");
                return new PremiumHttpResponse { Status = (int)request.responseCode, Json = request.downloadHandler.text };
            }
        }
    }
}
