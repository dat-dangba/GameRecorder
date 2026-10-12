using UnityEditor;
using UnityEngine;

namespace GameRecorder
{
    public class GameRecorder : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoad()
        {
            GameObject gameObject = new GameObject("GameRecorder");
            gameObject.AddComponent<GameRecorder>();
        }

        private void Awake()
        {
            EnsureSortingLayer("Hand");
            Hand prefab = Resources.Load<Hand>("Editor/Hand");
            if (prefab)
            {
                Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
            }

            DontDestroyOnLoad(gameObject);
        }

        private void EnsureSortingLayer(string layerName)
        {
            var tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("m_SortingLayers");

            int lastIndex = layers.arraySize - 1;
            int foundIndex = -1;

            for (int i = 0; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == layerName)
                {
                    foundIndex = i;
                    break;
                }
            }

            if (foundIndex >= 0)
            {
                // Đã có: chỉ move nếu chưa ở cuối
                if (foundIndex == lastIndex) return;

                layers.MoveArrayElement(foundIndex, lastIndex);
                tagManager.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                Debug.Log($"[SortingLayer] Moved to end: {layerName}");
                return;
            }

            // Chưa có: tạo mới ở cuối
            int newIndex = layers.arraySize;
            layers.InsertArrayElementAtIndex(newIndex);
            var newLayer = layers.GetArrayElementAtIndex(newIndex);
            newLayer.FindPropertyRelative("name").stringValue = layerName;
            newLayer.FindPropertyRelative("uniqueID").intValue = GenerateUniqueId(layers);
            newLayer.FindPropertyRelative("locked").boolValue = false;

            tagManager.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log($"[SortingLayer] Created: {layerName}");
        }

        // uniqueID phải khác mọi ID hiện có
        private int GenerateUniqueId(SerializedProperty layers)
        {
            int id;
            bool exists;
            do
            {
                id = Random.Range(1, int.MaxValue);
                exists = false;
                for (int i = 0; i < layers.arraySize; i++)
                {
                    if (layers.GetArrayElementAtIndex(i).FindPropertyRelative("uniqueID").intValue == id)
                    {
                        exists = true;
                        break;
                    }
                }
            } while (exists);

            return id;
        }
    }
}