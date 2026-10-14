using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameRecorder
{
    public class SettingPanel : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _fpsInputField;
        [SerializeField] private Toggle _audioToggle;
        [SerializeField] private Toggle _vFlipToggle;

        private void Reset()
        {
            _fpsInputField = transform.Find("FPSInputField").GetComponent<TMP_InputField>();
            _audioToggle = transform.Find("AudioToggle").GetComponent<Toggle>();
            _vFlipToggle = transform.Find("VFlipToggle").GetComponent<Toggle>();
        }

        private void Awake()
        {
            _fpsInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            _fpsInputField.text = $"{PlayerPrefs.GetInt("record_fps", 30)}";
            _fpsInputField.onValueChanged.AddListener(OnFpsChanged);

            _audioToggle.isOn = PlayerPrefs.GetInt("record_audio", 1) == 1;
            _audioToggle.onValueChanged.AddListener(OnAudioChanged);

            _vFlipToggle.isOn = PlayerPrefs.GetInt("record_vflip", 0) == 1;
            _vFlipToggle.onValueChanged.AddListener(OnVFlipChanged);
        }

        private void OnVFlipChanged(bool arg0)
        {
            PlayerPrefs.SetInt("record_vflip", arg0 ? 1 : 0);
        }

        private void OnAudioChanged(bool arg0)
        {
            PlayerPrefs.SetInt("record_audio", arg0 ? 1 : 0);
        }

        private void OnFpsChanged(string fps)
        {
            if (TryIntFloat(fps, out int value))
            {
                PlayerPrefs.SetInt("record_fps", value);
            }
        }

        private bool TryIntFloat(string fps, out int value)
        {
            return int.TryParse(
                fps,
                System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture,
                out value);
        }
    }
}