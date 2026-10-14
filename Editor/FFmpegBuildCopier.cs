using System.IO;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace GameRecorder.Editor
{
    public static class FFmpegBuildCopier
    {
        [InitializeOnLoadMethod]
        public static void OnEditorLoad()
        {
            if (File.Exists("Assets/StreamingAssets/ffmpeg.exe"))
            {
                return;
            }

            // Tìm đường dẫn thật của package (đúng cả khi cài qua git/UPM, nằm trong PackageCache)
            var pkg = PackageInfo.FindForAssembly(typeof(FFmpegBuildCopier).Assembly);
            if (pkg == null)
            {
                Debug.LogWarning("[FFmpeg] Không tìm thấy package");
                return;
            }

            string src = Path.Combine("Packages/com.ddb.game.recorder", "Tools~", "ffmpeg.exe");
            if (!File.Exists(src))
            {
                Debug.LogWarning($"[FFmpeg] Thiếu file: {src}");
                return;
            }

            string dstDir = Path.Combine("Assets", "StreamingAssets");
            Directory.CreateDirectory(dstDir);
            File.Copy(src, Path.Combine(dstDir, "ffmpeg.exe"), true);
            Debug.Log("[FFmpeg] Copied ffmpeg.exe vào build");
        }

        // public int callbackOrder => 0;
        //
        // public void OnPostprocessBuild(BuildReport report)
        // {
        //     if (report.summary.platform != BuildTarget.StandaloneWindows64) return;
        //
        //     // Tìm đường dẫn thật của package (đúng cả khi cài qua git/UPM, nằm trong PackageCache)
        //     var pkg = PackageInfo.FindForAssembly(typeof(FFmpegBuildCopier).Assembly);
        //     if (pkg == null)
        //     {
        //         Debug.LogWarning("[FFmpeg] Không tìm thấy package");
        //         return;
        //     }
        //
        //     string src = Path.Combine(pkg.resolvedPath, "Tools~", "ffmpeg.exe");
        //     if (!File.Exists(src))
        //     {
        //         Debug.LogWarning($"[FFmpeg] Thiếu file: {src}");
        //         return;
        //     }
        //
        //     string exePath = report.summary.outputPath; // .../Game.exe
        //     string dataDir = Path.Combine(
        //         Path.GetDirectoryName(exePath),
        //         Path.GetFileNameWithoutExtension(exePath) + "_Data");
        //     string dstDir = Path.Combine(dataDir, "StreamingAssets");
        //     Directory.CreateDirectory(dstDir);
        //
        //     File.Copy(src, Path.Combine(dstDir, "ffmpeg.exe"), true);
        //     Debug.Log("[FFmpeg] Copied ffmpeg.exe vào build");
        // }
    }
}