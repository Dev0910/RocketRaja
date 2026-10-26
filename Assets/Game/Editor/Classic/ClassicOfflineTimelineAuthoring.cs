using System;
using System.IO;
using System.Linq;
using RocketRaja.Classic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    public static partial class BuildClassicUI
    {
        /// <summary>Scoped Editor migration for the selected Capped Timeline. Never runs at startup.</summary>
        public static string AuthorCappedOffline()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required");
            var scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.isLoaded || scene.isDirty) throw new InvalidOperationException("Open a clean Main scene first");
            ui = scene.GetRootGameObjects().Single(g => g.name == RootName).GetComponent<ClassicUI>();
            if (ui.offlineView) return RefineCappedOffline();
            var previous = ui.dialogs.Single(v => v.route == "Offline");
            Undo.RecordObject(ui, "Bind Capped Timeline");
            Undo.RecordObject(previous.root, "Preserve previous offline popup");
            previous.root.name = "Offline__LegacyDisabled_20261025";
            previous.root.SetActive(false);
            body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Body SDF.asset");
            bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Bold SDF.asset");
            heading = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Heading SDF.asset");
            serif = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Card Serif SDF.asset");
            blue = ui.blueButton; ivory = ui.ivoryButton; sand = ui.disabledButton;
            row = Sprite("Ivory Row Frame");
            rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            popupFrame = AssetDatabase.LoadAssetAtPath<Sprite>(SyncArt + "frame.png");
            ringSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SyncArt + "ring.png");
            texts.Clear(); texts.AddRange(ui.textBindings.Where(b => !b.text.transform.IsChildOf(previous.root.transform)));
            images.Clear(); images.AddRange(ui.imageBindings.Where(b => !b.image.transform.IsChildOf(previous.root.transform)));
            views.Clear(); views.AddRange(ui.dialogs.Where(v => v.route != "Offline"));
            var view = Dialog(previous.root.transform.parent, "Offline", "Welcome back", out var content, 2);
            RefreshPopup(view);
            var header = (RectTransform)view.window.Find("Header");
            header.sizeDelta = new Vector2(0, 70);
            var title = header.Find("Title").GetComponent<TMP_Text>();
            title.rectTransform.anchorMin = Vector2.zero; title.rectTransform.anchorMax = Vector2.one;
            title.rectTransform.offsetMin = new Vector2(54, 12); title.rectTransform.offsetMax = new Vector2(-54, -12);
            title.font = serif; title.fontSize = title.fontSizeMax = 25; title.fontSizeMin = 20;
            var close = (RectTransform)header.Find("Close");
            close.anchorMin = close.anchorMax = new Vector2(1, .5f); close.pivot = new Vector2(1, .5f);
            close.sizeDelta = new Vector2(44, 44); close.anchoredPosition = new Vector2(-12, 0);
            var info = Fixed("Offline information", header, 9, 11, 48, 48);
            var infoButton = Button(info, "OfflineInfo", "", "", "", blue);
            ((Image)infoButton.targetGraphic).sprite = null; infoButton.targetGraphic.color = Color.clear;
            var circle = Picture("", Fixed("Blue info circle", info, 13, 13, 22, 22), ringSprite);
            circle.color = Ink;
            Label("", Fill("i", info), "i", 17, true, Ink, TextAlignmentOptions.Center, serif);
            var bodyRect = (RectTransform)view.scroll.transform;
            bodyRect.offsetMin = new Vector2(8, 132); bodyRect.offsetMax = new Vector2(-8, -72);
            var layout = content.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 6, 8); layout.spacing = 10;
            var presenter = view.window.gameObject.AddComponent<ClassicOfflineView>();
            ui.offlineView = presenter;
            var burgundy = new Color32(166, 34, 73, 255);
            var muted = new Color32(97, 113, 120, 255);
            var hero = New("Earnings", content); Height(hero, 162);
            Picture("", Region("Coins", hero, .35f, .53f, .65f, 1), Sprite("Coins"));
            OfflineLabel(hero, "Offline earnings", "OFFLINE EARNINGS", .05f, .42f, .95f, .55f, 12, Ink);
            presenter.amount = OfflineLabel(hero, "Amount", "₹147.45M", .03f, .12f, .97f, .43f, 38, burgundy, true);
            presenter.rewardState = OfflineLabel(hero, "Reward state", "Normal reward · already saved", .03f, 0, .97f, .14f, 12, new Color32(127, 86, 44, 255));

            var storage = OfflineCard(content, "Offline storage", 182);
            OfflineLabel(storage, "Title", "Offline storage", .04f, .78f, .61f, .98f, 19, Ink, true, TextAlignmentOptions.Left);
            presenter.status = OfflineLabel(storage, "Status", "CAP REACHED", .61f, .79f, .97f, .97f, 10, muted, false, TextAlignmentOptions.Right);
            var track = Region("Timeline", storage, .07f, .41f, .94f, .58f);
            Face(track, new Color32(191, 190, 169, 255), rounded);
            var inner = Fill("Inset", track, 1.2f); Face(inner, new Color32(207, 204, 181, 255), rounded);
            presenter.timeFill = Face(Fill("Stored time gold", inner), new Color32(242, 180, 43, 255));
            ConfigureProgressFill(presenter.timeFill); presenter.timeFill.fillAmount = .8f;
            presenter.stored = OfflineLabel(track, "Stored caption", "8h / 8h stored", .02f, 0, .98f, 1, 13, Ink);
            var marker = New("Cap boundary", track); marker.anchorMin = new Vector2(.8f, 0); marker.anchorMax = new Vector2(.8f, 1);
            marker.sizeDelta = new Vector2(2, 4); marker.anchoredPosition = Vector2.zero; Face(marker, Ink);
            presenter.capMarker = marker;
            var flag = New("Cap flag", marker); flag.anchorMin = flag.anchorMax = new Vector2(.5f, 1);
            flag.pivot = new Vector2(1, 0); flag.sizeDelta = new Vector2(48, 23); flag.anchoredPosition = new Vector2(1, 4);
            Face(flag, Ink, rounded);
            presenter.cap = Label("", Fill("Cap label", flag, 2), "8h cap", 11, true, Cream, TextAlignmentOptions.Center);
            OfflineLabel(storage, "Start", "0h", .04f, .25f, .17f, .4f, 11, muted, false, TextAlignmentOptions.Left);
            presenter.overflow = OfflineLabel(storage, "Overflow", "2h beyond cap", .18f, .25f, .73f, .4f, 11, muted);
            presenter.away = OfflineLabel(storage, "Time away", "10h away", .73f, .25f, .96f, .4f, 11, muted, false, TextAlignmentOptions.Right);
            Face(Region("Rule", storage, .18f, .235f, .82f, .239f), new Color(0.77f, .59f, .26f, .4f));
            var efficiencyTrack = Fixed("Efficiency ring", storage, 98, 147, 28, 28);
            var efficiencyBg = Picture("", efficiencyTrack, ringSprite); efficiencyBg.color = new Color32(223, 208, 161, 255);
            presenter.efficiencyFill = Picture("", Fill("Efficiency fill", efficiencyTrack), ringSprite);
            presenter.efficiencyFill.color = Gold; presenter.efficiencyFill.type = Image.Type.Filled;
            presenter.efficiencyFill.fillMethod = Image.FillMethod.Radial360; presenter.efficiencyFill.fillOrigin = (int)Image.Origin360.Top;
            presenter.efficiencyFill.fillClockwise = true; presenter.efficiencyFill.fillAmount = .5f;
            // Keep the ring and caption together as the card width stretches.
            efficiencyTrack.anchorMin = efficiencyTrack.anchorMax = new Vector2(.36f, 0);
            efficiencyTrack.pivot = new Vector2(1, .5f); efficiencyTrack.anchoredPosition = new Vector2(0, 20);
            presenter.efficiency = OfflineLabel(storage, "Efficiency", "50% efficiency", .39f, .025f, .94f, .215f, 15, Ink, false, TextAlignmentOptions.Left);

            var comparison = OfflineCard(content, "Reward comparison", 99);
            Face(Region("Divider", comparison, .499f, .1f, .501f, .9f), new Color(.77f, .59f, .26f, .35f));
            OfflineLabel(comparison, "Normal title", "Normal reward", .02f, .68f, .49f, .96f, 14, Ink, true);
            OfflineLabel(comparison, "Ad title", "With an ad", .51f, .68f, .98f, .96f, 14, Ink, true);
            presenter.normalReward = OfflineLabel(comparison, "Normal amount", "₹147.45M", .025f, .36f, .48f, .71f, 23, burgundy, true);
            presenter.adReward = OfflineLabel(comparison, "Ad amount", "₹294.90M", .52f, .36f, .975f, .71f, 23, burgundy, true);
            Picture("", Region("Normal coins", comparison, .18f, .02f, .32f, .36f), Sprite("Coins"));
            Picture("", Region("Double coins", comparison, .65f, .01f, .86f, .38f), Sprite("Coins"));
            var adNote = New("Ad status", content); Height(adNote, 28);
            presenter.adStatus = Label("", Fill("Status", adNote), "Ads unavailable. Your normal reward is already saved.", 11, false, muted, TextAlignmentOptions.Center);
            presenter.watch = Footer(view, 1, "OfflineAd", "watch", "", "Watch ad for 2× rewards");
            presenter.watchLabel = presenter.watch.GetComponentInChildren<TMP_Text>();
            var proceed = Footer(view, 0, "Offline", "close", "", "Continue", true);
            foreach (var button in new[] { presenter.watch, proceed })
            {
                var rect = (RectTransform)button.transform; rect.anchoredPosition += new Vector2(0, 6);
                var image = (Image)button.targetGraphic;
                image.sprite = button == proceed ? ui.ivoryButton : ui.blueButton;
                image.type = Image.Type.Sliced; image.pixelsPerUnitMultiplier = 4;
                var label = button.GetComponentInChildren<TMP_Text>(); label.fontSize = 15;
                button.GetComponent<ClassicControl>().htmlStyle = 0;
            }
            presenter.watch.interactable = false; // No ad SDK is configured in this build.
            var frame = view.window.Find("Painted gold and cobalt frame"); frame.SetAsLastSibling();
            ui.dialogs = views.ToArray(); ui.textBindings = texts.ToArray(); ui.imageBindings = images.ToArray();
            // A compact header i replaces this popup's old contextual banner.
            Undo.RecordObject(ui.guidance, "Bind offline information");
            ui.guidance.rows = ui.guidance.rows.Where(r => r.route != "Offline").ToArray();
            var tip = ui.guidance.topics.Single(t => t.id == "return-play");
            tip.body = "Your normal offline reward is already added to cash. The gold timeline shows stored time; grey time beyond the storage cap earns nothing. Efficiency is the share of normal sales earned while away. Night Sales improves efficiency, and Long Opening Hours increases storage. A completed rewarded ad adds one extra reward, once. Continue keeps your earnings and returns to the shop.";
            var responsive = ui.GetComponent<ClassicLayout>();
            responsive.touchGraphics = ui.GetComponentsInChildren<Button>(true).Select(b => b.targetGraphic).Where(g => g).ToArray();
            view.root.SetActive(false);
            EditorUtility.SetDirty(ui); EditorUtility.SetDirty(ui.guidance); EditorUtility.SetDirty(responsive);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return "Saved native Capped Timeline; previous popup retained inactive. " + RefineCappedOffline();
        }

        public static string RefineCappedOffline()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required");
            var scene = SceneManager.GetSceneByPath(ScenePath);
            var native = scene.GetRootGameObjects().Single(g => g.name == RootName).GetComponent<ClassicUI>();
            var view = native.dialogs.Single(v => v.route == "Offline");
            var p = native.offlineView;
            // 515 content units, 72 header clearance and 132 footer clearance; viewport still clamps the height.
            view.preferredHeight = 719;
            // A filled annulus, rather than the old outline-only icon, makes efficiency legible.
            const string path = Local + "Offline/efficiency-ring.png";
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                var pixels = new Color[128 * 128];
                for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
                {
                    float r = Vector2.Distance(new Vector2(x + .5f, y + .5f), new Vector2(64, 64));
                    pixels[y * 128 + x] = new Color(1, 1, 1, Mathf.Clamp01(Mathf.Min(62 - r, r - 40)));
                }
                texture.SetPixels(pixels); texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            }
            var annulus = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            p.efficiencyFill.sprite = annulus;
            p.efficiencyFill.transform.parent.GetComponent<Image>().sprite = annulus;
            var close = view.window.Find("Header/Close").GetComponentInChildren<TMP_Text>(true);
            close.text = "×"; close.fontSize = close.fontSizeMax = 24; close.fontSizeMin = 16; close.enableAutoSizing = true;
            close.overflowMode = TextOverflowModes.Overflow; close.textWrappingMode = TextWrappingModes.NoWrap;
            close.alignment = TextAlignmentOptions.Center; close.margin = Vector4.zero;
            var neutral = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Bootstrap/Art/Rounded Surface.png");
            foreach (var card in new[] { "Offline storage", "Reward comparison" })
                foreach (var image in view.scroll.content.Find(card).GetComponentsInChildren<Image>(true).Where(i => i.type == Image.Type.Sliced))
                { image.sprite = neutral; image.pixelsPerUnitMultiplier = 4; }
            var track = p.timeFill.transform.parent;
            track.GetComponent<Image>().sprite = neutral; track.GetComponent<Image>().pixelsPerUnitMultiplier = 2;
            track.parent.GetComponent<Image>().sprite = neutral; track.parent.GetComponent<Image>().pixelsPerUnitMultiplier = 2;
            if (!track.GetComponent<Mask>()) Undo.AddComponent<Mask>(track.gameObject).showMaskGraphic = true;
            foreach (var button in view.window.GetComponentsInChildren<ClassicControl>(true).Where(c => c.action == "Offline" || c.action == "OfflineAd"))
            {
                var image = (Image)button.GetComponent<Button>().targetGraphic;
                image.sprite = button.action == "Offline" ? native.ivoryButton : native.blueButton;
                image.type = Image.Type.Sliced; image.pixelsPerUnitMultiplier = 4;
                button.htmlStyle = 0;
            }
            native.GetComponent<ClassicLayout>().Apply();
            EditorUtility.SetDirty(native); EditorUtility.SetDirty(p); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return "Saved legible close, filled efficiency ring, rounded timeline and folk action borders";
        }

        static RectTransform OfflineCard(Transform parent, string name, float height)
        {
            var r = New(name, parent); Height(r, height); Face(r, new Color32(201, 166, 94, 255), rounded);
            Face(Fill("Paper", r, .8f), Cream, rounded); return r;
        }
        static TMP_Text OfflineLabel(Transform parent, string name, string copy, float x0, float y0, float x1, float y1,
            float size, Color color, bool display = false, TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            var t = Label("", Region(name, parent, x0, y0, x1, y1), copy, size, true, color, align, display ? serif : bold);
            t.enableAutoSizing = true; t.fontSizeMax = size; t.fontSizeMin = size * .8f; return t;
        }
    }
}
