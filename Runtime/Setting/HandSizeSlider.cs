using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameRecorder
{
    public class HandSizeSlider : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_InputField _sizeInputField;

        private void Reset()
        {
            _slider = GetComponent<Slider>();
            _sizeInputField = GetComponentInChildren<TMP_InputField>();
        }

        private void Awake()
        {
            _sizeInputField.contentType = TMP_InputField.ContentType.DecimalNumber;
            _sizeInputField.text = $"{PlayerPrefs.GetFloat("hand_size", 1)}";
            _slider.onValueChanged.AddListener(OnValueChanged);
            _sizeInputField.onValueChanged.AddListener(OnSizeInputChanged);
        }

        private void OnSizeInputChanged(string arg0)
        {
            if (TryGetFloat(out float value))
            {
                _slider.value = value;
            }
        }

        private bool TryGetFloat(out float value)
        {
            return float.TryParse(
                _sizeInputField.text,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out value);
        }

        private void OnValueChanged(float size)
        {
            PlayerPrefs.SetFloat("hand_size", size);
            _sizeInputField.text = $"{size}";
            Hand.OnHandSizeChanged?.Invoke(size);
        }
    }
}