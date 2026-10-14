using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace GameRecorder.Editor
{
    public static class HandSoCreator
    {
        private const string HAND_SO_ASSET_PATH = "Assets/Resources/HandSo.asset";

        [InitializeOnLoadMethod]
        public static void OnEditorLoad()
        {
            bool result = Create<HandSo>(HAND_SO_ASSET_PATH);
            if (result)
            {
                SetupHandSo();
            }
        }

        private static async void SetupHandSo()
        {
            await Task.Delay(200);
            HandSo handSo = Resources.Load<HandSo>("HandSo");
            handSo.Setup();
            EditorUtility.SetDirty(handSo);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static bool Create<T>(string assetPath) where T : ScriptableObject
        {
            var config = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (config != null) return false;

            EnsureFolderExists(assetPath);

            config = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return true;
        }

        private static void EnsureFolderExists(string assetPath)
        {
            string folderPath = assetPath[..assetPath.LastIndexOf("/", StringComparison.Ordinal)];

            if (AssetDatabase.IsValidFolder(folderPath)) return;

            var parts = folderPath.Split('/');
            var currentPath = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                var nextPath = currentPath + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, parts[i]);
                }

                currentPath = nextPath;
            }
        }
    }
}