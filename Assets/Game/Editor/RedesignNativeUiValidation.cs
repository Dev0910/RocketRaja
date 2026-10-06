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
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RocketRaja.Editor
{
    /// <summary>Saved-asset/Edit Mode checks; does not claim Play Mode or device acceptance.</summary>
    public static class RedesignNativeUiValidation
    {
        [MenuItem("Rocket Raja/Redesign/Validate Native UI Asset")]
        public static void Run()
        {
            if (EditorApplication.isPlaying || EditorUtility.scriptCompilationFailed) throw new InvalidOperationException("Require compiled Edit Mode");
            var checks = new List<string>();
            File.WriteAllText("Documentation/Implementation/Unity-Native-UI.txt", "Validation in progress; no current pass result.\n");
            void Check(bool value, string name) { if (!value) throw new InvalidOperationException(name); checks.Add("PASS " + name); }
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(BuildRedesignUI.PrefabPath);
            Check(asset != null, "Native prefab exists on disk");
            var preview = EditorSceneManager.NewPreviewScene();
            GameObject root = null;
            try
            {
                root = (GameObject)PrefabUtility.InstantiatePrefab(asset, preview);
                var ui = root.GetComponentInChildren<RedesignUpgradesScreen>(true);
                Check(ui != null && ui.rows.Length == 8 && ui.rows.All(r => r && r.title && r.effect && r.requirement && r.price && r.buy && r.details), "Eight serialized row instances and references reload");
                Check(root.GetComponentsInChildren<Button>(true).All(b => b.navigation.mode == Navigation.Mode.None && b.targetGraphic), "Saved Buttons have disabled keyboard auto-navigation and target graphics");
                Check(ui.scroll.content == ui.content && ui.scroll.viewport && !ui.scroll.horizontal && ui.scroll.vertical, "ScrollRect owns serialized content and viewport");
                Check(root.GetComponentsInChildren<TMP_Text>(true).All(t => t.font && EditorUtility.IsPersistent(t.font)), "All TMP text uses persistent font assets");
                var state = RedesignState.Fresh(); state.cash = 2000; state.products[0] = 1;
                var engine = new RedesignEngine(RedesignRuntimeContent.Load(), state, _ => { });
                string inspected = null; CashBatchQuote reviewed = null;
                // Invoke lifecycle registration explicitly for isolated Edit Mode binding checks.
                // Full Play Mode dispatch/raycast/listener lifecycle is a separate acceptance gate.
                foreach (var row in ui.rows) InvokeAwake(row);
                InvokeAwake(ui);
                var instances = root.GetComponentsInChildren<Transform>(true);
                ui.Bind(engine, () => true, engine.BuyUpgrade, id => inspected = id, q => reviewed = q, _ => { });
                ui.SetView(UpgradeView.Full); Check(ui.Browser.Count == 1000, "Native pool binds full 1,000-entry catalogue");
                ui.rows[0].details.onClick.Invoke(); Check(inspected == ui.rows[0].ItemId, "Details dispatches rebound stable ID");
                ui.search.text = "PP-PHU-01"; Check(ui.Browser.Count == 1 && ui.rows[0].ItemId == "PP-PHU-01", "Native search rebinds the matching stable ID");
                ui.rows[0].buy.onClick.Invoke(); Check(engine.State.upgrades.Contains("PP-PHU-01"), "Native row purchase calls actual core command");
                ui.search.text = ""; ui.SetCategory(UpgradeCategory.Products); ui.reviewButton.onClick.Invoke();
                Check(reviewed != null && reviewed.Ids.All(id => engine.Data.TryUpgrade(id, out int index) && UpgradeScope.MatchesCategory(engine.Data.Upgrades[index], UpgradeCategory.Products)), "Native Buy All dispatches frozen category quote");
                ui.SetCategory(UpgradeCategory.All); ui.SetView(UpgradeView.Full);
                ui.content.anchoredPosition = new Vector2(0, ui.Browser.Count * ui.rowHeight); ui.Refresh();
                Check(ui.rows.Any(r => r.gameObject.activeSelf && r.ItemId == ui.Browser[999].id), "Last authored choice reachable through saved pool");
                ui.search.text = "no matching item"; Check(ui.emptyLabel.gameObject.activeSelf && ui.rows.All(r => !r.gameObject.activeSelf), "Empty search hides stale pool rows");
                ui.search.text = ""; ui.SetView(UpgradeView.Available);
                Check(instances.SequenceEqual(root.GetComponentsInChildren<Transform>(true)), "Filtering/search/purchase/scroll preserve all serialized UI identities");
                ui.Bind(engine, () => false, engine.BuyUpgrade, null, null, null);
                Check(ui.rows.Where(r => r.gameObject.activeSelf).All(r => !r.buy.interactable) && !ui.reviewButton.interactable, "Blocked session disables native cash mutations");
                ui.Bind(engine, () => true, engine.BuyUpgrade, null, null, null);
                root.GetComponentsInChildren<TMP_Text>(true).Single(t => t.name == "header-summary").text = "₹" + engine.State.cash + " cash · Normal sales ₹" + engine.NormalIncome() + "/s";
                foreach (var size in new[] { new Vector2Int(390, 844), new Vector2Int(360, 640), new Vector2Int(360, 540), new Vector2Int(450, 916) })
                {
                    Render(root, preview, size.x, size.y, false);
                    Check(ui.scroll.viewport.rect.height > 0, "Positive scroll viewport at " + size);
                }
                Render(root, preview, 390, 844, true); Check(ui.scroll.viewport.rect.height > 0, "Notched safe-area inset retains usable catalogue");
                Check(!EditorUtility.scriptCompilationFailed, "Unity reports no script compilation failure");
            }
            finally { if (root) Object.DestroyImmediate(root); EditorSceneManager.ClosePreviewScene(preview); }
            File.WriteAllLines("Documentation/Implementation/Unity-Native-UI.txt", new[] { "Scope: saved asset and isolated Edit Mode binding/rendering; Play Mode/Main/device acceptance remains open." }.Concat(checks));
            Debug.Log("REDESIGN_NATIVE_UI_ASSET_OK " + checks.Count);
        }
        static void InvokeAwake(MonoBehaviour target) => target.GetType().GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(target, null);
        internal static void Render(GameObject root, UnityEngine.SceneManagement.Scene scene, int width, int height, bool notch, string prefix = "upgrades", Action afterLayout = null)
        {
            string folder = "Documentation/Implementation/NativeUI"; Directory.CreateDirectory(folder);
            float worldHeight = 1080f * height / width;
            var canvas = root.GetComponent<Canvas>(); root.GetComponent<CanvasScaler>().enabled = false;
            canvas.renderMode = RenderMode.WorldSpace;
            var rect = (RectTransform)root.transform; rect.sizeDelta = new Vector2(1080, worldHeight); rect.localScale = Vector3.one; rect.position = Vector3.zero;
            var safe = (RectTransform)root.transform.Find("Safe Area"); safe.GetComponent<ImportedSafeArea>().enabled = false;
            safe.anchorMin = new Vector2(0, notch ? 24f / height : 0); safe.anchorMax = new Vector2(1, notch ? 1 - 40f / height : 1); safe.offsetMin = safe.offsetMax = Vector2.zero;
            foreach (var utilitySafe in root.GetComponentsInChildren<ImportedSafeArea>(true))
            {
                utilitySafe.enabled = false; var area = (RectTransform)utilitySafe.transform;
                area.anchorMin = safe.anchorMin; area.anchorMax = safe.anchorMax; area.offsetMin = area.offsetMax = Vector2.zero;
            }
            var cameraGo = new GameObject("Native QA Camera", typeof(Camera)); UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraGo, scene);
            var camera = cameraGo.GetComponent<Camera>(); camera.scene = scene; camera.orthographic = true; camera.orthographicSize = worldHeight / 2;
            camera.aspect = (float)width / height; camera.transform.position = new Vector3(0, 0, -20); camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(36, 29, 50, 255); camera.nearClipPlane = .1f; camera.farClipPlane = 50;
            canvas.worldCamera = camera;
            var texture = new RenderTexture(width, height, 24); texture.Create(); camera.targetTexture = texture;
            var previous = RenderTexture.active; Texture2D read = null;
            try
            {
                Canvas.ForceUpdateCanvases(); foreach (var text in root.GetComponentsInChildren<TMP_Text>()) text.ForceMeshUpdate();
                root.GetComponentInChildren<RedesignUpgradesScreen>()?.Refresh();
                root.GetComponentInChildren<RedesignBatchScreen>()?.Refresh();
                Canvas.ForceUpdateCanvases();
                foreach (var fit in root.GetComponentsInChildren<ImportedDialogFit>()) fit.SendMessage("LateUpdate");
                Canvas.ForceUpdateCanvases(); afterLayout?.Invoke(); Canvas.ForceUpdateCanvases(); camera.Render();
                RenderTexture.active = texture; read = new Texture2D(width, height, TextureFormat.RGB24, false); read.ReadPixels(new Rect(0, 0, width, height), 0, 0); read.Apply();
                File.WriteAllBytes(folder + "/" + prefix + "-" + width + "x" + height + (notch ? "-notched" : "") + ".png", read.EncodeToPNG());
                if (read.GetPixels32().Count(p => p.r > 120 && p.g > 90) < width * height / 4)
                    throw new InvalidOperationException("UI capture is blank or missing its paper surface");
                foreach (var b in root.GetComponentsInChildren<Button>())
                {
                    var corners = new Vector3[4]; ((RectTransform)b.transform).GetWorldCorners(corners);
                    var points = corners.Select(camera.WorldToScreenPoint).ToArray();
                    float buttonWidth = points.Max(p => p.x) - points.Min(p => p.x), buttonHeight = points.Max(p => p.y) - points.Min(p => p.y);
                    if (buttonWidth < 47.9f || buttonHeight < 47.9f) throw new InvalidOperationException("Undersized touch target at " + width + "x" + height + ": " + b.name + " " + buttonWidth + "x" + buttonHeight);
                }
            }
            finally { RenderTexture.active = previous; camera.targetTexture = null; texture.Release(); Object.DestroyImmediate(texture); if (read) Object.DestroyImmediate(read); Object.DestroyImmediate(cameraGo); }
        }
    }
}
