using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

//오디오 최적화용.
//단순하게 타입에 맞춰 폴더를 넣고 버튼을 누르면 타입에 맞게 폴더 안의 오디오 클립들의 설정을 최적화함.
#if UNITY_EDITOR
public class AudioOptimize : EditorWindow
{
    private DefaultAsset folder2D;
    private DefaultAsset folder3D;
    private DefaultAsset folderBGM;
    [MenuItem("Utility/AudioOptimize")]
    private static void Init()
    {
        AudioOptimize editorWindow = (AudioOptimize)GetWindow(typeof(AudioOptimize));
        editorWindow.Show();
    }

    private void OnGUI()
    {
        folder2D = (DefaultAsset)EditorGUILayout.ObjectField("2D Audio Folder", folder2D, typeof(DefaultAsset), true);
        if (GUILayout.Button("2D Optimize"))
        {
            if (folder2D == null)
            {
                Debug.Log("Set Folder In Object Field");
                return;
            }
            Optimize2DAudioInFolder(AssetDatabase.GetAssetPath(folder2D));
        }

        folder3D = (DefaultAsset)EditorGUILayout.ObjectField("3D Audio Folder", folder3D, typeof(DefaultAsset), true);
        if (GUILayout.Button("3D Optimize"))
        {
            if (folder3D == null)
            {
                Debug.Log("Set Folder In Object Field");
                return;
            }
            Optimize3DAudioInFolder(AssetDatabase.GetAssetPath(folder3D));
        }

        folderBGM = (DefaultAsset)EditorGUILayout.ObjectField("BGM Audio Folder", folderBGM, typeof(DefaultAsset), true);
        if (GUILayout.Button("BGM Optimize"))
        {
            if(folderBGM == null)
            {
                Debug.Log("Set Folder In Object Field");
                return;
            }
            OptimizeBGMAudioInFolder(AssetDatabase.GetAssetPath(folderBGM));
        }
    }

    #region 최적화 함수
    private void Optimize2DAudioInFolder(string folderPath)
    {
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { folderPath });

        Debug.Log($"Found {guids.Length} audio clips in: {folderPath}");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            Debug.Log($"AudioClip{clip.name} at {path}");

            AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer != null)
            {
                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                long fileSizeBytes = new FileInfo(path).Length;
                float fileSizeKB = fileSizeBytes / 1024f;

                importer.forceToMono = true;
                importer.loadInBackground = false;
                importer.ambisonic = false;

                settings.preloadAudioData = true;
                if (fileSizeKB >= 200)
                    settings.loadType = AudioClipLoadType.CompressedInMemory;
                else
                    settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = 100;
                importer.SaveAndReimport();
            }
        }
    }

    private void Optimize3DAudioInFolder(string folderPath)
    {
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { folderPath });

        Debug.Log($"Found {guids.Length} audio clips in: {folderPath}");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            Debug.Log($"AudioClip{clip.name} at {path}");

            AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer != null)
            {
                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                long fileSizeBytes = new FileInfo(path).Length;
                float fileSizeKB = fileSizeBytes / 1024f;

                importer.forceToMono = false;
                importer.loadInBackground = false;
                importer.ambisonic = false;

                settings.preloadAudioData = true;
                if (fileSizeKB >= 200)
                    settings.loadType = AudioClipLoadType.CompressedInMemory;
                else
                    settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = 100;
                importer.SaveAndReimport();
            }
        }
    }

    private void OptimizeBGMAudioInFolder(string folderPath)
    {
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { folderPath });

        Debug.Log($"Found {guids.Length} audio clips in: {folderPath}");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            Debug.Log($"AudioClip{clip.name} at {path}");

            AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer != null)
            {
                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                long fileSizeBytes = new FileInfo(path).Length;
                float fileSizeKB = fileSizeBytes / 1024f;

                importer.forceToMono = true;
                importer.loadInBackground = true;
                importer.ambisonic = false;

                settings.preloadAudioData = false;
                settings.loadType = AudioClipLoadType.Streaming;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = 100;
                importer.SaveAndReimport();
            }
        }
    }
    #endregion
}
#endif