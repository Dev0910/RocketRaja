using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.Android;

// EDM4U can regenerate unescaped file URLs when the project path contains spaces.
public sealed class FirebaseRepositoryUriFix : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => int.MaxValue;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        RemoveDuplicateFirebaseUnityAars(Path.Combine(path, "build.gradle"));

        var settingsPath = Path.Combine(path, "settings.gradle");
        if (!File.Exists(settingsPath))
            settingsPath = Path.Combine(Path.GetDirectoryName(path), "settings.gradle");
        if (!File.Exists(settingsPath))
            return;

        var original = File.ReadAllText(settingsPath);
        const string declaration = "def unityProjectPath = ";
        var lines = original.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].TrimStart().StartsWith(declaration) &&
                lines[i].Contains("file:///") &&
                !lines[i].Contains(".replace(\" \", \"%20\")"))
            {
                lines[i] = lines[i].TrimEnd('\r') + ".replace(\" \", \"%20\")";
            }
        }

        var updated = string.Join("\n", lines);
        if (updated != original)
            File.WriteAllText(settingsPath, updated);
    }

    static void RemoveDuplicateFirebaseUnityAars(string gradlePath)
    {
        if (!File.Exists(gradlePath))
            return;

        var original = File.ReadAllText(gradlePath);
        var maven = new HashSet<string>(System.StringComparer.Ordinal);
        foreach (Match match in Regex.Matches(original,
                     @"(?m)^[ \t]*implementation[ \t]+['""]com\.google\.firebase:(?<artifact>firebase-[\w-]+-unity):(?<version>\d+(?:\.\d+)+)['""]"))
        {
            maven.Add(match.Groups["artifact"].Value + "-" + match.Groups["version"].Value);
        }

        if (maven.Count == 0)
            return;

        // EDM4U supplies these AARs through Maven. If Unity also imports a local
        // copy, Gradle sees two libraries with the same classes and manifest.
        var updated = Regex.Replace(original,
            @"(?m)^[ \t]*implementation[ \t]*\([ \t]*name[ \t]*:[ \t]*['""](?<name>firebase-[\w-]+-unity-\d+(?:\.\d+)+)['""][ \t]*,[ \t]*ext[ \t]*:[ \t]*['""]aar['""][ \t]*\)[ \t]*(?:\r?\n|$)",
            match => maven.Contains(match.Groups["name"].Value) ? string.Empty : match.Value);
        if (updated != original)
            File.WriteAllText(gradlePath, updated);
    }
}
