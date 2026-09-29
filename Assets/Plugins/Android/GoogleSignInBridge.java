package com.rocketrajagame.identity;

import android.app.Activity;
import android.os.CancellationSignal;
import androidx.annotation.Keep;
import androidx.credentials.CredentialManager;
import androidx.credentials.CredentialManagerCallback;
import androidx.credentials.ClearCredentialStateRequest;
import androidx.credentials.CustomCredential;
import androidx.credentials.GetCredentialRequest;
import androidx.credentials.GetCredentialResponse;
import androidx.credentials.exceptions.GetCredentialException;
import androidx.credentials.exceptions.GetCredentialCancellationException;
import androidx.credentials.exceptions.ClearCredentialException;
import com.google.android.libraries.identity.googleid.GetSignInWithGoogleOption;
import com.google.android.libraries.identity.googleid.GoogleIdTokenCredential;
import com.unity3d.player.UnityPlayer;
import org.json.JSONObject;

/** Native account chooser only. Firebase verifies the token; it is never logged or persisted here. */
@Keep public final class GoogleSignInBridge {
    private static CancellationSignal cancellation;
    public static void cancel() { if (cancellation != null) { cancellation.cancel(); cancellation = null; } }
    private static void reply(String target, String id, String token, String error) {
        try {
            JSONObject result = new JSONObject(); result.put("requestId", id);
            result.put("token", token == null ? "" : token); result.put("error", error == null ? "" : error);
            UnityPlayer.UnitySendMessage(target, "OnGoogleResult", result.toString());
        } catch (Exception ignored) { }
    }
    public static void signIn(String target, String id) {
        final Activity activity = UnityPlayer.currentActivity;
        activity.runOnUiThread(() -> {
            cancel(); cancellation = new CancellationSignal();
            int resource = activity.getResources().getIdentifier("default_web_client_id", "string", activity.getPackageName());
            if (resource == 0 || activity.getString(resource).isEmpty()) {
                reply(target, id, null, "Google login is not configured. Add the OAuth web client to the Firebase Android configuration."); return;
            }
            try {
                GetSignInWithGoogleOption google = new GetSignInWithGoogleOption.Builder(activity.getString(resource)).build();
                GetCredentialRequest request = new GetCredentialRequest.Builder().addCredentialOption(google).build();
                CredentialManager.create(activity).getCredentialAsync(activity, request, cancellation, activity::runOnUiThread,
                    new CredentialManagerCallback<GetCredentialResponse, GetCredentialException>() {
                        @Override public void onResult(GetCredentialResponse result) {
                            try {
                                if (!(result.getCredential() instanceof CustomCredential) ||
                                    !GoogleIdTokenCredential.TYPE_GOOGLE_ID_TOKEN_CREDENTIAL.equals(result.getCredential().getType()))
                                    throw new IllegalArgumentException();
                                String token = GoogleIdTokenCredential.createFrom(result.getCredential().getData()).getIdToken();
                                reply(target, id, token, null);
                            } catch (Exception e) { reply(target, id, null, "Google returned an unsupported credential."); }
                        }
                        @Override public void onError(GetCredentialException e) {
                            reply(target, id, null, e instanceof GetCredentialCancellationException ? "cancelled" : "Google sign-in failed. Check your connection and try again.");
                        }
                    });
            } catch (Exception e) { reply(target, id, null, "Google sign-in is unavailable on this device."); }
        });
    }
    public static void signOut(String target, String id) {
        final Activity activity = UnityPlayer.currentActivity;
        activity.runOnUiThread(() -> {
            cancel(); cancellation = new CancellationSignal();
            CredentialManager.create(activity).clearCredentialStateAsync(new ClearCredentialStateRequest(), cancellation, activity::runOnUiThread,
                new CredentialManagerCallback<Void, ClearCredentialException>() {
                    @Override public void onResult(Void result) { reply(target, id, "", null); }
                    @Override public void onError(ClearCredentialException e) { reply(target, id, null, "Could not clear the Google account chooser. Try again."); }
                });
        });
    }
}
