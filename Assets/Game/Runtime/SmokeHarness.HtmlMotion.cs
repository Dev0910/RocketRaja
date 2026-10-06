#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RocketRaja
{
    public sealed partial class SmokeHarness
    {
        IEnumerator HtmlControlChecks()
        {
            var state = game.Engine.State; state.reducedMotion = false;
            game.View.Switch("Home"); yield return new WaitForSecondsRealtime(.3f);
            game.View.ShowSettings(); yield return new WaitForSecondsRealtime(.3f);
            var button = game.SettingsScreen.GetComponentsInChildren<Button>().First(x => x.name == "Close Settings");
            var feedback = button.GetComponent<UiFeedback>();
            var target = feedback.Target; var origin = target.anchoredPosition; var scale = target.localScale;
            var pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left, pointerId = -1 };
            feedback.OnPointerDown(pointer); yield return new WaitForSecondsRealtime(.15f);
            Check(Vector3.Distance(target.localScale, scale * .98f) < .002f && Mathf.Abs((target.anchoredPosition.y - origin.y) / UiEasing.Pixels(target) + 1) < .05f,
                "HTML button reaches .98 scale and 1px press displacement in 120ms");
            feedback.OnPointerUp(pointer); yield return new WaitForSecondsRealtime(.15f);
            Check(Vector3.Distance(target.localScale, scale) < .001f && Vector2.Distance(target.anchoredPosition, origin) < .001f,
                "Button release restores exact authored geometry");
            Time.timeScale = 0; feedback.OnSubmit(new BaseEventData(EventSystem.current));
            yield return new WaitForSecondsRealtime(.4f); Time.timeScale = 1;
            Check(Vector3.Distance(target.localScale, scale) < .001f, "Keyboard submit releases using unscaled time");
            feedback.OnPointerDown(pointer); state.reducedMotion = true; yield return null; yield return null;
            Check(Vector3.Distance(target.localScale, scale) < .001f, "Reduced Motion cancels held-button transform");
            game.SettingsScreen.Close(); state.reducedMotion = false;
            game.View.Switch("Products"); yield return new WaitForSecondsRealtime(.3f);
            var nav = game.HomeScreen.GetComponentsInChildren<Button>().First(x => x.name == "nav-Products");
            var icon = nav.transform.Find("Icon");
            Check(Mathf.Abs(icon.localScale.x - 40f / 34) < .002f && Mathf.Abs(icon.localScale.y - 43f / 36) < .002f,
                "Selected navigation icon uses HTML width/height ratios");
            game.View.Switch("Home"); yield return new WaitForSecondsRealtime(.3f);
            Check(Vector3.Distance(icon.localScale, Vector3.one) < .001f, "Leaving navigation restores icon size");
            var notice = game.HomeScreen.GetComponentsInChildren<UiMotion>(true).First(x => x.name == "Notice");
            UiMotion.Hide(notice.gameObject, game); game.HomeScreen.Notice("Motion review");
            Check(notice.GetComponent<CanvasGroup>().alpha < .1f && !notice.GetComponent<CanvasGroup>().blocksRaycasts,
                "Toast begins transparent and never intercepts input");
            yield return new WaitForSecondsRealtime(.21f);
            Check(notice.GetComponent<CanvasGroup>().alpha == 1, "Toast finishes the 180ms HTML reveal");
            UiMotion.Hide(notice.gameObject, game);
            Check(!notice.gameObject.activeSelf, "Toast hides immediately as in HTML");
            game.View.ShowSponsor();
            Check(game.SponsorScreen == null || !game.SponsorScreen.gameObject.activeSelf, "Retired Sponsor has no animation or input surface");
            var all = game.HomeScreen.GetComponentsInChildren<UiFeedback>(true);
            Check(all.All(x => x.Target != null), "All Home feedback targets were authored before Play Mode");
            yield return HtmlMeterChecks();
            yield return HtmlTapChecks();
            yield return HtmlCelebrationChecks();
            yield return HtmlAmbientChecks();
        }
        IEnumerator HtmlAmbientChecks()
        {
            var state = game.Engine.State; state.reducedMotion = false;
            game.View.Switch("Home"); yield return new WaitForSecondsRealtime(.3f);
            var ambient = game.HomeScreen.GetComponentInChildren<ShopAmbient>(true);
            // The automation may run while the Editor is unfocused. Control this input explicitly.
            ambient.SendMessage("OnApplicationFocus", true);
            Check(ambient != null && ambient.GetComponent<CanvasRenderer>() != null && ambient.GetComponentInParent<RectMask2D>() != null && !ambient.raycastTarget,
                "Keeper motion uses a saved non-interactive component");
            Check(ambient.Keeper.pivot == new Vector2(.5f, 0) && Mathf.Abs(ShopAmbient.KeeperAngle(0) + .7f) < .001f && Mathf.Abs(ShopAmbient.KeeperAngle(1)) < .001f && Mathf.Abs(ShopAmbient.KeeperAngle(2) - .7f) < .001f && Mathf.Abs(ShopAmbient.KeeperAngle(4) + .7f) < .001f,
                "Keeper matches bottom pivot, .7-degree extremes and four-second HTML loop");
            var world = (RectTransform)ambient.Keeper.parent;
            var center = world.InverseTransformPoint(ambient.Keeper.TransformPoint(ambient.Keeper.rect.center));
            Check(Mathf.Abs((center.y - world.rect.yMin) / world.rect.height - .1585f) < .005f,
                "Keeper stays at the original lower-right counter position after the pivot change");
            Check(!ambient.VisitorsVisible, "Decorative visitors are removed");
            float before = ambient.Phase; yield return new WaitForSecondsRealtime(.2f);
            Check(ambient.Phase > before && !ambient.VisitorsVisible, "Keeper motion advances without visitors");
            game.View.ShowSettings(); before = ambient.Phase; var rotation = ambient.Keeper.localRotation;
            yield return new WaitForSecondsRealtime(.35f);
            Check(ambient.Phase == before && ambient.Keeper.localRotation == rotation, "Opening a modal freezes both ambient loops");
            game.SettingsScreen.Close(); yield return new WaitForSecondsRealtime(.35f);
            Check(ambient.Phase > before, "Ambient loops resume after modal release");
            ambient.SendMessage("OnApplicationFocus", false); before = ambient.Phase;
            yield return new WaitForSecondsRealtime(.15f);
            Check(ambient.Phase == before, "Background application focus pauses ambient motion");
            ambient.SendMessage("OnApplicationFocus", true);
            yield return new WaitForSecondsRealtime(.15f);
            Check(ambient.Phase > before, "Returning application focus resumes ambient motion");
            yield return Capture("10-ambient", 0);
            state.reducedMotion = true; yield return null; yield return null;
            Check(!ambient.VisitorsVisible && ambient.Phase == 0 && Quaternion.Angle(ambient.Keeper.localRotation, Quaternion.identity) < .001f,
                "Reduced Motion hides visitors and restores the keeper's authored pose");
            state.reducedMotion = false;
        }
        IEnumerator HtmlCelebrationChecks()
        {
            var state = game.Engine.State; state.reducedMotion = false;
            game.View.Switch("Home"); yield return new WaitForSecondsRealtime(.3f);
            var sparks = game.HomeScreen.GetComponentInChildren<FestivalSparkles>(true);
            Check(sparks != null && sparks.GetComponent<CanvasRenderer>() != null && !sparks.raycastTarget,
                "Celebration host and renderer are saved and non-interactive");
            int count = game.HomeScreen.GetComponentsInChildren<Transform>(true).Count(x => x.GetComponent<TMPro.TMP_SubMeshUI>() == null);
            game.HomeScreen.Celebrate();
            Check(sparks.ActiveSparkCount == 12, "Celebration uses twelve stars");
            yield return new WaitForSecondsRealtime(.15f); yield return Capture("09-celebration-mid", 0);
            for (int i = 0; i < 25; i++) game.HomeScreen.Celebrate();
            Check(sparks.ActiveSparkCount == 12 && sparks.Age < .01f && game.HomeScreen.GetComponentsInChildren<Transform>(true).Count(x => x.GetComponent<TMPro.TMP_SubMeshUI>() == null) == count,
                "Repeated celebrations restart the bounded star group without adding objects");
            yield return new WaitForSecondsRealtime(.9f);
            Check(sparks.ActiveSparkCount == 0, "Celebration clears after 850ms");
            game.HomeScreen.Celebrate(); state.reducedMotion = true; yield return null; yield return null;
            Check(sparks.ActiveSparkCount == 0, "Reduced Motion cancels an active celebration");
            game.HomeScreen.Celebrate(); Check(sparks.ActiveSparkCount == 0, "Reduced Motion suppresses future decorative stars");
            state.reducedMotion = false;
        }
        IEnumerator HtmlTapChecks()
        {
            var state = game.Engine.State; state.reducedMotion = false; state.sound = state.music = false;
            game.View.Switch("Home"); yield return new WaitForSecondsRealtime(.3f);
            var pool = game.HomeScreen.GetComponentsInChildren<RectTransform>(true).Where(x => x.name.StartsWith("Cash Popup ")).ToArray();
            foreach (var item in pool) item.gameObject.SetActive(false);
            // TMP can lazily add glyph submesh renderers; those are rendering internals, not effect hosts.
            int count = game.HomeScreen.GetComponentsInChildren<Transform>(true).Count(x => x.GetComponent<TMPro.TMP_SubMeshUI>() == null);
            game.HomeScreen.TapFeedback();
            var active = pool.Single(x => x.gameObject.activeSelf);
            var origin = active.anchoredPosition;
            Check(active.GetComponentInChildren<TMPro.TMP_Text>().text == "+₹" + game.Engine.TapIncome &&
                  !active.GetComponentInChildren<TMPro.TMP_Text>().raycastTarget,
                "Every tap immediately shows its reward without blocking input");
            yield return new WaitForSecondsRealtime(.25f);
            Check(active.anchoredPosition.y > origin.y &&
                  active.anchoredPosition.y <= origin.y + 12 * UiEasing.Pixels(active) &&
                  active.anchoredPosition.x == origin.x && active.localScale == Vector3.one,
                "Tap earnings rise briefly within the counter lane");
            Check(active.GetComponentInChildren<TMPro.TMP_Text>().color.a >= .99f,
                "Tap earnings remain readable during the rise");
            yield return Capture("08-tap-mid", 0);
            yield return new WaitForSecondsRealtime(.35f);
            Check(active.gameObject.activeSelf && active.GetComponentInChildren<TMPro.TMP_Text>().color.a > 0 &&
                  active.GetComponentInChildren<TMPro.TMP_Text>().color.a < 1,
                "Tap earnings fade only near the end of feedback");
            yield return new WaitForSecondsRealtime(.15f);
            Check(pool.All(x => !x.gameObject.activeSelf), "Tap earnings disappear promptly after feedback");
            for (int i = 0; i < 40; i++) game.HomeScreen.TapFeedback();
            Check(pool.Length == 12 && pool.Count(x => x.gameObject.activeSelf) == 1 &&
                  pool.Single(x => x.gameObject.activeSelf).GetComponentInChildren<TMPro.TMP_Text>().text ==
                      "+₹" + game.Engine.TapIncome * 40 &&
                  game.HomeScreen.GetComponentsInChildren<Transform>(true).Count(x => x.GetComponent<TMPro.TMP_SubMeshUI>() == null) == count,
                "Rapid taps aggregate in one authored label without growing the hierarchy");
            state.reducedMotion = true; yield return null; yield return null;
            Check(pool.All(x => !x.gameObject.activeSelf), "Enabling Reduced Motion cancels flying tap labels");
            game.HomeScreen.TapFeedback(); active = pool.Single(x => x.gameObject.activeSelf); origin = active.anchoredPosition;
            yield return new WaitForSecondsRealtime(.3f);
            Check(active.anchoredPosition == origin && active.GetComponentInChildren<TMPro.TMP_Text>().color.a == 1,
                "Reduced Motion keeps tap income readable without moving or fading");
            yield return new WaitForSecondsRealtime(.4f);
            Check(pool.All(x => !x.gameObject.activeSelf), "Static reduced-motion tap labels hide after 650ms");
            state.reducedMotion = false;
        }
        IEnumerator HtmlMeterChecks()
        {
            var state = game.Engine.State; state.reducedMotion = false;
            state.products[0].level = 13; game.View.ShowProductDetail(0);
            var progress = game.ProductDetailScreen.GetComponentInChildren<UiProgress>(true);
            Check(progress.Value < .01f, "Detail progress reveal starts from zero");
            yield return new WaitForSecondsRealtime(.18f);
            Check(progress.Value > .1f && progress.Value < .5f, "Progress is between endpoints halfway through its 360ms transition");
            yield return new WaitForSecondsRealtime(.22f);
            Check(Mathf.Abs(progress.Value - .5f) < .001f, "Periodic refresh does not restart the progress transition");
            state.products[0].level = 20; game.ProductDetailScreen.Refresh();
            yield return new WaitForSecondsRealtime(.08f);
            state.products[0].level = 21; game.ProductDetailScreen.Refresh();
            yield return new WaitForSecondsRealtime(.4f);
            Check(Mathf.Abs(progress.Value - 20f / 24) < .001f, "Rapid progress retargeting finishes at the latest authoritative value");
            state.products[0].level = 24; game.ProductDetailScreen.Refresh(); state.reducedMotion = true;
            yield return null; yield return null;
            Check(Mathf.Abs(progress.Value - 23f / 24) < .001f, "Reduced Motion snaps an active progress transition");
            game.ProductDetailScreen.Close(); game.View.Switch("Products"); game.ProductsScreen.Refresh();
            var meters = game.ProductsScreen.GetComponentsInChildren<UiProgress>(true);
            Check(meters.Length == 8 && Mathf.Abs(meters[0].Value - 23f / 24) < .001f, "All eight product rows have saved, bound progress fills");
            game.View.Switch("Home"); state.reducedMotion = false;
            state.upgrades.Add("skill-0-0"); state.skills[0].activeUntil = 0;
            double duration = game.Engine.Data.skills[0].cooldown * (1 - game.Engine.Data.legacy[11].valuePerRank * state.Legacy(11));
            state.skills[0].readyAt = state.now + duration * .5;
            yield return null; yield return null;
            var cover = game.HomeScreen.GetComponentsInChildren<UiCooldown>(true)[0];
            Check(Mathf.Abs(cover.Remaining - .5f) < .01f && !cover.raycastTarget, "Cooldown shows the authoritative remaining half and cannot intercept taps");
            state.skills[0].readyAt = 0; game.HomeScreen.Refresh(); yield return null; yield return null;
            Check(cover.Remaining == 0, "Ready skill clears the radial cover");
            yield return new WaitForSecondsRealtime(.4f);
            var skill = game.HomeScreen.GetComponentsInChildren<Button>(true).First(x => x.name == "home-skill-0");
            var feedback = skill.GetComponent<UiFeedback>();
            var pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left, pointerId = -1 };
            feedback.OnPointerDown(pointer); yield return new WaitForSecondsRealtime(.16f);
            Check(Mathf.Abs(feedback.Target.localScale.x - .93f) < .002f && skill.transform.localScale == Vector3.one,
                "HTML skill press scales only its disc to .93 in 140ms");
            feedback.OnPointerExit(pointer); yield return new WaitForSecondsRealtime(.16f);
            Check(feedback.Target.localScale == Vector3.one, "Pointer exit restores skill disc");
            var ring = skill.GetComponentInChildren<UiSkillRing>();
            state.skills[0].activeUntil = state.now + 30; game.HomeScreen.Refresh();
            yield return new WaitForSecondsRealtime(.2f);
            Check(ring.color == (Color)new Color32(146, 35, 73, 255), "Skill active ring completes its 180ms state-color transition");
            state.skills[0].activeUntil = 0; game.HomeScreen.Refresh(); state.reducedMotion = true;
            yield return null; yield return null;
            Check(ring.color == (Color)new Color32(24, 68, 119, 255), "Reduced Motion settles the skill ring immediately");
            state.reducedMotion = false;
        }
    }
}
#endif
