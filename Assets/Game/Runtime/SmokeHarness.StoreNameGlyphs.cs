#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Linq;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    public sealed partial class SmokeHarness
    {
        static bool DrawsSavedGlyph(TMP_Text text, char glyph)
        {
            text.ForceMeshUpdate();
            return text.textInfo.characterInfo.Take(text.textInfo.characterCount).Any(c =>
                c.character == glyph && c.isVisible && c.fontAsset != null &&
                c.fontAsset.HasCharacter(glyph));
        }

        IEnumerator StoreNameGlyphChecks()
        {
            var engine = game.Engine;
            engine.State.reducedMotion = true;
            engine.State.sound = engine.State.music = false;
            var home = game.HomeScreen.GetComponentInChildren<ImportedHomeScreen>(true);
            var sign = game.HomeScreen.GetComponentsInChildren<TMP_Text>(true)
                .Single(x => x.name == "Shop Sign");
            var detail = game.StoreNameScreen.GetComponentsInChildren<TMP_Text>(true)
                .Single(x => x.name == "Full Name");
            var settingsName = game.SettingsScreen.GetComponentsInChildren<TMP_Text>(true)
                .Single(x => x.name == "Description" && x.transform.parent.name == "Store name");
            var scrollCue = game.SettingsScreen.GetComponentsInChildren<TMP_Text>(true)
                .Single(x => x.name == "Lead Text");
            var settingsScroll = game.SettingsScreen.GetComponentInChildren<ScrollRect>(true);
            foreach (var size in new[] { new Vector2Int(360, 540), new Vector2Int(450, 1000) })
            {
                ResizeForReview(size.x, size.y);
                game.View.Switch("Home");
                Check(engine.SetStoreName("Jose\u0301") && engine.State.storeName == "Jos\u00e9",
                    "Decomposed accent saves as NFC at " + size);
                home.Refresh();
                yield return new WaitForSecondsRealtime(.4f);
                sign.ForceMeshUpdate();
                Check(sign.text == "Jos\u00e9" && sign.font.HasCharacter('\u00e9') &&
                      sign.textInfo.characterInfo.Any(c => c.character == '\u00e9' && c.isVisible) &&
                      !sign.isTextTruncated && !sign.isTextOverflowing,
                    "Home sign renders Jos\u00e9 with the saved accented glyph at " + size);
                yield return Capture("store-glyph-home-" + size.x, 0);
                game.View.ShowSettings();
                yield return new WaitForSecondsRealtime(.3f);
                settingsScroll.verticalNormalizedPosition = 1f;
                Canvas.ForceUpdateCanvases();
                yield return null;
                Check(scrollCue.text.Contains('\u2193') && DrawsSavedGlyph(scrollCue, '\u2193') &&
                      !scrollCue.isTextTruncated && !scrollCue.isTextOverflowing,
                    "Settings scroll cue draws the down arrow from a saved font at " + size);
                yield return Capture("store-glyph-scroll-cue-" + size.x, 0);
                settingsScroll.verticalNormalizedPosition = 0f;
                Canvas.ForceUpdateCanvases();
                yield return null;
                settingsName.ForceMeshUpdate();
                Check(settingsName.text == "Jos\u00e9" && !settingsName.richText &&
                      settingsName.font.HasCharacter('\u00e9') &&
                      settingsName.textInfo.characterInfo.Any(c => c.character == '\u00e9' && c.isVisible) &&
                      !settingsName.isTextTruncated && !settingsName.isTextOverflowing,
                    "Settings renders the full accented store name at " + size);
                yield return Capture("store-glyph-settings-" + size.x, 0);
                game.SettingsScreen.Close();
                yield return new WaitForSecondsRealtime(.3f);
                game.View.ShowStoreNameDetail();
                yield return new WaitForSecondsRealtime(.3f);
                detail.ForceMeshUpdate();
                Check(detail.text == "Jos\u00e9" && !detail.richText && detail.font.HasCharacter('\u00e9') &&
                      detail.textInfo.characterInfo.Any(c => c.character == '\u00e9' && c.isVisible) &&
                      !detail.isTextTruncated && !detail.isTextOverflowing,
                    "Full-name detail renders Jos\u00e9 at " + size);
                yield return Capture("store-glyph-detail-" + size.x, 0);
                game.StoreNameScreen.Dismiss();
                yield return new WaitForSecondsRealtime(.3f);

                const string markup = "<b>Jos\u00e9</b>";
                Check(engine.SetStoreName(markup), "Literal markup is accepted as a store name at " + size);
                home.Refresh();
                yield return new WaitForSecondsRealtime(.2f);
                sign.ForceMeshUpdate();
                Check(sign.text == markup && !sign.richText, "Home sign treats markup as text at " + size);
                game.View.ShowStoreNameDetail();
                yield return new WaitForSecondsRealtime(.3f);
                detail.ForceMeshUpdate();
                Check(detail.text == markup && !detail.richText &&
                      detail.textInfo.characterCount == markup.Length &&
                      !detail.isTextTruncated && !detail.isTextOverflowing,
                    "Full-name detail shows every markup character literally at " + size);
                yield return Capture("store-glyph-literal-" + size.x, 0);
                game.StoreNameScreen.Dismiss();
                yield return new WaitForSecondsRealtime(.3f);
            }

            ResizeForReview(360, 540);
            foreach (string tab in new[] { "Products", "Player", "Legacy" })
            {
                game.View.Switch(tab);
                yield return new WaitForSecondsRealtime(.3f);
                var root = tab == "Products" ? game.ProductsScreen.gameObject :
                    tab == "Player" ? game.PlayerScreen.gameObject : game.LegacyScreen.gameObject;
                var shop = root.GetComponentsInChildren<Button>(true)
                    .Single(x => x.name == "collapse-" + tab);
                var label = shop.GetComponentInChildren<TMP_Text>(true);
                Check(label.text == "\u2190 Shop" && DrawsSavedGlyph(label, '\u2190') &&
                      !label.isTextTruncated && !label.isTextOverflowing,
                    tab + " return cue draws the left arrow from a saved font at 360x540");
                yield return Capture("store-glyph-return-" + tab.ToLowerInvariant(), 0);
                game.View.Switch("Home");
                yield return new WaitForSecondsRealtime(.2f);
            }

            string prior = engine.State.storeName;
            foreach (string invalid in new[] { "\u200b", "\u00a0", "\u00ad", "\u0926\u0940\u0935\u093e\u0932\u0940", "\ud83c\udf86", "\ud800" })
                Check(!engine.SetStoreName(invalid) && engine.State.storeName == prior,
                    "Unsupported or invisible store name is rejected without changing saved value");
            engine.State.storeName = "\u0926\u0940\u0935\u093e\u0932\u0940";
            Check(engine.State.StoreName == GameState.StoreNameUpdateRequired &&
                  engine.State.NeedsStoreNameUpdate &&
                  engine.State.storeName == "\u0926\u0940\u0935\u093e\u0932\u0940",
                "Legacy unsupported raw name remains saved while display asks for an update");
            engine.State.storeName = null;
            Check(engine.State.StoreName == GameState.DefaultStoreName && !engine.State.NeedsStoreNameUpdate,
                "An unset store name still shows the default");
            engine.State.storeName = prior;
            home.Refresh();
        }
    }
}
#endif
