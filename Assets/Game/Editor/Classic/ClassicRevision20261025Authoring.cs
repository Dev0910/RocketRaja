using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RocketRaja.Classic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RocketRaja.Editor
{
    public static partial class BuildClassicUI
    {
        const string RevisionArt = Local + "Revision20261025/";
        static Sprite selectedToken, informationRing, neutralSurface;

        /// <summary>Explicit Editor-only transfer of the reviewed October 25 Classic refinements.</summary>
        [MenuItem("Rocket Raja/Classic/Apply reviewed October 25 revision")]
        public static void ApplyReviewedOctober25FromMenu() => Debug.Log(ApplyOctober25Revision());

        public static string ApplyOctober25Revision()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required");
            var scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.isLoaded || scene.isDirty) throw new InvalidOperationException("Open a clean Main scene first");
            ui = scene.GetRootGameObjects().Single(g => g.name == RootName).GetComponent<ClassicUI>();
            if (ui.transform.Find("Reviewed revision 20261025")) return "Reviewed revision already saved; use scoped refinements";
            Undo.RegisterFullObjectHierarchyUndo(ui.gameObject, "Apply reviewed Classic revision");
            body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Body SDF.asset");
            bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Bold SDF.asset");
            serif = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Card Serif SDF.asset");
            heading = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Heading SDF.asset");
            texts.Clear(); texts.AddRange(ui.textBindings); images.Clear(); images.AddRange(ui.imageBindings);
            blue = ui.blueButton; ivory = ui.ivoryButton;
            neutralSurface = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Bootstrap/Art/Rounded Surface.png");
            Directory.CreateDirectory(RevisionArt);
            selectedToken = ImportRevisionSprite("UI Builder/HTML/Classic/assets/currency/kaju-katli.png", "kaju-katli.png", 512);
            var names = new[] { "product-profit", "product-cost", "tap-direct", "tap-linked", "tap-cost", "global-profit", "upgrade-cost", "skill-power", "skill-duration", "skill-cooldown", "product-link", "product-group" };
            ui.upgradeTypeIcons = names.Select(n => ImportRevisionSprite("UI Builder/HTML/Classic/assets/upgrades/" + n + ".png", n + ".png", 256)).ToArray();
            informationRing = MakeInformationRing();

            var settings = ui.GetComponentsInChildren<ClassicControl>(true).First(c => c.action == "Navigate" && c.parameter == "Settings");
            var gear = (RectTransform)settings.transform;
            gear.anchorMin = gear.anchorMax = gear.pivot = new Vector2(0, 1); gear.sizeDelta = new Vector2(48, 48); gear.anchoredPosition = new Vector2(8, -8);
            foreach (var r in ui.nextUpgrades.Concat(ui.catalogueRows)) RevisionUpgradeRow(r);
            foreach (var p in ui.products)
            {
                var buy = (RectTransform)p.buy.transform; buy.anchorMin = buy.anchorMax = new Vector2(1, .5f); buy.pivot = new Vector2(1, .5f); buy.sizeDelta = new Vector2(90, 56); buy.anchoredPosition = new Vector2(-10, 0);
                var details = (RectTransform)buy.parent.Find("Details"); details.anchorMax = new Vector2(.55f, details.anchorMax.y);
                p.level.rectTransform.anchorMin = new Vector2(.555f, .32f); p.level.rectTransform.anchorMax = new Vector2(.70f, .73f); p.level.rectTransform.offsetMin = p.level.rectTransform.offsetMax = Vector2.zero;
                p.cost.enableAutoSizing = true; p.cost.fontSize = p.cost.fontSizeMax = 12; p.cost.fontSizeMin = 9;
            }
            if (ui.achievementTabLabel) ui.achievementTabLabel.text = "Achievements";

            string[] compact = { "Player", "Products", "Legacy", "Shop", "ProductDetail", "DrawReveal" };
            foreach (var guide in ui.guidance.rows.Where(r => compact.Contains(r.route))) guide.root.SetActive(false);
            ui.guidance.rows = ui.guidance.rows.Where(r => !compact.Contains(r.route)).ToArray();
            foreach (var guide in ui.guidance.rows)
            {
                var c = guide.root.GetComponentsInChildren<ClassicControl>(true).FirstOrDefault();
                if (c)
                {
                    // The row remains its existing large hit area; only the trailing glyph changes.
                    var read = guide.root.transform.Find("Read");
                    if (read) RevisionInfoGlyph((RectTransform)read);
                }
            }
            foreach (var route in new[] { "Player", "Products" })
            {
                var panel = ui.panels.Single(v => v.route == route);
                var info = New("Information", panel.root.transform.Find("Header"));
                info.anchorMin = info.anchorMax = new Vector2(1, .5f); info.pivot = new Vector2(1, .5f); info.sizeDelta = new Vector2(48, 48); info.anchoredPosition = new Vector2(-60, 0);
                RevisionInfoButton(info, "Info", route == "Player" ? "buy-all" : "product-levels");
            }
            var legacy = ui.panels.Single(v => v.route == "Legacy");
            var odds = legacy.root.GetComponentsInChildren<ClassicControl>(true).First(c => c.parameter == "Odds");
            RevisionInfoGlyph((RectTransform)odds.transform);
            var balance = ui.textBindings.Single(b => b.key == "legacy-kk").text;
            balance.rectTransform.anchorMin = new Vector2(.40f, .12f); balance.rectTransform.anchorMax = new Vector2(.69f, .9f); balance.rectTransform.offsetMin = balance.rectTransform.offsetMax = Vector2.zero;
            balance.fontSize = balance.fontSizeMax = 18; balance.fontSizeMin = 13; balance.enableAutoSizing = true;

            foreach (var view in ui.dialogs) RevisionPopup(view);
            var product = ui.dialogs.Single(v => v.route == "ProductDetail");
            var header = (RectTransform)product.window.Find("Header");
            header.Find("Title").GetComponent<TMP_Text>().color = Ink;
            var productInfo = Fixed("Information", header, 12, 20, 48, 48);
            RevisionInfoButton(productInfo, "Info", "product-levels");
            // The AFK implementation already has the same compact information entry.
            if (ui.offlineView)
            {
                var oldInfo = ui.offlineView.transform.Find("Header/Offline information");
                if (oldInfo) RevisionInfoGlyph((RectTransform)oldInfo);
            }
            RevisionSwitches();
            RevisionCurrency();
            ui.textBindings = texts.ToArray(); ui.imageBindings = images.ToArray();
            var layout = ui.GetComponent<ClassicLayout>();
            layout.touchGraphics = ui.GetComponentsInChildren<Button>(true).Select(b => b.targetGraphic).Where(g => g).ToArray();
            var marker = New("Reviewed revision 20261025", ui.transform); marker.gameObject.SetActive(false);
            Canvas.ForceUpdateCanvases(); layout.Apply();
            foreach (var view in ui.dialogs) view.root.SetActive(false);
            EditorUtility.SetDirty(ui); EditorUtility.SetDirty(ui.guidance); EditorUtility.SetDirty(layout);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return "Saved Classic revision: 12 upgrade icons / 30 row templates, currency token and TMP slots, compact info controls, popup frames, square gear, product gutters, Achievements label and three settings switches. " + RefineOctober25Revision();
        }

        static Sprite ImportRevisionSprite(string source, string name, int maximum)
        {
            string path = RevisionArt + name; File.Copy(source, path, true);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = maximum; importer.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        static Sprite MakeInformationRing()
        {
            string path = RevisionArt + "information-ring.png";
            var texture = new Texture2D(128, 128, TextureFormat.RGBA32, false); var pixels = new Color[128 * 128];
            for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
            {
                float r = Vector2.Distance(new Vector2(x + .5f, y + .5f), new Vector2(64, 64));
                pixels[y * 128 + x] = new Color(1, 1, 1, Mathf.Clamp01(Mathf.Min(62 - r, r - 51)));
            }
            texture.SetPixels(pixels); texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single; importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        static void RevisionUpgradeRow(ClassicUpgradeRow r)
        {
            var root = (RectTransform)r.root.transform;
            var artRect = Fixed("Upgrade type artwork", root, 8, 17, 36, 36);
            r.artwork = Picture("", artRect, ui.upgradeTypeIcons[0]);
            r.title.rectTransform.anchorMin = new Vector2(0, .52f); r.title.rectTransform.anchorMax = new Vector2(1, 1);
            r.title.rectTransform.offsetMin = new Vector2(52, 0); r.title.rectTransform.offsetMax = new Vector2(-110, -3);
            r.copy.rectTransform.anchorMin = Vector2.zero; r.copy.rectTransform.anchorMax = new Vector2(1, .52f);
            r.copy.rectTransform.offsetMin = new Vector2(52, 4); r.copy.rectTransform.offsetMax = new Vector2(-110, 0);
            r.title.enableAutoSizing = r.copy.enableAutoSizing = true;
            r.title.fontSizeMax = r.title.fontSize = 12; r.title.fontSizeMin = 9;
            r.copy.fontSizeMax = r.copy.fontSize = 10; r.copy.fontSizeMin = 8;
            var buy = (RectTransform)r.buy.transform; buy.anchorMin = buy.anchorMax = new Vector2(1, .5f); buy.pivot = new Vector2(1, .5f);
            buy.sizeDelta = new Vector2(96, 54); buy.anchoredPosition = new Vector2(-10, 0);
            r.cost.enableAutoSizing = true; r.cost.fontSizeMax = r.cost.fontSize = 11; r.cost.fontSizeMin = 8;
        }
        static void RevisionInfoButton(RectTransform root, string action, string topic)
        {
            var button = Button(root, action, topic, "", "", blue);
            button.GetComponent<ClassicControl>().htmlStyle = 3;
            RevisionInfoGlyph(root);
        }
        static void RevisionInfoGlyph(RectTransform root)
        {
            var oldGlyph = root.GetComponent<TMP_Text>(); if (oldGlyph) oldGlyph.text = "";
            foreach (var child in root.Cast<Transform>().ToArray())
                if (child.name != "Notification dot") child.gameObject.SetActive(false);
            var image = root.GetComponent<Image>(); if (image) { image.sprite = null; image.color = Color.clear; }
            var ring = New("Selected information circle", root); ring.anchorMin = ring.anchorMax = new Vector2(.5f, .5f); ring.sizeDelta = new Vector2(23, 23); ring.anchoredPosition = Vector2.zero;
            Picture("", ring, informationRing).color = Ink;
            var label = Label("", Fill("i", ring), "i", 16, true, Ink, TextAlignmentOptions.Center, serif);
            label.overflowMode = TextOverflowModes.Overflow; label.fontStyle = FontStyles.Bold;
        }

        public static string RefineOctober25Revision()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required");
            var scene = SceneManager.GetSceneByPath(ScenePath);
            var native = scene.GetRootGameObjects().Single(g => g.name == RootName).GetComponent<ClassicUI>();
            var ring = MakeInformationRing();
            foreach (var image in native.GetComponentsInChildren<Image>(true).Where(i => i.name == "Selected information circle")) image.sprite = ring;
            if (!native.legacyCurrencyIcon)
            {
                var legacy = native.panels.Single(v => v.route == "Legacy");
                var icon = New("Selected currency token", legacy.root.transform.Find("Header"));
                icon.anchorMin = icon.anchorMax = new Vector2(.665f, .5f); icon.sizeDelta = new Vector2(27, 27); icon.anchoredPosition = Vector2.zero;
                native.legacyCurrencyIcon = Picture("", icon, AssetDatabase.LoadAssetAtPath<Sprite>(RevisionArt + "kaju-katli.png"));
            }
            var balance = native.textBindings.Single(b => b.key == "legacy-kk").text;
            balance.rectTransform.anchorMax = new Vector2(.62f, .9f);
            balance.text = System.Text.RegularExpressions.Regex.Replace(balance.text ?? "0", @"\s*\bKK\b", "");
            var formatter = native.GetComponent<ClassicCurrencyPresentation>();
            if (formatter)
            {
                var relevant = RevisionCurrencyLabels(native);
                var keep = new HashSet<RedesignSavedText>(relevant);
                foreach (var text in native.GetComponentsInChildren<RedesignSavedText>(true).Where(t => !keep.Contains(t)))
                {
                    // Remove only this revision's unused extra slots, never the original saved font slots.
                    // TMP renames submeshes on enable; ownership comes from the new token references and slot index.
                    var extra = text.savedMaterialSlots.Skip(2).Where(s => s && (s.spriteAsset == formatter.token || s.sharedMaterial == formatter.token.material)).ToArray();
                    if (extra.Length == 0) continue;
                    text.savedMaterialSlots = text.savedMaterialSlots.Where(s => s && !extra.Contains(s)).ToArray();
                    foreach (var slot in extra) Undo.DestroyObjectImmediate(slot.gameObject);
                    text.spriteAsset = null; text.BindSavedSlots(); EditorUtility.SetDirty(text);
                }
                formatter.labels = relevant; EditorUtility.SetDirty(formatter);
            }
            foreach (var guide in native.guidance.rows)
            {
                var read = guide.root.transform.Find("Read");
                var text = read ? read.GetComponent<TMP_Text>() : null;
                if (text) text.text = "";
            }
            var help = native.dialogs.Single(v => v.route == "Help");
            help.scroll.content.Find("Start with a spark/Body").GetComponent<TMP_Text>().text = "Start with ₹0 and tap the counter for ₹1 per tap. Save ₹30, then stock Phuljhari in Products. Tap training starts at ₹10. Products earn automatically; tap the counter on any main tab for extra cash.";
            EditorUtility.SetDirty(native); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return "Refined guide glyphs and current fresh-economy Help copy";
        }
        static void RevisionPopup(ClassicView view)
        {
            var surface = view.window.GetComponent<Image>();
            foreach (var outline in view.window.GetComponents<Outline>()) outline.enabled = false;
            var frame = view.window.Find("Painted gold and cobalt frame") as RectTransform;
            if (view.route == "Skills" || view.route == "SkillDetail")
            {
                surface.sprite = neutralSurface; surface.type = Image.Type.Sliced; surface.pixelsPerUnitMultiplier = 4; surface.color = Gold;
                if (frame) frame.gameObject.SetActive(false);
                var paper = Fill("Single-frame paper", view.window, .8f); paper.SetAsFirstSibling();
                var image = Face(paper, Cream, neutralSurface); image.pixelsPerUnitMultiplier = 4;
            }
            else if (frame)
            {
                // The source frame has exterior padding: bleed to the clipped window edge, as in HTML.
                frame.anchorMin = Vector2.zero; frame.anchorMax = Vector2.one;
                frame.offsetMin = new Vector2(-4, -4); frame.offsetMax = new Vector2(4, 4);
                frame.GetComponent<Image>().fillCenter = false;
            }
            if (!view.window.GetComponent<RectMask2D>()) Undo.AddComponent<RectMask2D>(view.window.gameObject);
        }
        static void RevisionSwitches()
        {
            var view = ui.dialogs.Single(v => v.route == "Settings");
            foreach (string key in new[] { "music", "sound", "motion" })
            {
                var c = view.scroll.content.Find(key).GetComponentsInChildren<ClassicControl>(true).Single(c => c.action == "Setting");
                var target = (RectTransform)c.transform; target.anchorMin = target.anchorMax = new Vector2(1, .5f); target.pivot = new Vector2(1, .5f); target.sizeDelta = new Vector2(74, 48); target.anchoredPosition = new Vector2(-4, 0);
                foreach (Transform old in target) old.gameObject.SetActive(false);
                target.GetComponent<Image>().sprite = null; target.GetComponent<Image>().color = Color.clear; c.htmlStyle = 3;
                var track = New("Switch track", target); track.anchorMin = track.anchorMax = new Vector2(.5f, .5f); track.sizeDelta = new Vector2(74, 36); track.anchoredPosition = Vector2.zero;
                var skin = Face(track, Gold, neutralSurface); skin.pixelsPerUnitMultiplier = 1.8f;
                var inner = Fill("Track colour", track, .8f); var fill = Face(inner, key == "motion" ? new Color32(204, 193, 162, 255) : Ink, neutralSurface); fill.pixelsPerUnitMultiplier = 1.8f;
                var on = Label("", Region("On label", track, 0, 0, .52f, 1), "ON", 10, true, Cream, TextAlignmentOptions.Center);
                var off = Label("", Region("Off label", track, .46f, 0, 1, 1), "OFF", 9, true, Ink, TextAlignmentOptions.Center);
                on.enabled = key != "motion"; off.enabled = key == "motion";
                var thumb = New("Switch thumb", track); thumb.anchorMin = thumb.anchorMax = new Vector2(0, .5f); thumb.sizeDelta = new Vector2(28, 28); thumb.anchoredPosition = new Vector2(key == "motion" ? 16 : 58, 0);
                var rim = Face(thumb, Gold, AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"));
                Face(Fill("Cream thumb", thumb, 1.2f), Cream, AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"));
                var toggle = Undo.AddComponent<ClassicSettingsSwitch>(target.gameObject);
                toggle.ui = ui; toggle.key = key; toggle.track = track; toggle.thumb = thumb; toggle.surface = fill; toggle.onLabel = on; toggle.offLabel = off;
            }
        }
        static void RevisionCurrency()
        {
            string path = RevisionArt + "Kaju Katli Sprite.asset";
            var token = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(path);
            if (!token)
            {
                token = ScriptableObject.CreateInstance<TMP_SpriteAsset>(); token.name = "Kaju Katli Sprite";
                token.spriteSheet = selectedToken.texture;
                var face = token.faceInfo; face.pointSize = 128; face.scale = 1; face.ascentLine = 112; face.descentLine = -16; face.lineHeight = 128; token.faceInfo = face;
                var glyph = new TMP_SpriteGlyph(0, new GlyphMetrics(128, 128, 0, 112, 134), new GlyphRect(0, 0, selectedToken.texture.width, selectedToken.texture.height), 1, 0, selectedToken);
                token.spriteGlyphTable.Add(glyph); token.spriteCharacterTable.Add(new TMP_SpriteCharacter(0xE000, token, glyph) { name = "kaju-katli", scale = 1 });
                token.material = new Material(Shader.Find("TextMeshPro/Sprite")); token.material.name = "Kaju Katli Sprite Material"; token.material.mainTexture = selectedToken.texture;
                AssetDatabase.CreateAsset(token, path); AssetDatabase.AddObjectToAsset(token.material, token);
                var so = new SerializedObject(token); so.FindProperty("m_Version").stringValue = "1.1.0"; so.ApplyModifiedPropertiesWithoutUndo(); token.UpdateLookupTables(); EditorUtility.SetDirty(token); AssetDatabase.SaveAssetIfDirty(token);
            }
            var labels = RevisionCurrencyLabels(ui);
            foreach (var text in labels)
            {
                text.spriteAsset = token; text.richText = true;
                var slots = text.savedMaterialSlots.ToList();
                for (int i = slots.Count; i < 4; i++)
                {
                    var rect = Fill("Saved currency material " + (i + 1), text.transform);
                    rect.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
                    var slot = rect.gameObject.AddComponent<RedesignSavedSubMesh>();
                    var so = new SerializedObject(slot); so.FindProperty("m_TextComponent").objectReferenceValue = text; so.FindProperty("m_materialReferenceIndex").intValue = i + 1; so.ApplyModifiedPropertiesWithoutUndo();
                    slot.raycastTarget = false; slot.fontAsset = text.font; slot.spriteAsset = token; slot.sharedMaterial = token.material; slot.isDefaultMaterial = true; slot.hideFlags = HideFlags.None; slot.gameObject.hideFlags = HideFlags.None;
                    slots.Add(slot);
                }
                text.savedMaterialSlots = slots.ToArray(); text.BindSavedSlots(); EditorUtility.SetDirty(text);
            }
            var formatter = Undo.AddComponent<ClassicCurrencyPresentation>(ui.gameObject); formatter.labels = labels; formatter.token = token;
            // Existing dedicated artwork slots keep a clean number rather than duplicate currency words.
            foreach (var row in ui.achievementRows)
            {
                var artwork = row.claim.transform.parent.Find("Kaju Katli artwork"); if (artwork) artwork.GetComponent<Image>().sprite = selectedToken;
            }
            var shop = ui.panels.Single(v => v.route == "Shop");
            foreach (var control in shop.root.GetComponentsInChildren<ClassicControl>(true).Where(c => c.action == "Purchase"))
            {
                var art = control.transform.parent.Find("Pack"); if (art) art.GetComponent<Image>().sprite = selectedToken;
            }
            var purchase = ui.dialogs.Single(v => v.route == "Purchase");
            var pack = purchase.scroll.content.Find("Pack/Artwork"); if (pack) pack.GetComponent<Image>().sprite = selectedToken;
        }

        static RedesignSavedText[] RevisionCurrencyLabels(ClassicUI native)
        {
            var keys = new[] { "legacy-kk", "shop-kk", "draw-one", "draw-five", "purchase-amount" };
            var dynamic = new HashSet<TMP_Text>(native.textBindings.Where(b => keys.Contains(b.key)).Select(b => b.text));
            dynamic.Add(native.guidance.body);
            if (native.achievementWallet) dynamic.Add(native.achievementWallet);
            foreach (var row in native.guidance.rows) { dynamic.Add(row.title); dynamic.Add(row.brief); }
            var name = native.textBindings.Single(b => b.key == "shop-name").text;
            return native.GetComponentsInChildren<RedesignSavedText>(true).Where(t =>
            {
                for (var p = t.transform; p && p != native.transform; p = p.parent) if (p.name.Contains("__Legacy")) return false;
                return t != name && !t.GetComponentInParent<TMP_InputField>() &&
                    (dynamic.Contains(t) || System.Text.RegularExpressions.Regex.IsMatch(t.text ?? "", @"\bKK\b"));
            }).ToArray();
        }
    }
}
