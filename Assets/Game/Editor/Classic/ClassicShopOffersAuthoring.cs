using System;
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
        static void BuildShopOffers(RectTransform content)
        {
            var cash = Row("24-hour cash offer", content, 108);
            Label("", Region("Title", cash, .04f, .64f, .68f, .94f), "24 hours of cash", 15, true);
            Label("shop-day-cash", Region("Reward", cash, .04f, .08f, .68f, .64f), "Current automatic income × 24 hours", 12);
            Button(Region("Buy", cash, .71f, .15f, .97f, .85f), "BuyShopCash", "", "shop-cash-cost", "100 KK\nBuy cash");
            var profit = Row("24-hour double profits offer", content, 108);
            Label("", Region("Title", profit, .04f, .64f, .68f, .94f), "Double your profits", 15, true);
            Label("shop-profit-detail", Region("Duration", profit, .04f, .08f, .68f, .64f), "2× product and tap earnings\nFor the next 24 hours", 12);
            Button(Region("Buy", profit, .71f, .15f, .97f, .85f), "BuyShopProfit", "", "shop-profit-cost", "150 KK\nBuy boost");
        }

        [MenuItem("Rocket Raja/Classic/Author KK shop offers")]
        public static string AuthorShopOffers()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required");
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                ui = scene.GetRootGameObjects().Single(g => g.name == RootName).GetComponent<ClassicUI>();
                Undo.RegisterFullObjectHierarchyUndo(ui.gameObject, "Author KK shop offers");
                var content = (RectTransform)ui.panels.Single(p => p.route == "Shop").root.transform.Find("Scroll/Viewport/Content");
                if (!content) throw new InvalidOperationException("Missing Shop content");
                var oldCash = content.Find("24-hour cash offer");
                var oldProfit = content.Find("24-hour double profits offer");
                if (oldCash) Undo.DestroyObjectImmediate(oldCash.gameObject);
                if (oldProfit) Undo.DestroyObjectImmediate(oldProfit.gameObject);
                body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Body SDF.asset");
                bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Bold SDF.asset");
                blue = ui.blueButton; ivory = ui.ivoryButton; row = Sprite("Ivory Row Frame");
                texts.Clear();
                BuildShopOffers(content);
                var firstPack = content.GetComponentsInChildren<ClassicControl>(true).First(c => c.action == "Purchase");
                Transform packRow = firstPack.transform;
                while (packRow.parent != content) packRow = packRow.parent;
                int insert = packRow.GetSiblingIndex();
                content.Find("24-hour cash offer").SetSiblingIndex(insert);
                content.Find("24-hour double profits offer").SetSiblingIndex(insert + 1);
                ui.textBindings = ui.textBindings.Where(b => !b.key.StartsWith("shop-day-") && b.key != "shop-profit-detail" && b.key != "shop-cash-cost" && b.key != "shop-profit-cost").Concat(texts).ToArray();
                // Reuse the existing token and pre-authored TMP submesh slots.
                var currency = ui.GetComponent<ClassicCurrencyPresentation>();
                if (currency)
                {
                    var costs = texts.Where(t => t.key.EndsWith("-cost")).Select(t => t.text).ToArray();
                    foreach (var label in costs)
                    {
                        label.richText = true; label.spriteAsset = currency.token;
                        var text = (RedesignSavedText)label;
                        var slots = text.savedMaterialSlots.ToList();
                        for (int i = slots.Count; i < 4; i++)
                        {
                            var rect = Fill("Saved currency material " + (i + 1), text.transform);
                            rect.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
                            var slot = rect.gameObject.AddComponent<RedesignSavedSubMesh>();
                            var serialized = new SerializedObject(slot);
                            serialized.FindProperty("m_TextComponent").objectReferenceValue = text;
                            serialized.FindProperty("m_materialReferenceIndex").intValue = i + 1;
                            serialized.ApplyModifiedPropertiesWithoutUndo();
                            slot.raycastTarget = false; slot.fontAsset = text.font; slot.spriteAsset = currency.token;
                            slot.sharedMaterial = currency.token.material; slot.isDefaultMaterial = true;
                            slots.Add(slot);
                        }
                        text.savedMaterialSlots = slots.ToArray(); text.BindSavedSlots();
                    }
                    currency.labels = currency.labels.Where(t => t).Concat(costs).Distinct().ToArray();
                    EditorUtility.SetDirty(currency);
                }
                EditorUtility.SetDirty(ui);
                Canvas.ForceUpdateCanvases();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                return "Saved two KK offers before IAP rows, with live rewards, prices and boost countdown.";
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }
    }
}
