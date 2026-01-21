using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class FolderGeneratorWindow : EditorWindow
{
    private string userName = "MyName";

    [MenuItem("Tools/FolderGeneratorWindow")]
    public static void Open()
    {
        GetWindow<FolderGeneratorWindow>("Folder Generator");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("名前を入力してください", EditorStyles.boldLabel);

        userName = EditorGUILayout.TextField("Name", userName);

        GUILayout.Space(10);

        if (GUILayout.Button("フォルダ生成"))
        {
            GenerateFolder(userName);
        }
    }
    public static void GenerateFolder(string userName)
    {
        string assetsPath = "Assets";

        //  名前を最上位フォルダにする例
        string rootPath = $"{assetsPath}/{userName}";

        if (!AssetDatabase.IsValidFolder(rootPath))
        {
            AssetDatabase.CreateFolder("Assets", userName);
        }

        string artPath = "Arts";
        string animationPath = "Animation";

        string[] folders =
            new string[] { artPath, "AssetStoreTools", "Editor", "Resources", "Prefabs", "Scenes", "Scripts", "Settings" }
            .Select(f => $"{userName}/{f}")
            .Concat(
                new string[] { animationPath, "Audio", "Materials", "Meshes", "Textures", "Shaders", "Sprites" }
                .Select(s => $"{userName}/{artPath}/{s}")
            )
            .Concat(
                new string[] { "Clips", "Controllers" }
                .Select(s => $"{userName}/{artPath}/{animationPath}/{s}")
            )
            .ToArray();

        foreach (var folder in folders)
        {
            string fullPath = $"{assetsPath}/{folder}";
            CreateFolderRecursive(fullPath);
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("フォルダ生成", $"{userName} フォルダを生成しました", "OK");
    }
    private static void CreateFolderRecursive(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = Path.GetDirectoryName(path).Replace("\\", "/");
        string folderName = Path.GetFileName(path);

        if (!AssetDatabase.IsValidFolder(parent))
        {
            CreateFolderRecursive(parent);
        }

        AssetDatabase.CreateFolder(parent, folderName);
    }
}

