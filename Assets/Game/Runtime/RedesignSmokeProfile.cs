#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Linq;
using RocketRaja.Core;
using UnityEngine;

namespace RocketRaja
{
    /// <summary>Explicit development smoke requests create a new disposable profile, never edit a player save.</summary>
    public static class RedesignSmokeProfile
    {
        public static string Root { get; private set; }
        public static bool AndroidRequested
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                if (Application.identifier != "com.diwali.rocketrajaredesign.playtest") return false;
                using (var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unity.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var intent = activity.Call<AndroidJavaObject>("getIntent"))
                    return intent.Call<bool>("getBooleanExtra", "redesign_smoke", false);
#else
                return false;
#endif
            }
        }
        public static string Create()
        {
            if (!GameController.SmokeMode) throw new InvalidOperationException("Explicit smoke request required");
            Root = Path.Combine(Application.temporaryCachePath, "RedesignSmoke-" + Guid.NewGuid().ToString("N"));
            var state = RedesignState.Fresh(RedesignRuntimeDriver.Utc); state.cash = Amount.Pow10(100); state.products = Enumerable.Repeat(300, 8).ToArray();
            state.eligibleEarnings = state.target; state.reducedMotion = true; state.music = state.sound = false; state.storeName = "Smoke Fixture"; state.storeNamePromptHandled = true;
            state.learnedTopics.AddRange(RedesignGuidance.Topics);
            new RedesignSaveStore(Path.Combine(Root, "rocket-raja-v2.save"), RedesignRuntimeContent.Load(), JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>).Save(state);
            return Root;
        }
    }
}
#endif
