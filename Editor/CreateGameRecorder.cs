using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameRecorder.Editor
{
    public static class CreateGameRecorder
    {
        [MenuItem("GameRecorder/Create", false, 0)]
        public static void Create()
        {
            GameObject prefab = Resources.Load<GameObject>("GameRecorder");
            if (prefab == null)
            {
                Debug.LogError("Không tìm thấy prefab 'GameRecorder' trong thư mục Resources");
                return;
            }

            // Tránh tạo trùng nếu scene đã có
            var existing = Object.FindAnyObjectByType<GameRecorder>();
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing);
                return;
            }

            GameObject canvasGo = GameObject.Find("Canvas");
            Canvas canvas = canvasGo != null ? canvasGo.GetComponent<Canvas>() : Object.FindAnyObjectByType<Canvas>();

            GameObject instance;
            if (canvas == null)
            {
                instance =
                    (GameObject)PrefabUtility.InstantiatePrefab(prefab, SceneManager.GetActiveScene());
            }
            else
            {
                instance =
                    (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
            }

            Undo.RegisterCreatedObjectUndo(instance, "Create GameRecorder");

            PrefabUtility.UnpackPrefabInstance(
                instance,
                PrefabUnpackMode.Completely,
                InteractionMode.UserAction);

            Selection.activeGameObject = instance;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            Camera cam = Object.FindAnyObjectByType<Camera>();
            cam.gameObject.AddComponent<AudioCapture>();
        }
    }
}