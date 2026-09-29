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
        IEnumerator UnlockableChecks()
        {
            var e = game.Engine; var s = e.State;
            s.sound = s.music = false; s.reducedMotion = true;
            game.View.Switch("Player"); game.ModalPaused = true;
            var panel = game.PlayerScreen;
            var buttons = panel.GetComponentsInChildren<Button>(true);
            var buyAll = buttons.Single(x => x.name == "buy-all");
            var rows = panel.GetComponentsInChildren<Transform>(true).Where(x => x.name.StartsWith("Unlockable Row ")).ToArray();
            Check(!buttons.Any(x => x.name.Contains("Page")), "No pagination buttons remain");
            s.cash = 0; panel.Refresh();
            Check(!buyAll.interactable && rows.Count(x => x.gameObject.activeSelf) == 10, "Ten remaining rows with unaffordable Buy All disabled");
            s.cash = 1e30; panel.Refresh();
            var first = e.Data.upgrades.First();
            rows[0].GetComponentInChildren<Button>().onClick.Invoke();
            Check(s.upgrades.Contains(first.id) && rows[0].GetComponentsInChildren<TMP_Text>().Single(x => x.name == "Name").text == e.Data.upgrades.First(x => !s.upgrades.Contains(x.id)).name, "Individual purchase replaces row with next cheapest");
            e.MutationAllowed = () => false; panel.Refresh();
            Check(!buyAll.interactable && !rows.Any(x => x.GetComponentInChildren<Button>().interactable), "Read-only session disables all upgrade buttons");
            e.MutationAllowed = null; panel.Refresh(); buyAll.onClick.Invoke();
            Check(s.upgrades.Count == 84 && rows.Count(x => x.gameObject.activeSelf) == 10 && !buyAll.interactable, "Buy All reaches beyond visible ten and preserves skill gates");
            for (int i = 0; i < 3; i++) s.products[i].level = 1;
            panel.Refresh(); buyAll.onClick.Invoke();
            Check(s.upgrades.Count == 96 && rows.All(x => !x.gameObject.activeSelf) && !buyAll.interactable, "Completion hides rows and disables Buy All");
            Check(panel.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "unlockable-count").text == "All upgrades purchased", "Completion text shown");
            foreach (var size in new[] { new Vector2Int(360, 640), new Vector2Int(450, 1000) })
            {
                ResizeForReview(size.x, size.y); s.upgrades.Clear(); panel.Refresh();
                yield return new WaitForSecondsRealtime(.3f);
                var scroll = panel.GetComponentInChildren<ScrollRect>(true);
                scroll.verticalNormalizedPosition = .7f;
                yield return Capture("unlockables-" + size.x);
                var corners = new Vector3[4]; ((RectTransform)buyAll.transform).GetWorldCorners(corners);
                Check(corners[2].y - corners[0].y >= 44, "Buy All touch height at " + size);
                buttons.Single(x => x.name == "buy-quantity").onClick.Invoke();
                Check(panel.GetComponentsInChildren<Transform>(true).Single(x => x.name == "Quantity Menu").gameObject.activeSelf, "Quantity menu preserved");
                buttons.Single(x => x.name == "Quantity Buy ×10").onClick.Invoke();
                yield return Capture("unlockables-quantity-" + size.x);
            }
            game.ModalPaused = false;
        }
    }
}
#endif
