using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RocketRaja.Core;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RocketRaja.Editor
{
    public static class RedesignProductArtValidation
    {
        public static void Run()
        {
            if (EditorApplication.isPlaying || EditorUtility.scriptCompilationFailed) throw new InvalidOperationException("Require compiled Edit Mode");
            const string report = "Documentation/Implementation/Unity-Product-Art.txt";
            File.WriteAllText(report, "RUNNING: saved product art and isolated native rendering.\n");
            var checks = new List<string>(); void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); checks.Add("PASS " + label); }
            var art = AssetDatabase.LoadAssetAtPath<RedesignProductArt>(BuildRedesignUI.ProductArtPath);
            Check(art && art.appearances.Length == 32 && art.appearances.All(s => s && EditorUtility.IsPersistent(s)) && art.appearances.Distinct().Count() == 32, "All 32 distinct sprites reload as persistent Unity assets");
            for (int p = 0; p < 8; p++)
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Art/" + Catalog.ProductIds[p] + ".png");
                Check(Enumerable.Range(0, 4).All(t => art.At(p, t).texture == texture) && Enumerable.Range(0, 4).Select(t => art.At(p, t).rect).Distinct().Count() == 4, "Four unique reviewed regions share the original family texture " + p);
            }
            var scene = EditorSceneManager.NewPreviewScene(); GameObject root = null;
            try
            {
                root = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BuildRedesignUI.PrefabPath), scene);
                var router = root.GetComponent<RedesignScreenRouter>(); var home = router.home; var detail = router.productDetails;
                foreach (Transform page in home.transform.parent) page.gameObject.SetActive(false);
                Check(home.productArt == art && detail.productArt == art && detail.appearance, "Saved Home and product details reference the same reviewed art");
                Check(detail.appearance is RedesignSavedText saved && saved.savedMaterialSlots.Length == 4 && saved.savedMaterialSlots.All(s => s), "Appearance copy has four saved text material slots");
                foreach (var presenter in new MonoBehaviour[] { home, detail }) presenter.GetType().GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(presenter, null);
                var inventory = root.GetComponentsInChildren<Transform>(true); var data = RedesignRuntimeContent.Load();
                var anchors = home.products.Select(b => ((RectTransform)b.transform).anchoredPosition).ToArray();
                var counter = ((RectTransform)home.counter.transform).anchoredPosition;
                for (int tier = 0; tier < 4; tier++)
                {
                    var state = RedesignState.Fresh(); state.cash = Amount.Pow10(60); state.products = Enumerable.Repeat(25, 8).ToArray();
                    if (tier > 0) for (int p = 0; p < 8; p++)
                    {
                        string id = data.AppearanceUpgrade(p, tier); state.upgrades.Add(id); state.everPurchased.Add(id);
                    }
                    var e = new RedesignEngine(data, state, _ => { }); var session = new RedesignLifecycle(e);
                    var context = new RedesignUiContext(e, () => session.CanMutate, session.Run, (_, __) => { }, _ => { });
                    detail.gameObject.SetActive(false); home.Bind(context, session.TapGameplaySurface); home.Open(); detail.Bind(context);
                    router.title.text = "Rocket Raja"; router.summary.text = "Product display review · " + RedesignProductArt.Label(tier);
                    for (int p = 0; p < 8; p++) Check(home.productImages[p].sprite == art.At(p, tier) && home.productStatus[p].text == "Lv 25", "Home family/tier agrees with run ownership " + p + "/" + tier);
                    RedesignNativeUiValidation.Render(root, scene, 390, 844, true, "product-home-tier" + tier, () => home.scroll.verticalNormalizedPosition = 1);
                    if (tier == 3) RedesignNativeUiValidation.Render(root, scene, 360, 540, false, "product-home-tier3", () => home.scroll.verticalNormalizedPosition = 1);
                    home.gameObject.SetActive(false);
                    for (int p = 0; p < 8; p++)
                    {
                        detail.Open(p);
                        Check(detail.productImage.sprite == art.At(p, tier) && detail.appearance.text.Contains("Current display: " + RedesignProductArt.Label(tier)) && (tier == 3 || detail.appearance.text.Contains(data.AppearanceUpgrade(p, tier + 1))), "Details shows the matching look and exact next signature " + p + "/" + tier);
                        // Every family and tier has a phone-size native review, not just the largest source sheet.
                        RedesignNativeUiValidation.Render(root, scene, 390, 844, true, "product-detail-" + p + "-tier" + tier, () => detail.scroll.verticalNormalizedPosition = 1);
                    }
                    Check(home.products.Select(b => ((RectTransform)b.transform).anchoredPosition).SequenceEqual(anchors) && ((RectTransform)home.counter.transform).anchoredPosition == counter, "Appearance tier preserves all shelf and counter anchors " + tier);
                }
                RedesignNativeUiValidation.Render(root, scene, 360, 540, false, "product-detail-short-top", () => detail.scroll.verticalNormalizedPosition = 1);
                RedesignNativeUiValidation.Render(root, scene, 360, 540, false, "product-detail-short-actions", () => detail.scroll.verticalNormalizedPosition = 0);
                var fresh = new RedesignEngine(data, RedesignState.Fresh(), _ => { }); var freshSession = new RedesignLifecycle(fresh);
                var freshContext = new RedesignUiContext(fresh, () => freshSession.CanMutate, freshSession.Run, (_, __) => { }, _ => { });
                detail.gameObject.SetActive(false); home.Bind(freshContext, freshSession.TapGameplaySurface); home.Open();
                Check(home.productStatus[0].text == "Empty" && home.productImages[0].sprite == art.At(0, 0) && home.productImages[0].color.a < 1 && home.productImages.Skip(1).All(i => i.sprite == home.lockedSprite), "Fresh shared empty/locked states remain distinct from owned artwork");
                home.gameObject.SetActive(false); detail.Bind(freshContext); detail.Open(0);
                Check(detail.appearance.text.Contains("Preview") && detail.productImage.color.a < 1 && fresh.State.products[0] == 0, "Unowned detail labels its faded art as a preview without granting ownership");
                Check(new HashSet<Transform>(inventory).SetEquals(root.GetComponentsInChildren<Transform>(true)), "All 32 native states and shared states preserve every authored UI instance");
                Check(!EditorUtility.scriptCompilationFailed, "Unity compilation succeeds");
                File.WriteAllLines(report, new[] { "Scope: persistent asset reload, 32 native Home/details states, phone renders and stable saved geometry. Play purchase and core save/reset checks are separate reports." }.Concat(checks));
                Debug.Log("REDESIGN_PRODUCT_ART_OK " + checks.Count);
            }
            catch (Exception ex) { File.AppendAllText(report, string.Join("\n", checks) + "\nFAIL " + ex + "\n"); throw; }
            finally { if (root) UnityEngine.Object.DestroyImmediate(root); EditorSceneManager.ClosePreviewScene(scene); }
        }
    }
}
