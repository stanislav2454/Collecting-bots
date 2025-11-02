using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Linq;

public class FolderStructurePrinter : EditorWindow
{
    private string outputText = "";
    private Vector2 scrollPosition;

    [MenuItem("Tools/Project Structure Printer")]
    public static void ShowWindow()
    {
        GetWindow<FolderStructurePrinter>("Project Structure");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Generate Structure Report"))
        {
            GenerateStructure();
        }

        if (GUILayout.Button("Copy to Clipboard"))
        {
            GUIUtility.systemCopyBuffer = outputText;
            Debug.Log("Structure copied to clipboard!");
        }

        GUILayout.Space(10);
        GUILayout.Label("Project Structure:");

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
        GUILayout.TextArea(outputText, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();
    }

    private void GenerateStructure()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("📁 PROJECT STRUCTURE");
        sb.AppendLine("====================");

        PrintDirectory("Assets", "", sb, 0);

        outputText = sb.ToString();
        Debug.Log("Structure generated! Click 'Copy to Clipboard' to copy.");
    }

    private static void PrintDirectory(string path, string indent, StringBuilder sb, int depth)
    {
        if (depth > 8) return; // Защита от бесконечной рекурсии

        string folderName = Path.GetFileName(path);
        if (string.IsNullOrEmpty(folderName)) folderName = "Assets";

        sb.AppendLine(indent + "📁 " + folderName + "/");

        // Получаем все подпапки
        var subDirectories = Directory.GetDirectories(path)
            .Where(dir => !dir.Contains(".git") && !dir.Contains("PackageCache"))
            .OrderBy(dir => dir);

        foreach (string dir in subDirectories)
        {
            PrintDirectory(dir, indent + "  ", sb, depth + 1);
        }

        // Получаем важные файлы
        var files = Directory.GetFiles(path)
            .Where(file => !file.EndsWith(".meta"))
            .Where(file =>
                file.EndsWith(".cs") ||
                file.EndsWith(".prefab") ||
                file.EndsWith(".asset") ||
                file.EndsWith(".unity") ||
                file.EndsWith(".uss") ||
                file.EndsWith(".uxml") ||
                file.EndsWith(".shader") ||
                file.EndsWith(".mat"))
            .OrderBy(file => file);

        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            string icon = GetFileIcon(file);
            sb.AppendLine(indent + "  " + icon + " " + fileName);
        }
    }

    private static string GetFileIcon(string filePath)
    {
        return Path.GetExtension(filePath).ToLower() switch
        {
            ".cs" => "📄",
            ".prefab" => "🎯",
            ".asset" => "⚙️",
            ".unity" => "🔄",
            ".uss" => "🎨",
            ".uxml" => "📋",
            ".shader" => "✨",
            ".mat" => "🔶",
            _ => "📄"
        };
    }
}
//// FolderStructurePrinter.cs
//using UnityEngine;
//using UnityEditor;
//using System.IO;
//using System.Text;

//public class FolderStructurePrinter : MonoBehaviour
//{
//    [MenuItem("Tools/Print Folder Structure")]
//    public static void PrintFolderStructure()
//    {
//        StringBuilder sb = new StringBuilder();
//        PrintDirectory("Assets", "", sb);
//        Debug.Log(sb.ToString());
//    }

//    private static void PrintDirectory(string path, string indent, StringBuilder sb)
//    {
//        sb.AppendLine(indent + "📁 " + Path.GetFileName(path));

//        // Подпапки
//        string[] subDirectories = Directory.GetDirectories(path);
//        foreach (string dir in subDirectories)
//        {
//            PrintDirectory(dir, indent + "  ", sb);
//        }

//        // Файлы (только .cs и .prefab для краткости)
//        string[] files = Directory.GetFiles(path);
//        foreach (string file in files)
//        {
//            string ext = Path.GetExtension(file).ToLower();
//            if (ext == ".cs" || ext == ".prefab" || ext == ".asset")
//            {
//                string icon = ext == ".cs" ? "📄" : ext == ".prefab" ? "🎯" : "⚙️";
//                sb.AppendLine(indent + "  " + icon + " " + Path.GetFileName(file));
//            }
//        }
//    }
//}