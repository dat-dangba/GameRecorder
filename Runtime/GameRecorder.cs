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
            Hand prefab = Resources.Load<Hand>("Editor/Hand");
            if (prefab)
            {
                Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
            }
        }
    }
}