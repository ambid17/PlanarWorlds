#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
#if __MICROSPLAT_DIGGER__
using JBooth.MicroSplat;
#endif


namespace Digger.Modules.Core.Sources
{
    public static class EditorUtils
    {
        public static bool CTSExists(Terrain terrain)
        {
#if CTS_PRESENT
            var cts = terrain.GetComponent<CTS.CompleteTerrainShader>();
            if (!cts)
                Debug.LogWarning("[Digger] Looks like you have CTS in your project but the terrain doesn't use it. Is it intended?");
            return cts;
#else
            return false;
#endif
        }

        public static bool MicroSplatExists(Terrain terrain)
        {
#if __MICROSPLAT_DIGGER__
            return terrain.GetComponent<MicroSplatTerrain>();
#else
            return false;
#endif
        }

        public static T CreateOrReplaceAsset<T>(T asset, string path) where T : Object
        {
            Utils.Profiler.BeginSample("[Dig] EditorUtils.CreateOrReplaceAsset>LoadAssetAtPath");
            var existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);
            Utils.Profiler.EndSample();

            if (existingAsset == null) {
                Utils.Profiler.BeginSample("[Dig] EditorUtils.CreateOrReplaceAsset>CreateAsset");
                AssetDatabase.CreateAsset(asset, path);
                Utils.Profiler.EndSample();
                existingAsset = asset;
            } else {
                Utils.Profiler.BeginSample("[Dig] EditorUtils.CreateOrReplaceAsset>CopySerialized");
                EditorUtility.CopySerialized(asset, existingAsset);
                Utils.Profiler.EndSample();
            }

            return existingAsset;
        }

        public static T CreateOrReplaceAssetHard<T>(T asset, string path) where T : Object
        {
            if (!AssetDatabase.LoadAssetAtPath<T>(path))
                AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        public static int AspectSelectionGrid(int selected, Texture[] textures, int approxSize, GUIStyle style,
            GUIContent errorMessage)
        {
            GUILayout.BeginVertical("box", GUILayout.MinHeight(approxSize));
            var newSelected = 0;

            if (textures != null && textures.Length != 0) {
                var columns = (int)(EditorGUIUtility.currentViewWidth - 150) / approxSize;
                // ReSharper disable once PossibleLossOfFraction
                var rows = (int)Mathf.Ceil((textures.Length + columns - 1) / columns);
                var r = GUILayoutUtility.GetAspectRect(columns / (float)rows);

                var texturesPreview = new Texture[textures.Length];
                for (var i = 0; i < textures.Length; ++i) {
                    texturesPreview[i] = textures[i]
                        ? (AssetPreview.GetAssetPreview(textures[i]) ?? textures[i])
                        : EditorGUIUtility.whiteTexture;
                }

                newSelected = GUI.SelectionGrid(r, Math.Min(selected, texturesPreview.Length - 1), texturesPreview,
                    columns, style);
            } else {
                GUILayout.Label(errorMessage);
            }

            GUILayout.EndVertical();
            return newSelected;
        }
    }
}

#endif