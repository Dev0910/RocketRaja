using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace RocketRaja.Editor
{
    public static partial class BuildRedesignUI
    {
        [Serializable] sealed class ShopArtManifest { public string source, sha256; public int width, height; public ShopArtCell[] cells; public ShopArtKit[] kits; }
        [Serializable] sealed class ShopArtCell { public string id; public ArtRegion rect; }
        [Serializable] sealed class ShopArtKit { public string id; public float x, y, w, h; }
        public static void AddShopLayers()
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling) throw new InvalidOperationException("Require compiled Edit Mode");
            LoadAssets();
            var manifest = JsonUtility.FromJson<ShopArtManifest>(File.ReadAllText("UI Builder/HTML/Redesign/shop-stages.json"));
            using (var hash = SHA256.Create())
                if (BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(manifest.source))).Replace("-", "").ToLowerInvariant() != manifest.sha256) throw new InvalidOperationException("Decoration art differs from its reviewed manifest");
            var importer = (TextureImporter)AssetImporter.GetAtPath(manifest.source);
            if (!importer) throw new InvalidOperationException("Import reviewed decoration atlas before authoring");
            importer.mipmapEnabled = false; importer.alphaIsTransparency = true; importer.maxTextureSize = 2048; importer.npotScale = TextureImporterNPOTScale.None; importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(manifest.source);
            if (texture.width != manifest.width || texture.height != manifest.height) throw new InvalidOperationException("Unexpected decoration texture size");
            var sprites = manifest.cells.Select(cell =>
            {
                var r = cell.rect; string path = "Assets/Game/RedesignUI/Art/Decoration " + cell.id + ".asset";
                var rect = new Rect(r.x, manifest.height - r.yFromTop - r.height, r.width, r.height);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite && (sprite.texture != texture || sprite.rect != rect)) throw new InvalidOperationException("Reviewed decoration crop changed: " + cell.id);
                if (!sprite) { sprite = Sprite.Create(texture, rect, new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect); sprite.name = cell.id; AssetDatabase.CreateAsset(sprite, path); }
                return sprite;
            }).ToArray();
            AssetDatabase.SaveAssets(); var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var home = root.GetComponent<RedesignScreenRouter>().home;
                var ui = home.shopVisuals ? home.shopVisuals : home.gameObject.AddComponent<RedesignShopVisuals>(); home.shopVisuals = ui;
                var stall = home.counter.transform.parent;
                var layerRoot = stall.Find("Permanent shop decorations") as RectTransform;
                if (!layerRoot) { layerRoot = Fill("Permanent shop decorations", stall); layerRoot.SetSiblingIndex(1); var group = layerRoot.gameObject.AddComponent<CanvasGroup>(); group.blocksRaycasts = false; group.interactable = false; }
                ui.layers = new GameObject[5];
                for (int i = 0; i < 5; i++)
                {
                    string name = "Shop stage " + (i + 1); var layer = layerRoot.Find(name) as RectTransform;
                    if (!layer) layer = Fill(name, layerRoot);
                    ui.layers[i] = layer.gameObject;
                }
                for (int k = 0; k < manifest.kits.Length; k++)
                {
                    var kit = manifest.kits[k]; int layerIndex = k == 0 ? 0 : k <= 2 ? 1 : k - 1;
                    var parent = ui.layers[layerIndex].transform; var rect = parent.Find(kit.id) as RectTransform;
                    if (!rect) rect = New(kit.id, parent);
                    rect.anchorMin = new Vector2(kit.x, 1 - kit.y - kit.h); rect.anchorMax = new Vector2(kit.x + kit.w, 1 - kit.y); rect.offsetMin = rect.offsetMax = Vector2.zero;
                    var image = rect.GetComponent<UnityEngine.UI.Image>(); if (!image) image = Image(rect, Color.white);
                    image.sprite = sprites[Array.FindIndex(manifest.cells, c => c.id == (kit.id.StartsWith("lantern") ? "lantern" : kit.id))]; image.raycastTarget = false; image.preserveAspect = true;
                }
                foreach (var layer in ui.layers) layer.SetActive(false);
                if (!ui.progress)
                {
                    var panel = Panel(home.wallet.transform.parent, "Permanent shop progress", "");
                    panel.SetSiblingIndex(home.wallet.transform.GetSiblingIndex());
                    ui.progress = FlowText(panel, "shop-progress", "Shop 1/6 · Opening stall\n0 unique purchases · Next: Marigold welcome at 100\nDecorations stay after Prestige. Product displays rebuild.", 13);
                }
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            Debug.Log("REDESIGN_SHOP_LAYERS_SAVED 5");
        }
    }
}
