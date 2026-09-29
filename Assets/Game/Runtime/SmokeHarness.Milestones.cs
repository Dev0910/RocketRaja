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
        IEnumerator MilestoneChecks()
        {
            var state = game.Engine.State;
            state.sound = state.music = false;
            foreach (bool reduced in new[] { true, false })
            {
                state.reducedMotion = reduced;
                game.View.Switch("Products");
                yield return new WaitForSecondsRealtime(.4f);
                for (int product = 0; product < 8; product++)
                {
                    game.View.ShowProductDetail(product);
                    yield return new WaitForSecondsRealtime(.4f);
                    var detail = game.ProductDetailScreen;
                    var fill = detail.GetComponentInChildren<UiProgress>(true);
                    foreach (int level in new[] { 0, 1, 24, 25, 49, 50, 75, 99, 100, 199, 200, Catalog.MaxLevel })
                    {
                        state.products[product].level = level;
                        detail.Refresh(); game.ProductsScreen.Refresh();
                        if (ProductMilestones.Progress(level) == 0 || level == Catalog.MaxLevel)
                            Check(Mathf.Approximately(fill.Value, ProductMilestones.Progress(level)), "Immediate interval reset product " + product + " level " + level + " reduced " + reduced);
                        if (ProductMilestones.Progress(level) > 0 && level < Catalog.MaxLevel)
                            yield return new WaitForSecondsRealtime(.4f);
                        Check(Mathf.Abs(fill.Value - ProductMilestones.Progress(level)) < .001f, "Detail interval product " + product + " level " + level);
                        var row = game.ProductsScreen.GetComponentsInChildren<UiProgress>(true)[product];
                        Check(Mathf.Abs(row.Value - ProductMilestones.Progress(level)) < .001f, "Products interval matches detail " + product + "/" + level);
                    }
                    detail.Close();
                    yield return new WaitForSecondsRealtime(.2f);
                }
            }
            state.reducedMotion = true;
            foreach (var size in new[] { new Vector2Int(360, 640), new Vector2Int(450, 1000) })
            {
                ResizeForReview(size.x, size.y);
                state.products[0].level = 75;
                game.View.Switch("Products");
                yield return Capture("milestones-products-" + size.x);
                game.View.ShowProductDetail(0);
                yield return Capture("milestones-detail-" + size.x);
                Check(game.ModalPaused, "Product detail blocks gameplay " + size);
                game.ProductDetailScreen.GetComponentsInChildren<Button>(true).Single(x => x.name == "back-ProductDetail").onClick.Invoke();
                Check(!game.ProductDetailScreen.gameObject.activeSelf && game.ProductsScreen.gameObject.activeSelf, "Return to Products " + size);
            }
        }
    }
}
#endif
