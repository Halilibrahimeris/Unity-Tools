#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshRendererLightingTool : EditorWindow
{
    Vector2 scroll;
    List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
    List<bool> selected;

    // Bulk lighting parameters
    bool bulkCastShadows = true;
    bool bulkCastShadowsEnabled = false;
    ShadowCastingMode bulkShadowCastingMode = ShadowCastingMode.On;

    bool bulkReceiveShadows = true;
    bool bulkReceiveShadowsEnabled = false;

    LightProbeUsage bulkLightProbeUsage = LightProbeUsage.BlendProbes;
    bool bulkLightProbeUsageEnabled = false;

    ReflectionProbeUsage bulkReflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
    bool bulkReflectionProbeUsageEnabled = false;

    MotionVectorGenerationMode bulkMotionVectors = MotionVectorGenerationMode.Object;
    bool bulkMotionVectorsEnabled = false;

    bool bulkContributeGI = true;
    bool bulkContributeGIEnabled = false;

    ReceiveGI bulkReceiveGI = ReceiveGI.Lightmaps;
    bool bulkReceiveGIEnabled = false;

    [MenuItem("Tools/Lighting/Mesh Renderer Lighting Tool")]
    static void OpenWindow()
    {
        var w = GetWindow<MeshRendererLightingTool>("MeshRenderer Lighting Tool");
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
        MeshRenderer[] all = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        meshRenderers = all.OrderBy(m => m.gameObject.scene.name).ThenBy(m => m.name).ToList();

        if (selected == null || selected.Count != meshRenderers.Count)
            selected = Enumerable.Repeat(true, meshRenderers.Count).ToList();

        Repaint();
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
        EditorGUILayout.LabelField("Bulk Lighting Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Shadow Casting Mode
        EditorGUILayout.BeginHorizontal();
        bulkCastShadowsEnabled = EditorGUILayout.ToggleLeft("Enable Shadow Casting Mode", bulkCastShadowsEnabled, GUILayout.Width(200));
        bulkShadowCastingMode = (ShadowCastingMode)EditorGUILayout.EnumPopup(bulkShadowCastingMode, GUILayout.Width(150));
        if (GUILayout.Button("Apply To Selected", GUILayout.Width(140))) ApplyShadowCastingToSelected();
        if (GUILayout.Button("Apply To All", GUILayout.Width(100))) ApplyShadowCastingToAll();
        EditorGUILayout.EndHorizontal();

        // Receive Shadows
        EditorGUILayout.BeginHorizontal();
        bulkReceiveShadowsEnabled = EditorGUILayout.ToggleLeft("Enable Receive Shadows", bulkReceiveShadowsEnabled, GUILayout.Width(200));
        bulkReceiveShadows = EditorGUILayout.Toggle(bulkReceiveShadows, GUILayout.Width(150));
        if (GUILayout.Button("Apply To Selected", GUILayout.Width(140))) ApplyReceiveShadowsToSelected();
        if (GUILayout.Button("Apply To All", GUILayout.Width(100))) ApplyReceiveShadowsToAll();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Light Probe Usage
        EditorGUILayout.BeginHorizontal();
        bulkLightProbeUsageEnabled = EditorGUILayout.ToggleLeft("Enable Light Probe Usage", bulkLightProbeUsageEnabled, GUILayout.Width(200));
        bulkLightProbeUsage = (LightProbeUsage)EditorGUILayout.EnumPopup(bulkLightProbeUsage, GUILayout.Width(150));
        if (GUILayout.Button("Apply To Selected", GUILayout.Width(140))) ApplyLightProbeUsageToSelected();
        if (GUILayout.Button("Apply To All", GUILayout.Width(100))) ApplyLightProbeUsageToAll();
        EditorGUILayout.EndHorizontal();

        // Reflection Probe Usage
        EditorGUILayout.BeginHorizontal();
        bulkReflectionProbeUsageEnabled = EditorGUILayout.ToggleLeft("Enable Reflection Probe Usage", bulkReflectionProbeUsageEnabled, GUILayout.Width(200));
        bulkReflectionProbeUsage = (ReflectionProbeUsage)EditorGUILayout.EnumPopup(bulkReflectionProbeUsage, GUILayout.Width(150));
        if (GUILayout.Button("Apply To Selected", GUILayout.Width(140))) ApplyReflectionProbeUsageToSelected();
        if (GUILayout.Button("Apply To All", GUILayout.Width(100))) ApplyReflectionProbeUsageToAll();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Contribute GI
        EditorGUILayout.BeginHorizontal();
        bulkContributeGIEnabled = EditorGUILayout.ToggleLeft("Enable Contribute Global Illumination", bulkContributeGIEnabled, GUILayout.Width(250));
        bulkContributeGI = EditorGUILayout.Toggle(bulkContributeGI, GUILayout.Width(100));
        if (GUILayout.Button("Apply To Selected", GUILayout.Width(140))) ApplyContributeGIToSelected();
        if (GUILayout.Button("Apply To All", GUILayout.Width(100))) ApplyContributeGIToAll();
        EditorGUILayout.EndHorizontal();

        // Receive GI
        EditorGUILayout.BeginHorizontal();
        bulkReceiveGIEnabled = EditorGUILayout.ToggleLeft("Enable Receive Global Illumination", bulkReceiveGIEnabled, GUILayout.Width(250));
        bulkReceiveGI = (ReceiveGI)EditorGUILayout.EnumPopup(bulkReceiveGI, GUILayout.Width(100));
        if (GUILayout.Button("Apply To Selected", GUILayout.Width(140))) ApplyReceiveGIToSelected();
        if (GUILayout.Button("Apply To All", GUILayout.Width(100))) ApplyReceiveGIToAll();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Motion Vectors
        EditorGUILayout.BeginHorizontal();
        bulkMotionVectorsEnabled = EditorGUILayout.ToggleLeft("Enable Motion Vectors", bulkMotionVectorsEnabled, GUILayout.Width(200));
        bulkMotionVectors = (MotionVectorGenerationMode)EditorGUILayout.EnumPopup(bulkMotionVectors, GUILayout.Width(150));
        if (GUILayout.Button("Apply To Selected", GUILayout.Width(140))) ApplyMotionVectorsToSelected();
        if (GUILayout.Button("Apply To All", GUILayout.Width(100))) ApplyMotionVectorsToAll();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Found {meshRenderers.Count} MeshRenderers", EditorStyles.boldLabel);

        // Scroll list
        scroll = EditorGUILayout.BeginScrollView(scroll);
        for (int i = 0; i < meshRenderers.Count; i++)
        {
            var mr = meshRenderers[i];
            if (mr == null) continue;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            selected[i] = EditorGUILayout.Toggle(selected[i], GUILayout.Width(18));
            EditorGUILayout.LabelField(mr.gameObject.scene.name, GUILayout.Width(120));
            EditorGUILayout.LabelField(mr.name, GUILayout.Width(220));

            if (GUILayout.Button("Ping", GUILayout.Width(60)))
                EditorGUIUtility.PingObject(mr.gameObject);

            EditorGUILayout.EndHorizontal();

            // Shadow settings
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(24);
            ShadowCastingMode currentShadowMode = mr.shadowCastingMode;
            ShadowCastingMode newShadowMode = (ShadowCastingMode)EditorGUILayout.EnumPopup("Shadow Casting", currentShadowMode, GUILayout.Width(280));
            if (newShadowMode != currentShadowMode)
            {
                Undo.RecordObject(mr, "Change Shadow Casting Mode");
                mr.shadowCastingMode = newShadowMode;
                EditorUtility.SetDirty(mr);
                MarkSceneDirty(mr);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(24);
            bool currentReceiveShadows = mr.receiveShadows;
            bool newReceiveShadows = EditorGUILayout.Toggle("Receive Shadows", currentReceiveShadows, GUILayout.Width(280));
            if (newReceiveShadows != currentReceiveShadows)
            {
                Undo.RecordObject(mr, "Change Receive Shadows");
                mr.receiveShadows = newReceiveShadows;
                EditorUtility.SetDirty(mr);
                MarkSceneDirty(mr);
            }
            EditorGUILayout.EndHorizontal();

            // Probe settings
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(24);
            LightProbeUsage currentLightProbe = mr.lightProbeUsage;
            LightProbeUsage newLightProbe = (LightProbeUsage)EditorGUILayout.EnumPopup("Light Probes", currentLightProbe, GUILayout.Width(280));
            if (newLightProbe != currentLightProbe)
            {
                Undo.RecordObject(mr, "Change Light Probe Usage");
                mr.lightProbeUsage = newLightProbe;
                EditorUtility.SetDirty(mr);
                MarkSceneDirty(mr);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(24);
            ReflectionProbeUsage currentReflectionProbe = mr.reflectionProbeUsage;
            ReflectionProbeUsage newReflectionProbe = (ReflectionProbeUsage)EditorGUILayout.EnumPopup("Reflection Probes", currentReflectionProbe, GUILayout.Width(280));
            if (newReflectionProbe != currentReflectionProbe)
            {
                Undo.RecordObject(mr, "Change Reflection Probe Usage");
                mr.reflectionProbeUsage = newReflectionProbe;
                EditorUtility.SetDirty(mr);
                MarkSceneDirty(mr);
            }
            EditorGUILayout.EndHorizontal();

            // GI settings
            var so = new SerializedObject(mr);
            var contributeGIProp = so.FindProperty("m_StaticEditorFlags");

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(24);
            bool currentContributeGI = GameObjectUtility.AreStaticEditorFlagsSet(mr.gameObject, StaticEditorFlags.ContributeGI);
            bool newContributeGI = EditorGUILayout.Toggle("Contribute GI", currentContributeGI, GUILayout.Width(280));
            if (newContributeGI != currentContributeGI)
            {
                Undo.RecordObject(mr.gameObject, "Change Contribute GI");
                GameObjectUtility.SetStaticEditorFlags(mr.gameObject,
                    newContributeGI ?
                    GameObjectUtility.GetStaticEditorFlags(mr.gameObject) | StaticEditorFlags.ContributeGI :
                    GameObjectUtility.GetStaticEditorFlags(mr.gameObject) & ~StaticEditorFlags.ContributeGI);
                EditorUtility.SetDirty(mr.gameObject);
                MarkSceneDirty(mr);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(24);
            ReceiveGI currentReceiveGI = mr.receiveGI;
            ReceiveGI newReceiveGI = (ReceiveGI)EditorGUILayout.EnumPopup("Receive GI", currentReceiveGI, GUILayout.Width(280));
            if (newReceiveGI != currentReceiveGI)
            {
                Undo.RecordObject(mr, "Change Receive GI");
                mr.receiveGI = newReceiveGI;
                EditorUtility.SetDirty(mr);
                MarkSceneDirty(mr);
            }
            EditorGUILayout.EndHorizontal();

            // Motion Vectors
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(24);
            MotionVectorGenerationMode currentMotion = mr.motionVectorGenerationMode;
            MotionVectorGenerationMode newMotion = (MotionVectorGenerationMode)EditorGUILayout.EnumPopup("Motion Vectors", currentMotion, GUILayout.Width(280));
            if (newMotion != currentMotion)
            {
                Undo.RecordObject(mr, "Change Motion Vectors");
                mr.motionVectorGenerationMode = newMotion;
                EditorUtility.SetDirty(mr);
                MarkSceneDirty(mr);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();
    }

    void ApplyShadowCastingToSelected()
    {
        if (!bulkCastShadowsEnabled) return;
        for (int i = 0; i < meshRenderers.Count; i++)
            if (selected[i]) SetShadowCasting(meshRenderers[i], bulkShadowCastingMode);
    }

    void ApplyShadowCastingToAll()
    {
        if (!bulkCastShadowsEnabled) return;
        for (int i = 0; i < meshRenderers.Count; i++)
            SetShadowCasting(meshRenderers[i], bulkShadowCastingMode);
    }

    void ApplyReceiveShadowsToSelected()
    {
        if (!bulkReceiveShadowsEnabled) return;
        for (int i = 0; i < meshRenderers.Count; i++)
            if (selected[i]) SetReceiveShadows(meshRenderers[i], bulkReceiveShadows);
    }

    void ApplyReceiveShadowsToAll()
    {
        if (!bulkReceiveShadowsEnabled) return;
        for (int i = 0; i < meshRenderers.Count; i++)
            SetReceiveShadows(meshRenderers[i], bulkReceiveShadows);
    }

    void ApplyLightProbeUsageToSelected()
    {
        if (!bulkLightProbeUsageEnabled) return;
        for (int i = 0; i < meshRenderers.Count; i++)
            if (selected[i]) SetLightProbeUsage(meshRenderers[i], bulkLightProbeUsage);
    }

    void ApplyLightProbeUsageToAll()
    {
        if (!bulkLightProbeUsageEnabled) return;
        for (int i = 0; i < meshRenderers.Count; i++)
            SetLightProbeUsage(meshRenderers[i], bulkLightProbeUsage);
    }

    void ApplyReflectionProbeUsageToSelected()
    {
        if (!bulkReflectionProbeUsageEnabled) return;
        for (int i = 0; i < meshRenderers.Count; i++)
            if (selected[i]) SetReflectionProbeUsage(meshRenderers[i], bulkReflectionProbeUsage);
    }

    void ApplyReflectionProbeUsageToAll()
    {
        if (!bulkReflectionProbeUsageEnabled) return;
        for (int i = 0; i < meshRenderers.Count; i++)
            SetReflectionProbeUsage(meshRenderers[i], bulkReflectionProbeUsage);
    }

    void ApplyContributeGIToSelected()
    {
        if (!bulkContributeGIEnabled) return;
        for (int i = 0; i < meshRenderers.Count; i++)
            if (selected[i]) SetContributeGI(meshRenderers[i], bulkContributeGI);
    }

    void ApplyContributeGIToAll()
    {
        if (!bulkContributeGIEnabled) return;
        for (int i = 0; i < meshRenderers.Count; i++)
            SetContributeGI(meshRenderers[i], bulkContributeGI);
    }

    void ApplyReceiveGIToSelected()
    {
        if (!bulkReceiveGIEnabled) return;
        for (int i = 0; i < meshRenderers.Count; i++)
            if (selected[i]) SetReceiveGI(meshRenderers[i], bulkReceiveGI);
    }

    void ApplyReceiveGIToAll()
    {
        if (!bulkReceiveGIEnabled) return;
        for (int i = 0; i < meshRenderers.Count; i++)
            SetReceiveGI(meshRenderers[i], bulkReceiveGI);
    }

    void ApplyMotionVectorsToSelected()
    {
        if (!bulkMotionVectorsEnabled) return;
        for (int i = 0; i < meshRenderers.Count; i++)
            if (selected[i]) SetMotionVectors(meshRenderers[i], bulkMotionVectors);
    }

    void ApplyMotionVectorsToAll()
    {
        if (!bulkMotionVectorsEnabled) return;
        for (int i = 0; i < meshRenderers.Count; i++)
            SetMotionVectors(meshRenderers[i], bulkMotionVectors);
    }

    void SetShadowCasting(MeshRenderer mr, ShadowCastingMode mode)
    {
        if (mr == null) return;
        if (mr.shadowCastingMode == mode) return;
        Undo.RecordObject(mr, "Change Shadow Casting Mode");
        mr.shadowCastingMode = mode;
        EditorUtility.SetDirty(mr);
        MarkSceneDirty(mr);
    }

    void SetReceiveShadows(MeshRenderer mr, bool receive)
    {
        if (mr == null) return;
        if (mr.receiveShadows == receive) return;
        Undo.RecordObject(mr, "Change Receive Shadows");
        mr.receiveShadows = receive;
        EditorUtility.SetDirty(mr);
        MarkSceneDirty(mr);
    }

    void SetLightProbeUsage(MeshRenderer mr, LightProbeUsage usage)
    {
        if (mr == null) return;
        if (mr.lightProbeUsage == usage) return;
        Undo.RecordObject(mr, "Change Light Probe Usage");
        mr.lightProbeUsage = usage;
        EditorUtility.SetDirty(mr);
        MarkSceneDirty(mr);
    }

    void SetReflectionProbeUsage(MeshRenderer mr, ReflectionProbeUsage usage)
    {
        if (mr == null) return;
        if (mr.reflectionProbeUsage == usage) return;
        Undo.RecordObject(mr, "Change Reflection Probe Usage");
        mr.reflectionProbeUsage = usage;
        EditorUtility.SetDirty(mr);
        MarkSceneDirty(mr);
    }

    void SetContributeGI(MeshRenderer mr, bool contribute)
    {
        if (mr == null) return;
        bool current = GameObjectUtility.AreStaticEditorFlagsSet(mr.gameObject, StaticEditorFlags.ContributeGI);
        if (current == contribute) return;

        Undo.RecordObject(mr.gameObject, "Change Contribute GI");
        GameObjectUtility.SetStaticEditorFlags(mr.gameObject,
            contribute ?
            GameObjectUtility.GetStaticEditorFlags(mr.gameObject) | StaticEditorFlags.ContributeGI :
            GameObjectUtility.GetStaticEditorFlags(mr.gameObject) & ~StaticEditorFlags.ContributeGI);
        EditorUtility.SetDirty(mr.gameObject);
        MarkSceneDirty(mr);
    }

    void SetReceiveGI(MeshRenderer mr, ReceiveGI mode)
    {
        if (mr == null) return;
        if (mr.receiveGI == mode) return;
        Undo.RecordObject(mr, "Change Receive GI");
        mr.receiveGI = mode;
        EditorUtility.SetDirty(mr);
        MarkSceneDirty(mr);
    }

    void SetMotionVectors(MeshRenderer mr, MotionVectorGenerationMode mode)
    {
        if (mr == null) return;
        if (mr.motionVectorGenerationMode == mode) return;
        Undo.RecordObject(mr, "Change Motion Vectors");
        mr.motionVectorGenerationMode = mode;
        EditorUtility.SetDirty(mr);
        MarkSceneDirty(mr);
    }

    void MarkSceneDirty(MeshRenderer mr)
    {
        if (mr == null) return;
        var scene = mr.gameObject.scene;
        if (scene.IsValid())
            EditorSceneManager.MarkSceneDirty(scene);
    }
}
#endif