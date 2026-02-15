using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SpriteMetadataCopier : EditorWindow
{
    public Texture2D sourceTexture;
    public Texture2D targetTexture;
    public string newBaseName;
    public string oldBaseName;

    [MenuItem("Tools/Sprite Metadata Copier")]
    public static void ShowWindow() => GetWindow<SpriteMetadataCopier>("Sprite Copier");

    private void OnGUI()
    {
        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Source", sourceTexture, typeof(Texture2D), false);
        targetTexture = (Texture2D)EditorGUILayout.ObjectField("Target", targetTexture, typeof(Texture2D), false);
        oldBaseName = EditorGUILayout.TextField("Indicator to replace:", oldBaseName);
        newBaseName = EditorGUILayout.TextField("Replace indicator with:", newBaseName);

        if (GUILayout.Button("Copy and Rename Metadata")) CopyMetadata();
    }

    private void CopyMetadata()
    {
        if (sourceTexture == null || targetTexture == null) return;

        // 1. Get Data Providers
        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var sourceProvider = factory.GetSpriteEditorDataProviderFromObject(sourceTexture);
        var targetProvider = factory.GetSpriteEditorDataProviderFromObject(targetTexture);

        sourceProvider.InitSpriteEditorDataProvider();
        targetProvider.InitSpriteEditorDataProvider();

        // 2. Extract and modify SpriteRects
        var sourceRects = sourceProvider.GetSpriteRects();
        var newRects = new List<SpriteRect>();
        var nameFileIdPairs = new List<SpriteNameFileIdPair>();

        for (int i = 0; i < sourceRects.Length; i++)
        {
            var newRect = new SpriteRect();
            if (oldBaseName != null && newBaseName != null)
            {
                string name = ModifyName(sourceRects[i].name, oldBaseName, newBaseName); // Modify names here
                newRect.name = name;

            }
            else
            {
                newRect.name = sourceRects[i].name;
            }
                
            newRect.rect = sourceRects[i].rect;
            newRect.pivot = sourceRects[i].pivot;
            newRect.alignment = sourceRects[i].alignment;
            newRect.border = sourceRects[i].border;
            newRect.spriteID = GUID.Generate(); // Important for new unique slices
            
            newRects.Add(newRect);
            nameFileIdPairs.Add(new SpriteNameFileIdPair(newRect.name, newRect.spriteID));
        }

        // 3. Apply to Target
        targetProvider.SetSpriteRects(newRects.ToArray());

        // Also update the Name-to-FileID mapping to ensure internal Unity consistency
        var nameProvider = targetProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        nameProvider.SetNameFileIdPairs(nameFileIdPairs);

        targetProvider.Apply();

        // 4. Reimport to finalize changes
        var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(targetTexture));
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.SaveAndReimport();

        Debug.Log($"Successfully copied {newRects.Count} slices from {sourceTexture.name} to {targetTexture.name}");
    }

    private string ModifyName(string rectName, string oldBase, string newBase)
    {
        string newName;
        newName=rectName.Replace(oldBase, newBase);
        return newName;
    }
}
