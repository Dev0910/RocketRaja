using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    public static partial class BuildRedesignUI
    {
        public static void AddFeedbackDismissal()
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling) throw new InvalidOperationException("Require compiled Edit Mode");
            LoadAssets(); var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var router = root.GetComponent<RedesignScreenRouter>(); var toast = (RectTransform)router.toastRoot.transform;
                toast.anchorMin = Vector2.zero; toast.anchorMax = Vector2.right; toast.offsetMin = new Vector2(14, 80) * S; toast.offsetMax = new Vector2(-14, 228) * S;
                toast.GetComponent<Image>().raycastTarget = true;
                var message = router.toast.rectTransform; message.anchorMin = Vector2.zero; message.anchorMax = Vector2.one;
                message.offsetMin = new Vector2(12, 12) * S; message.offsetMax = new Vector2(-82, -12) * S;
                router.toast.fontSize = 13 * S; router.toast.alignment = TextAlignmentOptions.MidlineLeft;
                if (!router.toastDismiss)
                {
                    var button = New("Dismiss notice", toast); button.anchorMin = button.anchorMax = new Vector2(1, .5f); button.pivot = new Vector2(1, .5f); button.anchoredPosition = new Vector2(-12 * S, 0); button.sizeDelta = new Vector2(58, 52) * S;
                    router.toastDismiss = Button(button, "Dismiss"); router.toastDismiss.GetComponentInChildren<TMP_Text>().fontSize = 11 * S;
                }
                toast.gameObject.SetActive(false); PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            Debug.Log("REDESIGN_FEEDBACK_SAVED");
        }
    }
}
