using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RocketRaja.Classic;
using RocketRaja.Core;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RocketRaja.Editor
{
    public static partial class BuildClassicUI
    {
        const string LedgerAtlas = "Assets/Game/ClassicUI/Achievements/ledger-atlas.png";
        static readonly string[] LedgerNames = { "trophy", "kaju", "stall", "four-products", "eight-products", "prestige", "taps", "earnings", "upgrades" };
        // Pixel rectangles on the generated atlas, measured from its top left.
        static readonly Rect[] LedgerRects = {
            new Rect(0,80,455,315), new Rect(460,90,350,295), new Rect(820,50,410,350),
            new Rect(0,415,410,355), new Rect(425,420,390,350), new Rect(825,420,410,355),
            new Rect(0,790,410,400), new Rect(420,790,400,400), new Rect(830,790,410,400)
        };

        [MenuItem("Rocket Raja/Classic/Apply selected achievement reward ledger")]
        public static void RefineAchievementRewardLedger()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            var main = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !main.isLoaded;
            if (opened) main = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            ui = main.GetRootGameObjects().Single(g => g.name == RootName).GetComponent<ClassicUI>();
            if (main.isDirty) throw new InvalidOperationException("Save existing Main edits before authoring the ledger");
            var previous = ui.dialogs.Single(v => v.route == "Achievements");
            if (previous.scroll.content.Find("Ledger trophy"))
            {
                ApplyLedgerControls(ui);
                EditorUtility.SetDirty(ui); EditorSceneManager.MarkSceneDirty(main); EditorSceneManager.SaveScene(main);
                if (opened) EditorSceneManager.CloseScene(main, true); return;
            }
            var sprites = ImportLedgerAtlas();
            body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Body SDF.asset");
            bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Bold SDF.asset");
            serif = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Card Serif SDF.asset");
            heading = serif; blue = ui.blueButton; ivory = ui.ivoryButton;
            rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            row = AssetDatabase.LoadAssetAtPath<Sprite>(SyncArt + "skills-list-row.png");
            texts.Clear(); views.Clear();
            Undo.RegisterFullObjectHierarchyUndo(ui.gameObject, "Apply selected reward ledger");
            previous.root.SetActive(false); previous.root.name = "Achievements__Legacy_20261021";
            var view = Dialog(previous.root.transform.parent, "Achievements", "Your achievements", out var content);
            StatisticsSkin(view.window.GetComponent<UnityEngine.UI.Image>(), AssetDatabase.LoadAssetAtPath<Sprite>(SyncArt + "skills-list-panel.png"));
            var oldFrame = previous.window.Find("Painted gold and cobalt frame");
            if (oldFrame) { var frame = UnityEngine.Object.Instantiate(oldFrame.gameObject, view.window, false); frame.name = oldFrame.name; frame.transform.SetAsFirstSibling(); }
            var title = view.window.Find("Header/Title").GetComponent<TMP_Text>();
            LedgerText(title, 23, 18, TextAlignmentOptions.Center); title.font = serif;
            ConfigureAchievementClose(view);
            view.root.transform.Find("Outside dismissal").GetComponent<UnityEngine.UI.Button>().interactable = true;
            ClassicPopupClearanceAuthoring.ApplyView(view);

            var flow = content.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            flow.padding = new RectOffset(4, 4, 0, 6); flow.spacing = 6;
            // Hero and summary scroll away together on short screens; actions stay reachable.
            var hero = New("Ledger trophy", content); Height(hero, 80);
            Picture("", Fill("Trophy artwork", hero), sprites["trophy"]);
            hero.name = "Ledger trophy";
            var summary = Row("Ledger summary", content, 45); StatisticsSkin(summary.GetComponent<UnityEngine.UI.Image>(), row);
            for (int i = 1; i < 3; i++) Face(Region("Gold separator " + i, summary, i / 3f, .20f, i / 3f + .002f, .80f), new Color32(193, 148, 61, 255));
            ui.achievementClaimedCount = LedgerSummary(summary, 0, "Claimed", "0 / 15", "claimed");
            ui.achievementReadyCount = LedgerSummary(summary, 1, "Ready", "0", "rewards ready");
            ui.achievementWallet = LedgerSummary(summary, 2, "Wallet", "0", "Kaju Katli");
            ui.achievementSummary = ui.achievementClaimedCount;

            var rows = new List<ClassicAchievementRow>();
            foreach (var definition in RedesignAchievements.All)
            {
                var card = Row(definition.Id, content, 90); StatisticsSkin(card.GetComponent<UnityEngine.UI.Image>(), row);
                Picture("", Region("Achievement artwork", card, .012f, .08f, .208f, .93f), sprites[LedgerIcon(definition)]);
                var name = Label("", Region("Title", card, .225f, .69f, .77f, .98f), definition.Title, 12, true);
                LedgerText(name, 12, 10, TextAlignmentOptions.MidlineLeft);
                var description = Label("", Region("Description", card, .225f, .405f, .626f, .71f), definition.Description, 9.5f, false, Muted);
                LedgerText(description, 9.5f, 9, TextAlignmentOptions.TopLeft);
                var track = Region("Progress track", card, .225f, .255f, .618f, .325f);
                Face(track, new Color32(218, 196, 145, 255), rounded);
                var fill = Face(Fill("Fill", track, .5f), new Color32(207, 156, 43, 255), rounded);
                ConfigureProgressFill(fill); fill.fillAmount = 0;
                var progress = Label("", Region("Progress", card, .225f, .045f, .62f, .24f), definition.Progress(RedesignState.Fresh()), 10, true);
                LedgerText(progress, 10, 9, TextAlignmentOptions.MidlineLeft);
                Picture("", Region("Kaju Katli artwork", card, .642f, .40f, .773f, .76f), sprites["kaju"]);
                var reward = Label("", Region("Reward", card, .628f, .045f, .782f, .43f), "+" + definition.Reward + "\nKaju Katli", 9.5f, true, Ink, TextAlignmentOptions.Center);
                LedgerText(reward, 9.5f, 8.5f, TextAlignmentOptions.Center);
                var badge = Region("Ready badge", card, .787f, .735f, .976f, .95f);
                Face(badge, new Color32(159, 111, 18, 255), rounded);
                var badgeText = Label("", Fill("READY", badge), "READY", 8.5f, true, Cream, TextAlignmentOptions.Center);
                LedgerText(badgeText, 8.5f, 8, TextAlignmentOptions.Center);
                badge.gameObject.SetActive(false);
                var claimRect = Region("Claim reward", card, .787f, 0, .976f, 0);
                claimRect.offsetMin = new Vector2(0, 8); claimRect.offsetMax = new Vector2(0, 56);
                var claim = Button(claimRect, "ClaimAchievement", definition.Id, "", "In progress", ivory, 13);
                var colors = claim.colors; colors.disabledColor = Color.white; claim.colors = colors;
                claim.interactable = false; var action = claim.GetComponentInChildren<TMP_Text>(true);
                action.rectTransform.offsetMin = new Vector2(3, 2); action.rectTransform.offsetMax = new Vector2(-3, -2);
                LedgerText(action, 13, 9, TextAlignmentOptions.Center);
                rows.Add(new ClassicAchievementRow { id = definition.Id, progress = progress, reward = reward, action = action, claim = claim, fill = fill, readyBadge = badge.gameObject });
            }
            var note = Bottom("Permanent rewards note", view.window, 20, 63);
            Label("", Fill("Copy", note, 3), "Progress stays after prestige.", 10, false, Ink, TextAlignmentOptions.Center);
            var back = Footer(view, 0, "Close", "", "", "Back to your stall", true);
            LedgerText(back.GetComponentInChildren<TMP_Text>(true), 14, 12, TextAlignmentOptions.Center);
            var scroll = (RectTransform)view.scroll.transform; scroll.offsetMin = new Vector2(10, 87); scroll.offsetMax = new Vector2(-10, -90);
            ui.achievementRows = rows.ToArray(); ui.achievementRowOffset = 2;
            ApplyLedgerControls(ui);
            ui.dialogs = ui.dialogs.Select(v => v == previous ? view : v).ToArray();
            view.root.SetActive(false);
            foreach (var text in view.root.GetComponentsInChildren<TMP_Text>(true)) text.ForceMeshUpdate(true, true);
            EditorUtility.SetDirty(ui); Canvas.ForceUpdateCanvases(); ui.GetComponent<ClassicLayout>().Apply();
            EditorSceneManager.MarkSceneDirty(main); EditorSceneManager.SaveScene(main);
            if (opened) EditorSceneManager.CloseScene(main, true);
            Debug.Log("Selected achievement reward ledger saved in Main; previous page preserved inactive.");
        }

        // Small geometric UI surfaces, with live TMP labels. Kept local to this
        // ledger so shared popup/folk button skins cannot replace its state art.
        static void ApplyLedgerControls(ClassicUI target)
        {
            var ready = LedgerControlSurface("claim-ready", false, new Color32(28, 92, 148, 255), new Color32(17, 66, 113, 255), new Color32(223, 166, 52, 255));
            var progress = LedgerControlSurface("claim-progress", false, new Color32(199, 221, 237, 255), new Color32(171, 199, 221, 255), new Color32(110, 155, 188, 255));
            var claimed = LedgerControlSurface("claim-claimed", false, new Color32(213, 206, 188, 255), new Color32(190, 182, 162, 255), new Color32(167, 157, 135, 255));
            var badge = LedgerControlSurface("ready-badge", true, new Color32(174, 126, 24, 255), new Color32(142, 92, 10, 255), new Color32(195, 145, 40, 255));
            Undo.RegisterFullObjectHierarchyUndo(target.gameObject, "Correct achievement READY and Claim controls");
            target.achievementClaimReady = ready; target.achievementClaimProgress = progress; target.achievementClaimed = claimed;
            foreach (var binding in target.achievementRows)
            {
                var face = (UnityEngine.UI.Image)binding.claim.targetGraphic;
                face.sprite = progress; face.type = UnityEngine.UI.Image.Type.Simple; face.preserveAspect = false; face.color = Color.white;
                var colors = binding.claim.colors; colors.normalColor = colors.disabledColor = Color.white;
                colors.highlightedColor = new Color(1, 1, 1, 1); colors.pressedColor = new Color(.9f, .9f, .9f, 1); binding.claim.colors = colors;
                LedgerText(binding.action, 14, 10, TextAlignmentOptions.Center);
                binding.action.rectTransform.offsetMin = new Vector2(3, 2); binding.action.rectTransform.offsetMax = new Vector2(-3, -2);
                var image = binding.readyBadge.GetComponent<UnityEngine.UI.Image>();
                image.sprite = badge; image.type = UnityEngine.UI.Image.Type.Simple; image.preserveAspect = false; image.color = Color.white;
                var label = binding.readyBadge.GetComponentInChildren<TMP_Text>(true);
                LedgerText(label, 9, 8, TextAlignmentOptions.Center);
                label.rectTransform.anchorMin = new Vector2(.17f, 0); label.rectTransform.anchorMax = new Vector2(.83f, 1);
                label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
            }
        }

        static Sprite LedgerControlSurface(string name, bool badge, Color top, Color bottom, Color border)
        {
            const int width = 192; int height = badge ? 56 : 144;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
            {
                Color sum = Color.clear;
                for (int sy = 0; sy < 4; sy++) for (int sx = 0; sx < 4; sx++)
                {
                    float px = x + (sx + .5f) / 4, py = y + (sy + .5f) / 4;
                    float edge;
                    if (badge)
                    {
                        float cx = Mathf.Min(px, width - px), cy = Mathf.Min(py, height - py);
                        // Concave corner cuts reproduce the small gold READY plaque.
                        edge = Mathf.Min(Mathf.Min(cx, cy), Mathf.Sqrt(cx * cx + cy * cy) - 10);
                    }
                    else
                    {
                        var q = new Vector2(Mathf.Abs(px - width / 2f) - (width / 2f - 16), Mathf.Abs(py - height / 2f) - (height / 2f - 16));
                        edge = 16 - (Vector2.Max(q, Vector2.zero).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0));
                    }
                    if (edge < 0) continue;
                    var color = Color.Lerp(bottom, top, py / height);
                    if (edge < (badge ? 1.8f : 4)) color = border;
                    else if (!badge && edge < 6) color = Color.Lerp(top, Color.white, .48f);
                    if (badge)
                    {
                        float sparkX = Mathf.Min(Mathf.Abs(px - 21), Mathf.Abs(px - (width - 21)));
                        float sparkY = Mathf.Abs(py - height / 2f);
                        bool sparkle = sparkX / 6 + sparkY / 13 < 1;
                        bool dot = Mathf.Pow(sparkX - 8, 2) + Mathf.Pow(sparkY - 10, 2) < 3;
                        if (sparkle || dot) color = new Color32(255, 231, 148, 255);
                    }
                    sum += color / 16;
                }
                // Unpremultiplied pixels preserve antialiased transparent edges.
                if (sum.a > 0) { float alpha = sum.a; sum /= alpha; sum.a = alpha; }
                pixels[y * width + x] = sum;
            }
            texture.SetPixels(pixels); texture.Apply();
            string path = "Assets/Game/ClassicUI/Achievements/" + name + ".png";
            var png = texture.EncodeToPNG(); File.WriteAllBytes(path, png); UnityEngine.Object.DestroyImmediate(texture);
            foreach (var folder in new[] { "UI Builder/HTML/Classic/assets/achievements/", "UI Builder/HTML/Achievements/assets/achievements/" }) File.WriteAllBytes(folder + name + ".png", png);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static TMP_Text LedgerSummary(RectTransform parent, int column, string name, string value, string caption)
        {
            var cell = Region(name, parent, column / 3f, 0, (column + 1) / 3f, 1);
            var count = Label("", Region("Value", cell, .02f, .38f, .98f, .94f), value, 16, true, Ink, TextAlignmentOptions.Center);
            LedgerText(count, 16, 11, TextAlignmentOptions.Center);
            var label = Label("", Region("Caption", cell, .02f, .04f, .98f, .43f), caption, 9, true, Ink, TextAlignmentOptions.Center);
            LedgerText(label, 9, 8.5f, TextAlignmentOptions.Center); return count;
        }
        static void LedgerText(TMP_Text text, float size, float minimum, TextAlignmentOptions alignment)
        {
            text.fontSize = text.fontSizeMax = size; text.fontSizeMin = minimum; text.enableAutoSizing = true;
            text.alignment = alignment; text.margin = Vector4.zero; text.overflowMode = TextOverflowModes.Ellipsis;
        }
        static string LedgerIcon(AchievementDefinition a) => a.Metric == AchievementMetric.Taps ? "taps" : a.Metric == AchievementMetric.Earnings ? "earnings"
            : a.Metric == AchievementMetric.Upgrades ? "upgrades" : a.Metric == AchievementMetric.Prestiges ? "prestige"
            : a.Target == 1 ? "stall" : a.Target == 4 ? "four-products" : "eight-products";
        static Dictionary<string, Sprite> ImportLedgerAtlas()
        {
            AssetDatabase.ImportAsset(LedgerAtlas, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(LedgerAtlas);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.maxTextureSize = 2048;
            importer.GetSourceTextureWidthAndHeight(out int sourceWidth, out int sourceHeight);
#pragma warning disable 0618
            importer.spritesheet = LedgerNames.Select((name, i) => new SpriteMetaData { name = name, alignment = 9, pivot = new Vector2(.5f, .5f),
                rect = new Rect(LedgerRects[i].x, sourceHeight - LedgerRects[i].y - LedgerRects[i].height, LedgerRects[i].width, LedgerRects[i].height) }).ToArray();
#pragma warning restore 0618
            importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(LedgerAtlas).OfType<Sprite>().ToDictionary(s => s.name, StringComparer.Ordinal);
        }
    }
}
