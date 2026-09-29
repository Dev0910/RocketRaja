using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RocketRaja.EditorTools
{
    public static class AuthorHtmlMotion
    {
        static T Ensure<T>(GameObject obj) where T : Component => obj.TryGetComponent<T>(out var value) ? value : obj.AddComponent<T>();
        static RectTransform Host(RectTransform target)
        {
            if (target.parent.name == target.name + " Motion Host") return (RectTransform)target.parent;
            var parent = target.parent; int index = target.GetSiblingIndex();
            var host = new GameObject(target.name + " Motion Host", typeof(RectTransform)).GetComponent<RectTransform>();
            host.SetParent(parent, false); host.SetSiblingIndex(index);
            host.anchorMin = Vector2.zero; host.anchorMax = Vector2.one;
            host.offsetMin = host.offsetMax = Vector2.zero;
            target.SetParent(host, false);
            return host;
        }
        static void Motion(GameObject root, UiMotion.Kind kind, RectTransform target, bool wrapper)
        {
            var group = Ensure<CanvasGroup>(root);
            group.alpha = 1; group.interactable = true; group.blocksRaycasts = kind != UiMotion.Kind.Notice;
            var motion = Ensure<UiMotion>(root);
            var data = new SerializedObject(motion);
            data.FindProperty("kind").enumValueIndex = (int)kind;
            data.FindProperty("group").objectReferenceValue = group;
            data.FindProperty("surface").objectReferenceValue = wrapper ? Host(target) : target;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
        static GameObject Field(Component c, string name) => new SerializedObject(c).FindProperty(name).objectReferenceValue as GameObject;
        static RectTransform MessageHost(Transform row)
        {
            var saved = row.Find("Message Motion Host") as RectTransform;
            if (saved != null) return saved;
            var children = row.Cast<Transform>().ToArray();
            var host = new GameObject("Message Motion Host", typeof(RectTransform)).GetComponent<RectTransform>();
            host.SetParent(row, false); host.anchorMin = Vector2.zero; host.anchorMax = Vector2.one;
            host.offsetMin = host.offsetMax = Vector2.zero;
            foreach (var child in children) child.SetParent(host, false);
            return host;
        }
        static void Feedback(GameObject root)
        {
            foreach (var graphic in root.GetComponentsInChildren<Graphic>(true))
            {
                var f = Ensure<UiFeedback>(graphic.gameObject);
                var data = new SerializedObject(f);
                data.FindProperty("kind").enumValueIndex = (int)UiFeedback.Kind.Accent;
                data.FindProperty("target").objectReferenceValue = graphic.rectTransform;
                data.ApplyModifiedPropertiesWithoutUndo();
                f.enabled = false;
            }
            foreach (var control in root.GetComponentsInChildren<Button>(true))
            {
                var f = Ensure<UiFeedback>(control.gameObject); f.enabled = true;
                bool skill = control.name.StartsWith("home-skill-"); bool nav = control.name.StartsWith("nav-");
                var data = new SerializedObject(f);
                data.FindProperty("kind").enumValueIndex = (int)(skill ? UiFeedback.Kind.Skill : nav ? UiFeedback.Kind.Navigation : UiFeedback.Kind.Button);
                data.FindProperty("target").objectReferenceValue = skill ? control.transform.Find("Icon") : control.transform;
                data.FindProperty("navigationIcon").objectReferenceValue = nav ? control.transform.Find("Icon") : null;
                data.FindProperty("face").objectReferenceValue = skill ? null : control.targetGraphic;
                var image = control.targetGraphic as Image;
                data.FindProperty("disabledFace").objectReferenceValue = image != null && image.sprite != null && image.sprite.name.StartsWith("Folk ")
                    ? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/ImportedUI/Art/Folk Sand.png") : null;
                data.ApplyModifiedPropertiesWithoutUndo();
                var colors = control.colors; colors.fadeDuration = .16f; control.colors = colors;
                // Skill icon press and activation share one transform channel.
                if (skill)
                {
                    var duplicate = control.transform.Find("Icon").GetComponent<UiFeedback>();
                    if (duplicate != null) UnityEngine.Object.DestroyImmediate(duplicate);
                }
            }
            var player = root.GetComponent<ImportedPlayerScreen>();
            if (player != null)
            {
                var rows = new SerializedObject(player).FindProperty("upgradeRows");
                for (int i = 0; i < rows.arraySize; i++)
                {
                    var row = rows.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
                    var f = Ensure<UiFeedback>(row); var data = new SerializedObject(f);
                    data.FindProperty("kind").enumValueIndex = (int)UiFeedback.Kind.Accent;
                    data.FindProperty("target").objectReferenceValue = row.transform;
                    data.ApplyModifiedPropertiesWithoutUndo(); f.enabled = false;
                }
            }
        }
        static RectTransform Child(Transform parent, string name)
        {
            var rect = parent.Find(name) as RectTransform;
            if (rect != null) return rect;
            rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero; return rect;
        }
        static UiProgress Progress(RectTransform fill)
        {
            var progress = Ensure<UiProgress>(fill.gameObject); var data = new SerializedObject(progress);
            data.FindProperty("fill").objectReferenceValue = fill; data.ApplyModifiedPropertiesWithoutUndo(); return progress;
        }
        public static void Meters(GameObject root)
        {
            var product = root.GetComponent<ImportedProductsScreen>();
            if (product != null)
            {
                var data = new SerializedObject(product); var images = data.FindProperty("productImages");
                var meters = data.FindProperty("displayProgress"); meters.arraySize = 8;
                for (int i = 0; i < 8; i++)
                {
                    var art = (Image)images.GetArrayElementAtIndex(i).objectReferenceValue;
                    art.rectTransform.offsetMin = new Vector2(8, 30);
                    var track = Child(art.transform.parent, "Display Progress Track");
                    track.anchorMin = track.anchorMax = Vector2.zero; track.anchorMax = Vector2.right;
                    track.pivot = new Vector2(.5f, 0); track.offsetMin = new Vector2(8, 8); track.offsetMax = new Vector2(-8, 24);
                    var background = Ensure<Image>(track.gameObject); background.color = new Color32(88, 119, 132, 255); background.raycastTarget = false;
                    var inner = Child(track, "Track"); inner.offsetMin = Vector2.one * 2; inner.offsetMax = Vector2.one * -2;
                    var empty = Ensure<Image>(inner.gameObject); empty.color = new Color32(208, 196, 167, 255); empty.raycastTarget = false;
                    var fill = Child(inner, "Fill"); fill.anchorMax = new Vector2(.52f, 1);
                    var graphic = Ensure<Image>(fill.gameObject); graphic.color = new Color32(35, 101, 160, 255); graphic.raycastTarget = false;
                    meters.GetArrayElementAtIndex(i).objectReferenceValue = Progress(fill);
                }
                data.ApplyModifiedPropertiesWithoutUndo();
            }
            var detail = root.GetComponent<ImportedProductDetailScreen>();
            if (detail != null)
            {
                var data = new SerializedObject(detail);
                data.FindProperty("progressMotion").objectReferenceValue = Progress((RectTransform)data.FindProperty("progressFill").objectReferenceValue);
                data.ApplyModifiedPropertiesWithoutUndo();
            }
            var home = root.GetComponent<ImportedHomeScreen>();
            if (home != null)
            {
                var data = new SerializedObject(home); var images = data.FindProperty("skillImages");
                var covers = data.FindProperty("skillCooldowns"); covers.arraySize = 3;
                var rings = data.FindProperty("skillRings"); rings.arraySize = 3;
                for (int i = 0; i < 3; i++)
                {
                    var icon = (Image)images.GetArrayElementAtIndex(i).objectReferenceValue;
                    var coverObject = Child(icon.transform, "Cooldown Sweep").gameObject;
                    Ensure<CanvasRenderer>(coverObject);
                    var cover = Ensure<UiCooldown>(coverObject);
                    cover.color = new Color32(65, 68, 71, 199); cover.raycastTarget = false;
                    covers.GetArrayElementAtIndex(i).objectReferenceValue = cover;
                    var ringObject = Child(icon.transform, "Skill State Ring").gameObject; Ensure<CanvasRenderer>(ringObject);
                    var ring = Ensure<UiSkillRing>(ringObject); ring.raycastTarget = false; ring.color = new Color32(153, 138, 108, 255);
                    rings.GetArrayElementAtIndex(i).objectReferenceValue = ring;
                }
                data.ApplyModifiedPropertiesWithoutUndo();
            }
        }
        static void TapLabels(GameObject root)
        {
            var home = root.GetComponent<ImportedHomeScreen>(); if (home == null) return;
            var data = new SerializedObject(home); var rects = data.FindProperty("tapPopupRects"); var texts = data.FindProperty("tapPopupLabels");
            for (int i = 0; i < rects.arraySize; i++)
            {
                var rect = (RectTransform)rects.GetArrayElementAtIndex(i).objectReferenceValue;
                rect.anchorMin = rect.anchorMax = new Vector2(.48f, .35f); rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = Vector2.zero; rect.sizeDelta = new Vector2(440, 100);
                var text = (TMP_Text)texts.GetArrayElementAtIndex(i).objectReferenceValue;
                text.fontSize = 72; text.alignment = TextAlignmentOptions.TopLeft;
                text.color = new Color32(22, 75, 128, 255); text.raycastTarget = false;
            }
        }
        static void Celebration(GameObject root)
        {
            var home = root.GetComponent<ImportedHomeScreen>(); if (home == null) return;
            var data = new SerializedObject(home);
            var world = ((Component)data.FindProperty("stallTap").objectReferenceValue).transform;
            var host = Child(world, "Festival Sparkles"); Ensure<CanvasRenderer>(host.gameObject);
            var sparks = Ensure<FestivalSparkles>(host.gameObject); sparks.raycastTarget = false;
            data.FindProperty("sparkles").objectReferenceValue = sparks; data.ApplyModifiedPropertiesWithoutUndo();
        }
        static void Ambient(GameObject root)
        {
            var home = root.GetComponent<ImportedHomeScreen>(); if (home == null) return;
            var data = new SerializedObject(home);
            var world = ((Component)data.FindProperty("stallTap").objectReferenceValue).transform;
            var keeper = (RectTransform)world.Find("Shopkeeper");
            // This illustration stretches between its two saved anchors, with zero sizeDelta.
            // Keep the zero offsets from the protected prefab; world-space compensation is
            // invalid while a loaded prefab's Canvas/layout has not been evaluated.
            keeper.pivot = new Vector2(.5f, 0);
            keeper.anchoredPosition = Vector2.zero;
            var clip = Child(world, "Visitors Clip"); clip.anchorMax = new Vector2(1, .09f); clip.offsetMin = clip.offsetMax = Vector2.zero;
            Ensure<RectMask2D>(clip.gameObject);
            var host = Child(clip, "Passing Visitors"); Ensure<CanvasRenderer>(host.gameObject);
            var ambient = Ensure<ShopAmbient>(host.gameObject); ambient.raycastTarget = false; ambient.color = new Color32(20, 52, 80, 179);
            var motion = new SerializedObject(ambient); motion.FindProperty("keeper").objectReferenceValue = keeper; motion.ApplyModifiedPropertiesWithoutUndo();
            data.FindProperty("ambient").objectReferenceValue = ambient; data.ApplyModifiedPropertiesWithoutUndo();
            // Feedback is above ambient silhouettes; both remain inside the authored world.
            world.Find("Festival Sparkles").SetAsLastSibling();
        }

        [MenuItem("Tools/Rocket Raja/Author HTML Motion")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Author outside Play Mode.");
            foreach (var path in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Game/ImportedUI" })
                .Select(AssetDatabase.GUIDToAssetPath).Where(p => p.EndsWith(" Screen.prefab")))
            {
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var dialog = root.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(x => x.name.EndsWith("Dialog"));
                    var fit = root.GetComponent<ImportedPanelFit>();
                    var home = root.GetComponent<ImportedHomeScreen>();
                    if (dialog != null) Motion(root, UiMotion.Kind.Modal, dialog, true);
                    else if (fit != null)
                    {
                        var owner = home != null ? fit.MotionTarget.gameObject : root;
                        Motion(owner, UiMotion.Kind.Panel, fit.MotionTarget, true);
                    }
                    foreach (var presenter in root.GetComponents<MonoBehaviour>())
                    {
                        var data = new SerializedObject(presenter);
                        var menu = data.FindProperty("quantityMenu")?.objectReferenceValue as GameObject;
                        if (menu != null) Motion(menu, UiMotion.Kind.Quantity, (RectTransform)menu.transform, false);
                        foreach (string field in new[] { "messageRow", "failureRow" })
                        {
                            var row = data.FindProperty(field)?.objectReferenceValue as GameObject;
                            if (row != null) Motion(row, UiMotion.Kind.Notice, MessageHost(row.transform), false);
                        }
                    }
                    if (home != null)
                    {
                        var notice = Field(home, "noticeRoot");
                        Motion(notice, UiMotion.Kind.Notice, (RectTransform)notice.transform, false);
                    }
                    Meters(root);
                    TapLabels(root);
                    Celebration(root);
                    Ambient(root);
                    Feedback(root);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            Debug.Log("Saved HTML motion hosts and references in all 13 screen prefabs.");
        }
    }
}
