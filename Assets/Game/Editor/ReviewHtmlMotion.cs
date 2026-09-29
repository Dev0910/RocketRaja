using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.EditorTools
{
    public static class ReviewHtmlMotion
    {
        public static string Transitions()
        {
            var checks = new List<string>();
            void Check(bool condition, string description)
            { if (!condition) throw new InvalidOperationException(description); checks.Add("PASS " + description); }
            foreach (var path in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Game/ImportedUI" })
                .Select(AssetDatabase.GUIDToAssetPath).Where(p => p.EndsWith(" Screen.prefab")))
            {
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    Check(root.GetComponentsInChildren<UiMotion>(true).Length > 0, root.name + " has a transition profile");
                    Check(root.GetComponentsInChildren<Graphic>(true).All(x => x.GetComponent<CanvasRenderer>() != null), root.name + " every saved graphic has a renderer");
                    Check(root.GetComponentsInChildren<Button>(true).All(x => x.GetComponent<UiFeedback>() != null && x.GetComponent<UiFeedback>().Target != null), root.name + " all controls have saved feedback references");
                    var home = root.GetComponent<ImportedHomeScreen>();
                    if (home != null)
                    {
                        Check(root.GetComponentsInChildren<FestivalSparkles>(true).Length == 1 && root.GetComponentsInChildren<ShopAmbient>(true).Length == 1, "Home effect hosts are preauthored exactly once");
                        Check(root.GetComponentsInChildren<UiCooldown>(true).Length == 3 && root.GetComponentsInChildren<UiSkillRing>(true).Length == 3, "Three saved cooldown overlays and state rings");
                        var keeper = root.GetComponentsInChildren<RectTransform>(true).Single(x => x.name == "Shopkeeper");
                        Check(keeper.anchoredPosition == Vector2.zero && keeper.sizeDelta == Vector2.zero, "Keeper preserves the protected stretched illustration offsets");
                    }
                    foreach (var m in root.GetComponentsInChildren<UiMotion>(true))
                    {
                        var group = m.GetComponent<CanvasGroup>();
                        Check(group != null && m.Surface != null, root.name + "/" + m.name + " saved motion references");
                        var position = m.Surface.anchoredPosition; var scale = m.Surface.localScale;
                        float expected = m.Profile == UiMotion.Kind.Modal ? .25f : m.Profile == UiMotion.Kind.Quantity ? .17f : m.Profile == UiMotion.Kind.Notice ? .18f : .23f;
                        Check(Mathf.Abs(m.Duration(true) - expected) < .00001f && m.Duration(false) == .16f, m.name + " HTML timings");
                        m.Sample(true, 0);
                        Check(group.alpha < .001f, m.name + " opens from transparent");
                        float pixels = UiEasing.Pixels(m.Surface);
                        float distance = m.Profile == UiMotion.Kind.Quantity ? 7 : m.Profile == UiMotion.Kind.Notice ? 5 : 16;
                        Check(Mathf.Abs(Vector2.Distance(position, m.Surface.anchoredPosition) / pixels - distance) < .01f, m.name + " HTML travel distance");
                        m.Sample(true, expected);
                        Check(group.alpha > .999f && Vector2.Distance(position, m.Surface.anchoredPosition) < .001f && Vector3.Distance(scale, m.Surface.localScale) < .001f, m.name + " opening restores authored geometry");
                        if (m.Profile == UiMotion.Kind.Panel)
                        {
                            m.Sample(true, 0, -1);
                            Check(Mathf.Abs((m.Surface.anchoredPosition.x - position.x) / pixels + 18) < .01f && m.Surface.localScale == scale, m.name + " directional panel has no scaling");
                        }
                        m.Sample(false, .16f);
                        Check(group.alpha < .001f, m.name + " exit becomes transparent");
                        m.Sample(true, expected);
                    }
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            Directory.CreateDirectory("Documentation/AnimationValidation/HtmlMotion");
            File.WriteAllLines("Documentation/AnimationValidation/HtmlMotion/Transitions.txt", checks);
            return checks.Count + " transition trajectory/reference checks passed.";
        }
    }
}
