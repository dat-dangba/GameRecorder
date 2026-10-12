using UnityEngine;

namespace GameRecorder
{
    public class GameRecorder : MonoBehaviour
    {
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