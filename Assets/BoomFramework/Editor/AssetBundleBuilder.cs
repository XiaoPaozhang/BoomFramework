using UnityEditor;
using System.IO;
using UnityEngine;

public class AssetBundleBuilder
{
    private const string ASSET_BUNDLE_DIRECTORY = "Assets/StreamingAssets";

    [MenuItem("BoomFramework/打ab包")]
    static void BuildAllAssetBundles()
    {
        try
        {
            Debug.Log("========== 开始打包 AssetBundle ==========");
            Debug.Log("• 压缩策略: LZ4 (ChunkBasedCompression)");
            Debug.Log("• 输出目录: Assets/StreamingAssets");
            Debug.Log("• 构建目标: StandaloneWindows64");
            Debug.Log("========================================");

            // 确保输出目录存在
            if (!Directory.Exists(ASSET_BUNDLE_DIRECTORY))
            {
                Directory.CreateDirectory(ASSET_BUNDLE_DIRECTORY);
                Debug.Log($"✓ 创建输出目录: {ASSET_BUNDLE_DIRECTORY}");
            }

            Debug.Log("正在打包资源...");

            // 打包 AssetBundle
            BuildPipeline.BuildAssetBundles(
                ASSET_BUNDLE_DIRECTORY,
                BuildAssetBundleOptions.ChunkBasedCompression, // LZ4 压缩
                BuildTarget.StandaloneWindows64
            );

            Debug.Log("刷新资源数据库...");

            // 刷新 AssetDatabase
            AssetDatabase.Refresh();

            // 计算打包结果
            long totalSize = GetDirectorySize(ASSET_BUNDLE_DIRECTORY);
            string sizeStr = FormatBytes(totalSize);

            Debug.Log("========== 打包完成 ==========");
            Debug.Log($"✓ 输出目录: {Path.GetFullPath(ASSET_BUNDLE_DIRECTORY)}");
            Debug.Log($"✓ 总大小: {sizeStr}");
            Debug.Log($"✓ 压缩策略: LZ4 (ChunkBasedCompression)");
            Debug.Log("==============================");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("========== 打包失败 ==========");
            Debug.LogError($"✗ 错误信息: {ex.Message}");
            Debug.LogError($"✗ 堆栈跟踪: {ex.StackTrace}");
            Debug.LogError("==============================");
        }
    }

    /// <summary>
    /// 获取目录总大小
    /// </summary>
    static long GetDirectorySize(string path)
    {
        if (!Directory.Exists(path))
            return 0;

        long totalSize = 0;
        var dirInfo = new DirectoryInfo(path);

        foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
        {
            totalSize += file.Length;
        }

        return totalSize;
    }

    /// <summary>
    /// 格式化字节大小为可读格式
    /// </summary>
    static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}

