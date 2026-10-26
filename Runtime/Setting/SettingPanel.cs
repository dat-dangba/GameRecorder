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
        [SerializeField] private Toggle _bgToggle;
        [SerializeField] private ColorPicker _colorPicker;
        [SerializeField] private Camera _cam;
        [SerializeField] private SpriteRenderer _background;

        private void Reset()
        {
            _fpsInputField = transform.Find("FPSInputField").GetComponent<TMP_InputField>();
            _audioToggle = transform.Find("AudioToggle").GetComponent<Toggle>();
            _vFlipToggle = transform.Find("VFlipToggle").GetComponent<Toggle>();
            _bgToggle = transform.Find("BackgroundToggle").GetComponent<Toggle>();
            _colorPicker = GetComponentInChildren<ColorPicker>();
            _cam = FindAnyObjectByType<Camera>();
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

            bool isShowBg = PlayerPrefs.GetInt("record_is_show_bg", 1) == 1;
            _bgToggle.isOn = isShowBg;
            _background.gameObject.SetActive(isShowBg);

            _bgToggle.onValueChanged.AddListener(isOn =>
            {
                _bgToggle.isOn = isOn;
                _background.gameObject.SetActive(isOn);
                PlayerPrefs.SetInt("record_is_show_bg", isOn ? 1 : 0);
            });

            if (_cam == null || _background == null) return;
            string colorString = PlayerPrefs.GetString("record_bg_color",
                $"#{ColorUtility.ToHtmlStringRGB(_cam.backgroundColor)}");
            bool result = ColorUtility.TryParseHtmlString(colorString, out var colorSave);
            if (result)
            {
                _cam.backgroundColor = colorSave;
                _colorPicker.SetColor(colorSave);
            }

            _colorPicker.OnColorChanged += color =>
            {
                _cam.backgroundColor = color;
                PlayerPrefs.SetString("record_bg_color", $"#{ColorUtility.ToHtmlStringRGB(color)}");
            };
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