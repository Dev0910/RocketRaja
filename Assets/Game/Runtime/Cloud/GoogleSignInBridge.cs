using System;
using System.Threading.Tasks;
using RocketRaja.Core;
using UnityEngine;
using UnityEngine.Scripting;

namespace RocketRaja.Cloud
{
    [Preserve] public sealed class GoogleSignInBridge : MonoBehaviour, IGoogleSignInProvider
    {
        [Serializable] sealed class Result { public string requestId, token, error; }
        TaskCompletionSource<string> pending;
        string requestId;
        public bool Supported
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }
        public Task<string> SignInAsync() => InvokeNative("signIn");
        public async Task SignOutAsync() { if (Supported) await InvokeNative("signOut"); }
        async Task<string> InvokeNative(string method)
        {
            if (!Supported) throw new NotSupportedException("Google login is available in the Android build.");
            if (pending != null) throw new InvalidOperationException("Google login is already in progress.");
            requestId = Guid.NewGuid().ToString("N");
            var completion = pending = new TaskCompletionSource<string>();
            try
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                using (var bridge = new AndroidJavaClass("com.rocketrajagame.identity.GoogleSignInBridge"))
                    bridge.CallStatic(method, gameObject.name, requestId);
#endif
                if (await Task.WhenAny(completion.Task, Task.Delay(120000)) != completion.Task)
                    throw new TimeoutException("Google sign-in timed out. Please try again.");
                return await completion.Task;
            }
            finally
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                using (var bridge = new AndroidJavaClass("com.rocketrajagame.identity.GoogleSignInBridge")) bridge.CallStatic("cancel");
#endif
                pending = null; requestId = null;
            }
        }
        [Preserve] public void OnGoogleResult(string json)
        {
            if (pending == null) return;
            Result result;
            try { result = JsonUtility.FromJson<Result>(json); }
            catch { pending.TrySetException(new InvalidOperationException("Google returned an invalid response.")); return; }
            if (result == null || result.requestId != requestId) return;
            if (result.error == "cancelled") pending.TrySetCanceled();
            else if (!string.IsNullOrEmpty(result.error)) pending.TrySetException(new InvalidOperationException(result.error));
            else pending.TrySetResult(result.token);
        }
        void OnDestroy() { pending?.TrySetCanceled(); }
    }
}
