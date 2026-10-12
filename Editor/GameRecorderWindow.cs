using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameRecorder.Editor
{
    public class GameRecorderWindow : EditorWindow
    {
        private const float PreviewSize = 50f;
        private const float RowSpacing = 18f;

        private const float SliderTopMargin = 24f;
        private const float SliderWidth = 300;

        private int _selectedIndex;

        private readonly List<Toggle> _toggles = new();
        private Slider _slider;

        private string[] _imagePaths =
        {
            "Editor/Images/Hand_1_1",
            "Editor/Images/Hand_2_1",
            "Editor/Images/Hand_3_1",
        };

        private Texture2D LoadTexture(string resPath)
        {
            var tex = Resources.Load<Texture2D>(resPath);
            if (tex == null)
                Debug.LogWarning($"Không tìm thấy texture trong Resources: {resPath}");
            return tex;
        }

        [MenuItem("Metal/Game Recorder", false, 1)]
        public static void ShowWindow()
        {
            GameRecorderWindow window = GetWindow<GameRecorderWindow>("Game Recorder");
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.backgroundColor = new Color(0.2196078f, 0.2196078f, 0.2196078f);
            root.style.alignItems = Align.Center;
            root.style.justifyContent = Justify.Center;

            // Container căn giữa toàn bộ cụm
            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column
                }
            };
            root.Add(container);

            _toggles.Clear();

            for (int i = 0; i < _imagePaths.Length; i++)
            {
                int index = i;

                var row = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        alignItems = Align.Center,
                        marginBottom = i < _imagePaths.Length - 1 ? RowSpacing : 0
                    }
                };

                var toggle = new Toggle();
                toggle.RegisterValueChangedCallback(evt => OnToggleChanged(index, evt.newValue));
                _toggles.Add(toggle);

                var image = new Image
                {
                    image = LoadTexture(_imagePaths[i]),
                    scaleMode = ScaleMode.ScaleToFit,
                    style =
                    {
                        width = PreviewSize,
                        height = PreviewSize
                    }
                };

                row.Add(toggle);
                row.Add(image);
                container.Add(row);
            }

            int selected = SessionState.GetInt("selected", 0);
            for (int i = 0; i < _toggles.Count; i++)
            {
                _toggles[i].SetValueWithoutNotify(i == selected);
            }

            // Slider ở dưới 3 hàng
            _slider = new Slider(0f, 1f)
            {
                value = SessionState.GetFloat("size", 0.5f),
                showInputField = true,
                direction = SliderDirection.Horizontal,
                style =
                {
                    width = SliderWidth,
                    marginTop = SliderTopMargin
                }
            };
            _slider.RegisterValueChangedCallback(evt => OnSliderChanged(evt.newValue));

            container.Add(_slider);
        }

        private void OnSliderChanged(float value)
        {
            SessionState.SetFloat("size", value);
        }

        private void OnToggleChanged(int index, bool isOn)
        {
            if (isOn)
            {
                _selectedIndex = index;

                // Tắt 2 toggle còn lại (SetValueWithoutNotify để không gọi lại callback)
                for (int i = 0; i < _toggles.Count; i++)
                {
                    if (i != index)
                        _toggles[i].SetValueWithoutNotify(false);
                }
            }
            else if (_selectedIndex == index)
            {
                // Không cho bỏ chọn chính nó -> luôn có 1 cái được chọn (radio behavior)
                _toggles[index].SetValueWithoutNotify(true);
            }

            SessionState.SetInt("selected", index);
        }
    }
}