using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameRecorder
{
    public class Hand : MonoBehaviour
    {
        [SerializeField] private RectTransform parentRect;
        [SerializeField] private RectTransform dragObj;
        [SerializeField] private Transform _icon;

        private HandSo _handSo;
        private Canvas _canvas;
        private List<Sprite> _sprites;
        private Image _handImage;

        public static Action<int> OnHandSelected;
        public static Action<float> OnHandSizeChanged;

        private void Reset()
        {
            parentRect = transform.parent.GetComponent<RectTransform>();
            dragObj = GetComponent<RectTransform>();

            _icon = transform.Find("Icon");
        }

        private void OnEnable()
        {
            OnHandSelected += SelectHand;
            OnHandSizeChanged += ChangeSizeHand;
        }

        private void OnDisable()
        {
            OnHandSelected -= SelectHand;
            OnHandSizeChanged += ChangeSizeHand;
        }

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            _handSo = Resources.Load<HandSo>("HandSo");
            ChangeSizeHand(PlayerPrefs.GetFloat("hand_size", 1f));
            SelectHand(PlayerPrefs.GetInt("hand_selected", 0));
        }

        private void ChangeSizeHand(float size)
        {
            _icon.localScale = Vector3.one * size;
        }

        private void SelectHand(int selected)
        {
            _sprites = _handSo.Hands[selected].Sprites;
            for (int i = 0; i < _icon.childCount; i++)
            {
                Transform trans = _icon.GetChild(i);
                if (i == selected)
                {
                    trans.gameObject.SetActive(true);
                    _handImage = trans.GetComponent<Image>();
                }
                else
                {
                    trans.gameObject.SetActive(false);
                }
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _handImage.sprite = _sprites[1];
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _handImage.sprite = _sprites[0];
            }

            MoveToMouse();
        }

        private void MoveToMouse()
        {
            Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect, Input.mousePosition, cam, out Vector2 local))
            {
                dragObj.anchoredPosition = local;
            }
        }
    }
}