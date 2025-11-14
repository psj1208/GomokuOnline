using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

//텍스처 최적화하기
//오디오랑 비슷하게 폴더 넣고 원하는 해상도를 설정하면 해상도에 맞게 폴더 안의 텍스처들을 바꿈.
#if UNITY_EDITOR
public class TextureOptimizer : EditorWindow
{
    private enum ESize
    {
        Size256 = 256,
        Size512 = 512,
        Size1024 = 1024,
        Size2048 = 2048
    }

    private DefaultAsset SelectFolder;
    private ESize size = ESize.Size1024;
    [MenuItem("Utility/TextureOptimize")]
    private static void Init()
    {
        TextureOptimizer editorWindow = (TextureOptimizer)GetWindow(typeof(TextureOptimizer));
        editorWindow.Show();
    }

    private void OnGUI()
    {
        SelectFolder = (DefaultAsset)EditorGUILayout.ObjectField("Texture Folder", SelectFolder, typeof(DefaultAsset), true);
        size = (ESize)EditorGUILayout.EnumPopup("Max Size", size);

        if (GUILayout.Button("최적화 실행"))
        {
            string path = AssetDatabase.GetAssetPath(SelectFolder);
            if (!string.IsNullOrEmpty(path))
            {
                OptimizeTextureInFolder(path);
            }
            else
            {
                Debug.LogWarning("폴더를 선택해주세요.");
            }
        }
    }

    private void OptimizeTextureInFolder(string folderPath)
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture", new[] { folderPath });

        Debug.Log($"Found {guids.Length} Textures in: {folderPath}");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(path);
            Debug.Log($"Texture{texture.name} at {path}");

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.maxTextureSize = (int)size;

                importer.SaveAndReimport();
            }
        }
    }
}
#endif