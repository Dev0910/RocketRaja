using System;
using System.Linq;
using RocketRaja.Classic;
using RocketRaja.Cloud;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RocketRaja.Editor
{
    /// <summary>Editor-only cleanup shared by authoring commands and player-build validation.</summary>
    public static class LocalOnlyPresentationAuthoring
    {
        public static readonly string[] RemovedRoutes = { "Account", "Welcome", "AccountCompare", "Handoff", "SignOut", "Promote", "RecoveryConfirm" };
        public const string Art = "Assets/Game/ClassicUI/Loading/festival-stall-r1.png";
        public static bool RemovedRoute(string route) => RemovedRoutes.Contains(route);

        public static void EnforceMain(Scene scene)
        {
            foreach (var ui in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<ClassicUI>(true)))
            {
                foreach (var view in ui.dialogs.Where(v => RemovedRoute(v.route)).ToArray())
                    if (view.root) Object.DestroyImmediate(view.root);
                ui.dialogs = ui.dialogs.Where(v => !RemovedRoute(v.route) && v.root).ToArray();
                foreach (var control in ui.GetComponentsInChildren<ClassicControl>(true).Where(c => c.action == "Account" || c.action == "Navigate" && RemovedRoute(c.parameter)).ToArray())
                    Object.DestroyImmediate(control.gameObject);
                ui.textBindings = ui.textBindings.Where(b => b.text).ToArray();
                ui.imageBindings = ui.imageBindings.Where(b => b.image).ToArray();
                var privacy = ui.dialogs.FirstOrDefault(v => v.route == "Privacy");
                if (privacy != null && privacy.scroll)
                {
                    SetCopy(privacy.scroll.content, "Your progress/Body", "Your game is saved on this device. Cloud saves and cross-device sync are currently unavailable.");
                    SetCopy(privacy.scroll.content, "Purchases & information/Body", "Store purchases are currently unavailable. Rewarded ads are optional and grant rewards only after successful completion.");
                }
                var exit = ui.dialogs.FirstOrDefault(v => v.route == "RecoveryExit");
                if (exit != null)
                {
                    foreach (var c in exit.root.GetComponentsInChildren<ClassicControl>(true))
                        if (c.action == "Navigate") { c.action = "RecoveryExit"; c.parameter = ""; c.GetComponentInChildren<TMP_Text>().text = "Keep files and leave"; }
                }
                var layout = ui.GetComponent<ClassicLayout>();
                if (layout) { layout.infoRows = layout.infoRows.Where(r => r.layout && r.copy).ToArray(); layout.touchGraphics = layout.touchGraphics.Where(g => g).ToArray(); }
                EditorUtility.SetDirty(ui);
            }
        }
        static void SetCopy(Transform root, string path, string copy) { var target = root.Find(path); if (target) target.GetComponent<TMP_Text>().text = copy; }

        [MenuItem("Rocket Raja/Local only/Apply reviewed loading and account removal")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Idle Edit Mode required");
            foreach (var path in new[] { ClassicProductionAuthoring.Main, ClassicProductionAuthoring.Bootstrap })
            {
                var scene = SceneManager.GetSceneByPath(path);
                if (!scene.isLoaded) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                if (scene.isDirty) throw new InvalidOperationException("Preserve unsaved scene edits before applying local startup: " + path);
                if (path == ClassicProductionAuthoring.Main) EnforceMain(scene); else AuthorLoading(scene);
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("LOCAL_ONLY_PRESENTATION_SAVED");
        }

        public static void AuthorLoading(Scene scene)
        {
            var owner = scene.GetRootGameObjects().Where(g => g.activeSelf).Select(g => g.GetComponent<BootstrapController>()).Single(c => c);
            var root = owner.transform;
            Undo.RegisterFullObjectHierarchyUndo(root.gameObject, "Author reviewed local loading");
            // Keep the replaced presentation as an inactive Editor reference; no legacy controller can execute.
            foreach (var snapshot in scene.GetRootGameObjects().Where(g => !g.activeSelf && g.GetComponent<BootstrapController>()))
                snapshot.SetActive(false);
            var existing = root.Find("Local Loading Presentation");
            if (existing) { RefineLoading(existing); owner.PreviewState("loading"); return; }
            var original = new GameObject("Previous Welcome Presentation", typeof(RectTransform)).transform;
            original.SetParent(root, false);
            foreach (var child in root.Cast<Transform>().Where(t => t != original).ToArray()) child.SetParent(original, true);
            original.gameObject.SetActive(false);
            var saved = new SerializedObject(owner);
            var oldPanel = saved.FindProperty("popupPanel").objectReferenceValue as GameObject;
            if (!oldPanel) oldPanel = saved.FindProperty("heading").objectReferenceValue is Component h ? h.transform.parent.gameObject : null;
            var layer = Rect("Local Loading Presentation", root);
            var background = Rect("Festival Illustration", layer);
            var textureImporter = (TextureImporter)AssetImporter.GetAtPath(Art);
            if (!textureImporter) throw new InvalidOperationException("Import approved loading illustration first");
            textureImporter.textureType = TextureImporterType.Sprite; textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.mipmapEnabled = false; textureImporter.maxTextureSize = 4096; textureImporter.textureCompression = TextureImporterCompression.CompressedHQ; textureImporter.SaveAndReimport();
            var picture = background.gameObject.AddComponent<Image>(); picture.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art); picture.raycastTarget = true;
            var aspect = background.gameObject.AddComponent<AspectRatioFitter>(); aspect.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent; aspect.aspectRatio = picture.sprite.rect.width / picture.sprite.rect.height;
            var safe = Rect("Loading Safe Area", layer); safe.gameObject.AddComponent<ImportedSafeArea>();
            var title = Text("Game Title", safe, "ROCKET RAJA", 120, "Assets/Game/Bootstrap/Fonts/Fraunces ExtraBold SDF.asset");
            Box(title.rectTransform, .02f, .76f, .98f, .91f); title.color = new Color32(255, 241, 201, 255);
            title.fontStyle = FontStyles.Bold; title.enableAutoSizing = true; title.fontSizeMin = 72; title.fontSizeMax = 120;
            var shadow = title.gameObject.AddComponent<Shadow>(); shadow.effectColor = new Color32(65, 23, 28, 220); shadow.effectDistance = new Vector2(0, -5);
            var status = Rect("Loading Status", safe); Box(status, .12f, .045f, .88f, .125f);
            var label = Text("Loading Label", status, "Getting your stall ready…", 42, "Assets/Game/ClassicUI/Fonts/Bold SDF.asset"); Box(label.rectTransform, 0, .48f, 1, 1);
            var track = Rect("Loading Track", status); Box(track, 0, 0, 1, .22f);
            var skin = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            var rim = track.gameObject.AddComponent<Image>(); rim.sprite = skin; rim.type = Image.Type.Sliced; rim.color = new Color32(217, 172, 87, 255); rim.raycastTarget = false;
            var inside = Rect("Track Interior", track); inside.offsetMin = new Vector2(4, 4); inside.offsetMax = new Vector2(-4, -4);
            var interior = inside.gameObject.AddComponent<Image>(); interior.sprite = skin; interior.type = Image.Type.Sliced; interior.color = new Color32(37, 24, 62, 255); interior.raycastTarget = false;
            var fillRect = Rect("Loading Fill", inside); fillRect.offsetMin = new Vector2(4, 3); fillRect.offsetMax = new Vector2(-4, -3);
            var fill = fillRect.gameObject.AddComponent<Image>(); BuildClassicUI.ConfigureProgressFill(fill); fill.color = new Color32(239, 189, 83, 255); fill.raycastTarget = false; fill.fillAmount = 0;
            var recovery = Rect("Local Recovery Safe Area", root); recovery.gameObject.AddComponent<ImportedSafeArea>();
            var dim = recovery.gameObject.AddComponent<Image>(); dim.color = new Color(0, 0, 0, .65f); dim.raycastTarget = true;
            if (!oldPanel) throw new InvalidOperationException("Preserve existing Bootstrap recovery panel");
            oldPanel.transform.SetParent(recovery, false);
            var panelRect = (RectTransform)oldPanel.transform;
            panelRect.anchorMin = new Vector2(.06f, .2f); panelRect.anchorMax = new Vector2(.94f, .8f); panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;
            var fitter = oldPanel.GetComponent<ContentSizeFitter>(); if (fitter) fitter.enabled = false;
            saved.FindProperty("loadingPresentation").objectReferenceValue = layer.gameObject;
            saved.FindProperty("loadingFill").objectReferenceValue = fill;
            saved.FindProperty("progress").objectReferenceValue = label;
            saved.FindProperty("loadingGroup").objectReferenceValue = null;
            saved.FindProperty("popupPanel").objectReferenceValue = oldPanel;
            saved.FindProperty("popupBackdrop").objectReferenceValue = recovery.gameObject;
            saved.FindProperty("popupOpener").objectReferenceValue = null;
            saved.ApplyModifiedPropertiesWithoutUndo();
            foreach (var marker in oldPanel.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Google Mark").ToArray()) Object.DestroyImmediate(marker.gameObject);
            foreach (var t in original.GetComponentsInChildren<TMP_Text>(true))
                if (t.text.IndexOf("Google", StringComparison.OrdinalIgnoreCase) >= 0 || t.text.IndexOf("guest", StringComparison.OrdinalIgnoreCase) >= 0) t.text = "";
            RefineLoading(layer);
            owner.PreviewState("loading");
            recovery.gameObject.SetActive(false);
            EditorUtility.SetDirty(owner);
        }
        static RectTransform Rect(string name, Transform parent)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent, false);
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; return r;
        }
        static void Box(RectTransform r, float x0, float y0, float x1, float y1) { r.anchorMin = new Vector2(x0, y0); r.anchorMax = new Vector2(x1, y1); r.offsetMin = r.offsetMax = Vector2.zero; }
        static TMP_Text Text(string name, Transform parent, string copy, float size, string font)
        {
            var text = Rect(name, parent).gameObject.AddComponent<RedesignSavedText>(); text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(font);
            text.text = copy; text.fontSize = size; text.color = new Color32(255, 241, 201, 255); text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap; text.raycastTarget = false; AuthorTextSlots(text); return text;
        }
        static void RefineLoading(Transform layer)
        {
            foreach (var text in layer.GetComponentsInChildren<RedesignSavedText>(true)) AuthorTextSlots(text);
            var title = layer.Find("Loading Safe Area/Game Title").GetComponent<TMP_Text>();
            Box(title.rectTransform, .07f, .79f, .93f, .94f); title.fontStyle = FontStyles.Normal; title.fontSize = title.fontSizeMax = 112;
            var track = layer.Find("Loading Safe Area/Loading Status/Loading Track");
            Box((RectTransform)track, 0, 0, 1, .24f);
            RefineLoadingBar(track);
        }

        // Also used by the scoped repair, without rebuilding any other startup presentation.
        public static void RefineLoadingBar(Transform track)
        {
            var inside = track.Find("Track Interior");
            var bounds = inside.Find("Fill Bounds");
            if (!bounds)
            {
                bounds = Rect("Fill Bounds", inside);
                var area = (RectTransform)bounds;
                area.offsetMin = new Vector2(4, 3); area.offsetMax = new Vector2(-4, -3);
                inside.Find("Loading Fill").SetParent(bounds, false);
            }
            var fill = (RectTransform)bounds.Find("Loading Fill");
            // Progress scales the padded interior, including near-zero progress.
            fill.offsetMin = fill.offsetMax = Vector2.zero;
            var rounded = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Bootstrap/Art/Rounded Surface.png");
            Canvas.ForceUpdateCanvases();
            foreach (var image in track.GetComponentsInChildren<Image>(true))
            {
                image.sprite = rounded; image.type = Image.Type.Sliced;
                var corners = image.GetComponent<LoadingBarCorners>();
                if (!corners) corners = image.gameObject.AddComponent<LoadingBarCorners>();
                corners.Refresh();
            }
        }
        static void AuthorTextSlots(RedesignSavedText text)
        {
            if (text.savedMaterialSlots.Length == 4 && text.savedMaterialSlots.All(s => s)) { text.BindSavedSlots(); return; }
            text.savedMaterialSlots = new TMP_SubMeshUI[4];
            for (int i = 0; i < 4; i++)
            {
                var r = Rect("Saved material " + (i + 1), text.transform);
                r.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
                var slot = r.gameObject.AddComponent<RedesignSavedSubMesh>();
                var so = new SerializedObject(slot); so.FindProperty("m_TextComponent").objectReferenceValue = text; so.FindProperty("m_materialReferenceIndex").intValue = i + 1; so.ApplyModifiedPropertiesWithoutUndo();
                slot.raycastTarget = false; slot.fontAsset = text.font; slot.sharedMaterial = text.fontSharedMaterial; slot.isDefaultMaterial = true;
                slot.hideFlags = HideFlags.None; slot.gameObject.hideFlags = HideFlags.None; text.savedMaterialSlots[i] = slot;
            }
            text.BindSavedSlots(); EditorUtility.SetDirty(text);
        }
    }

    public sealed class LocalOnlyPlayerSceneGuard : IProcessSceneWithReport
    {
        public int callbackOrder => 200;
        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (report == null) return;
            if (scene.path == ClassicProductionAuthoring.Main) LocalOnlyPresentationAuthoring.EnforceMain(scene);
            // Historical disabled canvases are Editor recovery references, not shipped UI.
            foreach (var root in scene.GetRootGameObjects().Where(g => !g.activeSelf && g.name.Contains("LegacyDisabled")).ToArray()) Object.DestroyImmediate(root);
            if (scene.path == ClassicProductionAuthoring.Bootstrap)
            {
                var owner = scene.GetRootGameObjects().Select(g => g.GetComponent<BootstrapController>()).SingleOrDefault(c => c);
                if (!owner || !owner.transform.Find("Local Loading Presentation")) throw new BuildFailedException("Author the reviewed local loading screen before building.");
                var so = new SerializedObject(owner);
                if (!so.FindProperty("loadingFill").objectReferenceValue) throw new BuildFailedException("Local loading progress binding is missing.");
                // Keep inactive referenced recovery controls, but remove obsolete welcome artwork/UI.
                var old = owner.transform.Find("Previous Welcome Presentation");
                if (old) old.gameObject.SetActive(false);
            }
        }
    }
}
