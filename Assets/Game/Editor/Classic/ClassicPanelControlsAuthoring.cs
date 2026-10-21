using System;
using System.IO;
using System.Linq;
using RocketRaja.Classic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    /// <summary>Saved native equivalents of the reviewed Shop cleanup and four collapse icons.</summary>
    public static class ClassicPanelControlsAuthoring
    {
        const string IconPath = "Assets/Game/ClassicUI/HTMLSync/collapse-chevron.png";

        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required");
            var scene = SceneManager.GetSceneByPath(BuildClassicUI.ScenePath);
            if (!scene.isLoaded || scene.isDirty) throw new InvalidOperationException("Open saved Main first");
            var ui = scene.GetRootGameObjects().Single(g => g.name == BuildClassicUI.RootName).GetComponent<ClassicUI>();
            Undo.RegisterFullObjectHierarchyUndo(ui.gameObject, "Refine panel controls");
            ApplyToUI(ui);
            Canvas.ForceUpdateCanvases(); ui.GetComponent<ClassicLayout>().Apply();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        }

        public static void ApplyToUI(ClassicUI ui)
        {
            foreach (var panel in ui.panels.Where(v => v.route != "Home"))
                Configure(panel.root.transform.Find("Header/Collapse").GetComponent<Button>());
            var shop = ui.panels.Single(v => v.route == "Shop").scroll.content;
            foreach (var path in new[] { "Balance/Collection", "Card draws", "Odds" })
            {
                var retired = shop.Find(path);
                if (retired) retired.gameObject.SetActive(false);
            }
        }

        public static void Configure(Button button)
        {
            var rect = (RectTransform)button.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1, .5f); rect.pivot = new Vector2(1, .5f);
            rect.sizeDelta = new Vector2(48, 48); rect.anchoredPosition = new Vector2(-8, 0);
            var rootImage = button.GetComponent<Image>(); rootImage.sprite = null; rootImage.color = Color.clear;
            rootImage.raycastTarget = true; button.targetGraphic = rootImage;
            foreach (var label in button.GetComponentsInChildren<TMPro.TMP_Text>(true)) label.gameObject.SetActive(false);
            var icon = rect.Find("Collapse chevron") as RectTransform;
            if (!icon)
            {
                var go = new GameObject("Collapse chevron", typeof(RectTransform), typeof(Image));
                Undo.RegisterCreatedObjectUndo(go, "Author collapse icon");
                icon = (RectTransform)go.transform; icon.SetParent(rect, false);
            }
            icon.anchorMin = icon.anchorMax = icon.pivot = new Vector2(.5f, .5f);
            icon.sizeDelta = new Vector2(36, 36); icon.anchoredPosition = Vector2.zero;
            var image = icon.GetComponent<Image>(); image.sprite = Icon(); image.color = Color.white;
            image.type = Image.Type.Simple; image.raycastTarget = false;
            var control = button.GetComponent<ClassicControl>(); control.htmlStyle = 3; control.motionTarget = icon;
        }

        static Sprite Icon()
        {
            var saved = AssetDatabase.LoadAssetAtPath<Sprite>(IconPath); if (saved) return saved;
            Directory.CreateDirectory(Path.GetDirectoryName(IconPath));
            const int size = 256; var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var ink = new Color32(22, 75, 128, 255); var gold = new Color32(198, 152, 67, 255);
            var cream = new Color32(255, 244, 219, 255);
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                var p = new Vector2((x + .5f - size / 2f) * 36 / size, (y + .5f - size / 2f) * 36 / size);
                float radius = p.magnitude;
                var color = Color.Lerp(gold, ink, Mathf.Clamp01((17 - radius) * size / 36f + .5f));
                float stroke = Mathf.Min(Distance(p, new Vector2(-7, 3), new Vector2(0, -4)), Distance(p, new Vector2(0, -4), new Vector2(7, 3)));
                color = Color.Lerp(color, cream, Mathf.Clamp01((1.375f - stroke) * size / 36f + .5f));
                color.a = Mathf.Clamp01((18 - radius) * size / 36f + .5f); texture.SetPixel(x, y, color);
            }
            texture.Apply(); File.WriteAllBytes(IconPath, texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(IconPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(IconPath);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false; importer.alphaIsTransparency = true; importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(IconPath);
        }

        static float Distance(Vector2 p, Vector2 a, Vector2 b)
        { var ab = b - a; return Vector2.Distance(p, a + ab * Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude)); }
    }
}
