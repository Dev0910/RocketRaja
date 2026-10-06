using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RocketRaja.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    public static class RedesignShopArtValidation
    {
        public static void Run()
        {
            if (EditorApplication.isPlaying || EditorUtility.scriptCompilationFailed) throw new InvalidOperationException("Require compiled Edit Mode");
            const string report = "Documentation/Implementation/Unity-Shop-Art.txt"; File.WriteAllText(report, "RUNNING\n");
            var checks = new List<string>(); void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); checks.Add("PASS " + label); }
            var scene = EditorSceneManager.NewPreviewScene(); GameObject root = null;
            try
            {
                root = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BuildRedesignUI.PrefabPath), scene);
                var r = root.GetComponent<RedesignScreenRouter>(); var home = r.home; var art = home.shopVisuals;
                foreach (Transform page in home.transform.parent) page.gameObject.SetActive(false);
                typeof(RedesignHomeScreen).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(home, null);
                var instances = root.GetComponentsInChildren<Transform>(true); var anchors = home.products.Select(b => ((RectTransform)b.transform).anchoredPosition).ToArray(); var counter = ((RectTransform)home.counter.transform).anchoredPosition;
                Check(art && art.layers.Length == 5 && art.layers.All(l => l) && art.progress, "Five cumulative layer roots and progress copy reload from the saved prefab");
                var images = art.layers.SelectMany(l => l.GetComponentsInChildren<Image>(true)).ToArray();
                Check(images.Length == 6 && images.All(i => !i.raycastTarget && i.sprite && EditorUtility.IsPersistent(i.sprite)) && images.Select(i => i.sprite).Distinct().Count() == 5, "Six noninteractive images reuse five persistent decorative sprites");
                Check(art.progress is RedesignSavedText text && text.savedMaterialSlots.Length == 4 && text.savedMaterialSlots.All(s => s), "Progress text material slots are serialized");
                var data = RedesignRuntimeContent.Load();
                for (int stage = 0; stage < 6; stage++)
                {
                    var s = RedesignState.Fresh(); s.products = Enumerable.Repeat(25, 8).ToArray(); s.everPurchased = data.Upgrades.Take(RedesignShopProgress.Threshold(stage)).Select(u => u.id).ToList();
                    var e = new RedesignEngine(data, s, _ => { }); var session = new RedesignLifecycle(e);
                    home.Bind(new RedesignUiContext(e, () => session.CanMutate, session.Run, (_, __) => { }, _ => { }), session.TapGameplaySurface); home.Open();
                    r.title.text = "Rocket Raja"; r.summary.text = s.UniqueCount + " lifetime unique purchases";
                    Check(art.DisplayedStage == stage && art.layers.Select((l, i) => l.activeSelf == (i < stage)).All(ok => ok), "Loading directly settles the exact cumulative shop stage " + stage);
                    Check(art.progress.text.Contains(RedesignShopVisuals.Names[stage]) && (stage == 5 ? art.progress.text.Contains("All shop decorations earned") : art.progress.text.Contains(RedesignShopProgress.Threshold(stage + 1).ToString())), "Stage and next milestone copy match lifetime progress " + stage);
                    Check(home.productImages.Select((i, p) => i.sprite == home.productArt.At(p, 0)).All(ok => ok), "Lifetime shop history never restores run-specific product art " + stage);
                    RedesignNativeUiValidation.Render(root, scene, 390, 844, true, "shop-stage-" + stage, () => home.scroll.verticalNormalizedPosition = 1);
                    RedesignNativeUiValidation.Render(root, scene, 360, 540, false, "shop-progress-" + stage, () => home.scroll.verticalNormalizedPosition = 0);
                    Check(home.products.Select(b => ((RectTransform)b.transform).anchoredPosition).SequenceEqual(anchors) && ((RectTransform)home.counter.transform).anchoredPosition == counter, "Decoration stage preserves all product and counter anchors " + stage);
                    home.Refresh(); Check(art.DisplayedStage == stage && s.decorationStage == 0, "Repeated binding neither saves nor queues older milestone celebrations " + stage);
                }
                Check(new HashSet<Transform>(instances).SetEquals(root.GetComponentsInChildren<Transform>(true)), "All six stage renders preserve the exact saved object inventory");
                Check(!EditorUtility.scriptCompilationFailed, "Unity compilation succeeds");
                File.WriteAllLines(report, new[] { "Scope: saved sprites/layers, six native phone stages and progress text. Actual batch/reload/reset/rebuy is a separate Play Mode report." }.Concat(checks)); Debug.Log("REDESIGN_SHOP_ART_OK " + checks.Count);
            }
            catch (Exception ex) { File.AppendAllText(report, string.Join("\n", checks) + "\nFAIL " + ex); throw; }
            finally { if (root) UnityEngine.Object.DestroyImmediate(root); EditorSceneManager.ClosePreviewScene(scene); }
        }
    }
}
