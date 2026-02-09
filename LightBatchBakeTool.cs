#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class LightBatchBakeTool : EditorWindow
{
    Vector2 scroll;
    List<Light> lights = new List<Light>();
    List<bool> selected;
    LightmapBakeType bulkType = LightmapBakeType.Realtime;

    // Bulk size will be applied to Light.range
    float bulkRange = 5f;
    bool bulkRangeEnabled = false;

    // Bulk intensity
    float bulkIntensity = 1f;
    bool bulkIntensityEnabled = false;

    [MenuItem("Tools/Lighting/Light Bake Tool")]
    static void OpenWindow()
    {
        var w = GetWindow<LightBatchBakeTool>("Light Bake Tool");
        w.RefreshList();
    }

    void OnEnable()
    {
        RefreshList();
        EditorApplication.hierarchyChanged += RefreshList;
    }

    void OnDisable()
    {
        EditorApplication.hierarchyChanged -= RefreshList;
    }

    void RefreshList()
    {
        Light[] all = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        // exclude area lights (Area/Rectangle/Disc)
        lights = all.Where(l => !IsAreaLightType(l.type)).OrderBy(l => l.gameObject.scene.name).ThenBy(l => l.name).ToList();

        if (selected == null || selected.Count != lights.Count)
            selected = Enumerable.Repeat(true, lights.Count).ToList();

        Repaint();
    }

    static bool IsAreaLightType(LightType t)
    {
        return t == LightType.Rectangle || t == LightType.Disc;
    }

    void OnGUI()
    {
        EditorGUILayout.Space();

        // Top buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh", GUILayout.Width(100))) RefreshList();
        if (GUILayout.Button("Select All", GUILayout.Width(100))) { for (int i = 0; i < selected.Count; i++) selected[i] = true; }
        if (GUILayout.Button("Deselect All", GUILayout.Width(100))) { for (int i = 0; i < selected.Count; i++) selected[i] = false; }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Bulk bake type
        EditorGUILayout.BeginHorizontal();
        bulkType = (LightmapBakeType)EditorGUILayout.EnumPopup("Bulk set bake type:", bulkType);
        if (GUILayout.Button("Apply BakeType To Selected", GUILayout.Width(220))) ApplyBulkBakeTypeToSelected();
        if (GUILayout.Button("Apply BakeType To All", GUILayout.Width(180))) ApplyBulkBakeTypeToAll();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Bulk range controls
        EditorGUILayout.BeginHorizontal();
        bulkRangeEnabled = EditorGUILayout.ToggleLeft("Enable bulk range (applies to Light.range)", bulkRangeEnabled, GUILayout.Width(300));
        bulkRange = EditorGUILayout.FloatField("Bulk Range", bulkRange);
        if (GUILayout.Button("Apply Range To Selected", GUILayout.Width(180))) ApplyBulkRangeToSelected();
        if (GUILayout.Button("Apply Range To All", GUILayout.Width(140))) ApplyBulkRangeToAll();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Bulk intensity controls
        EditorGUILayout.BeginHorizontal();
        bulkIntensityEnabled = EditorGUILayout.ToggleLeft("Enable bulk intensity (applies to Light.intensity)", bulkIntensityEnabled, GUILayout.Width(300));
        bulkIntensity = EditorGUILayout.FloatField("Bulk Intensity", bulkIntensity);
        if (GUILayout.Button("Apply Intensity To Selected", GUILayout.Width(200))) ApplyBulkIntensityToSelected();
        if (GUILayout.Button("Apply Intensity To All", GUILayout.Width(160))) ApplyBulkIntensityToAll();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // List header
        EditorGUILayout.LabelField($"Found {lights.Count} lights (area types excluded)", EditorStyles.boldLabel);

        // Scroll list
        scroll = EditorGUILayout.BeginScrollView(scroll);
        for (int i = 0; i < lights.Count; i++)
        {
            var light = lights[i];
            if (light == null) continue;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            selected[i] = EditorGUILayout.Toggle(selected[i], GUILayout.Width(18));
            EditorGUILayout.LabelField(light.gameObject.scene.name, GUILayout.Width(120));
            EditorGUILayout.LabelField(light.name, GUILayout.Width(220));
            EditorGUILayout.LabelField(light.type.ToString(), GUILayout.Width(90));

            // Bake type per light
            LightmapBakeType current = light.lightmapBakeType;
            LightmapBakeType newType = (LightmapBakeType)EditorGUILayout.EnumPopup(current, GUILayout.Width(120));
            if (newType != current)
            {
                Undo.RecordObject(light, "Change Light Bake Type");
                light.lightmapBakeType = newType;
                EditorUtility.SetDirty(light);
                MarkSceneDirty(light);
            }

            if (GUILayout.Button("Ping", GUILayout.Width(60)))
                EditorGUIUtility.PingObject(light.gameObject);

            EditorGUILayout.EndHorizontal();

            // Intensity editing row (shown for all light types)
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(24);
            float curIntensity = light.intensity;
            float newIntensity = EditorGUILayout.FloatField("Intensity", curIntensity, GUILayout.Width(260));
            if (!Mathf.Approximately(newIntensity, curIntensity))
            {
                Undo.RecordObject(light, "Change Light Intensity");
                light.intensity = Mathf.Max(0f, newIntensity);
                EditorUtility.SetDirty(light);
                MarkSceneDirty(light);
            }
            EditorGUILayout.EndHorizontal();

            // Range editing row and spot angle helper
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(24);
            if (light.type == LightType.Point || light.type == LightType.Spot || light.type == LightType.Rectangle || light.type == LightType.Disc)
            {
                // For Point/Spot, range is meaningful. (Area types are excluded earlier, but keep check safe.)
                float curRange = light.range;
                float newRange = EditorGUILayout.FloatField("Range (m)", curRange, GUILayout.Width(260));
                if (!Mathf.Approximately(newRange, curRange))
                {
                    Undo.RecordObject(light, "Change Light Range");
                    light.range = Mathf.Max(0f, newRange);
                    EditorUtility.SetDirty(light);
                    MarkSceneDirty(light);
                }

                // For Spot, still show spot angle as helper
                if (light.type == LightType.Spot)
                {
                    float curAngle = light.spotAngle;
                    float newAngle = EditorGUILayout.FloatField("Spot Angle", curAngle, GUILayout.Width(200));
                    if (!Mathf.Approximately(newAngle, curAngle))
                    {
                        Undo.RecordObject(light, "Change Spot Angle");
                        light.spotAngle = Mathf.Clamp(newAngle, 0.1f, 179f);
                        EditorUtility.SetDirty(light);
                        MarkSceneDirty(light);
                    }
                }
            }
            else if (light.type == LightType.Directional)
            {
                EditorGUILayout.LabelField("Directional ýþýklar sonsuzdur — 'range' yoktur.", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField("Bu ýþýk tipi için range düzenlenemez.", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();
    }

    void ApplyBulkBakeTypeToSelected()
    {
        for (int i = 0; i < lights.Count; i++)
            if (selected[i]) SetLightBakeType(lights[i], bulkType);
    }

    void ApplyBulkBakeTypeToAll()
    {
        for (int i = 0; i < lights.Count; i++)
            SetLightBakeType(lights[i], bulkType);
    }

    void ApplyBulkRangeToSelected()
    {
        if (!bulkRangeEnabled) return;
        for (int i = 0; i < lights.Count; i++)
            if (selected[i]) SetLightRange(lights[i], bulkRange);
    }

    void ApplyBulkRangeToAll()
    {
        if (!bulkRangeEnabled) return;
        for (int i = 0; i < lights.Count; i++)
            SetLightRange(lights[i], bulkRange);
    }

    void ApplyBulkIntensityToSelected()
    {
        if (!bulkIntensityEnabled) return;
        for (int i = 0; i < lights.Count; i++)
            if (selected[i]) SetLightIntensity(lights[i], bulkIntensity);
    }

    void ApplyBulkIntensityToAll()
    {
        if (!bulkIntensityEnabled) return;
        for (int i = 0; i < lights.Count; i++)
            SetLightIntensity(lights[i], bulkIntensity);
    }

    void SetLightBakeType(Light l, LightmapBakeType t)
    {
        if (l == null) return;
        if (l.lightmapBakeType == t) return;
        Undo.RecordObject(l, "Change Light Bake Type");
        l.lightmapBakeType = t;
        EditorUtility.SetDirty(l);
        MarkSceneDirty(l);
    }

    void SetLightRange(Light l, float range)
    {
        if (l == null) return;
        range = Mathf.Max(0f, range);

        // Directional lights don't have range
        if (l.type == LightType.Directional) return;

        if (!Mathf.Approximately(l.range, range))
        {
            Undo.RecordObject(l, "Change Light Range");
            l.range = range;
            EditorUtility.SetDirty(l);
            MarkSceneDirty(l);
        }
    }

    void SetLightIntensity(Light l, float intensity)
    {
        if (l == null) return;
        intensity = Mathf.Max(0f, intensity);

        if (!Mathf.Approximately(l.intensity, intensity))
        {
            Undo.RecordObject(l, "Change Light Intensity");
            l.intensity = intensity;
            EditorUtility.SetDirty(l);
            MarkSceneDirty(l);
        }
    }

    void MarkSceneDirty(Light l)
    {
        if (l == null) return;
        var scene = l.gameObject.scene;
        if (scene.IsValid())
            EditorSceneManager.MarkSceneDirty(scene);
    }
}
#endif
