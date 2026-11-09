using UnityEditor;
using UnityEngine;

namespace GoldCore.RetroFX.EditorTools
{
    /// <summary>
    /// Switches the selected materials to GoldCore/RetroLit and keeps their texture, tiling,
    /// color and emission, so vertex snapping and texture warping apply to them. Undoable.
    /// </summary>
    public static class RetroMaterialConverter
    {
        const string MenuPath = "Tools/GoldCore/Convert Selected Materials to RetroLit";

        [MenuItem(MenuPath)]
        static void ConvertSelected()
        {
            Shader retroLit = Shader.Find("GoldCore/RetroLit");
            if (retroLit == null)
            {
                Debug.LogError("GoldCore/RetroLit shader not found.");
                return;
            }

            int converted = 0;
            foreach (Object obj in Selection.GetFiltered(typeof(Material), SelectionMode.Assets))
            {
                var mat = (Material)obj;
                if (mat.shader == retroLit) continue;

                Texture tex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
                Vector2 scale = mat.HasProperty("_MainTex") ? mat.GetTextureScale("_MainTex") : Vector2.one;
                Vector2 offset = mat.HasProperty("_MainTex") ? mat.GetTextureOffset("_MainTex") : Vector2.zero;
                Color color = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white;
                bool emissive = mat.IsKeywordEnabled("_EMISSION") && mat.HasProperty("_EmissionColor");
                Color emission = emissive ? mat.GetColor("_EmissionColor") : Color.black;

                Undo.RecordObject(mat, "Convert to RetroLit");
                mat.shader = retroLit;
                mat.SetTexture("_MainTex", tex);
                mat.SetTextureScale("_MainTex", scale);
                mat.SetTextureOffset("_MainTex", offset);
                mat.SetColor("_Color", color);
                mat.SetColor("_EmissionColor", emission);
                EditorUtility.SetDirty(mat);
                converted++;
            }
            Debug.Log($"Converted {converted} material(s) to GoldCore/RetroLit.");
        }

        [MenuItem(MenuPath, true)]
        static bool ValidateConvertSelected() =>
            Selection.GetFiltered(typeof(Material), SelectionMode.Assets).Length > 0;
    }
}
