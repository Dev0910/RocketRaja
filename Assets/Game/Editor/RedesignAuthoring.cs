using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RocketRaja.Core;
using UnityEditor;
using UnityEngine;

namespace RocketRaja.Editor
{
    public static class RedesignAuthoring
    {
        public static RedesignContent Import() => RedesignContentImporter.Import(
            File.ReadAllText("Game Design/1000 Unlockables Catalogue.md"),
            File.ReadAllText("Game Design/Talent and Prestige Draft.md"));
        [MenuItem("Rocket Raja/Redesign/Regenerate Content")]
        public static void Regenerate()
        {
            var data = Import();
            Directory.CreateDirectory("Assets/Resources");
            Directory.CreateDirectory("Documentation");
            UpsertAsset("Assets/Resources/GameContent.asset", data);
            string expected = JsonUtility.ToJson(data, true);
            string export = "Documentation/RedesignContentCatalog.json";
            if (!File.Exists(export) || File.ReadAllText(export) != expected + "\n") File.WriteAllText(export, expected + "\n");
            string manifestPath = "Assets/Resources/RedesignContentManifest.json";
            string manifest = JsonUtility.ToJson(new RedesignContentManifest { sourceHash = data.sourceHash, economyVersion = RedesignCatalog.EconomyVersion, contentVersion = RedesignCatalog.ContentVersion }, true) + "\n";
            if (!File.Exists(manifestPath) || File.ReadAllText(manifestPath) != manifest) { File.WriteAllText(manifestPath, manifest); AssetDatabase.ImportAsset(manifestPath); }
            Validate();
        }
        internal static GameContent UpsertAsset(string assetPath, RedesignContent data)
        {
            // Validate before creating or replacing a serialized asset. There is no old
            // catalogue fallback and no Legacy/skill repair in the redesign authoring path.
            RedesignCatalog.Validate(data);
            var asset = AssetDatabase.LoadAssetAtPath<GameContent>(assetPath);
            if (asset == null)
            {
                if (File.Exists(assetPath)) throw new InvalidOperationException("Content path holds an incompatible asset; preserve and inspect it before regenerating.");
                asset = ScriptableObject.CreateInstance<GameContent>();
                asset.redesign = data;
                AssetDatabase.CreateAsset(asset, assetPath);
                AssetDatabase.SaveAssetIfDirty(asset);
            }
            else if (!Equivalent(asset.redesign, data))
            {
                Undo.RecordObject(asset, "Regenerate redesign catalogue");
                asset.redesign = data; EditorUtility.SetDirty(asset); AssetDatabase.SaveAssetIfDirty(asset);
            }
            return asset;
        }
        [MenuItem("Rocket Raja/Redesign/Validate Content And Isolation")]
        public static void Validate()
        {
            var checks = new List<string>();
            void Check(bool value, string name) { if (!value) throw new InvalidOperationException(name); checks.Add("PASS " + name); }
            var asset = AssetDatabase.LoadAssetAtPath<GameContent>("Assets/Resources/GameContent.asset");
            Check(asset != null, "Authored GameContent exists");
            RequireSourceMatch(asset.redesign, Import());
            Check(true, "Serialized content matches source (numeric tolerance 1e-14; authored price strings exact)");
            var exported = JsonUtility.FromJson<RedesignContent>(File.ReadAllText("Documentation/RedesignContentCatalog.json"));
            RedesignCatalog.Validate(exported);
            Check(Equivalent(exported, asset.redesign), "Simulator export matches Unity asset (numeric tolerance 1e-14)");
            var manifest = JsonUtility.FromJson<RedesignContentManifest>(File.ReadAllText("Assets/Resources/RedesignContentManifest.json"));
            Check(manifest.sourceHash == asset.redesign.sourceHash && manifest.economyVersion == RedesignCatalog.EconomyVersion && manifest.contentVersion == RedesignCatalog.ContentVersion, "Runtime manifest matches validated source version");
            var number = new Natural(System.Numerics.BigInteger.Pow(10, 100) + 1);
            Check(JsonUtility.FromJson<Natural>(JsonUtility.ToJson(number)).Value == number.Value, "Unity codec retains unbounded exact counters");
            Check((RedesignEnvironment.ProfileRoot == RedesignEnvironment.DefaultProfileRoot || RedesignEnvironment.ProfileRoot == RedesignEnvironment.SeparateProfileRoot) && !RedesignEnvironment.ProfileRoot.EndsWith("redesign-v2-disposable"), "Classic uses the selected persistent profile, never the disposable preview directory");
            Check(RedesignEnvironment.PreferenceKey("RocketRaja.IdentityMode") != "RocketRaja.IdentityMode", "Identity preferences isolated");
            var cloud = Cloud.RedesignCloudSettings.Load();
            Check(cloud != null && (!cloud.enabledAfterReview || cloud.MatchesApplication), "Enabled cloud configuration must match this application");
            Check(!EditorUtility.scriptCompilationFailed, "Unity reports no compilation failure");
            Directory.CreateDirectory("Documentation/Implementation");
            File.WriteAllLines("Documentation/Implementation/Unity-Content-Isolation.txt", checks);
            Debug.Log("REDESIGN_CONTENT_ISOLATION_OK " + checks.Count);
        }
        internal static void RequireSourceMatch(RedesignContent actual, RedesignContent expected)
        {
            RedesignCatalog.Validate(actual);
            if (!Equivalent(actual, expected)) throw new InvalidOperationException("Serialized redesign content is outdated. Regenerate it from the authored sources before building.");
        }
        static bool Equivalent(object a, object b)
        {
            if (a == null || b == null) return a == b;
            var type = a.GetType();
            if (type != b.GetType()) return false;
            if (a is double x) { double y = (double)b; return Math.Abs(x - y) <= Math.Max(1, Math.Max(Math.Abs(x), Math.Abs(y))) * 1e-14; }
            if (type.IsPrimitive || type.IsEnum || a is string) return a.Equals(b);
            if (a is Array aa) { var bb = (Array)b; return aa.Length == bb.Length && aa.Cast<object>().Zip(bb.Cast<object>(), Equivalent).All(v => v); }
            return type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).All(f => Equivalent(f.GetValue(a), f.GetValue(b)));
        }
    }
}
