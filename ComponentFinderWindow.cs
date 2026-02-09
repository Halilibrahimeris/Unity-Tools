using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Place this file into Assets/Editor/ (create the Editor folder if it doesn't exist).
// Window: Tools -> Component Finder

public class ComponentFinderWindow : EditorWindow
{
    MonoScript scriptAsset;
    Type targetType;

    enum SearchScope { OpenScenes, ActiveScene, ProjectPrefabs, SelectedFolder }
    SearchScope scope = SearchScope.OpenScenes;

    string selectedFolder = "Assets";

    Vector2 scrollPos;
    List<GameObject> results = new List<GameObject>();

    [MenuItem("Tools/Component Finder")]
    static void OpenWindow()
    {
        var w = GetWindow<ComponentFinderWindow>("Component Finder");
        w.minSize = new Vector2(420, 240);
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Component / Script Finder", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        scriptAsset = (MonoScript)EditorGUILayout.ObjectField("Script (drag .cs) :", scriptAsset, typeof(MonoScript), false);
        if (EditorGUI.EndChangeCheck())
        {
            if (scriptAsset != null) targetType = scriptAsset.GetClass();
            else targetType = null;
        }

        if (scriptAsset == null)
        {
            EditorGUILayout.HelpBox("Bir MonoBehaviour/Component scripti sürükleyin veya seçin.", MessageType.Info);
        }
        else if (targetType == null)
        {
            EditorGUILayout.HelpBox("Seçilen dosyadan bir Type alýnamadý. Script derlenmemiþ olabilir veya bir Component sýnýfý olmayabilir.", MessageType.Warning);
        }
        else if (!typeof(Component).IsAssignableFrom(targetType))
        {
            EditorGUILayout.HelpBox("Seçilen sýnýf UnityEngine.Component'ten türemiyor. Bu araç Component aramak için tasarlanmýþtýr.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.LabelField("Target Type:", targetType.FullName);
        }

        EditorGUILayout.Space();

        scope = (SearchScope)EditorGUILayout.EnumPopup("Search Scope:", scope);
        if (scope == SearchScope.SelectedFolder)
        {
            EditorGUILayout.BeginHorizontal();
            selectedFolder = EditorGUILayout.TextField(selectedFolder);
            if (GUILayout.Button("Pick", GUILayout.Width(40)))
            {
                var path = EditorUtility.OpenFolderPanel("Select folder (inside Assets)", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath)) selectedFolder = "Assets" + path.Substring(Application.dataPath.Length);
                    else EditorUtility.DisplayDialog("Invalid folder", "Please select a folder inside the project's Assets folder.", "OK");
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Find", GUILayout.Height(30)))
        {
            DoFind();
        }
        if (GUILayout.Button("Clear", GUILayout.Height(30)))
        {
            results.Clear();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField($"Results: {results.Count}", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        for (int i = 0; i < results.Count; i++)
        {
            var go = results[i];
            if (go == null) continue;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(go.name, "Select and Ping in Hierarchy"), GUILayout.Height(20)))
            {
                Selection.activeGameObject = go;
                EditorGUIUtility.PingObject(go);
            }

            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeGameObject = go;
            }

            if (GUILayout.Button("Ping", GUILayout.Width(50)))
            {
                EditorGUIUtility.PingObject(go);
            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select All"))
        {
            Selection.objects = results.Where(r => r != null).Select(r => (UnityEngine.Object)r).ToArray();
        }
        if (GUILayout.Button("Ping All"))
        {
            foreach (var r in results) if (r != null) EditorGUIUtility.PingObject(r);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Not: Bu araç editör içindir. Sahnedeki (scene) nesneleri veya proje içindeki prefab'larý arar. Prefab aramasý büyük projilerde uzun sürebilir.", MessageType.Info);
    }

    void DoFind()
    {
        results.Clear();

        if (scriptAsset == null || targetType == null || !typeof(Component).IsAssignableFrom(targetType))
        {
            EditorUtility.DisplayDialog("Invalid Type", "Lütfen geçerli bir Component scripti seçin.", "OK");
            return;
        }

        switch (scope)
        {
            case SearchScope.OpenScenes:
                for (int s = 0; s < SceneManager.sceneCount; s++)
                {
                    var scene = SceneManager.GetSceneAt(s);
                    if (!scene.isLoaded) continue;
                    AddFromScene(scene);
                }
                break;

            case SearchScope.ActiveScene:
                AddFromScene(SceneManager.GetActiveScene());
                break;

            case SearchScope.ProjectPrefabs:
                AddFromPrefabs(null);
                break;

            case SearchScope.SelectedFolder:
                AddFromPrefabs(selectedFolder);
                break;
        }

        // remove duplicates
        results = results.Where(x => x != null).Distinct().ToList();

        // focus first found if any
        if (results.Count > 0)
        {
            Selection.activeGameObject = results[0];
            EditorGUIUtility.PingObject(results[0]);
        }
    }

    void AddFromScene(Scene scene)
    {
        if (!scene.isLoaded) return;
        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            var comps = root.GetComponentsInChildren(targetType, true);
            foreach (var c in comps)
            {
                if (c is Component comp)
                    results.Add(comp.gameObject);
            }
        }
    }

    void AddFromPrefabs(string folderFilter)
    {
        string[] guids;
        if (string.IsNullOrEmpty(folderFilter)) guids = AssetDatabase.FindAssets("t:prefab");
        else guids = AssetDatabase.FindAssets("t:prefab", new[] { folderFilter });

        try
        {
            int processed = 0;
            for (int i = 0; i < guids.Length; i++)
            {
                processed++;
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                var comps = prefab.GetComponentsInChildren(targetType, true);
                if (comps != null && comps.Length > 0)
                {
                    // We can't select a GameObject inside a prefab asset directly in the scene, but we report the prefab root here.
                    results.Add(prefab);
                }

                if (processed % 200 == 0) EditorUtility.DisplayProgressBar("Searching Prefabs", $"Processed {processed}/{guids.Length}", (float)processed / guids.Length);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
}
