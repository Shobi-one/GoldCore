using System.Collections.Generic;
using System.Linq;
using GoldCore.RetroFX;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GoldCore.EditorTools
{
    /// <summary>
    /// One-click project setup for URP: adds the RetroFX renderer feature to every URP renderer
    /// and turns depth-only overlay cameras (like the Player's weapon camera) into URP overlay
    /// cameras stacked on their base camera.
    /// </summary>
    public static class GoldCoreSetup
    {
        const string ConverterMenu = "Window/Rendering/Render Pipeline Converter";

        [MenuItem("Tools/GoldCore/Set Up URP", priority = 0)]
        public static void SetUpUrp()
        {
            if (!(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset))
            {
                bool open = EditorUtility.DisplayDialog("GoldCore: URP not active yet",
                    "Convert the project first:\n\n" +
                    "1. In the Render Pipeline Converter, pick \"Built-in to URP\".\n" +
                    "2. Tick Rendering Settings, Material Upgrade, Read-only Material Converter and Animation Clip.\n" +
                    "3. Click Initialize And Convert.\n\n" +
                    "Then run Tools > GoldCore > Set Up URP again.",
                    "Open the converter", "Cancel");
                if (open) EditorApplication.ExecuteMenuItem(ConverterMenu);
                return;
            }

            int features = AddRetroFXToRenderers();
            int stacks = StackOverlayCameras();
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("GoldCore: URP set up",
                $"Added RetroFX to {features} renderer(s).\nMade {stacks} weapon camera(s) into URP overlays.\n\n" +
                "RetroLit lights additional lights per vertex on the Forward rendering path. " +
                "On Forward+ it uses the main light only.", "OK");
        }

        /// <summary>Adds a RetroFXFeature to every URP renderer that doesn't have one.</summary>
        [MenuItem("Tools/GoldCore/Add RetroFX to URP Renderers", priority = 1)]
        public static int AddRetroFXToRenderers()
        {
            int added = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:UniversalRendererData"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
                if (data == null || data.rendererFeatures.Any(f => f is RetroFXFeature)) continue;

                var feature = ScriptableObject.CreateInstance<RetroFXFeature>();
                feature.name = "RetroFX";
                AssetDatabase.AddObjectToAsset(feature, data);
                data.rendererFeatures.Add(feature);
                data.SetDirty();
                EditorUtility.SetDirty(data);
                added++;
                Debug.Log($"GoldCore: added RetroFX to {path}", data);
            }
            return added;
        }

        /// <summary>
        /// In every prefab and open scene, a camera that clears depth only is made a URP overlay
        /// on the base camera with the lowest depth in the same hierarchy.
        /// </summary>
        [MenuItem("Tools/GoldCore/Stack Weapon Cameras as URP Overlays", priority = 2)]
        public static int StackOverlayCameras()
        {
            int stacked = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                int count = StackIn(root.GetComponentsInChildren<Camera>(true));
                if (count > 0)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    Debug.Log($"GoldCore: stacked {count} overlay camera(s) in {path}");
                }
                PrefabUtility.UnloadPrefabContents(root);
                stacked += count;
            }
            return stacked;
        }

        static int StackIn(Camera[] cameras)
        {
            List<Camera> overlays = cameras.Where(c => c.clearFlags == CameraClearFlags.Depth).ToList();
            Camera baseCamera = cameras.Where(c => c.clearFlags != CameraClearFlags.Depth).OrderBy(c => c.depth).FirstOrDefault();
            if (baseCamera == null || overlays.Count == 0) return 0;

            var baseData = baseCamera.GetUniversalAdditionalCameraData();
            baseData.renderType = CameraRenderType.Base;
            int count = 0;
            foreach (Camera overlay in overlays.OrderBy(c => c.depth))
            {
                var data = overlay.GetUniversalAdditionalCameraData();
                if (data.renderType == CameraRenderType.Overlay && baseData.cameraStack.Contains(overlay)) continue;
                data.renderType = CameraRenderType.Overlay;
                if (!baseData.cameraStack.Contains(overlay)) baseData.cameraStack.Add(overlay);
                count++;
            }
            return count;
        }
    }
}
