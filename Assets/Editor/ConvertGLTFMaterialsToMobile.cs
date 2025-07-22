using UnityEngine;
using UnityEditor;

public class ConvertGLTFMaterialsToMobile : EditorWindow
{
    [MenuItem("Tools/Convert glTF Materials to Mobile/Diffuse")]
    public static void ConvertMaterials()
    {
        string[] materialGuids = AssetDatabase.FindAssets("t:Material");
        int converted = 0;

        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat != null && mat.shader != null && mat.shader.name == "glTF/PbrMetallicRoughness")
            {
                // Store the base color texture if present
                Texture baseColor = null;
                if (mat.HasProperty("_BaseColorTex"))
                    baseColor = mat.GetTexture("_BaseColorTex");

                // Change shader to Mobile/Diffuse
                mat.shader = Shader.Find("Mobile/Diffuse");

                // Assign the base color texture to the new shader
                if (baseColor != null && mat.HasProperty("_MainTex"))
                    mat.SetTexture("_MainTex", baseColor);

                EditorUtility.SetDirty(mat);
                converted++;
            }
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Conversion Complete", $"Converted {converted} materials to Mobile/Diffuse.", "OK");
    }
}
