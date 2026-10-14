using UnityEngine;
using UnityEngine.Events;

namespace GameRecorder
{
    public class GameRecorder : MonoBehaviour
    {
        [SerializeField] private UnityEvent _f5Event;
        [SerializeField] private UnityEvent _f6Event;
        [SerializeField] private UnityEvent _f7Event;

        private KeyCode _f5 = KeyCode.F5;
        private KeyCode _f6 = KeyCode.F6;
        private KeyCode _f7 = KeyCode.F7;
        private KeyCode _openSetting = KeyCode.F8;

        private SettingPanel _settingPanel;

        private void Awake()
        {
            _settingPanel = GetComponentInChildren<SettingPanel>(true);
        }

        private void Update()
        {
            if (Input.GetKeyDown(_openSetting))
            {
                _settingPanel?.gameObject.SetActive(!_settingPanel.gameObject.activeSelf);
            }
            else if (Input.GetKeyDown(_f5))
            {
                _f5Event?.Invoke();
            }
            else if (Input.GetKeyDown(_f6))
            {
                _f6Event?.Invoke();
            }
            else if (Input.GetKeyDown(_f7))
            {
                _f7Event?.Invoke();
            }
        }

        public void Test()
        {
            Debug.Log($"datdb - OpenCheat");
        }
    }
}