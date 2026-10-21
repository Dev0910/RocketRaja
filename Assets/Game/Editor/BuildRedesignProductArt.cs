using System;
using System.IO;
using System.Security.Cryptography;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace RocketRaja.Editor
{
    public static partial class BuildRedesignUI
    {
        public const string ProductArtPath = "Assets/Game/RedesignUI/Product Appearances.asset";
        [Serializable] sealed class ArtManifest { public ArtFamily[] products; }
        [Serializable] sealed class ArtFamily { public string id, source, sha256; public int width, height; public ArtCell[] cells; }
        [Serializable] sealed class ArtCell { public int tier; public ArtRegion rect; }
        [Serializable] sealed class ArtRegion { public int x, yFromTop, width, height; }
        public static void AddProductAppearances()
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling) throw new InvalidOperationException("Require compiled Edit Mode");
            LoadAssets();
            var manifest = JsonUtility.FromJson<ArtManifest>(File.ReadAllText("UI Builder/HTML/Redesign/product-appearances.json"));
            if (manifest.products.Length != 8) throw new InvalidOperationException("Review all eight source families first");
            if (!AssetDatabase.IsValidFolder("Assets/Game/RedesignUI/Product Sprites")) AssetDatabase.CreateFolder("Assets/Game/RedesignUI", "Product Sprites");
            var art = AssetDatabase.LoadAssetAtPath<RedesignProductArt>(ProductArtPath);
            if (!art) { art = ScriptableObject.CreateInstance<RedesignProductArt>(); AssetDatabase.CreateAsset(art, ProductArtPath); }
            for (int product = 0; product < 8; product++)
            {
                var family = manifest.products[product];
                if (family.id != Core.Catalog.ProductIds[product] || family.cells.Length != 4) throw new InvalidOperationException("Appearance identity mismatch");
                using (var hash = SHA256.Create())
                    if (BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(family.source))).Replace("-", "").ToLowerInvariant() != family.sha256) throw new InvalidOperationException("Source art changed after review: " + family.id);
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(family.source);
                if (!texture) throw new InvalidOperationException("Missing reviewed atlas: " + family.source);
                float sx = texture.width / (float)family.width, sy = texture.height / (float)family.height;
                for (int tier = 0; tier < 4; tier++)
                {
                    var cell = family.cells[tier]; var r = cell.rect;
                    if (cell.tier != tier || r.width <= 0 || r.height <= 0 || r.x < 0 || r.yFromTop < 0 || r.x + r.width > family.width || r.yFromTop + r.height > family.height) throw new InvalidOperationException("Invalid reviewed sprite region");
                    string path = "Assets/Game/RedesignUI/Product Sprites/" + family.id + "-" + tier + ".asset";
                    var rect = new Rect(r.x * sx, (family.height - r.yFromTop - r.height) * sy, r.width * sx, r.height * sy);
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sprite && (sprite.texture != texture || sprite.rect != rect)) throw new InvalidOperationException("Existing sprite differs from reviewed mapping; apply an explicit art revision");
                    if (!sprite) { sprite = Sprite.Create(texture, rect, new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect); sprite.name = family.id + "-" + tier; AssetDatabase.CreateAsset(sprite, path); }
                    art.appearances[product * 4 + tier] = sprite;
                }
            }
            EditorUtility.SetDirty(art); AssetDatabase.SaveAssets();
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var router = root.GetComponent<RedesignScreenRouter>(); router.home.productArt = art; router.productDetails.productArt = art;
                ApplyTalentArt(root);
                if (!router.productDetails.appearance)
                {
                    var description = FlowText(router.productDetails.productImage.transform.parent, "product-appearance", "Current display: Basic", 13);
                    description.transform.SetSiblingIndex(router.productDetails.productImage.transform.GetSiblingIndex() + 1);
                    router.productDetails.appearance = description;
                }
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            Debug.Log("REDESIGN_PRODUCT_APPEARANCES_SAVED 32");
        }
    }
}
